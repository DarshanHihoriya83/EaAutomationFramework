using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json;

namespace EaTestAutomation.Reporting
{
    /// <summary>
    /// Builds ZIP packages for single-run and whole-execution dashboard report downloads.
    /// </summary>
    public static class DashboardReportDownloadService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static bool TryCreateSingleRunZip(string runId, Stream output)
        {
            if (!TryResolveRun(runId, out DashboardRunRow? run, out string artifactFolder))
            {
                return false;
            }

            string artifactsRoot = Path.GetFullPath(DashboardPaths.ResolveArtifactsRoot());
            string runDir = Path.GetFullPath(Path.Combine(artifactsRoot, artifactFolder));

            using var zip = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true);

            var summary = new
            {
                exportedUtc = DateTime.UtcNow,
                run.RunId,
                run.TestName,
                run.Status,
                run.StartedUtc,
                run.FinishedUtc,
                run.DurationMs,
                run.DurationDisplay,
                run.ArtifactFolder,
                run.HealingMappingCount,
                run.Links
            };

            WriteJsonEntry(zip, "report/run-summary.json", summary);
            WriteTextEntry(
                zip,
                "report/run-summary.html",
                BuildSingleRunHtml(run, summary.exportedUtc));

            if (Directory.Exists(runDir) && runDir.StartsWith(artifactsRoot, StringComparison.OrdinalIgnoreCase))
            {
                AddDirectoryToZip(zip, runDir, "artifacts");
            }

            return true;
        }

        public static void CreateWholeExecutionZip(Stream output)
        {
            RealtimeDashboardPayload payload = MasterDashboardGenerator.RefreshLivePayload();
            string artifactsRoot = Path.GetFullPath(DashboardPaths.ResolveArtifactsRoot());

            using var zip = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true);

            WriteJsonEntry(zip, "execution-report.json", payload);
            WriteJsonEntry(zip, "dashboard-data.json", payload);
            WriteTextEntry(
                zip,
                "execution-summary.html",
                BuildWholeExecutionHtml(payload));

            string? dashboardIndex = DashboardPaths.GetMasterIndexPath();
            if (File.Exists(dashboardIndex))
            {
                zip.CreateEntryFromFile(dashboardIndex, "dashboard/index.html", CompressionLevel.Optimal);
            }

            foreach (DashboardRunRow run in payload.Runs)
            {
                if (string.IsNullOrWhiteSpace(run.ArtifactFolder))
                {
                    continue;
                }

                string runDir = Path.GetFullPath(Path.Combine(artifactsRoot, run.ArtifactFolder));

                if (!Directory.Exists(runDir)
                    || !runDir.StartsWith(artifactsRoot, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var runSummary = new
                {
                    run.RunId,
                    run.TestName,
                    run.Status,
                    run.StartedUtc,
                    run.FinishedUtc,
                    run.DurationMs,
                    run.DurationDisplay,
                    run.ArtifactFolder,
                    run.HealingMappingCount,
                    run.Links
                };

                WriteJsonEntry(
                    zip,
                    $"runs/{run.ArtifactFolder}/run-summary.json",
                    runSummary);

                AddDirectoryToZip(zip, runDir, $"runs/{run.ArtifactFolder}/artifacts");
            }
        }

        public static string SanitizeRunId(string runId)
        {
            if (string.IsNullOrWhiteSpace(runId))
            {
                return "";
            }

            string decoded = Uri.UnescapeDataString(runId.Trim());

            if (decoded.Contains("..", StringComparison.Ordinal)
                || decoded.Contains('/', StringComparison.Ordinal)
                || decoded.Contains('\\', StringComparison.Ordinal)
                || decoded.Contains(':', StringComparison.Ordinal))
            {
                return "";
            }

            return Path.GetFileName(decoded);
        }

        public static string BuildSingleRunZipFileName(DashboardRunRow run) =>
            $"EA-SingleRun-{SanitizeFileName(run.TestName)}-{UtcStamp()}.zip";

        public static string BuildWholeExecutionZipFileName() =>
            $"EA-WholeExecutionReport-{UtcStamp()}.zip";

        private static bool TryResolveRun(
            string runId,
            out DashboardRunRow run,
            out string artifactFolder)
        {
            run = new DashboardRunRow();
            artifactFolder = "";

            string safeId = SanitizeRunId(runId);
            if (string.IsNullOrEmpty(safeId))
            {
                return false;
            }

            RealtimeDashboardPayload payload = MasterDashboardGenerator.RefreshLivePayload();
            DashboardRunRow? match = payload.Runs.FirstOrDefault(r =>
                string.Equals(r.RunId, safeId, StringComparison.OrdinalIgnoreCase)
                || string.Equals(r.ArtifactFolder, safeId, StringComparison.OrdinalIgnoreCase));

            if (match == null)
            {
                string artifactsRoot = DashboardPaths.ResolveArtifactsRoot();
                string candidate = Path.Combine(artifactsRoot, safeId);

                if (!Directory.Exists(candidate))
                {
                    return false;
                }

                match = new DashboardRunRow
                {
                    RunId = safeId,
                    ArtifactFolder = safeId,
                    TestName = safeId,
                    Status = "Unknown"
                };
            }

            run = match;
            artifactFolder = string.IsNullOrWhiteSpace(match.ArtifactFolder)
                ? match.RunId
                : match.ArtifactFolder;

            return !string.IsNullOrWhiteSpace(artifactFolder);
        }

        private static void AddDirectoryToZip(ZipArchive zip, string sourceDir, string entryPrefix)
        {
            foreach (string file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                string relative = Path.GetRelativePath(sourceDir, file).Replace('\\', '/');
                string entryName = $"{entryPrefix}/{relative}";
                zip.CreateEntryFromFile(file, entryName, CompressionLevel.Optimal);
            }
        }

        private static void WriteJsonEntry(ZipArchive zip, string entryName, object data)
        {
            string json = JsonSerializer.Serialize(data, JsonOptions);
            WriteTextEntry(zip, entryName, json);
        }

        private static void WriteTextEntry(ZipArchive zip, string entryName, string content)
        {
            ZipArchiveEntry entry = zip.CreateEntry(entryName, CompressionLevel.Optimal);
            using Stream stream = entry.Open();
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static string BuildSingleRunHtml(DashboardRunRow run, DateTime exportedUtc)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html lang='en'><head><meta charset='utf-8'/>");
            sb.AppendLine("<title>EA Single Run Report</title>");
            sb.AppendLine("<style>body{font-family:Segoe UI,sans-serif;margin:24px;color:#0f172a}");
            sb.AppendLine("table{border-collapse:collapse}td,th{border:1px solid #cbd5e1;padding:8px 12px}");
            sb.AppendLine(".pass{color:#166534}.fail{color:#991b1b}</style></head><body>");
            sb.AppendLine("<h1>EA Test Automation — Single execution report</h1>");
            sb.AppendLine($"<p>Exported (UTC): {exportedUtc:yyyy-MM-dd HH:mm:ss}</p>");
            sb.AppendLine("<table>");
            AppendRow(sb, "Test", run.TestName);
            AppendRow(sb, "Run ID", run.RunId);
            AppendRow(sb, "Status", run.Status);
            AppendRow(sb, "Started (UTC)", run.StartedUtc?.ToString("u") ?? "—");
            AppendRow(sb, "Finished (UTC)", run.FinishedUtc?.ToString("u") ?? "—");
            AppendRow(sb, "Duration", run.DurationDisplay);
            AppendRow(sb, "Artifact folder", run.ArtifactFolder);
            AppendRow(sb, "Healing mappings", run.HealingMappingCount.ToString());
            sb.AppendLine("</table>");
            sb.AppendLine("<h2>Artifacts</h2><ul>");
            AppendLink(sb, "Video", run.Links.Video);
            AppendLink(sb, "Trace", run.Links.Trace);
            AppendLink(sb, "Screenshot", run.Links.Screenshot);
            AppendLink(sb, "Logs", run.Links.Logs);
            AppendLink(sb, "Healing", run.Links.Healing);
            sb.AppendLine("</ul><p>ZIP also contains <code>artifacts/</code> when files exist on disk.</p>");
            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private static string BuildWholeExecutionHtml(RealtimeDashboardPayload payload)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html lang='en'><head><meta charset='utf-8'/>");
            sb.AppendLine("<title>EA Whole Execution Report</title>");
            sb.AppendLine("<style>body{font-family:Segoe UI,sans-serif;margin:24px;color:#0f172a}");
            sb.AppendLine("table{border-collapse:collapse;width:100%}td,th{border:1px solid #cbd5e1;padding:8px}");
            sb.AppendLine("</style></head><body>");
            sb.AppendLine("<h1>EA Test Automation — Whole execution report</h1>");
            sb.AppendLine($"<p>Exported (UTC): {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}</p>");
            sb.AppendLine("<p>");
            sb.AppendLine(
                $"Total: {payload.Total} · Passed: {payload.Passed} · Failed: {payload.Failed} · ");
            sb.AppendLine($"Pass rate: {payload.PassRatePercent}% · Healing: {payload.HealingTotal}");
            sb.AppendLine("</p><table><thead><tr>");
            sb.AppendLine("<th>Test</th><th>Status</th><th>Duration</th><th>Started (UTC)</th><th>Folder</th>");
            sb.AppendLine("</tr></thead><tbody>");

            foreach (DashboardRunRow run in payload.Runs)
            {
                sb.AppendLine("<tr>");
                sb.Append("<td>").Append(Escape(run.TestName)).AppendLine("</td>");
                sb.Append("<td>").Append(Escape(run.Status)).AppendLine("</td>");
                sb.Append("<td>").Append(Escape(run.DurationDisplay)).AppendLine("</td>");
                sb.Append("<td>").Append(run.StartedUtc?.ToString("u") ?? "—").AppendLine("</td>");
                sb.Append("<td>").Append(Escape(run.ArtifactFolder)).AppendLine("</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody></table>");
            sb.AppendLine(
                "<p>Each run folder under <code>runs/&lt;artifactFolder&gt;/</code> includes summary JSON and artifacts.</p>");
            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private static void AppendRow(StringBuilder sb, string label, string value)
        {
            sb.Append("<tr><th>").Append(Escape(label)).Append("</th><td>")
                .Append(Escape(value))
                .AppendLine("</td></tr>");
        }

        private static void AppendLink(StringBuilder sb, string label, string? href)
        {
            if (string.IsNullOrWhiteSpace(href))
            {
                sb.Append("<li>").Append(Escape(label)).AppendLine(": —</li>");
                return;
            }

            sb.Append("<li>").Append(Escape(label)).Append(": <a href=\"")
                .Append(Escape(href))
                .Append("\">")
                .Append(Escape(href))
                .AppendLine("</a></li>");
        }

        private static string Escape(string? s) =>
            WebUtility.HtmlEncode(s ?? "");

        private static string SanitizeFileName(string name)
        {
            string safe = string.Join(
                "_",
                (name ?? "run").Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries))
                .Trim();

            if (safe.Length > 48)
            {
                safe = safe[..48];
            }

            return string.IsNullOrWhiteSpace(safe) ? "run" : safe;
        }

        private static string UtcStamp() =>
            DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
    }
}
