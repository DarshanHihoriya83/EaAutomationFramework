using EAFramework.Base;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaTestAutomation.Pages
{
    public class RegisterEmpPage : PageBase
    {
        public RegisterEmpPage(IPage page) : base(page)
        {
        }


        private ILocator NavigationBar => Locator("nav");    
        private ILocator ContainerClass => NavigationBar.Locator(".container");   
        private ILocator RegistrationButton => ContainerClass.Locator("a:has-text('Register')");
        private ILocator RegistratonFromDiv => Locator(".register-wrapper");
        private ILocator RegistratonFromContainer => RegistratonFromDiv.Locator("form");
        private ILocator InputDiv => RegistratonFromContainer.Locator(".mb-3");
        private ILocator UsernameInput => RegistratonFromContainer.Locator("input[name='UserName']");
        private ILocator EmailInput => RegistratonFromContainer.Locator("input[name='Email']");
        private ILocator PasswordInput => RegistratonFromContainer.Locator("input[name='Password']");
        private ILocator ConfirmPasswordInput => RegistratonFromContainer.Locator("input[name='ConfirmPassword']");
        private ILocator CreateAccountButton => RegistratonFromContainer.Locator("button:has-text('Create Account')");
        private ILocator ProfileButton => NavigationBar.GetByTitle("Manage");         
        private ILocator LogoutNavBarForm => NavigationBar.Locator("form.form-inline");
        private ILocator LogoutButton => LogoutNavBarForm.Locator("button:has-text('Logout')");

        

        public async Task ClickonRegistationButton()
        {
            await ClickExAsync(RegistrationButton);
            await WaitForVisibleAsync(RegistratonFromContainer, 15000);
        }
         
        public async Task EnterUsername(string username)
        {
               await FillExAsync(UsernameInput, username);      
        }

        public async Task EnterEmail(string email)
        {
            await FillExAsync(EmailInput, email);      
        }

        public async Task EnterPassword(string password)
        {
            await FillExAsync(PasswordInput, password);      
        }

        public async Task EnterConfirmPassword(string confirmPassword)
        {
            await FillExAsync(ConfirmPasswordInput, confirmPassword);      
        }

        public async Task ClickonCreateAccountButton()
        {
            await ClickExAsync(CreateAccountButton);
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        public async Task profileButton()
        {
            await ClickExAsync(ProfileButton);      
        }

        public async Task ClickonLogoutButton()
        {
            await ClickExAsync(LogoutButton);      
        }

        /// <summary>Returns true when registration succeeded and the signed-in nav is visible.</summary>
        public async Task<bool> WaitForRegisteredSessionAsync(int timeoutMs = 45000)
        {
            try
            {
                await WaitForVisibleAsync("nav a[title='Manage']", timeoutMs);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
    }
}
