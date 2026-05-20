using EAFramework.Config;
using Microsoft.Playwright;

namespace EAFramework.Driver
{
    public class PlaywrightDriver : IDisposable
    {
        private readonly AsyncTask<IBrowser> _browser;
        private readonly AsyncTask<IBrowserContext> _browserContext;
        private readonly AsyncTask<IPage> _page;
        private readonly IPlaywrightDriverInitializer _playwrightDriverInitializer;
        private readonly TestSettings _testSettings;
        private readonly PlaywrightDriverOptions? _driverOptions;
        private bool _tracingStarted;
        private bool _isDisposed;

        public PlaywrightDriver(
            TestSettings testSettings,
            IPlaywrightDriverInitializer playwrightDriverInitializer,
            PlaywrightDriverOptions? driverOptions = null)
        {
            _testSettings = testSettings;
            _playwrightDriverInitializer = playwrightDriverInitializer;
            _driverOptions = driverOptions;
            _browser = new AsyncTask<IBrowser>(InitalizePlaywrightAsync);
            _browserContext = new AsyncTask<IBrowserContext>(CreateBrowserContextAsync);
            _page = new AsyncTask<IPage>(CreatePageAsync);
        }

        public Task<IPage> Page => _page.Value;

        public Task<IBrowser> Browser => _browser.Value;

        public Task<IBrowserContext> BrowserContext => _browserContext.Value;

        public string? ArtifactRoot => _driverOptions?.ArtifactRoot;

        private async Task<IBrowser> InitalizePlaywrightAsync()
        {
            return _testSettings.DriverType switch
            {
                DriverType.Chromium => await _playwrightDriverInitializer.GetChrominumDriverAsync(_testSettings),
                DriverType.Chrome => await _playwrightDriverInitializer.GetChromeDriverAsync(_testSettings),
                DriverType.Edge => await _playwrightDriverInitializer.GetEdgeDriverAsync(_testSettings),
                DriverType.Firefox => await _playwrightDriverInitializer.GetFirefoxDriverAsync(_testSettings),
                _ => await _playwrightDriverInitializer.GetChrominumDriverAsync(_testSettings),
            };
        }

        private async Task<IBrowserContext> CreateBrowserContextAsync()
        {
            IBrowser browser = await _browser;
            var contextOptions = new BrowserNewContextOptions();

            string? root = _driverOptions?.ArtifactRoot;

            if (!string.IsNullOrWhiteSpace(root)
                && _driverOptions?.EnableVideo == true)
            {
                string videoDir = Path.Combine(root, "video");
                Directory.CreateDirectory(videoDir);
                contextOptions.RecordVideoDir = videoDir;
                contextOptions.RecordVideoSize = new RecordVideoSize
                {
                    Width = 1280,
                    Height = 720
                };
            }

            IBrowserContext context = await browser.NewContextAsync(contextOptions);

            if (!string.IsNullOrWhiteSpace(root)
                && _driverOptions?.EnableTracing != false)
            {
                await context.Tracing.StartAsync(new()
                {
                    Screenshots = true,
                    Snapshots = true,
                    Sources = true
                });

                _tracingStarted = true;
            }

            return context;
        }

        private async Task<IPage> CreatePageAsync()
        {
            return await (await _browserContext).NewPageAsync();
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            try
            {
                if (!_browserContext.IsValueCreated)
                {
                    return;
                }

                Task.Run(async () =>
                {
                    try
                    {
                        IBrowserContext context = await _browserContext;

                        if (_tracingStarted
                            && !string.IsNullOrWhiteSpace(_driverOptions?.ArtifactRoot))
                        {
                            string traceDir =
                                Path.Combine(_driverOptions!.ArtifactRoot!, "trace");

                            Directory.CreateDirectory(traceDir);

                            string traceZip =
                                Path.Combine(traceDir, "trace.zip");

                            await context.Tracing.StopAsync(new()
                            {
                                Path = traceZip
                            });
                        }

                        if (_page.IsValueCreated)
                        {
                            await (await _page).CloseAsync();
                        }

                        await context.CloseAsync();
                    }
                    catch
                    {
                        // Best-effort shutdown.
                    }

                    try
                    {
                        if (_browser.IsValueCreated)
                        {
                            IBrowser browser = await _browser;
                            await browser.CloseAsync();
                            await browser.DisposeAsync();
                        }
                    }
                    catch
                    {
                        // Best-effort shutdown.
                    }
                }).GetAwaiter().GetResult();
            }
            catch
            {
                // Swallow teardown issues so Dispose never throws.
            }
        }
    }
}
