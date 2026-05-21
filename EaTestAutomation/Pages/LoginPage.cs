using EAFramework.Base;
using Microsoft.Playwright;

namespace EaTestAutomation.Pages
{
    public class LoginPage : PageBase
    {
        public LoginPage(IPage page) : base(page) { }

        private ILocator Navbar => Locator("nav");
        private ILocator LoginButton => Navbar.Locator("//a[text()='Login']");
        private ILocator Form => Locator("form");
        private ILocator Username => Form.GetByPlaceholder("Enter your username");
        private ILocator Password => Form.GetByPlaceholder("Enter your password");
        private ILocator SignInButton => Form.Locator("button:has-text('Sign In')");

        public async Task ClickOnLoginButton() => await ClickAsync(LoginButton);

        public async Task FillUsername() => await FillAsync(Username, "admin");

        public async Task FillPassword() => await FillAsync(Password, "password");

        public async Task ClickOnSignInButton() => await ClickAsync(SignInButton);
    }
}
