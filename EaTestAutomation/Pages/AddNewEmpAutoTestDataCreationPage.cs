using EAFramework.Base;
using Microsoft.Playwright;

namespace EaTestAutomation.Pages
{
    public class AddNewEmpAutoTestDataCreationPage : PageBase
    {
        public AddNewEmpAutoTestDataCreationPage(IPage page) : base(page) { }

        private ILocator EmployeeButton => Locator("//a[contains(text(),'Dashboard')]/parent::li/preceding-sibling::li/child::a[contains(text(),'Employees')]");
        private ILocator NewEmployeeButton => Locator("//a[contains(text(),'New Employee')]");
        private ILocator FullnameInput => Locator("//label[text()='Full Name']/following-sibling::div/child::input");
        private ILocator AgeInput => Locator("//input[@name='Age']");
        private ILocator SalaryInput => Locator("//input[@name='Salary']");
        private ILocator DurationWorkedInput => Locator("//input[@name='DurationWorked']");
        private ILocator EmailInput => Locator("//input[@name='Email']");
        private ILocator CreateEmployeeButton => Locator("//button[contains(text(),'Create Employee')]");

        public async Task ClickonEmployeeButton() => await ClickAsync(EmployeeButton);

        public async Task ClickonNewEmployeeButton() => await ClickAsync(NewEmployeeButton);

        public async Task FillFullname(string name) => await FillAsync(FullnameInput, name);

        public async Task FillAge(int age) => await FillAsync(AgeInput, age.ToString());

        public async Task FillSalaryInput(int salary)
        {
            await ClickAsync(SalaryInput);
            await TypeTextOnAsync(SalaryInput, salary.ToString());
        }

        public async Task FillDurationWorkedInput(int durationworked)
        {
            await ClickAsync(DurationWorkedInput);
            await TypeTextOnAsync(DurationWorkedInput, durationworked.ToString());
        }

        public async Task SelectDropdownOptionAsync(string selector, string label) =>
            await SelectPageOptionAsync(selector, label);

        public async Task FillEmailInput(string email)
        {
            await ClickAsync(EmailInput);
            await TypeTextOnAsync(EmailInput, email);
        }

        public async Task ClickonCreateEmployeeButton() => await ClickAsync(CreateEmployeeButton);
    }
}
