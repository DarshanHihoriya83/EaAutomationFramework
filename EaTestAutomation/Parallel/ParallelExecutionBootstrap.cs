using System.Runtime.CompilerServices;
using System.Text.Json;
using EAFramework.Config;
using EaFramework.Config;

namespace EaTestAutomation.Parallel
{
    /// <summary>
    /// Applies <c>appsettings.json</c> parallel flags to <c>xunit.runner.json</c> before the test host starts collections.
    /// </summary>
    internal static class ParallelExecutionBootstrap
    {
        [ModuleInitializer]
        internal static void ApplyFromAppSettings()
        {
            try
            {
                TestSettings settings = ConfigReader.ReadConfig();
                string outputDir = AppContext.BaseDirectory;
                string runnerPath = Path.Combine(outputDir, "xunit.runner.json");

                bool parallel = settings.EnableParallelExecution;
                int maxThreads = parallel
                    ? Math.Max(1, settings.MaxParallelBrowsers)
                    : 1;

                var runnerConfig = new
                {
                    schema = "https://xunit.net/schema/v2.3/xunit.runner.schema.json",
                    parallelizeAssembly = parallel,
                    parallelizeTestCollections = parallel,
                    maxParallelThreads = maxThreads
                };

                string json = JsonSerializer.Serialize(
                    runnerConfig,
                    new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(runnerPath, json);
            }
            catch
            {
                // Keep packaged xunit.runner.json defaults when config is unavailable.
            }
        }
    }
}
