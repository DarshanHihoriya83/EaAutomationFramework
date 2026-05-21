using System.Text;
using System.Text.Json;
using EAFramework.AIHealing;

namespace EAFramework.Reporting
{
    /// <summary>
    /// Copies locator healing store and auto-heal text report into the current test artifact folder.
    /// </summary>
    public static class LocatorHealingArtifactExporter
    {
        public static void CopyHealingArtifacts(string artifactRoot)
        {
            if (string.IsNullOrWhiteSpace(artifactRoot) || !Directory.Exists(artifactRoot))
            {
                return;
            }

            string healingDir = Path.Combine(artifactRoot, "healing");
            Directory.CreateDirectory(healingDir);

            try
            {
                string store = HealingPaths.ResolveFailedLocatorStorePath();

                if (File.Exists(store))
                {
                    File.Copy(
                        store,
                        Path.Combine(healingDir, "FailedLocatorStore.json"),
                        overwrite: true);
                }

                string centralReport = HealingPaths.ResolveAutoHealReportPath();
                SyncAutoHealReportFromStore(store, centralReport);

                if (File.Exists(centralReport))
                {
                    File.Copy(
                        centralReport,
                        Path.Combine(healingDir, "AutoHealReport.txt"),
                        overwrite: true);
                }
            }
            catch
            {
                // Best-effort; never fail test teardown on copy issues.
            }
        }

        /// <summary>
        /// Rebuilds <c>AutoHealReport.txt</c> from the JSON store so the report stays current even when
        /// healing reuses cached mappings (no new append during the run).
        /// </summary>
        public static void SyncAutoHealReportFromStore(
            string? storePath = null,
            string? reportPath = null)
        {
            storePath ??= HealingPaths.ResolveFailedLocatorStorePath();
            reportPath ??= HealingPaths.ResolveAutoHealReportPath();

            string? folder = Path.GetDirectoryName(reportPath);

            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }

            Dictionary<string, string> mappings = LoadStore(storePath);

            var lines = new List<string>
            {
                "========== AI SELF-HEALING REPORT ==========",
                $"Generated On : {DateTime.Now:O}",
                $"Store File    : {storePath}",
                $"Total Mappings: {mappings.Count}",
                ""
            };

            foreach (KeyValuePair<string, string> item in mappings)
            {
                lines.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]");
                lines.Add($"  Original: {item.Key}");
                lines.Add($"  Healed  : {item.Value}");
                lines.Add("");
            }

            File.WriteAllLines(reportPath, lines, Encoding.UTF8);
        }

        private static Dictionary<string, string> LoadStore(string storePath)
        {
            if (!File.Exists(storePath))
            {
                return new Dictionary<string, string>();
            }

            try
            {
                string json = File.ReadAllText(storePath);

                return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                       ?? new Dictionary<string, string>();
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }
    }
}
