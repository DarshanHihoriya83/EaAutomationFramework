using EaFramework.Config;
using EAFramework.Config;
using EAFramework.Driver;
using EAFramework.Reporting;
using EaTestAutomation.Pages;
using EaTestAutomation.Reporting;
using Microsoft.Playwright;
using AventStack.ExtentReports;
using Xunit;

namespace EaTestAutomation.Base
{
    /// <summary>
    /// Base test with Playwright session, per-test artifacts (video/trace/logs/screenshots), and Extent reporting.
    /// Apply <see cref="PlaywrightTestArtifactAttribute"/> on test classes for per-case artifact folders.
    /// </summary>
    [PlaywrightTestArtifact]
    public class BaseTest : IDisposable
    {
        private readonly object _initLock = new();
        private PlaywrightDriver? _playwrightDriver;
        private TestExecutionFileLogger? _executionLog;
        private ExtentTest? _extentTest;

        protected readonly TestSettings _testSettings;
        protected readonly IPlaywrightDriverInitializer _playwrightDriverInitializer;

        protected LoginPage LoginPage = null!;

        /// <summary>Per-test artifact root (video, trace, logs, screenshots, healing snapshot).</summary>
        protected string ArtifactRoot { get; private set; } = "";

        protected IPage Page => EnsurePlaywrightSession().Page.Result;

        public BaseTest()
        {
            _testSettings = ConfigReader.ReadConfig();
            _playwrightDriverInitializer = new PlaywrightDriverInitializer();
        }

        /// <summary>
        /// Rebinds artifact folder to a data-driven case name (call before first <see cref="Page"/> use).
        /// </summary>
        protected void BindArtifactToTestCase(string caseName)
        {
            string identity = ArtifactDirectoryBuilder.Sanitize(caseName);
            string artifactRoot =
                ArtifactDirectoryBuilder.CreateTestRunDirectory(identity);

            TestArtifactScope.Begin(identity, artifactRoot);
        }

        private PlaywrightDriver EnsurePlaywrightSession()
        {
            if (_playwrightDriver != null)
            {
                return _playwrightDriver;
            }

            lock (_initLock)
            {
                if (_playwrightDriver != null)
                {
                    return _playwrightDriver;
                }

                string identity =
                    TestArtifactScope.Identity
                    ?? GetType().Name;

                ArtifactRoot =
                    TestArtifactScope.ArtifactRoot
                    ?? ArtifactDirectoryBuilder.CreateTestRunDirectory(identity);

                string logPath = Path.Combine(ArtifactRoot, "logs", "execution.log");
                _executionLog = new TestExecutionFileLogger(logPath);
                _executionLog.WriteLine($"Artifact root: {ArtifactRoot}");
                _executionLog.WriteLine($"Test identity: {identity}");

                _extentTest = ExtentReportManager.CreateTest(identity);
                _extentTest.AssignCategory("Playwright");
                _extentTest.Info($"Artifact directory: <b>{ArtifactRoot}</b>");

                var driverOptions = new PlaywrightDriverOptions
                {
                    ArtifactRoot = ArtifactRoot,
                    EnableTracing = true,
                    EnableVideo = true
                };

                _playwrightDriver = new PlaywrightDriver(
                    _testSettings,
                    _playwrightDriverInitializer,
                    driverOptions);

                LoginApplication().GetAwaiter().GetResult();
                _executionLog.WriteLine("Login completed.");

                return _playwrightDriver;
            }
        }

        private async Task LoginApplication()
        {
            IPage page = await _playwrightDriver!.Page;

            await page.GotoAsync(_testSettings.Applicationurl);

            LoginPage = new LoginPage(page);

            await LoginPage.ClickOnLoginButton();
            await LoginPage.FillUsername();
            await LoginPage.FillPassword();
            await LoginPage.ClickOnSignInButton();
            Assert.Contains("Employee", await page.ContentAsync());
            await Task.Delay(2000);
        }

        public void Dispose()
        {
            try
            {
                if (_playwrightDriver == null)
                {
                    return;
                }

                _executionLog?.WriteLine("Dispose: capturing final screenshot and closing browser.");

                try
                {
                    string shotDir = Path.Combine(ArtifactRoot, "screenshots");
                    Directory.CreateDirectory(shotDir);

                    bool failed = TestArtifactScope.Passed == false;
                    string prefix = failed ? "failure" : "final";
                    string shotPath = Path.Combine(
                        shotDir,
                        $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss}.png");

                    IPage page = Page;
                    page.ScreenshotAsync(new()
                        {
                            Path = shotPath,
                            FullPage = true
                        })
                        .GetAwaiter()
                        .GetResult();

                    _executionLog?.WriteLine($"Screenshot: {shotPath}");

                    if (failed)
                    {
                        _extentTest?.Fail($"Test failed. Screenshot: {shotPath}");
                    }
                    else
                    {
                        _extentTest?.Info($"Final screenshot: {shotPath}");
                    }
                }
                catch (Exception ex)
                {
                    _executionLog?.WriteLine($"Screenshot skipped: {ex.Message}");
                }

                LocatorHealingArtifactExporter.CopyHealingArtifacts(ArtifactRoot);
                HtmlArtifactDashboard.Write(
                    ArtifactRoot,
                    TestArtifactScope.Identity ?? GetType().Name);

                RecordTestRunForDashboard();

                if (TestArtifactScope.Passed != false)
                {
                    _extentTest?.Pass("Test completed; see artifact folder for trace/video/logs.");
                }
            }
            catch (Exception ex)
            {
                _executionLog?.WriteLine($"Dispose reporting error: {ex}");
                _extentTest?.Warning($"Reporting step issue: {ex.Message}");
            }
            finally
            {
                _playwrightDriver?.Dispose();
                _executionLog?.Dispose();
                ExtentReportManager.Flush();
                TestRunSummaryHtml.WriteRunIndex();
                MasterDashboardGenerator.Generate();
            }

            GC.SuppressFinalize(this);
        }

        private void RecordTestRunForDashboard()
        {
            string identity = TestArtifactScope.Identity ?? GetType().Name;
            string folder = Path.GetFileName(ArtifactRoot);
            bool failed = TestArtifactScope.Passed == false;

            int healingCount = 0;
            string healingPath = Path.Combine(ArtifactRoot, "healing", "FailedLocatorStore.json");

            if (File.Exists(healingPath))
            {
                try
                {
                    string json = File.ReadAllText(healingPath);
                    var map = System.Text.Json.JsonSerializer
                        .Deserialize<Dictionary<string, string>>(json);

                    healingCount = map?.Count ?? 0;
                }
                catch
                {
                    // ignore
                }
            }

            TestRunRegistry.Record(new TestRunRecord
            {
                TestName = identity,
                ArtifactFolder = folder,
                Status = failed ? "Failed" : "Passed",
                FinishedUtc = DateTime.UtcNow,
                HasVideo = Directory.Exists(Path.Combine(ArtifactRoot, "video"))
                           && Directory.EnumerateFiles(Path.Combine(ArtifactRoot, "video")).Any(),
                HasTrace = File.Exists(Path.Combine(ArtifactRoot, "trace", "trace.zip")),
                HasScreenshot = Directory.Exists(Path.Combine(ArtifactRoot, "screenshots"))
                                && Directory.EnumerateFiles(
                                    Path.Combine(ArtifactRoot, "screenshots"),
                                    "*.png").Any(),
                HealingMappingCount = healingCount
            });
        }
    }
}
