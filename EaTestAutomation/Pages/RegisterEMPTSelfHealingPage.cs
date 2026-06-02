using EAFramework.Base;
using Microsoft.Playwright;

namespace EaTestAutomation.Pages
{
    /// <summary>
    /// Registration page with string selectors routed through <see cref="EAFramework.AIHealing.SelfHealingEngine"/>.
    /// </summary>
    public class RegisterEMPTSelfHealingPage : PageBase
    {
        private const string RegisterLinkSelector =
            "nav .container a:has-text('Register')";

        private const string UsernameSelector =
            ".register-wrapper input[name='UserName']";

        private const string EmailSelector =
            ".register-wrapper input[name='Email']";

        private const string PasswordSelector =
            ".register-wrapper input[name='Password']";

        private const string ConfirmPasswordSelector =
            ".register-wrapper input[name='ConfirmPassword']";

        private const string CreateAccountSelector =
            ".register-wrapper button:has-text('Create Account')";

        private const string ProfileLinkSelector =
            "nav a[title='Manage']";

        private const string LogoutButtonSelector =
            "nav form.form-inline button:has-text('Logout')";

        public RegisterEMPTSelfHealingPage(IPage page) : base(page)
        {
        }

        public async Task ClickonRegistationButton()
        {
            await ClickExAsync(RegisterLinkSelector);
            await _page.Locator(".register-wrapper").WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });
        }

        public async Task EnterUsername(string username) =>
            await FillExAsync(UsernameSelector, username);

        public async Task EnterEmail(string email) =>
            await FillExAsync(EmailSelector, email);

        public async Task EnterPassword(string password) =>
            await FillExAsync(PasswordSelector, password);

        public async Task EnterConfirmPassword(string confirmPassword) =>
            await FillExAsync(ConfirmPasswordSelector, confirmPassword);

        public async Task ClickonCreateAccountButton()
        {
            await ClickExAsync(CreateAccountSelector);
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        /// <summary>Returns true when registration succeeded and the signed-in nav is visible.</summary>
        public async Task<bool> WaitForRegisteredSessionAsync(int timeoutMs = 30000)
        {
            ILocator profile = _page.Locator(ProfileLinkSelector);

            try
            {
                await profile.First.WaitForAsync(new()
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = timeoutMs
                });

                return await profile.CountAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task ProfileButton()
        {
            await ClickExAsync(ProfileLinkSelector);
        }

        public async Task ClickonLogoutButton() =>
            await ClickExAsync(LogoutButtonSelector);
    }
}
