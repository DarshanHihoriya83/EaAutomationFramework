using EAFramework.Config;
using EAFramework.Driver;
using EAFramework.Reporting;
using EaTestAutomation.Pages;
using Microsoft.Playwright;

namespace EaTestAutomation.Parallel
{
    /// <summary>
    /// Standalone Playwright session for parallel workers (own browser, artifacts, login).
    /// </summary>
    public sealed class ParallelTestSession : IAsyncDisposable
    {
        private readonly PlaywrightDriver _driver;
        private readonly TestExecutionFileLogger? _log;
        private bool _gateHeld;

        private ParallelTestSession(
            PlaywrightDriver driver,
            TestExecutionFileLogger? log,
            bool gateHeld,
            string artifactRoot,
            string identity)
        {
            _driver = driver;
            _log = log;
            _gateHeld = gateHeld;
            ArtifactRoot = artifactRoot;
            Identity = identity;
        }

        public string ArtifactRoot { get; }
        public string Identity { get; }
        public IPage Page { get; private set; } = null!;

        public static async Task<ParallelTestSession> StartAsync(
            TestSettings settings,
            string identity,
            CancellationToken cancellationToken = default)
        {
            BrowserExecutionGate.Acquire();

            string artifactRoot =
                ArtifactDirectoryBuilder.CreateTestRunDirectory(
                    ArtifactDirectoryBuilder.Sanitize(identity));

            TestArtifactScope.Begin(identity, artifactRoot);

            string logPath = Path.Combine(artifactRoot, "logs", "execution.log");
            var log = new TestExecutionFileLogger(logPath);
            log.WriteLine($"[Parallel] Starting session: {identity}");
            log.WriteLine($"[Parallel] Artifact root: {artifactRoot}");

            var driverOptions = new PlaywrightDriverOptions
            {
                ArtifactRoot = artifactRoot,
                EnableTracing = true,
                EnableVideo = true
            };

            var driver = new PlaywrightDriver(
                settings,
                new PlaywrightDriverInitializer(),
                driverOptions);

            IPage page = await driver.Page;
            await LoginAsync(page, settings, log, cancellationToken);

            var session = new ParallelTestSession(driver, log, gateHeld: true, artifactRoot, identity)
            {
                Page = page
            };

            return session;
        }

        private static async Task LoginAsync(
            IPage page,
            TestSettings settings,
            TestExecutionFileLogger log,
            CancellationToken cancellationToken)
        {
            await page.GotoAsync(
                settings.Applicationurl,
                new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 60_000 });

            var loginPage = new LoginPage(page);
            await loginPage.ClickOnLoginButton();
            await loginPage.FillUsername();
            await loginPage.FillPassword();
            await loginPage.ClickOnSignInButton();

            string content = await page.ContentAsync();
            if (!content.Contains("Employee", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Login did not reach Employee page. Check Applicationurl and credentials.");
            }

            log.WriteLine("[Parallel] Login completed.");
            cancellationToken.ThrowIfCancellationRequested();
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                _log?.WriteLine("[Parallel] Closing browser session.");
                _driver.Dispose();
                LocatorHealingArtifactExporter.CopyHealingArtifacts(ArtifactRoot);
            }
            catch (Exception ex)
            {
                _log?.WriteLine($"[Parallel] Dispose error: {ex.Message}");
            }
            finally
            {
                _log?.Dispose();

                if (_gateHeld)
                {
                    BrowserExecutionGate.Release();
                    _gateHeld = false;
                }

                TestArtifactScope.Clear();
            }

            await ValueTask.CompletedTask;
        }
    }
}
