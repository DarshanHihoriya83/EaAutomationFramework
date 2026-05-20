namespace EaTestAutomation.Reporting
{
    /// <summary>
    /// Resolves <c>DashboardReport</c> and <c>Artifacts</c> folders (runtime + project).
    /// </summary>
    public static class DashboardPaths
    {
        public static string ResolveArtifactsRoot()
        {
            string runtime = Path.Combine(AppContext.BaseDirectory, "Artifacts");

            if (Directory.Exists(runtime))
            {
                return Path.GetFullPath(runtime);
            }

            string cwd = Path.Combine(Directory.GetCurrentDirectory(), "Artifacts");

            if (Directory.Exists(cwd))
            {
                return Path.GetFullPath(cwd);
            }

            return Path.GetFullPath(runtime);
        }

        /// <summary>
        /// All dashboard output folders (runtime bin + project source folder).
        /// </summary>
        public static IReadOnlyList<string> GetAllDashboardRoots()
        {
            var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "DashboardReport"))
            };

            string? projectRoot = TryResolveProjectDashboardRoot();

            if (!string.IsNullOrEmpty(projectRoot))
            {
                roots.Add(projectRoot);
            }

            return roots.ToList();
        }

        public static string ResolveDashboardRoot() =>
            GetAllDashboardRoots().First();

        public static string? TryResolveProjectDashboardRoot()
        {
            try
            {
                var current = new DirectoryInfo(AppContext.BaseDirectory);

                for (int i = 0; i < 12 && current != null; i++)
                {
                    string testProj = Path.Combine(current.FullName, "EaTestAutomation.csproj");

                    if (File.Exists(testProj))
                    {
                        return Path.Combine(current.FullName, "DashboardReport");
                    }

                    current = current.Parent;
                }
            }
            catch
            {
                // fall through
            }

            return null;
        }

        public static string GetTestResultsJsonPath() =>
            Path.Combine(ResolveDashboardRoot(), "test-results.json");

        public static string GetMasterIndexPath() =>
            Path.Combine(ResolveDashboardRoot(), "index.html");
    }
}
