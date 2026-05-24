using System.Net;
using System.Text;
using System.Text.Json;

namespace EaTestAutomation.Reporting
{
    /// <summary>
    /// Builds the single framework report at <c>DashboardReport/index.html</c>
    /// with real-time charts, search, filters, and artifact links (polls <c>dashboard-data.json</c>).
    /// </summary>
    public static class MasterDashboardGenerator
    {
        private static readonly object WriteGate = new();

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static void Generate()
        {
            DashboardReportServer.EnsureStarted();
            WriteAllDashboardOutputs();
        }

        /// <summary>
        /// Rebuilds JSON from disk (called on each live API poll while the server is running).
        /// </summary>
        public static RealtimeDashboardPayload RefreshLivePayload()
        {
            RealtimeDashboardPayload payload = BuildLatestPayload();
            string primaryRoot = DashboardPaths.ResolveDashboardRoot();
            Directory.CreateDirectory(primaryRoot);

            string json = JsonSerializer.Serialize(payload, JsonOptions);
            File.WriteAllText(Path.Combine(primaryRoot, "dashboard-data.json"), json);

            foreach (string root in DashboardPaths.GetAllDashboardRoots())
            {
                if (!string.Equals(root, primaryRoot, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        Directory.CreateDirectory(root);
                        File.WriteAllText(Path.Combine(root, "dashboard-data.json"), json);
                    }
                    catch
                    {
                        // best-effort mirror
                    }
                }
            }

            return payload;
        }

        private static RealtimeDashboardPayload BuildLatestPayload()
        {
            string artifactsRoot = DashboardPaths.ResolveArtifactsRoot();
            Directory.CreateDirectory(artifactsRoot);

            List<TestRunRecord> history = TestRunRegistry.LoadAll();
            List<ArtifactRunInfo> artifactRuns = ScanArtifactFolders(artifactsRoot);
            var artifactByFolder = artifactRuns.ToDictionary(
                r => r.FolderName,
                StringComparer.OrdinalIgnoreCase);

            MergeHistoryWithArtifacts(history, artifactRuns);

            return BuildPayload(
                history,
                artifactByFolder,
                DashboardPaths.ResolveDashboardRoot());
        }

        private static void WriteAllDashboardOutputs()
        {
            lock (WriteGate)
            {
                RealtimeDashboardPayload payload = BuildLatestPayload();

                foreach (string dashboardRoot in DashboardPaths.GetAllDashboardRoots())
                {
                    Directory.CreateDirectory(dashboardRoot);

                    File.WriteAllText(
                        Path.Combine(dashboardRoot, "dashboard-data.json"),
                        JsonSerializer.Serialize(payload, JsonOptions));

                    File.WriteAllText(
                        Path.Combine(dashboardRoot, "index.html"),
                        BuildShellHtml(payload));
                }
            }
        }

        private static RealtimeDashboardPayload BuildPayload(
            List<TestRunRecord> history,
            Dictionary<string, ArtifactRunInfo> artifactByFolder,
            string dashboardRoot)
        {
            var deduped = history
                .GroupBy(h => h.ArtifactFolder, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderByDescending(x => x.FinishedUtc).First())
                .ToList();

            string artifactsRoot = DashboardPaths.ResolveArtifactsRoot();

            foreach (TestRunRecord row in deduped)
            {
                row.Status = NormalizeStatus(row, artifactsRoot);
            }

            int passed = deduped.Count(r => r.Status == "Passed");
            int failed = deduped.Count(r => r.Status == "Failed");
            int unknown = deduped.Count(r => r.Status == "Unknown");
            int total = deduped.Count;

            double passRate = total > 0 ? Math.Round(100.0 * passed / total, 1) : 0;
            double failRate = total > 0 ? Math.Round(100.0 * failed / total, 1) : 0;
            double unknownRate = total > 0 ? Math.Round(100.0 * unknown / total, 1) : 0;

            var runs = deduped
                .OrderByDescending(h => h.FinishedUtc)
                .Take(100)
                .Select(row =>
                {
                    artifactByFolder.TryGetValue(row.ArtifactFolder, out ArtifactRunInfo? info);

                    DateTime? started = row.StartedUtc == default ? null : row.StartedUtc;
                    DateTime? finished = row.FinishedUtc == default ? null : row.FinishedUtc;
                    long durationMs = row.DurationMs;

                    if (durationMs <= 0 && started.HasValue && finished.HasValue)
                    {
                        durationMs = Math.Max(0, (long)(finished.Value - started.Value).TotalMilliseconds);
                    }

                    return new DashboardRunRow
                    {
                        RunId = string.IsNullOrWhiteSpace(row.RunId) ? row.ArtifactFolder : row.RunId,
                        TestName = row.TestName,
                        Status = row.Status,
                        StartedUtc = started,
                        FinishedUtc = finished,
                        DurationMs = durationMs,
                        DurationDisplay = FormatDuration(durationMs),
                        ArtifactFolder = row.ArtifactFolder,
                        HealingMappingCount = row.HealingMappingCount,
                        Links = info == null
                            ? new DashboardRunLinks()
                            : new DashboardRunLinks
                            {
                                Video = ToArtifactUrl(info.VideoFilePath),
                                Trace = ToArtifactUrl(info.TraceFilePath),
                                Screenshot = ToArtifactUrl(info.ScreenshotFilePath),
                                Logs = ToArtifactUrl(info.LogFilePath),
                                Healing = ToArtifactUrl(info.HealingStorePath)
                            }
                    };
                })
                .ToList();

            var timeline = runs
                .Where(r => r.StartedUtc.HasValue && r.FinishedUtc.HasValue)
                .Select(r => new TimelinePoint
                {
                    TestName = r.TestName,
                    Status = r.Status,
                    StartedUtc = r.StartedUtc!.Value,
                    FinishedUtc = r.FinishedUtc!.Value,
                    DurationMs = r.DurationMs
                })
                .ToList();

            return new RealtimeDashboardPayload
            {
                UpdatedUtc = DateTime.UtcNow,
                DashboardUrl = DashboardReportServer.BaseUrl,
                Total = total,
                Passed = passed,
                Failed = failed,
                Unknown = unknown,
                HealingTotal = deduped.Sum(r => r.HealingMappingCount),
                PassRatePercent = passRate,
                FailRatePercent = failRate,
                UnknownRatePercent = unknownRate,
                Runs = runs,
                Timeline = timeline
            };
        }

        private static string NormalizeStatus(TestRunRecord row, string artifactsRoot)
        {
            if (row.Status.Equals("Passed", StringComparison.OrdinalIgnoreCase))
            {
                return "Passed";
            }

            if (row.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase))
            {
                return "Failed";
            }

            if (ArtifactHasFailureEvidence(artifactsRoot, row.ArtifactFolder))
            {
                return "Failed";
            }

            return "Unknown";
        }

        private static bool ArtifactHasFailureEvidence(string artifactsRoot, string artifactFolder)
        {
            if (string.IsNullOrWhiteSpace(artifactFolder))
            {
                return false;
            }

            string root = Path.Combine(artifactsRoot, artifactFolder);
            string shotDir = Path.Combine(root, "screenshots");

            if (Directory.Exists(shotDir)
                && Directory.EnumerateFiles(shotDir, "failure_*.png").Any())
            {
                return true;
            }

            string logPath = Path.Combine(root, "logs", "execution.log");

            if (File.Exists(logPath))
            {
                try
                {
                    string log = File.ReadAllText(logPath);

                    if (log.Contains("Test failed", StringComparison.OrdinalIgnoreCase)
                        || log.Contains("Assert.", StringComparison.OrdinalIgnoreCase)
                        || log.Contains("PlaywrightException", StringComparison.OrdinalIgnoreCase)
                        || log.Contains("Screenshot skipped", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                catch
                {
                    // log may be locked during an active run
                }
            }

            return false;
        }

        private static string FormatDuration(long durationMs)
        {
            if (durationMs < 1000)
            {
                return $"{durationMs} ms";
            }

            var span = TimeSpan.FromMilliseconds(durationMs);

            if (span.TotalHours >= 1)
            {
                return $"{(int)span.TotalHours}h {span.Minutes}m {span.Seconds}s";
            }

            if (span.TotalMinutes >= 1)
            {
                return $"{span.Minutes}m {span.Seconds}s";
            }

            return $"{span.Seconds}.{span.Milliseconds:D3}s";
        }

        private static void MergeHistoryWithArtifacts(
            List<TestRunRecord> history,
            List<ArtifactRunInfo> artifactRuns)
        {
            foreach (ArtifactRunInfo run in artifactRuns)
            {
                if (!run.HasExecutionArtifacts)
                {
                    continue;
                }

                TestRunRecord? existing = history
                    .FirstOrDefault(h =>
                        string.Equals(
                            h.ArtifactFolder,
                            run.FolderName,
                            StringComparison.OrdinalIgnoreCase));

                if (existing == null)
                {
                    history.Add(new TestRunRecord
                    {
                        RunId = run.FolderName,
                        TestName = run.DisplayName,
                        ArtifactFolder = run.FolderName,
                        Status = "Unknown",
                        StartedUtc = run.LastWriteUtc,
                        FinishedUtc = run.LastWriteUtc,
                        DurationMs = 0,
                        HasVideo = run.HasVideo,
                        HasTrace = run.HasTrace,
                        HasScreenshot = run.HasScreenshot,
                        HealingMappingCount = run.HealingMappingCount
                    });

                    continue;
                }

                existing.HasVideo = run.HasVideo;
                existing.HasTrace = run.HasTrace;
                existing.HasScreenshot = run.HasScreenshot;
                existing.HealingMappingCount = run.HealingMappingCount;
            }
        }

        private static List<ArtifactRunInfo> ScanArtifactFolders(string artifactsRoot)
        {
            var list = new List<ArtifactRunInfo>();

            if (!Directory.Exists(artifactsRoot))
            {
                return list;
            }

            foreach (string dir in Directory.GetDirectories(artifactsRoot))
            {
                string name = Path.GetFileName(dir);

                if (string.Equals(name, "ExtentReport", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string? videoFile = FindFirstFile(Path.Combine(dir, "video"), "*.webm")
                    ?? FindFirstFile(Path.Combine(dir, "video"), "*.mp4");

                string? screenshotFile = FindFirstFile(
                    Path.Combine(dir, "screenshots"),
                    "*.png");

                string traceZip = Path.Combine(dir, "trace", "trace.zip");
                string logFile = Path.Combine(dir, "logs", "execution.log");
                string healingStore = Path.Combine(dir, "healing", "FailedLocatorStore.json");

                bool hasLog = File.Exists(logFile);

                list.Add(new ArtifactRunInfo
                {
                    FolderName = name,
                    DisplayName = ExtractDisplayName(name),
                    FullPath = dir,
                    LastWriteUtc = Directory.GetLastWriteTimeUtc(dir),
                    VideoFilePath = videoFile,
                    TraceFilePath = File.Exists(traceZip) ? traceZip : null,
                    ScreenshotFilePath = screenshotFile,
                    LogFilePath = hasLog ? logFile : null,
                    HealingStorePath = File.Exists(healingStore) ? healingStore : null,
                    HasVideo = videoFile != null,
                    HasTrace = File.Exists(traceZip),
                    HasScreenshot = screenshotFile != null,
                    HasExecutionArtifacts = hasLog || videoFile != null || screenshotFile != null,
                    HealingMappingCount = CountHealingMappings(healingStore)
                });
            }

            return list.OrderByDescending(r => r.LastWriteUtc).ToList();
        }

        private static int CountHealingMappings(string healingStorePath)
        {
            if (!File.Exists(healingStorePath))
            {
                return 0;
            }

            try
            {
                string json = File.ReadAllText(healingStorePath);
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                return dict?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private static string ExtractDisplayName(string folderName)
        {
            int lastUnderscore = folderName.LastIndexOf('_');

            if (lastUnderscore > 0)
            {
                string tail = folderName[(lastUnderscore + 1)..];

                if (tail.Length == 8 && tail.All(Uri.IsHexDigit))
                {
                    int secondLast = folderName.LastIndexOf('_', lastUnderscore - 1);

                    if (secondLast > 0)
                    {
                        return folderName[..secondLast];
                    }
                }
            }

            return folderName;
        }

        private static string? FindFirstFile(string directory, string pattern)
        {
            if (!Directory.Exists(directory))
            {
                return null;
            }

            return Directory.EnumerateFiles(directory, pattern).FirstOrDefault();
        }

        /// <summary>
        /// Builds an HTTP URL served by <see cref="DashboardReportServer"/> under <c>/artifacts/</c>.
        /// </summary>
        private static string? ToArtifactUrl(string? absolutePath)
        {
            if (string.IsNullOrWhiteSpace(absolutePath) || !File.Exists(absolutePath))
            {
                return null;
            }

            string artifactsRoot = Path.GetFullPath(DashboardPaths.ResolveArtifactsRoot());
            string fullPath = Path.GetFullPath(absolutePath);

            if (!fullPath.StartsWith(artifactsRoot, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            string relative = Path.GetRelativePath(artifactsRoot, fullPath)
                .Replace('\\', '/');

            string encoded = string.Join(
                "/",
                relative
                    .Split('/', StringSplitOptions.RemoveEmptyEntries)
                    .Select(Uri.EscapeDataString));

            return $"{DashboardReportServer.BaseUrl}artifacts/{encoded}";
        }

        private static string BuildShellHtml(RealtimeDashboardPayload payload)
        {
            string embeddedJson = JsonSerializer.Serialize(payload, JsonOptions);

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang='en'><head>");
            sb.AppendLine("<meta charset='utf-8'/>");
            sb.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1'/>");
            sb.AppendLine("<title>EA Test Automation — Live Report</title>");
            sb.AppendLine("<script src='https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js'></script>");
            sb.AppendLine("<style>");
            sb.AppendLine(Css());
            sb.AppendLine("</style></head><body>");

            sb.AppendLine("<header class='header'>");
            sb.AppendLine("<div>");
            sb.AppendLine("<h1>EA Test Automation</h1>");
            sb.AppendLine("<p class='subtitle'>Single live report — all framework runs and artifacts</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class='header-meta'>");
            sb.AppendLine("<span class='live-badge'><span class='live-dot'></span> Live</span>");
            sb.AppendLine("<div class='stamp' id='updatedStamp'>Loading…</div>");
            sb.AppendLine("<button type='button' class='btn-download-header' id='btnDownloadWhole'>Download whole execution report</button>");
            sb.AppendLine("</div>");
            sb.AppendLine("</header>");

            sb.AppendLine("<section class='kpi-grid kpi-grid-6'>");
            sb.AppendLine("<div class='kpi kpi-blue'><div class='kpi-value' id='kpiTotal'>—</div><div class='kpi-label'>Total runs</div></div>");
            sb.AppendLine("<div class='kpi kpi-green'><div class='kpi-value' id='kpiPassed'>—</div><div class='kpi-label'>Passed</div></div>");
            sb.AppendLine("<div class='kpi kpi-red'><div class='kpi-value' id='kpiFailed'>—</div><div class='kpi-label'>Failed</div></div>");
            sb.AppendLine("<div class='kpi kpi-purple'><div class='kpi-value' id='kpiPassRate'>—</div><div class='kpi-label'>Pass rate</div></div>");
            sb.AppendLine("<div class='kpi kpi-slate'><div class='kpi-value' id='kpiFailRate'>—</div><div class='kpi-label'>Fail rate</div></div>");
            sb.AppendLine("<div class='kpi kpi-amber'><div class='kpi-value' id='kpiHealing'>—</div><div class='kpi-label'>Healed locators</div></div>");
            sb.AppendLine("</section>");

            sb.AppendLine("<section class='charts-grid charts-grid-5'>");
            sb.AppendLine("<div class='card chart-card'><h2>Pass / fail %</h2><canvas id='passRateChart'></canvas></div>");
            sb.AppendLine("<div class='card chart-card'><h2>Test status</h2><canvas id='statusChart'></canvas></div>");
            sb.AppendLine("<div class='card chart-card'><h2>Run duration</h2><canvas id='runtimeChart'></canvas></div>");
            sb.AppendLine("<div class='card chart-card'><h2>Execution timeline</h2><canvas id='timelineChart'></canvas></div>");
            sb.AppendLine("<div class='card chart-card'><h2>AI healing mappings</h2><canvas id='healingChart'></canvas></div>");
            sb.AppendLine("</section>");

            sb.AppendLine("<section class='card'>");
            sb.AppendLine("<div class='toolbar'>");
            sb.AppendLine("<h2>Test execution report</h2>");
            sb.AppendLine("<div class='toolbar-controls'>");
            sb.AppendLine("<input type='search' id='searchInput' placeholder='Search test, folder, status, runtime…' autocomplete='off'/>");
            sb.AppendLine("<select id='statusFilter'>");
            sb.AppendLine("<option value='all'>All statuses</option>");
            sb.AppendLine("<option value='Passed'>Passed</option>");
            sb.AppendLine("<option value='Failed'>Failed</option>");
            sb.AppendLine("<option value='Unknown'>Unknown</option>");
            sb.AppendLine("</select>");
            sb.AppendLine("</div></div>");
            sb.AppendLine("<p class='muted' id='refreshHint'>Open via local server for live refresh, search, delete, and report downloads.</p>");
            sb.AppendLine("<p class='muted'><a id='dashboardLiveLink' href='#' target='_blank' rel='noopener'>Open live dashboard</a></p>");
            sb.AppendLine("<div class='table-wrap'><table>");
            sb.AppendLine("<thead><tr>");
            sb.AppendLine("<th>Test</th><th>Status</th><th>Runtime</th><th>Started (UTC)</th><th>Finished (UTC)</th><th>Artifacts</th><th>Actions</th>");
            sb.AppendLine("</tr></thead><tbody id='runsBody'></tbody>");
            sb.AppendLine("</table></div></section>");

            sb.AppendLine("<footer class='footer'>EA Framework · Playwright · Self-healing · Single live report</footer>");
            sb.AppendLine("<script type=\"application/json\" id=\"embeddedDashboardData\">");
            sb.AppendLine(embeddedJson);
            sb.AppendLine("</script>");
            sb.AppendLine("<script>");
            sb.AppendLine(ClientScript());
            sb.AppendLine("</script></body></html>");

            return sb.ToString();
        }

        private static string ClientScript() => """
            let lastPayload = null;
            let statusChart, passRateChart, runtimeChart, timelineChart, healingChart;
            const searchInput = document.getElementById('searchInput');
            const statusFilter = document.getElementById('statusFilter');
            const API_BASE = 'http://127.0.0.1:8765';

            function saveUiState() {
              sessionStorage.setItem('eaSearch', searchInput.value || '');
              sessionStorage.setItem('eaStatus', statusFilter.value || 'all');
            }
            function restoreUiState() {
              const s = sessionStorage.getItem('eaSearch');
              const f = sessionStorage.getItem('eaStatus');
              if (s != null) searchInput.value = s;
              if (f) statusFilter.value = f;
            }

            searchInput.addEventListener('input', () => { saveUiState(); renderTable(lastPayload); });
            statusFilter.addEventListener('change', () => { saveUiState(); renderTable(lastPayload); });

            document.querySelector('.kpi-green')?.addEventListener('click', () => { statusFilter.value='Passed'; saveUiState(); renderTable(lastPayload); });
            document.querySelector('.kpi-red')?.addEventListener('click', () => { statusFilter.value='Failed'; saveUiState(); renderTable(lastPayload); });
            document.querySelector('.kpi-blue')?.addEventListener('click', () => { statusFilter.value='all'; saveUiState(); renderTable(lastPayload); });

            function readEmbeddedPayload() {
              const el = document.getElementById('embeddedDashboardData');
              if (!el || !el.textContent) return null;
              try { return JSON.parse(el.textContent); } catch { return null; }
            }

            function normalizeStatus(s) { return (s || '').toLowerCase(); }

            function applyPayload(data) {
              if (!data) return;
              lastPayload = data;
              const live = data.dashboardUrl || API_BASE + '/';
              const link = document.getElementById('dashboardLiveLink');
              if (link) { link.href = live; link.textContent = live; }
              document.getElementById('updatedStamp').textContent =
                'Updated ' + new Date(data.updatedUtc).toLocaleString();
              document.getElementById('kpiTotal').textContent = data.total;
              document.getElementById('kpiPassed').textContent = data.passed;
              document.getElementById('kpiFailed').textContent = data.failed;
              document.getElementById('kpiHealing').textContent = data.healingTotal;
              document.getElementById('kpiPassRate').textContent = (data.passRatePercent ?? 0) + '%';
              document.getElementById('kpiFailRate').textContent = (data.failRatePercent ?? 0) + '%';
              updateCharts(data);
              renderTable(data);
            }

            async function refresh() {
              try {
                const res = await fetch(API_BASE + '/api/dashboard-data.json?_=' + Date.now(), { cache: 'no-store' });
                if (res.ok) { applyPayload(await res.json()); return; }
              } catch (e) { /* fall through */ }
              applyPayload(readEmbeddedPayload());
            }

            async function deleteRun(runId) {
              if (!runId) return;
              if (!confirm('Delete this test run and all artifacts (video, trace, logs, screenshots, healing)?')) return;
              try {
                const res = await fetch(API_BASE + '/api/runs/' + encodeURIComponent(runId), { method: 'DELETE' });
                if (!res.ok) { alert('Delete failed. Is the dashboard server running? Run a test first.'); return; }
                await refresh();
              } catch (e) {
                alert('Delete failed: ' + e.message);
              }
            }

            function downloadRunReport(runId) {
              if (!runId) return;
              window.location.href = API_BASE + '/api/download/run/' + encodeURIComponent(runId);
            }

            function downloadWholeReport() {
              window.location.href = API_BASE + '/api/download/execution-report';
            }

            document.getElementById('btnDownloadWhole')?.addEventListener('click', downloadWholeReport);

            function truncate(s, n) { return (s||'').length > n ? s.slice(0, n) + '…' : (s||''); }
            function fmtUtc(iso) {
              if (!iso) return '—';
              return new Date(iso).toISOString().slice(0, 19).replace('T', ' ');
            }
            function statusColor(status) {
              if (status === 'Passed') return '#22c55e';
              if (status === 'Failed') return '#ef4444';
              return '#94a3b8';
            }

            function updateCharts(data) {
              const runs = (data.runs || []).slice(0, 12);
              const passed = data.passed || 0;
              const failed = data.failed || 0;
              const unknown = data.unknown || 0;
              const passPct = data.passRatePercent ?? 0;
              const failPct = data.failRatePercent ?? 0;
              const unknownPct = data.unknownRatePercent ?? 0;

              if (!passRateChart) {
                passRateChart = new Chart(document.getElementById('passRateChart'), {
                  type: 'doughnut',
                  data: { labels: ['Pass %','Fail %','Unknown %'],
                    datasets: [{ data: [passPct, failPct, unknownPct],
                      backgroundColor: ['#22c55e','#ef4444','#64748b'], borderWidth: 0 }] },
                  options: {
                    plugins: {
                      legend: { position: 'bottom', labels: { color: '#e2e8f0' } },
                      tooltip: { callbacks: { label: (ctx) => ctx.label + ': ' + ctx.raw + '%' } }
                    },
                    cutout: '58%'
                  }
                });
              } else {
                passRateChart.data.datasets[0].data = [passPct, failPct, unknownPct];
                passRateChart.update('none');
              }

              if (!statusChart) {
                statusChart = new Chart(document.getElementById('statusChart'), {
                  type: 'doughnut',
                  data: { labels: ['Passed','Failed','Unknown'],
                    datasets: [{ data: [passed, failed, unknown],
                      backgroundColor: ['#22c55e','#ef4444','#64748b'], borderWidth: 0 }] },
                  options: {
                    plugins: {
                      legend: { position: 'bottom', labels: { color: '#e2e8f0' } },
                      tooltip: { callbacks: { label: (ctx) => {
                        const total = passed + failed + unknown;
                        const pct = total ? Math.round(100 * ctx.raw / total) : 0;
                        return ctx.label + ': ' + ctx.raw + ' (' + pct + '%)';
                      } } }
                    },
                    cutout: '62%'
                  }
                });
              } else {
                statusChart.data.datasets[0].data = [passed, failed, unknown];
                statusChart.update('none');
              }

              const rtLabels = runs.map(r => truncate(r.testName, 18));
              const rtData = runs.map(r => Math.round((r.durationMs || 0) / 1000));
              const rtColors = runs.map(r => statusColor(r.status));

              if (!runtimeChart) {
                runtimeChart = new Chart(document.getElementById('runtimeChart'), {
                  type: 'bar',
                  data: { labels: rtLabels, datasets: [{ label: 'Seconds', data: rtData, backgroundColor: rtColors }] },
                  options: {
                    scales: { x: { ticks: { color: '#94a3b8', maxRotation: 45 } }, y: { ticks: { color: '#94a3b8' }, beginAtZero: true, title: { display: true, text: 'seconds', color: '#94a3b8' } } },
                    plugins: { legend: { display: false } }
                  }
                });
              } else {
                runtimeChart.data.labels = rtLabels;
                runtimeChart.data.datasets[0].data = rtData;
                runtimeChart.data.datasets[0].backgroundColor = rtColors;
                runtimeChart.update('none');
              }

              const timeline = (data.timeline || []).slice(0, 15);
              const tLabels = timeline.map(t => truncate(t.testName, 16));
              const tDurations = timeline.map(t => Math.round((t.durationMs || 0) / 1000));
              const tColors = timeline.map(t => statusColor(t.status));

              if (!timelineChart) {
                timelineChart = new Chart(document.getElementById('timelineChart'), {
                  type: 'bar',
                  data: { labels: tLabels, datasets: [{ label: 'Duration (s)', data: tDurations, backgroundColor: tColors }] },
                  options: {
                    indexAxis: 'y',
                    scales: { x: { ticks: { color: '#94a3b8' }, beginAtZero: true }, y: { ticks: { color: '#94a3b8' } } },
                    plugins: { legend: { display: false }, title: { display: true, text: 'Run timeline (duration)', color: '#94a3b8' } }
                  }
                });
              } else {
                timelineChart.data.labels = tLabels;
                timelineChart.data.datasets[0].data = tDurations;
                timelineChart.data.datasets[0].backgroundColor = tColors;
                timelineChart.update('none');
              }

              const healingLabels = runs.map(r => truncate(r.testName, 20));
              const healingData = runs.map(r => r.healingMappingCount || 0);

              if (!healingChart) {
                healingChart = new Chart(document.getElementById('healingChart'), {
                  type: 'bar',
                  data: { labels: healingLabels, datasets: [{ label: 'Mappings', data: healingData, backgroundColor: '#f59e0b' }] },
                  options: { indexAxis: 'y', scales: { x: { ticks: { color: '#94a3b8' }, beginAtZero: true }, y: { ticks: { color: '#94a3b8' } } }, plugins: { legend: { display: false } } }
                });
              } else {
                healingChart.data.labels = healingLabels;
                healingChart.data.datasets[0].data = healingData;
                healingChart.update('none');
              }
            }

            function badgeClass(status) {
              if (status === 'Passed') return 'badge-pass';
              if (status === 'Failed') return 'badge-fail';
              return 'badge-unknown';
            }

            function linkCell(href, label) {
              if (!href) return `<span class="link-disabled">${label}</span> `;
              return `<a href="${href}" target="_blank" rel="noopener">${label}</a> `;
            }

            function runSearchText(r) {
              return [r.testName, r.artifactFolder, r.status, r.durationDisplay, r.runId].join(' ').toLowerCase();
            }

            function renderTable(data) {
              const body = document.getElementById('runsBody');
              if (!data || !data.runs) {
                body.innerHTML = '<tr><td colspan="7" class="muted">No runs yet.</td></tr>';
                return;
              }
              const q = (searchInput.value || '').trim().toLowerCase();
              const status = statusFilter.value;
              const filtered = data.runs.filter(r => {
                if (status !== 'all' && normalizeStatus(r.status) !== normalizeStatus(status)) return false;
                if (q && !runSearchText(r).includes(q)) return false;
                return true;
              });
              if (!filtered.length) {
                body.innerHTML = '<tr><td colspan="7" class="muted">No matching runs.</td></tr>';
                return;
              }
              body.innerHTML = filtered.map(r => {
                const links = r.links || {};
                const rid = escapeHtml(r.runId || r.artifactFolder || '');
                return `<tr>
                  <td>${escapeHtml(r.testName)}</td>
                  <td><span class="badge ${badgeClass(r.status)}">${escapeHtml(r.status)}</span></td>
                  <td>${escapeHtml(r.durationDisplay || '—')}</td>
                  <td>${fmtUtc(r.startedUtc)}</td>
                  <td>${fmtUtc(r.finishedUtc)}</td>
                  <td class="links">
                    ${linkCell(links.video, 'Video')}
                    ${linkCell(links.trace, 'Trace')}
                    ${linkCell(links.screenshot, 'Screenshot')}
                    ${linkCell(links.logs, 'Logs')}
                    ${linkCell(links.healing, 'Healing')}
                  </td>
                  <td class="actions">
                    <button type="button" class="btn-download" data-run-id="${rid}">Download</button>
                    <button type="button" class="btn-delete" data-run-id="${rid}">Delete</button>
                  </td>
                </tr>`;
              }).join('');
              body.querySelectorAll('.btn-download').forEach(btn => {
                btn.addEventListener('click', () => downloadRunReport(btn.getAttribute('data-run-id')));
              });
              body.querySelectorAll('.btn-delete').forEach(btn => {
                btn.addEventListener('click', () => deleteRun(btn.getAttribute('data-run-id')));
              });
            }

            function escapeHtml(s) {
              const d = document.createElement('div');
              d.textContent = s || '';
              return d.innerHTML;
            }

            restoreUiState();
            applyPayload(readEmbeddedPayload());
            refresh();
            setInterval(refresh, 2000);
            """;

        private static string Css() => """
            :root { --bg:#0b1220; --card:#111827; --border:#1e293b; --text:#e2e8f0; --muted:#94a3b8; }
            * { box-sizing:border-box; }
            body { margin:0; font-family:'Segoe UI',system-ui,sans-serif; background:var(--bg); color:var(--text); }
            .header { display:flex; justify-content:space-between; align-items:flex-start; padding:28px 32px; border-bottom:1px solid var(--border); background:linear-gradient(135deg,#0f172a 0%,#1e1b4b 100%); }
            h1 { margin:0 0 6px; font-size:1.75rem; }
            .subtitle { margin:0; color:var(--muted); }
            .header-meta { text-align:right; }
            .stamp { color:var(--muted); font-size:.85rem; margin-top:8px; }
            .live-badge { display:inline-flex; align-items:center; gap:8px; font-size:.85rem; color:#86efac; font-weight:600; }
            .live-dot { width:8px; height:8px; border-radius:50%; background:#22c55e; animation:pulse 1.5s infinite; }
            @keyframes pulse { 0%,100%{opacity:1} 50%{opacity:.4} }
            .kpi-grid { display:grid; grid-template-columns:repeat(auto-fit,minmax(160px,1fr)); gap:16px; padding:24px 32px; }
            .kpi-grid-6 { grid-template-columns:repeat(auto-fit,minmax(140px,1fr)); }
            .kpi { background:var(--card); border:1px solid var(--border); border-radius:12px; padding:20px; }
            .kpi-value { font-size:2rem; font-weight:700; }
            .kpi-label { color:var(--muted); margin-top:4px; font-size:.9rem; }
            .kpi-green .kpi-value { color:#22c55e; } .kpi-red .kpi-value { color:#ef4444; }
            .kpi-blue .kpi-value { color:#38bdf8; } .kpi-amber .kpi-value { color:#f59e0b; }
            .kpi-purple .kpi-value { color:#a78bfa; } .kpi-slate .kpi-value { color:#94a3b8; }
            .charts-grid { display:grid; grid-template-columns:repeat(auto-fit,minmax(280px,1fr)); gap:20px; padding:0 32px 24px; }
            .charts-grid-4 { grid-template-columns:repeat(auto-fit,minmax(260px,1fr)); }
            .charts-grid-5 { grid-template-columns:repeat(auto-fit,minmax(240px,1fr)); }
            .kpi { cursor:pointer; }
            .btn-delete { background:#7f1d1d; color:#fecaca; border:1px solid #991b1b; border-radius:6px; padding:6px 12px; cursor:pointer; font-size:.8rem; }
            .btn-delete:hover { background:#991b1b; }
            .btn-download { background:#1e3a5f; color:#bae6fd; border:1px solid #2563eb; border-radius:6px; padding:6px 12px; cursor:pointer; font-size:.8rem; margin-right:6px; }
            .btn-download:hover { background:#1d4ed8; }
            .btn-download-header { margin-top:12px; background:#1e3a5f; color:#e0f2fe; border:1px solid #38bdf8; border-radius:8px; padding:10px 16px; cursor:pointer; font-size:.85rem; font-weight:600; }
            .btn-download-header:hover { background:#1d4ed8; }
            .actions { white-space:nowrap; }
            #dashboardLiveLink { color:#38bdf8; }
            .card { background:var(--card); border:1px solid var(--border); border-radius:12px; padding:20px 24px; margin:0 32px 24px; }
            .chart-card { margin:0; min-height:320px; }
            .chart-card h2 { margin:0 0 16px; font-size:1.1rem; }
            .muted { color:var(--muted); font-size:.9rem; }
            .toolbar { display:flex; flex-wrap:wrap; justify-content:space-between; align-items:center; gap:12px; margin-bottom:8px; }
            .toolbar h2 { margin:0; font-size:1.1rem; }
            .toolbar-controls { display:flex; gap:10px; flex-wrap:wrap; }
            #searchInput { padding:10px 14px; border-radius:8px; border:1px solid var(--border); background:#0f172a; color:var(--text); min-width:220px; }
            #statusFilter { padding:10px 14px; border-radius:8px; border:1px solid var(--border); background:#0f172a; color:var(--text); }
            table { width:100%; border-collapse:collapse; font-size:.9rem; }
            th,td { border:1px solid var(--border); padding:10px 12px; text-align:left; }
            th { background:#1e293b; color:var(--muted); font-weight:600; }
            .table-wrap { overflow-x:auto; margin-top:12px; }
            .badge { padding:4px 10px; border-radius:999px; font-size:.75rem; font-weight:600; }
            .badge-pass { background:#14532d; color:#86efac; }
            .badge-fail { background:#7f1d1d; color:#fca5a5; }
            .badge-unknown { background:#334155; color:#cbd5e1; }
            .links a { color:#38bdf8; margin-right:8px; text-decoration:none; font-size:.85rem; }
            .links a:hover { text-decoration:underline; }
            .link-disabled { color:#475569; margin-right:8px; font-size:.85rem; }
            .footer { text-align:center; padding:24px; color:var(--muted); font-size:.8rem; border-top:1px solid var(--border); }
            """;

        private sealed class ArtifactRunInfo
        {
            public string FolderName { get; set; } = "";
            public string DisplayName { get; set; } = "";
            public string FullPath { get; set; } = "";
            public DateTime LastWriteUtc { get; set; }
            public string? VideoFilePath { get; set; }
            public string? TraceFilePath { get; set; }
            public string? ScreenshotFilePath { get; set; }
            public string? LogFilePath { get; set; }
            public string? HealingStorePath { get; set; }
            public bool HasVideo { get; set; }
            public bool HasTrace { get; set; }
            public bool HasScreenshot { get; set; }
            public bool HasExecutionArtifacts { get; set; }
            public int HealingMappingCount { get; set; }
        }
    }
}
