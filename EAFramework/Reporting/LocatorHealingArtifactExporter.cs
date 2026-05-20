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

                string report = HealingPaths.ResolveAutoHealReportPath();

                if (File.Exists(report))
                {
                    File.Copy(
                        report,
                        Path.Combine(healingDir, "AutoHealReport.txt"),
                        overwrite: true);
                }
            }
            catch
            {
                // Best-effort; never fail test teardown on copy issues.
            }
        }
    }
}
