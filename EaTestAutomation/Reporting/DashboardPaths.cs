namespace EaTestAutomation.Reporting
{
    /// <summary>
    /// Resolves <c>DashboardReport</c> and <c>Artifacts</c> folders (test bin, project, or server host).
    /// </summary>
    public static class DashboardPaths
    {
        public static string ResolveArtifactsRoot()
        {
            foreach (string candidate in GetArtifactRootCandidates())
            {
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }
            }

            string fallback = Path.Combine(AppContext.BaseDirectory, "Artifacts");
            Directory.CreateDirectory(fallback);
            return Path.GetFullPath(fallback);
        }

        public static IReadOnlyList<string> GetAllDashboardRoots()
        {
            return GetDashboardRootCandidates()
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static string ResolveDashboardRoot() =>
            GetDashboardRootCandidates().First();

        public static string GetTestResultsJsonPath() =>
            Path.Combine(ResolveDashboardRoot(), "test-results.json");

        public static IReadOnlyList<string> GetAllTestResultsJsonPaths()
        {
            return GetDashboardRootCandidates()
                .Select(root => Path.Combine(root, "test-results.json"))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static IReadOnlyList<string> GetAllArtifactRoots()
        {
            return GetArtifactRootCandidates()
                .Where(Directory.Exists)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static string GetMasterIndexPath() =>
            Path.Combine(ResolveDashboardRoot(), "index.html");

        /// <summary>
        /// Dashboard folders ordered by where real test data exists (test bin first).
        /// </summary>
        private static IEnumerable<string> GetDashboardRootCandidates()
        {
            var scored = new List<(string Path, int Score)>();

            foreach (string path in EnumerateDashboardRootPaths())
            {
                scored.Add((path, ScoreDashboardRoot(path)));
            }

            foreach ((string path, int score) in scored.OrderByDescending(s => s.Score))
            {
                yield return path;
            }
        }

        private static IEnumerable<string> GetArtifactRootCandidates()
        {
            string? testBin = TryResolveEaTestAutomationBinDirectory();

            if (!string.IsNullOrEmpty(testBin))
            {
                yield return Path.Combine(testBin, "Artifacts");
            }

            string cwdArtifacts = Path.Combine(Directory.GetCurrentDirectory(), "Artifacts");

            if (Directory.Exists(cwdArtifacts))
            {
                yield return Path.GetFullPath(cwdArtifacts);
            }

            yield return Path.Combine(AppContext.BaseDirectory, "Artifacts");
        }

        private static IEnumerable<string> EnumerateDashboardRootPaths()
        {
            string? testBin = TryResolveEaTestAutomationBinDirectory();

            if (!string.IsNullOrEmpty(testBin))
            {
                yield return Path.Combine(testBin, "DashboardReport");
            }

            string? projectDashboard = TryResolveProjectDashboardRoot();

            if (!string.IsNullOrEmpty(projectDashboard))
            {
                yield return projectDashboard;
            }

            yield return Path.Combine(AppContext.BaseDirectory, "DashboardReport");
            yield return Path.Combine(Directory.GetCurrentDirectory(), "DashboardReport");
        }

        private static int ScoreDashboardRoot(string dashboardRoot)
        {
            int score = 0;
            string results = Path.Combine(dashboardRoot, "test-results.json");

            if (File.Exists(results))
            {
                score += 100;

                try
                {
                    if (new FileInfo(results).Length > 10)
                    {
                        score += 50;
                    }
                }
                catch
                {
                    // ignore
                }
            }

            if (File.Exists(Path.Combine(dashboardRoot, "dashboard-data.json")))
            {
                score += 25;
            }

            if (File.Exists(Path.Combine(dashboardRoot, "index.html")))
            {
                score += 10;
            }

            if (dashboardRoot.Contains("EaTestAutomation", StringComparison.OrdinalIgnoreCase)
                && dashboardRoot.Contains("bin", StringComparison.OrdinalIgnoreCase))
            {
                score += 40;
            }

            return score;
        }

        public static string? TryResolveEaTestAutomationBinDirectory()
        {
            string? projectDir = TryResolveEaTestAutomationProjectDirectory();

            if (string.IsNullOrEmpty(projectDir))
            {
                return null;
            }

            string bin = Path.Combine(projectDir, "bin", "Debug", "net8.0");

            return Directory.Exists(bin) ? Path.GetFullPath(bin) : null;
        }

        public static string? TryResolveEaTestAutomationProjectDirectory()
        {
            try
            {
                var current = new DirectoryInfo(AppContext.BaseDirectory);

                for (int i = 0; i < 14 && current != null; i++)
                {
                    string direct = Path.Combine(current.FullName, "EaTestAutomation.csproj");

                    if (File.Exists(direct))
                    {
                        return current.FullName;
                    }

                    string nested = Path.Combine(
                        current.FullName,
                        "EaTestAutomation",
                        "EaTestAutomation.csproj");

                    if (File.Exists(nested))
                    {
                        return Path.Combine(current.FullName, "EaTestAutomation");
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

        public static string? TryResolveProjectDashboardRoot()
        {
            string? projectDir = TryResolveEaTestAutomationProjectDirectory();

            return string.IsNullOrEmpty(projectDir)
                ? null
                : Path.Combine(projectDir, "DashboardReport");
        }
    }
}
