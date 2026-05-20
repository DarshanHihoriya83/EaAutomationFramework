using System.Net;
using System.Text;

namespace EAFramework.Reporting
{
    /// <summary>
    /// Generates a small HTML dashboard inside the test artifact directory with links to captured assets.
    /// </summary>
    public static class HtmlArtifactDashboard
    {
        public static void Write(string artifactRoot, string testTitle)
        {
            if (string.IsNullOrWhiteSpace(artifactRoot) || !Directory.Exists(artifactRoot))
            {
                return;
            }

            string indexPath = Path.Combine(artifactRoot, "dashboard.html");
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html lang='en'><head><meta charset='utf-8'/>");
            sb.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1'/>");
            sb.Append("<title>Test artifacts — ");
            sb.Append(WebUtility.HtmlEncode(testTitle));
            sb.AppendLine("</title>");
            sb.AppendLine("<style>body{font-family:Segoe UI,system-ui,sans-serif;margin:24px;background:#0f172a;color:#e2e8f0}a{color:#38bdf8}table{border-collapse:collapse;width:100%;max-width:960px}th,td{border:1px solid #334155;padding:8px;text-align:left}th{background:#1e293b}.muted{color:#94a3b8}</style>");
            sb.AppendLine("</head><body>");
            sb.Append("<h1>Test artifacts</h1><p class='muted'>");
            sb.Append(WebUtility.HtmlEncode(testTitle));
            sb.AppendLine("</p>");
            sb.AppendLine("<h2>Files</h2><table><thead><tr><th>Relative path</th></tr></thead><tbody>");

            foreach (string rel in ListRelativeFiles(artifactRoot))
            {
                string href = rel.Replace('\\', '/');
                sb.Append("<tr><td><a href='");
                sb.Append(WebUtility.HtmlEncode(href));
                sb.Append("'>");
                sb.Append(WebUtility.HtmlEncode(href));
                sb.AppendLine("</a></td></tr>");
            }

            sb.AppendLine("</tbody></table>");
            sb.AppendLine("<p class='muted'>Open this file from disk; relative links resolve next to this HTML file.</p>");
            sb.AppendLine("<p><a href='../../DashboardReport/index.html' style='color:#38bdf8'>← Master Dashboard</a></p>");
            sb.AppendLine("</body></html>");
            File.WriteAllText(indexPath, sb.ToString());
        }

        private static IEnumerable<string> ListRelativeFiles(string root)
        {
            foreach (string file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            {
                string rel = Path.GetRelativePath(root, file);
                if (rel.Equals("dashboard.html", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                yield return rel;
            }
        }
    }
}
