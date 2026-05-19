using EaFramework.Config;
using EAFramework.Config;
using EAFramework.Driver;
using EaTestAutomation.Pages;
using Microsoft.Playwright;
using Xunit;

namespace EaTestAutomation.Base
{
    public class BaseTest : IDisposable
    {
        protected readonly PlaywrightDriver _playwrightDriver;
        protected readonly TestSettings _testSettings;
        protected readonly IPlaywrightDriverInitializer _playwrightDriverInitializer;

        protected IPage Page => _playwrightDriver.Page.Result;
        protected LoginPage LoginPage;


        public BaseTest()
        {
            
            _testSettings = ConfigReader.ReadConfig();
            _playwrightDriverInitializer = new PlaywrightDriverInitializer();
            _playwrightDriver = new PlaywrightDriver( _testSettings, _playwrightDriverInitializer);
            LoginApplication().GetAwaiter().GetResult();
        }

        private async Task LoginApplication()
        {            
            await Page.GotoAsync(_testSettings.Applicationurl); 

            LoginPage = new LoginPage(Page);

            await LoginPage.ClickOnLoginButton();
            await LoginPage.FillUsername();
            await LoginPage.FillPassword();
            await LoginPage.ClickOnSignInButton();
            Assert.Contains("Employee", await Page.ContentAsync());
            await Task.Delay(2000);
        }

        public void Dispose()
        {
            _playwrightDriver.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}