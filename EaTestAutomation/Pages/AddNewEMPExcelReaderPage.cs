using EAFramework.Base;
using Microsoft.Playwright;

namespace EaTestAutomation.Pages
{
    public class AddNewEMPExcelReaderPage : PageBase
    {
        public AddNewEMPExcelReaderPage(IPage page) : base(page) { }

        private ILocator EmployeeButton => Locator("//a[contains(text(),'Dashboard')]/parent::li/preceding-sibling::li/child::a[contains(text(),'Employees')]");
        private ILocator NewEmployeeButton => Locator("//a[contains(text(),'New Employee')]");
        private ILocator FullnameInput => Locator("//input[@name='Name']");
        private ILocator AgeInput => Locator("//input[@name='Age']");
        private ILocator SalaryInput => Locator("//input[@name='Salary']");
        private ILocator DurationWorkedInput => Locator("//input[@name='DurationWorked']");
        private ILocator EmailInput => Locator("//input[@name='Email']");
        private ILocator CreateEmployeeButton => Locator("//button[contains(text(),'Create Employee')]");

        public async Task ClickonEmployeeButton() => await ClickAsync(EmployeeButton);

        public async Task ClickonNewEmployeeButton() => await ClickAsync(NewEmployeeButton);

        public async Task FillFullname(string name) => await FillAsync(FullnameInput, name);

        public async Task FillAge(string age) => await FillAsync(AgeInput, age);

        public async Task FillSalaryInput(string salary) => await FillAsync(SalaryInput, salary);

        public async Task FillDurationWorkedInput(string durationWorked) =>
            await FillAsync(DurationWorkedInput, durationWorked);

        public async Task SelectDropdownOptionAsync(string selector, string grade) =>
            await SelectPageOptionAsync(selector, grade);

        public async Task FillEmailInput(string email) => await FillAsync(EmailInput, email);

        public async Task ClickonCreateEmployeeButton() => await ClickAsync(CreateEmployeeButton);
    }
}
