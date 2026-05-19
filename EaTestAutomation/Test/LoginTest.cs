//using EaFramework.Config;
//using EAFramework.Config;
//using EAFramework.Driver;
//using EaTestAutomation.Pages;
//using EaTestAutomation.TestBase;
//using Microsoft.Playwright;
//using Xunit;

//namespace EaTestAutomation.Test
//{
//    public class LoginTest 
//    {
//        private readonly PlaywrightDriver _playwrightdriver;
//        private readonly PlaywrightDriverInitializer _playwrightDriverInitializer;
//        private readonly TestSettings _testSettings;

//        public LoginTest(PlaywrightDriverInitializer playwrightDriverInitializer)
//        {
//            _testSettings = ConfigReader.ReadConfig();

//            _playwrightDriverInitializer = playwrightDriverInitializer;

//            _playwrightdriver = new PlaywrightDriver(
//                _testSettings,
//                _playwrightDriverInitializer);
//        }

//        [Fact]
//        public async Task LoginAppTest()
//        {

//            var page = await _playwrightdriver.Page;

//            await page.GotoAsync(_testSettings.Applicationurl);

//            LoginPage loginPage = new LoginPage(page);

//            await loginPage.ClickOnLoginButton();
//            await loginPage.FillUsername();
//            await loginPage.FillPassword();
//            await loginPage.ClickOnSignInButton();


//            Assert.Contains("Login", await page.TitleAsync());
//        }


//        public void Dispose()
//        {
//            _playwrightdriver.Dispose();
//        }
//    }
//}
