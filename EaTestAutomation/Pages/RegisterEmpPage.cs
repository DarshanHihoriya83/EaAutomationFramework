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
        private ILocator RegistrationButton => ContainerClass.Locator("//a[text()='Register1']");
        private ILocator RegistratonFromDiv => Locator(".register-wrapper");
        private ILocator RegistratonFromContainer => RegistratonFromDiv.Locator("form");
        private ILocator InputDiv => RegistratonFromContainer.Locator(".mb-3");
        private ILocator UsernameInput => InputDiv.Locator("//input[@name='UserName']");
        private ILocator EmailInput => InputDiv.Locator("#Email");
        private ILocator PasswordInput => InputDiv.GetByPlaceholder("Create a strong password");
        private ILocator ConfirmPasswordInput => InputDiv.GetByPlaceholder("Repeat your password");
        private ILocator CreateAccountButton => RegistratonFromContainer.Locator("button:has-text('Create Account')");
        private ILocator ProfileButton => NavigationBar.GetByTitle("Manage");         
        private ILocator LogoutNavBarForm => NavigationBar.Locator("form.form-inline");
        private ILocator LogoutButton => LogoutNavBarForm.Locator("button:has-text('Logout')");

        

        public async Task ClickonRegistationButton()
        {
            await ClickExAsync(RegistrationButton);      
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
        }

        public async Task profileButton()
        {
            await ClickExAsync(ProfileButton);      
        }

        public async Task ClickonLogoutButton()
        {
            await ClickExAsync(LogoutButton);      
        }
        
    }
}
