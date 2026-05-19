using Microsoft.Playwright;

public class LoginPage
{
    private readonly IPage _page;

    public LoginPage(IPage page)
    {
        _page = page;
    }

    private ILocator Navbar => _page.Locator("nav");
    private ILocator LoginButton => Navbar.Locator("//a[text()='Login']");
    private ILocator Form => _page.Locator("form");
    private ILocator Username => Form.GetByPlaceholder("Enter your username");
    private ILocator Password => Form.GetByPlaceholder("Enter your password");
    private ILocator SignInButton => Form.Locator("button:has-text('Sign In')");


    public async Task ClickOnLoginButton()
    {
        await LoginButton.ClickAsync();
    }

    public async Task FillUsername()
    {
        await Username.FillAsync("admin");
    }

    public async Task FillPassword()
    {
        await Password.FillAsync("password");
    }

    public async Task ClickOnSignInButton()
    {
        await SignInButton.ClickAsync();       
    }

}