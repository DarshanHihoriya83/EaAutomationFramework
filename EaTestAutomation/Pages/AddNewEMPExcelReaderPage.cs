using EAFramework.Base;
using Microsoft.Playwright;

namespace EaTestAutomation.Pages
{

    public class AddNewEMPExcelReaderPage : PageBase
    {
        

        public AddNewEMPExcelReaderPage(IPage page) :base(page) 
        {
        }

        private ILocator EmployeeButton => _page.Locator("//a[contains(text(),'Dashboard')]/parent::li/preceding-sibling::li/child::a[contains(text(),'Employees')]");
        private ILocator NewEmployeeButton => _page.Locator("//a[contains(text(),'New Employee')]");
        private ILocator FullnameInput => _page.Locator("//input[@name='Name']");
        private ILocator AgeInput => _page.Locator("//input[@name='Age']");
        private ILocator SalaryInput => _page.Locator("//input[@name='Salary']");
        private ILocator DurationWorkedInput => _page.Locator("//input[@name='DurationWorked']");
        private ILocator EmailInput =>  _page.Locator("//input[@name='Email']");
        private ILocator CreateEmployeeButton =>  _page.Locator("//button[contains(text(),'Create Employee')]");



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

        public async Task FillAge(string age)
        {
            await AgeInput.FillAsync(age);
        }

        public async Task FillSalaryInput(string salary)
        {
            await SalaryInput.FillAsync(salary);
        }

        public async Task FillDurationWorkedInput(string durationWorked)
        {
            await DurationWorkedInput.FillAsync(durationWorked);
        }

        public async Task SelectDropdownOptionAsync(string selector, string grade)
        {
            await _page.SelectOptionAsync(selector,
                new SelectOptionValue
                {
                    Label = grade
                });
        }

        public async Task FillEmailInput(string email)
        {
            await EmailInput.FillAsync(email);
        }

        public async Task ClickonCreateEmployeeButton()
        {
            await CreateEmployeeButton.ClickAsync();
        }

    }
}


