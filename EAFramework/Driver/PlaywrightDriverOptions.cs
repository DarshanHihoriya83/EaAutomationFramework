namespace EAFramework.Driver
{
    /// <summary>
    /// Optional per-run browser context settings (video, trace, artifact root).
    /// </summary>
    public sealed class PlaywrightDriverOptions
    {
        /// <summary>
        /// Root folder for this test run: video/, trace/, logs/, screenshots/, etc.
        /// </summary>
        public string? ArtifactRoot { get; init; }

        /// <summary>
        /// When true (default), start Playwright tracing on the browser context.
        /// </summary>
        public bool EnableTracing { get; init; } = true;

        /// <summary>
        /// When true (default), record video under ArtifactRoot/video.
        /// </summary>
        public bool EnableVideo { get; init; } = true;
    }
}
