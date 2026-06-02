using EAFramework.AIHealing;
using EAFramework.Extension;
using EaTestAutomation.Base;
using EaTestAutomation.Pages;
using Microsoft.Playwright;
using Xunit;

namespace EaTestAutomation.Test
{
    /// <summary>
    /// Proves AI self-healing resolves broken selectors at runtime.
    /// </summary>
    public class AiSelfHealingVerificationTest : BaseTest
    {
        private const string BrokenRegisterSelector =
            "nav .container a:has-text('Register1')";

        [Fact]
        public async Task HealingClickAsync_RepairsBrokenRegisterLink()
        {
            BindArtifactToTestCase(nameof(HealingClickAsync_RepairsBrokenRegisterLink));

            Page.ClearHealingStore();

            await Page.GotoAsync(_testSettings.Applicationurl);

            await Page.HealingClickAsync(BrokenRegisterSelector);

            await Page.Locator(".register-wrapper")
                .WaitForAsync(new()
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 15000
                });

            IReadOnlyDictionary<string, string> mappings = Page.GetHealedMappings();

            Assert.True(
                mappings.ContainsKey(BrokenRegisterSelector),
                "Expected healed mapping for broken Register link selector.");

            Assert.Contains(
                "Register",
                mappings[BrokenRegisterSelector],
                StringComparison.OrdinalIgnoreCase);

            MarkTestPassed(true);
        }

        [Fact]
        public async Task ChainedLocator_ClickExAsync_HealsBrokenRegistrationButton()
        {
            BindArtifactToTestCase(nameof(ChainedLocator_ClickExAsync_HealsBrokenRegistrationButton));

            Page.ClearHealingStore();

            var page = new BrokenRegisterEmpPage(Page);

            await Page.GotoAsync(_testSettings.Applicationurl);

            await page.OpenRegistrationWithBrokenChainAsync();

            await Page.Locator(".register-wrapper input[name='UserName']")
                .WaitForAsync(new()
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 15000
                });

            MarkTestPassed(true);
        }

        [Fact]
        public async Task RegisterEmpFlow_EndToEnd_WithAutomaticHealing()
        {
            BindArtifactToTestCase(nameof(RegisterEmpFlow_EndToEnd_WithAutomaticHealing));

            var registerPage = new RegisterEmpPage(Page);

            await Page.GotoAsync(_testSettings.Applicationurl);

            await registerPage.ClickonRegistationButton();

            await Page.Locator(".register-wrapper input[name='UserName']")
                .WaitForAsync(new()
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 15000
                });

            MarkTestPassed(true);
        }

        /// <summary>
        /// Page with an intentional typo in the Register link text (Register1).
        /// </summary>
        private sealed class BrokenRegisterEmpPage(IPage page) : EAFramework.Base.PageBase(page)
        {
            private ILocator NavigationBar => Locator("nav");
            private ILocator ContainerClass => NavigationBar.Locator(".container");
            private ILocator BrokenRegistrationButton =>
                ContainerClass.Locator("a:has-text('Register1')");

            public Task OpenRegistrationWithBrokenChainAsync() =>
                ClickExAsync(BrokenRegistrationButton);
        }
    }
}
