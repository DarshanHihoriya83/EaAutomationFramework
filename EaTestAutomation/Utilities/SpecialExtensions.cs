namespace EaTestAutomation.Utilities
{
    /// <summary>Typo alias for <see cref="SpecialExtensions"/> (legacy tests).</summary>
    public static class SpecialExtenstions
    {
        public static string GetExcelPath() => SpecialExtensions.GetExcelPath();
    }

    public static class SpecialExtensions
    {
        /// <summary>
        /// Resolves TestData.xlsx — prefers the project <c>TestData</c> folder so Excel updates are visible in source control path.
        /// </summary>
        public static string GetExcelPath()
        {
            string? projectDir = ResolveProjectDirectory();

            if (!string.IsNullOrEmpty(projectDir))
            {
                string projectFile = Path.Combine(projectDir, "TestData", "TestData.xlsx");

                if (File.Exists(projectFile))
                {
                    return Path.GetFullPath(projectFile);
                }
            }

            string[] fallbackCandidates =
            {
                Path.Combine(AppContext.BaseDirectory, "TestData", "TestData.xlsx"),
                Path.Combine(Directory.GetCurrentDirectory(), "TestData", "TestData.xlsx"),
                @"D:\EaAutomationFramework\EaTestAutomation\TestData\TestData.xlsx"
            };

            foreach (string path in fallbackCandidates)
            {
                if (File.Exists(path))
                {
                    return Path.GetFullPath(path);
                }
            }

            throw new FileNotFoundException(
                "TestData.xlsx not found. Expected under EaTestAutomation/TestData/TestData.xlsx");
        }

        private static string? ResolveProjectDirectory()
        {
            try
            {
                var current = new DirectoryInfo(AppContext.BaseDirectory);

                for (int i = 0; i < 12 && current != null; i++)
                {
                    string direct = Path.Combine(current.FullName, "EaTestAutomation.csproj");
                    string nested = Path.Combine(current.FullName, "EaTestAutomation", "EaTestAutomation.csproj");

                    if (File.Exists(direct))
                    {
                        return current.FullName;
                    }

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
    }
}
