using EAFramework.Config;
using Microsoft.Playwright;

namespace EAFramework.Driver
{
    public class PlaywrightDriverInitializer : IPlaywrightDriverInitializer
    {
        public const float DEFAULT_TIMEOUT = 30f;

        public async Task<IBrowser> GetChromeDriverAsync(TestSettings testSettings)
        {
            var options = Getparameters(testSettings.Args, testSettings.Timeout, testSettings.Headless, testSettings.SlowMo);
            options.Channel = "Chrome";
            return await GetBrowserAsnc(DriverType.Chromium, options);
        }
        public async Task<IBrowser> GetEdgeDriverAsync(TestSettings testSettings)
        {
            var options = Getparameters(testSettings.Args, testSettings.Timeout, testSettings.Headless, testSettings.SlowMo);
            options.Channel = "msedge";
            return await GetBrowserAsnc(DriverType.Chromium, options);
        }

        public async Task<IBrowser> GetFirefoxDriverAsync(TestSettings testSettings)
        {
            var options = Getparameters(testSettings.Args, testSettings.Timeout, testSettings.Headless, testSettings.SlowMo);
            options.Channel = "firefox";
            return await GetBrowserAsnc(DriverType.Firefox, options);
        }


        public async Task<IBrowser> GetWebkitDriverAsync(TestSettings testSettings)
        {
            var options = Getparameters(testSettings.Args, testSettings.Timeout, testSettings.Headless, testSettings.SlowMo);
            options.Channel = "";
            return await GetBrowserAsnc(DriverType.Webkit, options);
        }

        public async Task<IBrowser> GetChrominumDriverAsync(TestSettings testSettings)
        {
            var options = Getparameters(testSettings.Args, testSettings.Timeout, testSettings.Headless, testSettings.SlowMo);
            options.Channel = "";
            return await GetBrowserAsnc(DriverType.Chromium, options);
        }


        private async Task<IBrowser> GetBrowserAsnc(DriverType driverType, BrowserTypeLaunchOptions options)
        {
            var playwright = await Playwright.CreateAsync();
            return await playwright[driverType.ToString().ToLower()].LaunchAsync(options);
        }

        private BrowserTypeLaunchOptions Getparameters(string[]? args, float? timeout = DEFAULT_TIMEOUT, bool headless = true, float? slowmo = null)
        {
            return new BrowserTypeLaunchOptions
            {
                Args = args,
                Timeout = ToMilliSeconds(timeout),
                Headless = headless,
                SlowMo = slowmo
            };
        }

        private static float? ToMilliSeconds(float? Seconds)
        {
            return Seconds * 1000;
        }

     
    }
}
