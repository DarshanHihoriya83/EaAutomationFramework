using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaTestAutomation.Pages
{
    public class AddNewEmpAutoTestDataCreationPage
    {
        private readonly IPage _page;
        public AddNewEmpAutoTestDataCreationPage(IPage page)
        {
            _page = page;

        }

        private ILocator EmployeeButton => _page.Locator("//a[contains(text(),'Dashboard')]/parent::li/preceding-sibling::li/child::a[contains(text(),'Employees')]");
        private ILocator NewEmployeeButton => _page.Locator("//a[contains(text(),'New Employee')]");
        private ILocator FullnameInput => _page.Locator("//label[text()='Full Name']/following-sibling::div/child::input");
        private ILocator AgeInput => _page.Locator("//input[@name='Age']");
        private ILocator SalaryInput => _page.Locator("//input[@name='Salary']");
        private ILocator DurationWorkedInput => _page.Locator("//input[@name='DurationWorked']");
        private ILocator EmailInput => _page.Locator("//input[@name='Email']");
        private ILocator CreateEmployeeButton => _page.Locator("//button[contains(text(),'Create Employee')]");





        public async Task ClickonEmployeeButton()
        {
            await EmployeeButton.ClickAsync();
        }

        public async Task ClickonNewEmployeeButton()
        {
            await NewEmployeeButton.ClickAsync();
        }

        public async Task FillFullname(string name)
        {
            await FullnameInput.FillAsync(name);
        }

        public async Task FillAge(int age)
        {
            await AgeInput.FillAsync(age.ToString());
        }

        public async Task FillSalaryInput(int salary)
        {
            await SalaryInput.ClickAsync();
            await SalaryInput.TypeAsync(salary.ToString());
        }

        public async Task FillDurationWorkedInput(int durationworked)
        {
            await DurationWorkedInput.ClickAsync();
            await DurationWorkedInput.TypeAsync(durationworked.ToString());
        }

        public async Task SelectDropdownOptionAsync(string selector, string label)
        {
            await _page.SelectOptionAsync(selector, new SelectOptionValue
            {
                Label = label
            });
        }

        public async Task FillEmailInput(string email)
        {
            await EmailInput.ClickAsync();
            await EmailInput.TypeAsync(email);
        }

        public async Task ClickonCreateEmployeeButton()
        {
            await CreateEmployeeButton.ClickAsync();
        }


    }
}

