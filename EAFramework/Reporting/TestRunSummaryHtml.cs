using System.Net;
using System.Text;

namespace EAFramework.Reporting
{
    /// <summary>
    /// Builds <c>Artifacts/index.html</c> listing all per-test artifact folders from the current run.
    /// </summary>
    public static class TestRunSummaryHtml
    {
        public static void WriteRunIndex()
        {
            string artifactsRoot = ArtifactDirectoryBuilder.EnsureArtifactRoot();

            var runFolders = Directory
                .GetDirectories(artifactsRoot)
                .Where(d => !string.Equals(
                    Path.GetFileName(d),
                    "ExtentReport",
                    StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(Directory.GetLastWriteTimeUtc)
                .ToList();

            string indexPath = Path.Combine(artifactsRoot, "index.html");
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html lang='en'><head><meta charset='utf-8'/>");
            sb.AppendLine("<title>EA Automation — Test run artifacts</title>");
            sb.AppendLine("<style>body{font-family:Segoe UI,sans-serif;margin:24px;background:#0f172a;color:#e2e8f0}a{color:#38bdf8}table{border-collapse:collapse;width:100%;max-width:1200px}th,td{border:1px solid #334155;padding:10px}th{background:#1e293b}</style>");
            sb.AppendLine("</head><body>");
            sb.AppendLine("<h1>Test run artifacts</h1>");
            sb.AppendLine($"<p>Generated {DateTime.Now:O}</p>");
            sb.AppendLine("<table><thead><tr><th>Run folder</th><th>Dashboard</th><th>Extent report</th></tr></thead><tbody>");

            foreach (string folder in runFolders)
            {
                string name = Path.GetFileName(folder);
                string rel = name.Replace('\\', '/');
                sb.Append("<tr><td>");
                sb.Append(WebUtility.HtmlEncode(name));
                sb.Append("</td><td><a href='");
                sb.Append(WebUtility.HtmlEncode($"{rel}/dashboard.html"));
                sb.Append("'>dashboard.html</a></td><td>—</td></tr>");
            }

            sb.AppendLine("</tbody></table>");
            sb.AppendLine("<p><a href='ExtentReport/ExtentDashboard.html'>Extent aggregate report</a></p>");
            sb.AppendLine("<p><a href='../DashboardReport/index.html'>Master dashboard (charts &amp; all reports)</a></p>");
            sb.AppendLine("</body></html>");
            File.WriteAllText(indexPath, sb.ToString());
        }
    }
}
