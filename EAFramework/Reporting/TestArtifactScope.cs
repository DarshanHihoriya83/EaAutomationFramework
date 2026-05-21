namespace EAFramework.Reporting
{
    /// <summary>
    /// Holds the current test identity and artifact root for the running xUnit test (set by test hooks).
    /// </summary>
    public static class TestArtifactScope
    {
        private static readonly AsyncLocal<ScopeData?> CurrentScope = new();

        public static string? Identity => CurrentScope.Value?.Identity;

        public static string? ArtifactRoot =>
            string.IsNullOrWhiteSpace(CurrentScope.Value?.ArtifactRoot)
                ? null
                : CurrentScope.Value.ArtifactRoot;

        public static bool? Passed => CurrentScope.Value?.Passed;

        public static DateTime? StartedUtc => CurrentScope.Value?.StartedUtc;

        public static void Begin(string identity, string? artifactRoot = null)
        {
            CurrentScope.Value = new ScopeData
            {
                Identity = identity,
                ArtifactRoot = artifactRoot ?? "",
                Passed = null,
                StartedUtc = DateTime.UtcNow
            };
        }

        public static void MarkPassed(bool passed)
        {
            if (CurrentScope.Value != null)
            {
                CurrentScope.Value.Passed = passed;
            }
        }

        public static void MarkFailed()
        {
            MarkPassed(false);
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
            public DateTime StartedUtc { get; init; }
        }
    }
}
