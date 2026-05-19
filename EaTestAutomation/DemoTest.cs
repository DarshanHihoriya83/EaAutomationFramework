using EaFramework.Config;
using EAFramework.Driver;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Xunit;

namespace PlaywrightDemo
{
    public class DemoTest : IClassFixture<PlaywrightDriverInitializer>
    {
        private readonly PlaywrightDriver _playwrightdriver;
        private readonly PlaywrightDriverInitializer _playwrightDriverInitializer;
        private readonly EAFramework.Config.TestSettings _testSettings;



        public DemoTest(PlaywrightDriverInitializer playwrightDriverInitializer)
        {
            _testSettings = ConfigReader.ReadConfig();
            _playwrightDriverInitializer = new PlaywrightDriverInitializer();
            _playwrightdriver = new PlaywrightDriver(_testSettings, _playwrightDriverInitializer);

           
        }


        [Fact]
        public async Task test1()
        {
            var page = await _playwrightdriver.Page;
            await page.GotoAsync(_testSettings.Applicationurl);


            await page.ClickAsync("text=Login");
        }

        [Fact]
        public async Task LoginTest()
        {
            var page = await _playwrightdriver.Page;
            await page.GotoAsync("http://eaapp.somee.com");

            await page.ClickAsync("text=Login");
            await page.GetByLabel("User Name").FillAsync("admin");
            await page.GetByLabel("Password").FillAsync("password");

            //await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign In" }).ClickAsync();
            //await page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Employee List" }).ClickAsync();
        }


        [Fact]
        public async Task LunchingBrowserInAnotherOptions()
        {
            var PlaywrightDriver = await Playwright.CreateAsync();

            var browserOption = new BrowserTypeLaunchOptions();
            browserOption.Headless = false;
            //browserOption.Devtools = false;
            browserOption.Channel = "msedge";

            var chromium = await PlaywrightDriver["chromium"].LaunchAsync(browserOption);
            var browsercontext = await chromium.NewContextAsync();
            var page = await browsercontext.NewPageAsync();


            await page.GotoAsync("http://eaapp.somee.com");

        }

        [Fact]
        public async Task RegisterandLoginTest()
        {
            var page = await _playwrightdriver.Page;
            await page.GotoAsync(_testSettings.Applicationurl);

            await page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
            await page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).ClickAsync();
            await page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).FillAsync("yashraj3");
            await page.GetByRole(AriaRole.Textbox, new() { Name = "Email Address" }).ClickAsync();
            await page.GetByRole(AriaRole.Textbox, new() { Name = "Email Address" }).FillAsync("yashraj3@gmail.com");
            await page.GetByRole(AriaRole.Textbox, new() { Name = "Password", Exact = true }).ClickAsync();
            await page.GetByRole(AriaRole.Textbox, new() { Name = "Password", Exact = true }).FillAsync("yash@123");
            await page.GetByRole(AriaRole.Textbox, new() { Name = "Confirm Password" }).ClickAsync();
            await page.GetByRole(AriaRole.Textbox, new() { Name = "Confirm Password" }).ClickAsync();
            await page.GetByRole(AriaRole.Textbox, new() { Name = "Confirm Password" }).FillAsync("yash@123");
            await page.GetByRole(AriaRole.Button, new() { Name = "Create Account" }).ClickAsync();
            await page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();
            await page.GetByRole(AriaRole.Link, new() { Name = "Sign In", Exact = true }).ClickAsync();
            await page.GetByRole(AriaRole.Textbox, new() { Name = "User Name" }).ClickAsync();
            await page.GetByRole(AriaRole.Textbox, new() { Name = "User Name" }).FillAsync("yashraj3");
            await page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
            await page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("yash@123");      
            await page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();
            await page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();

        }
    }
}