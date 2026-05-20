namespace EAFramework.AIHealing
{
    /// <summary>
    /// Central paths for self-healing persistence so all services write to the same store and reports.
    /// </summary>
    public static class HealingPaths
    {
        public static string ResolveFailedLocatorStorePath()
        {
            try
            {
                var current = new DirectoryInfo(AppContext.BaseDirectory);

                for (int i = 0; i < 10 && current != null; i++)
                {
                    string candidate = Path.Combine(
                        current.FullName,
                        "EAFramework",
                        "AIHealing",
                        "FailedLocatorStore.json");

                    if (File.Exists(candidate))
                    {
                        return candidate;
                    }

                    string csprojCandidate =
                        Path.Combine(current.FullName, "EAFramework.csproj");

                    if (File.Exists(csprojCandidate))
                    {
                        return Path.Combine(
                            current.FullName,
                            "AIHealing",
                            "FailedLocatorStore.json");
                    }

                    current = current.Parent;
                }
            }
            catch
            {
                // fall through
            }

            return Path.Combine(
                AppContext.BaseDirectory,
                "AIHealing",
                "FailedLocatorStore.json");
        }

        public static string ResolveAutoHealReportPath()
        {
            string? dir = Path.GetDirectoryName(ResolveFailedLocatorStorePath());

            return Path.Combine(
                dir ?? AppContext.BaseDirectory,
                "AutoHealReport.txt");
        }
    }
}
