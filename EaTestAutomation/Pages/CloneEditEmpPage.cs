using EAFramework.Base;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaTestAutomation.Pages
{
    public class CloneEditEmpPage : PageBase
    {
        public CloneEditEmpPage(IPage page) : base(page)
        {
        }

        private ILocator NavigationBar => Locator("nav");
        private ILocator EmployeesDiv => NavigationBar.Locator(".container");
        private ILocator EmployeesLink => EmployeesDiv.Locator("a:has-text('Employees')");
        private ILocator SearchFormDiv => Locator("form.search-card");
        private ILocator SearchInput => SearchFormDiv.GetByPlaceholder("Search by name...");
        private ILocator SearchButton => SearchFormDiv.Locator("button.btn-search");
        private ILocator EmployeeTable => Locator(".employee-table-card table");
        private ILocator EditForm => Locator(".form-card-body");
        private ILocator FullNameInput => EditForm.Locator("//input[@name='Name']");
        private ILocator Formrow2Input => EditForm.Locator(".form-row-2");
        private ILocator EditAge => Formrow2Input.Locator("//input[@name='Age']");
        private ILocator EditSalary => Formrow2Input.Locator("#Salary");
        private ILocator DurationWorked => Formrow2Input.Locator("//input[@name='DurationWorked']");
        private ILocator EditEmail => Formrow2Input.Locator("#Email");
        private ILocator SaveChangesButtonDiv => EditForm.Locator(".form-actions");
        private ILocator SaveChaangesButton => SaveChangesButtonDiv.Locator("button:has-text('Save Changes')");

        public async Task ClickEmployees() => await ClickExAsync(EmployeesLink);

        public async Task FillSearchInput(string name) => await FillAsync(SearchInput, name);

        public async Task ClickSearchButton()
        {
            await ClickAsync(SearchButton);
            await WaitForNetworkIdleAsync();
        }

        public async Task FilterUserandEditUser(string name)
        {
            var employeeRow = EmployeeTable
                .Locator("tbody tr")
                .Filter(new()
                {
                    Has = Page.Locator(".emp-name").GetByText(name, new() { Exact = true })
                });

            await employeeRow.WaitForAsync(new() { State = WaitForSelectorState.Visible });

            var editButton = employeeRow.Locator(".action-group a.btn-edit");
            await ClickAsync(editButton);
        }

        public async Task EditFullname(string name)
        {
            await ClickAsync(FullNameInput);
            await ClearTextAsync(FullNameInput);
            await FillAsync(FullNameInput, name);
        }

        public async Task EditEditAge(string age)
        {
            await ClickAsync(EditAge);
            await ClearTextAsync(EditAge);
            await FillAsync(EditAge, age);
        }

        public async Task EditSalaryInput(string salary)
        {
            await ClickAsync(EditSalary);
            await ClearTextAsync(EditSalary);
            await FillAsync(EditSalary, salary);
        }

        public async Task EditDurationwork(string durationWorked)
        {
            await ClickAsync(DurationWorked);
            await ClearTextAsync(DurationWorked);
            await FillAsync(DurationWorked, durationWorked);
        }

        public async Task EditGradeSelectDropdownOptionAsync(string selector, string label) =>
            await SelectPageOptionAsync(selector, label);

        public async Task EditEmailInput(string email)
        {
            await ClickAsync(EditEmail);
            await ClearTextAsync(EditEmail);
            await FillAsync(EditEmail, email);
        }

        public async Task ClickonSaveChangesButton()
        {
            await ClickAsync(SaveChaangesButton);
        }


    }
}
