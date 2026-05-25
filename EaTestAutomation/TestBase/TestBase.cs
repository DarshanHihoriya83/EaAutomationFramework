using EaFramework.Config;
using EAFramework.Config;
using EAFramework.Driver;
using EAFramework.Reporting;
using EaTestAutomation.Pages;
using EaTestAutomation.Parallel;
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

        private string _reportTestName = "";
        private DateTime _reportStartedUtc;
        private bool? _reportPassed;
        private bool _browserGateHeld;

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
            ArtifactRoot = artifactRoot;
            _reportTestName = identity;
            _reportStartedUtc = DateTime.UtcNow;
            _reportPassed = null;
        }

        /// <summary>Records pass/fail for the dashboard (call from test methods).</summary>
        protected void MarkTestPassed(bool passed)
        {
            _reportPassed = passed;
            TestArtifactScope.MarkPassed(passed);
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

                if (!_browserGateHeld)
                {
                    BrowserExecutionGate.Acquire();
                    _browserGateHeld = true;
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

            //await LoginPage.ClickOnLoginButton();
            //await LoginPage.FillUsername();
            //await LoginPage.FillPassword();
            //await LoginPage.ClickOnSignInButton();
            //Assert.Contains("Employee", await page.ContentAsync());
            await Task.Delay(2000);
        }

        public void Dispose()
        {
            string capturedIdentity = string.IsNullOrWhiteSpace(_reportTestName)
                ? TestArtifactScope.Identity ?? ""
                : _reportTestName;

            bool? capturedPassed = _reportPassed ?? TestArtifactScope.Passed;
            DateTime capturedStarted = _reportStartedUtc != default
                ? _reportStartedUtc
                : TestArtifactScope.StartedUtc ?? DateTime.UtcNow;

            try
            {
                if (_playwrightDriver == null)
                {
                    FinalizeReportingWithoutBrowser(capturedIdentity, capturedPassed, capturedStarted);
                    return;
                }

                _executionLog?.WriteLine("Dispose: capturing final screenshot and closing browser.");

                try
                {
                    string shotDir = Path.Combine(ArtifactRoot, "screenshots");
                    Directory.CreateDirectory(shotDir);

                    bool failed = capturedPassed == false;
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
                        _executionLog?.WriteLine("Test failed.");
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
                    TestArtifactScope.MarkFailed();
                }

                LocatorHealingArtifactExporter.CopyHealingArtifacts(ArtifactRoot);

                if (capturedPassed != false)
                {
                    _extentTest?.Pass("Test completed; see artifact folder for trace/video/logs.");
                }
            }
            catch (Exception ex)
            {
                _executionLog?.WriteLine("Test failed.");
                _executionLog?.WriteLine($"Dispose reporting error: {ex}");
                TestArtifactScope.MarkFailed();
                _extentTest?.Warning($"Reporting step issue: {ex.Message}");
            }
            finally
            {
                _playwrightDriver?.Dispose();
                _executionLog?.Dispose();

                if (_browserGateHeld)
                {
                    BrowserExecutionGate.Release();
                    _browserGateHeld = false;
                }

                RecordTestRunForDashboard(capturedIdentity, capturedPassed, capturedStarted);
                ExtentReportManager.Flush();
                MasterDashboardGenerator.Generate();
            }

            GC.SuppressFinalize(this);
        }

        private void FinalizeReportingWithoutBrowser(
            string capturedIdentity,
            bool? capturedPassed,
            DateTime capturedStarted)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ArtifactRoot))
                {
                    ArtifactRoot = TestArtifactScope.ArtifactRoot ?? "";
                }

                if (!string.IsNullOrWhiteSpace(ArtifactRoot))
                {
                    LocatorHealingArtifactExporter.CopyHealingArtifacts(ArtifactRoot);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reporting without browser failed: {ex.Message}");
                TestArtifactScope.MarkFailed();
            }
            finally
            {
                RecordTestRunForDashboard(capturedIdentity, capturedPassed, capturedStarted);
                ExtentReportManager.Flush();
                MasterDashboardGenerator.Generate();
            }
        }

        private void RecordTestRunForDashboard(
            string capturedIdentity,
            bool? capturedPassed,
            DateTime capturedStarted)
        {
            if (string.IsNullOrWhiteSpace(ArtifactRoot))
            {
                return;
            }

            string identity = string.IsNullOrWhiteSpace(capturedIdentity)
                ? ExtractTestNameFromArtifactFolder(ArtifactRoot) ?? GetType().Name
                : capturedIdentity;

            string folder = Path.GetFileName(ArtifactRoot);
            string status = ResolveTestStatus(capturedPassed);
            DateTime finishedUtc = DateTime.UtcNow;
            DateTime startedUtc = capturedStarted;
            long durationMs = Math.Max(0, (long)(finishedUtc - startedUtc).TotalMilliseconds);

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
                RunId = folder,
                TestName = identity,
                ArtifactFolder = folder,
                Status = status,
                StartedUtc = startedUtc,
                FinishedUtc = finishedUtc,
                DurationMs = durationMs,
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

        private bool DetectTestFailure()
        {
            string shotDir = Path.Combine(ArtifactRoot, "screenshots");

            return Directory.Exists(shotDir)
                   && Directory.EnumerateFiles(shotDir, "failure_*.png").Any();
        }

        private string ResolveTestStatus(bool? capturedPassed)
        {
            if (capturedPassed == false)
            {
                return "Failed";
            }

            if (capturedPassed == true)
            {
                return "Passed";
            }

            if (DetectTestFailure())
            {
                return "Failed";
            }

            string shotDir = Path.Combine(ArtifactRoot, "screenshots");

            if (Directory.Exists(shotDir)
                && Directory.EnumerateFiles(shotDir, "final_*.png").Any())
            {
                return "Passed";
            }

            return "Unknown";
        }

        private static string? ExtractTestNameFromArtifactFolder(string artifactRoot)
        {
            string folder = Path.GetFileName(artifactRoot);

            if (string.IsNullOrWhiteSpace(folder))
            {
                return null;
            }

            // Strip _{guid8} _{time} _{yyyyMMdd} suffixes from artifact folder name.
            for (int i = 0; i < 3; i++)
            {
                int last = folder.LastIndexOf('_');

                if (last <= 0)
                {
                    break;
                }

                folder = folder[..last];
            }

            return folder;
        }
    }
}
