namespace EAFramework.Reporting
{
    /// <summary>
    /// Holds the current test identity and artifact root for the running xUnit test (set by test hooks).
    /// </summary>
    public static class TestArtifactScope
    {
        private static readonly AsyncLocal<ScopeData?> CurrentScope = new();

        public static string? Identity => CurrentScope.Value?.Identity;

        public static string? ArtifactRoot => CurrentScope.Value?.ArtifactRoot;

        public static bool? Passed => CurrentScope.Value?.Passed;

        public static void Begin(string identity, string artifactRoot)
        {
            CurrentScope.Value = new ScopeData
            {
                Identity = identity,
                ArtifactRoot = artifactRoot,
                Passed = null
            };
        }

        public static void MarkPassed(bool passed)
        {
            if (CurrentScope.Value != null)
            {
                CurrentScope.Value.Passed = passed;
            }
        }

        public static void Clear()
        {
            CurrentScope.Value = null;
        }

        private sealed class ScopeData
        {
            public string Identity { get; init; } = "";
            public string ArtifactRoot { get; init; } = "";
            public bool? Passed { get; set; }
        }
    }
}
