using System.Net;
using System.Text;
using System.Text.Json;

namespace EaTestAutomation.Reporting
{
    /// <summary>
    /// Builds the unified master dashboard under <c>DashboardReport/index.html</c>
    /// with charts and links to Extent, per-test, video, trace, healing, and logs.
    /// </summary>
    public static class MasterDashboardGenerator
    {
        public static void Generate()
        {
            string artifactsRoot = DashboardPaths.ResolveArtifactsRoot();
            Directory.CreateDirectory(artifactsRoot);

            List<TestRunRecord> history = TestRunRegistry.LoadAll();
            List<ArtifactRunInfo> artifactRuns = ScanArtifactFolders(artifactsRoot);
            var artifactByFolder = artifactRuns.ToDictionary(
                r => r.FolderName,
                StringComparer.OrdinalIgnoreCase);

            MergeHistoryWithArtifacts(history, artifactRuns);

            var chartData = new DashboardChartData
            {
                Passed = history.Count(r => r.Status == "Passed"),
                Failed = history.Count(r => r.Status == "Failed"),
                Unknown = history.Count(r => r.Status != "Passed" && r.Status != "Failed"),
                RecentRuns = history
                    .OrderByDescending(r => r.FinishedUtc)
                    .Take(15)
                    .Select(r => new { r.TestName, r.Status, r.FinishedUtc })
                    .ToList(),
                HealingCounts = artifactRuns
                    .Select(r => new { r.FolderName, r.HealingMappingCount })
                    .ToList()
            };

            foreach (string dashboardRoot in DashboardPaths.GetAllDashboardRoots())
            {
                Directory.CreateDirectory(dashboardRoot);

                string html = BuildMasterHtml(
                    history,
                    artifactByFolder,
                    dashboardRoot,
                    artifactsRoot);

                File.WriteAllText(Path.Combine(dashboardRoot, "index.html"), html);

                File.WriteAllText(
                    Path.Combine(dashboardRoot, "dashboard-data.json"),
                    JsonSerializer.Serialize(chartData, new JsonSerializerOptions { WriteIndented = true }));
            }
        }

        private static void MergeHistoryWithArtifacts(
            List<TestRunRecord> history,
            List<ArtifactRunInfo> artifactRuns)
        {
            foreach (ArtifactRunInfo run in artifactRuns)
            {
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
                        TestName = run.DisplayName,
                        ArtifactFolder = run.FolderName,
                        Status = "Unknown",
                        FinishedUtc = run.LastWriteUtc,
                        HasVideo = run.HasVideo,
                        HasTrace = run.HasTrace,
                        HasScreenshot = run.HasScreenshot,
                        HealingMappingCount = run.HealingMappingCount
                    });
                }
                else
                {
                    existing.HasVideo = run.HasVideo;
                    existing.HasTrace = run.HasTrace;
                    existing.HasScreenshot = run.HasScreenshot;
                    existing.HealingMappingCount = run.HealingMappingCount;
                }
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

                string dashboardHtml = Path.Combine(dir, "dashboard.html");
                string traceZip = Path.Combine(dir, "trace", "trace.zip");
                string logFile = Path.Combine(dir, "logs", "execution.log");
                string healingStore = Path.Combine(dir, "healing", "FailedLocatorStore.json");

                list.Add(new ArtifactRunInfo
                {
                    FolderName = name,
                    DisplayName = ExtractDisplayName(name),
                    FullPath = dir,
                    LastWriteUtc = Directory.GetLastWriteTimeUtc(dir),
                    DashboardHtmlPath = File.Exists(dashboardHtml) ? dashboardHtml : null,
                    VideoFilePath = videoFile,
                    TraceFilePath = File.Exists(traceZip) ? traceZip : null,
                    ScreenshotFilePath = screenshotFile,
                    LogFilePath = File.Exists(logFile) ? logFile : null,
                    HealingStorePath = File.Exists(healingStore) ? healingStore : null,
                    HasVideo = videoFile != null,
                    HasTrace = File.Exists(traceZip),
                    HasScreenshot = screenshotFile != null,
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
        /// Builds a relative URL from the dashboard HTML folder to a file under Artifacts.
        /// </summary>
        private static string? ToRelativeHref(string dashboardRoot, string? absolutePath)
        {
            if (string.IsNullOrWhiteSpace(absolutePath))
            {
                return null;
            }

            if (!File.Exists(absolutePath))
            {
                return null;
            }

            string rel = Path.GetRelativePath(
                Path.GetFullPath(dashboardRoot),
                Path.GetFullPath(absolutePath));

            return EncodeHref(rel);
        }

        private static string EncodeHref(string relativePath)
        {
            return string.Join(
                "/",
                relativePath
                    .Replace('\\', '/')
                    .Split('/', StringSplitOptions.RemoveEmptyEntries)
                    .Select(Uri.EscapeDataString));
        }

        private static string BuildMasterHtml(
            List<TestRunRecord> history,
            Dictionary<string, ArtifactRunInfo> artifactByFolder,
            string dashboardRoot,
            string artifactsRoot)
        {
            int passed = history.Count(r => r.Status == "Passed");
            int failed = history.Count(r => r.Status == "Failed");
            int unknown = history.Count(r => r.Status != "Passed" && r.Status != "Failed");
            int total = history.Count;
            int healingTotal = history.Sum(r => r.HealingMappingCount);

            string extentFile = Path.Combine(artifactsRoot, "ExtentReport", "ExtentDashboard.html");
            string? extentRel = ToRelativeHref(dashboardRoot, extentFile);
            string? artifactsIndexRel = ToRelativeHref(
                dashboardRoot,
                Path.Combine(artifactsRoot, "index.html"));

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang='en'><head>");
            sb.AppendLine("<meta charset='utf-8'/>");
            sb.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1'/>");
            sb.AppendLine("<title>EA Test Automation — Master Dashboard</title>");
            sb.AppendLine("<script src='https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js'></script>");
            sb.AppendLine("<style>");
            sb.AppendLine(Css());
            sb.AppendLine("</style></head><body>");

            sb.AppendLine("<header class='header'>");
            sb.AppendLine("<div><h1>EA Test Automation</h1>");
            sb.AppendLine("<p class='subtitle'>Unified reporting dashboard — all runs, artifacts, and reports</p></div>");
            sb.AppendLine($"<div class='stamp'>Updated {DateTime.Now:yyyy-MM-dd HH:mm:ss}</div>");
            sb.AppendLine("</header>");

            sb.AppendLine("<section class='kpi-grid'>");
            AppendKpi(sb, "Total runs", total.ToString(), "kpi-blue");
            AppendKpi(sb, "Passed", passed.ToString(), "kpi-green");
            AppendKpi(sb, "Failed", failed.ToString(), "kpi-red");
            AppendKpi(sb, "Healed locators", healingTotal.ToString(), "kpi-amber");
            sb.AppendLine("</section>");

            sb.AppendLine("<section class='charts-grid'>");
            sb.AppendLine("<div class='card chart-card'><h2>Test status</h2><canvas id='statusChart'></canvas></div>");
            sb.AppendLine("<div class='card chart-card'><h2>Recent runs</h2><canvas id='recentChart'></canvas></div>");
            sb.AppendLine("<div class='card chart-card'><h2>AI healing mappings</h2><canvas id='healingChart'></canvas></div>");
            sb.AppendLine("</section>");

            sb.AppendLine("<section class='card'>");
            sb.AppendLine("<h2>Separate reports</h2>");
            sb.AppendLine("<p class='muted'>Open dedicated reports in a new tab.</p>");
            sb.AppendLine("<div class='report-links'>");

            if (!string.IsNullOrEmpty(extentRel))
            {
                sb.AppendLine(ReportButton(
                    "Extent Report",
                    "Full Spark HTML report with steps and screenshots",
                    extentRel,
                    "btn-extent"));
            }

            if (!string.IsNullOrEmpty(artifactsIndexRel))
            {
                sb.AppendLine(ReportButton(
                    "Artifacts index",
                    "All per-test artifact folders",
                    artifactsIndexRel,
                    "btn-artifacts"));
            }

            sb.AppendLine("</div></section>");

            sb.AppendLine("<section class='card'>");
            sb.AppendLine("<h2>Test execution report</h2>");
            sb.AppendLine("<p class='muted'>Status and timestamps are informational. Use <strong>Execution report</strong> links to open each file.</p>");
            sb.AppendLine("<div class='table-wrap'><table>");
            sb.AppendLine("<thead><tr>");
            sb.AppendLine("<th>Test</th><th>Status</th><th>Finished (UTC)</th><th>Artifact detail</th><th>Execution report</th>");
            sb.AppendLine("</tr></thead><tbody>");

            var ordered = history
                .OrderByDescending(h => h.FinishedUtc)
                .Take(50)
                .ToList();

            foreach (TestRunRecord row in ordered)
            {
                string folder = row.ArtifactFolder;

                if (string.IsNullOrWhiteSpace(folder))
                {
                    continue;
                }

                artifactByFolder.TryGetValue(folder, out ArtifactRunInfo? info);

                string statusClass = row.Status switch
                {
                    "Passed" => "badge-pass",
                    "Failed" => "badge-fail",
                    _ => "badge-unknown"
                };

                string? detailHref = info != null
                    ? ToRelativeHref(dashboardRoot, info.DashboardHtmlPath)
                    : null;

                sb.Append("<tr>");
                sb.Append("<td>");
                sb.Append(WebUtility.HtmlEncode(row.TestName));
                sb.Append("</td><td><span class='badge ");
                sb.Append(statusClass);
                sb.Append("'>");
                sb.Append(WebUtility.HtmlEncode(row.Status));
                sb.Append("</span></td><td>");
                sb.Append(row.FinishedUtc == default
                    ? "—"
                    : row.FinishedUtc.ToString("yyyy-MM-dd HH:mm:ss"));
                sb.Append("</td><td class='links'>");
                sb.Append(IconLink(detailHref, "Open detail", detailHref != null));
                sb.Append("</td><td class='links'>");

                if (info != null)
                {
                    sb.Append(IconLink(ToRelativeHref(dashboardRoot, info.VideoFilePath), "Video", info.VideoFilePath != null));
                    sb.Append(IconLink(ToRelativeHref(dashboardRoot, info.TraceFilePath), "Trace", info.TraceFilePath != null));
                    sb.Append(IconLink(ToRelativeHref(dashboardRoot, info.ScreenshotFilePath), "Screenshot", info.ScreenshotFilePath != null));
                    sb.Append(IconLink(ToRelativeHref(dashboardRoot, info.LogFilePath), "Logs", info.LogFilePath != null));
                    sb.Append(IconLink(ToRelativeHref(dashboardRoot, info.HealingStorePath), "Healing", info.HealingStorePath != null));
                }

                sb.AppendLine("</td></tr>");
            }

            sb.AppendLine("</tbody></table></div></section>");
            sb.AppendLine("<footer class='footer'>EA Framework · Playwright · Extent · Self-healing</footer>");

            sb.AppendLine("<script>");
            sb.AppendLine($"const passed={passed}, failed={failed}, unknown={unknown};");
            sb.AppendLine(BuildChartScript(ordered));
            sb.AppendLine("</script></body></html>");

            return sb.ToString();
        }

        private static string BuildChartScript(List<TestRunRecord> recent)
        {
            var slice = recent.Take(10).ToList();

            var labels = slice.Select(r => JsonSerializer.Serialize(
                r.TestName.Length > 24 ? r.TestName[..24] + "…" : r.TestName));

            var statusColors = slice.Select(r =>
                r.Status == "Passed" ? "'#22c55e'"
                : r.Status == "Failed" ? "'#ef4444'"
                : "'#94a3b8'");

            var healingLabels = slice.Select(r => JsonSerializer.Serialize(
                r.TestName.Length > 20 ? r.TestName[..20] + "…" : r.TestName));

            var healingData = slice.Select(r => r.HealingMappingCount.ToString());
            int barCount = slice.Count;

            return $@"
new Chart(document.getElementById('statusChart'), {{
  type: 'doughnut',
  data: {{
    labels: ['Passed','Failed','Unknown'],
    datasets: [{{ data: [passed, failed, unknown],
      backgroundColor: ['#22c55e','#ef4444','#64748b'], borderWidth: 0 }}]
  }},
  options: {{ plugins: {{ legend: {{ position: 'bottom', labels: {{ color: '#e2e8f0' }} }} }}, cutout: '62%' }}
}});
new Chart(document.getElementById('recentChart'), {{
  type: 'bar',
  data: {{
    labels: [{string.Join(",", labels)}],
    datasets: [{{ label: 'Runs', data: [{string.Join(",", Enumerable.Repeat("1", barCount))}],
      backgroundColor: [{string.Join(",", statusColors)}] }}]
  }},
  options: {{ scales: {{ x: {{ ticks: {{ color: '#94a3b8', maxRotation: 45 }} }}, y: {{ ticks: {{ color: '#94a3b8' }}, beginAtZero: true }} }} }},
    plugins: {{ legend: {{ display: false }} }}
}});
new Chart(document.getElementById('healingChart'), {{
  type: 'bar',
  data: {{
    labels: [{string.Join(",", healingLabels)}],
    datasets: [{{ label: 'Mappings', data: [{string.Join(",", healingData)}], backgroundColor: '#f59e0b' }}]
  }},
  options: {{ indexAxis: 'y', scales: {{ x: {{ ticks: {{ color: '#94a3b8' }}, beginAtZero: true }}, y: {{ ticks: {{ color: '#94a3b8' }} }} }} }},
    plugins: {{ legend: {{ display: false }} }}
}});";
        }

        private static void AppendKpi(StringBuilder sb, string label, string value, string cssClass)
        {
            sb.Append($"<div class='kpi {cssClass}'><div class='kpi-value'>{WebUtility.HtmlEncode(value)}</div>");
            sb.Append($"<div class='kpi-label'>{WebUtility.HtmlEncode(label)}</div></div>");
        }

        private static string ReportButton(string title, string desc, string href, string css)
        {
            return $@"<a class='report-btn {css}' href='{WebUtility.HtmlEncode(href)}' target='_blank' rel='noopener'>
  <span class='report-title'>{WebUtility.HtmlEncode(title)}</span>
  <span class='report-desc'>{WebUtility.HtmlEncode(desc)}</span></a>";
        }

        private static string IconLink(string? href, string label, bool enabled)
        {
            if (!enabled || string.IsNullOrWhiteSpace(href))
            {
                return $"<span class='link-disabled'>{WebUtility.HtmlEncode(label)}</span> ";
            }

            return $"<a href=\"{href}\" target=\"_blank\" rel=\"noopener\">{WebUtility.HtmlEncode(label)}</a> ";
        }

        private static string Css() => """
            :root { --bg:#0b1220; --card:#111827; --border:#1e293b; --text:#e2e8f0; --muted:#94a3b8; }
            * { box-sizing:border-box; }
            body { margin:0; font-family:'Segoe UI',system-ui,sans-serif; background:var(--bg); color:var(--text); }
            .header { display:flex; justify-content:space-between; align-items:flex-start; padding:28px 32px; border-bottom:1px solid var(--border); background:linear-gradient(135deg,#0f172a 0%,#1e1b4b 100%); }
            h1 { margin:0 0 6px; font-size:1.75rem; }
            .subtitle { margin:0; color:var(--muted); }
            .stamp { color:var(--muted); font-size:.85rem; }
            .kpi-grid { display:grid; grid-template-columns:repeat(auto-fit,minmax(160px,1fr)); gap:16px; padding:24px 32px; }
            .kpi { background:var(--card); border:1px solid var(--border); border-radius:12px; padding:20px; }
            .kpi-value { font-size:2rem; font-weight:700; }
            .kpi-label { color:var(--muted); margin-top:4px; font-size:.9rem; }
            .kpi-green .kpi-value { color:#22c55e; } .kpi-red .kpi-value { color:#ef4444; }
            .kpi-blue .kpi-value { color:#38bdf8; } .kpi-amber .kpi-value { color:#f59e0b; }
            .charts-grid { display:grid; grid-template-columns:repeat(auto-fit,minmax(280px,1fr)); gap:20px; padding:0 32px 24px; }
            .card { background:var(--card); border:1px solid var(--border); border-radius:12px; padding:20px 24px; margin:0 32px 24px; }
            .chart-card { margin:0; min-height:320px; }
            .chart-card h2 { margin:0 0 16px; font-size:1.1rem; }
            .muted { color:var(--muted); font-size:.9rem; }
            .report-links { display:grid; grid-template-columns:repeat(auto-fit,minmax(220px,1fr)); gap:12px; margin-top:12px; }
            .report-btn { display:block; padding:16px; border-radius:10px; text-decoration:none; color:var(--text); border:1px solid var(--border); transition:transform .15s,border-color .15s; }
            .report-btn:hover { transform:translateY(-2px); border-color:#38bdf8; }
            .report-title { display:block; font-weight:600; margin-bottom:4px; }
            .report-desc { display:block; font-size:.8rem; color:var(--muted); }
            .btn-extent { background:linear-gradient(135deg,#1e3a5f,#0f172a); }
            .btn-artifacts { background:linear-gradient(135deg,#312e81,#0f172a); }
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
            public string? DashboardHtmlPath { get; set; }
            public string? VideoFilePath { get; set; }
            public string? TraceFilePath { get; set; }
            public string? ScreenshotFilePath { get; set; }
            public string? LogFilePath { get; set; }
            public string? HealingStorePath { get; set; }
            public bool HasVideo { get; set; }
            public bool HasTrace { get; set; }
            public bool HasScreenshot { get; set; }
            public int HealingMappingCount { get; set; }
        }

        private sealed class DashboardChartData
        {
            public int Passed { get; set; }
            public int Failed { get; set; }
            public int Unknown { get; set; }
            public object? RecentRuns { get; set; }
            public object? HealingCounts { get; set; }
        }
    }
}
