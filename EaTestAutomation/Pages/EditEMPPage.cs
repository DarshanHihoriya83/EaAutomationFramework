using DocumentFormat.OpenXml.Spreadsheet;
using EAFramework.Extension;
using Microsoft.Playwright;

namespace EaTestAutomation.Pages
{
    public class EditEMPPage
    {
        private readonly IPage _page;

        public EditEMPPage(IPage page)
        {
            _page = page;
        }

        private ILocator NavigationBar => _page.Locator("nav");
        private ILocator EmployeesDiv => NavigationBar.Locator(".container");
        private ILocator EmployeesLink => EmployeesDiv.Locator("a:has-text('Employees')");
        private ILocator SearchFormDiv => _page.Locator("form.search-card");
        private ILocator SearchInput => SearchFormDiv.GetByPlaceholder("Search by name...");
        private ILocator SearchButton => SearchFormDiv.Locator("button.btn-search");
        private ILocator EmployeeTable => _page.Locator(".employee-table-card table");
        private ILocator EditForm => _page.Locator(".form-card-body");
        private ILocator FullNameInput => EditForm.Locator("//input[@name='Name']");
        private ILocator Formrow2Input => EditForm.Locator(".form-row-2");
        private ILocator EditAge => Formrow2Input.Locator("//input[@name='Age']");
        private ILocator EditSalary => Formrow2Input.Locator("#Salary");
        private ILocator DurationWorked => Formrow2Input.Locator("//input[@name='DurationWorked']");
        private ILocator EditEmail => Formrow2Input.Locator("#Email");
        private ILocator SaveChangesButtonDiv => EditForm.Locator(".form-actions");
        private ILocator SaveChaangesButton => SaveChangesButtonDiv.Locator("button:has-text('Save Changes')");





        public async Task ClickEmployees()
        {
            await EmployeesLink.ClickExAsync();
        }

        public async Task FillSearchInput(string name)
        {
            await SearchInput.FillAsync(name);
        }

        public async Task ClickSearchButton()
        {
            await SearchButton.ClickAsync();
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        public async Task FilterUserandEditUser(string name)
        {
            var employeeRow = EmployeeTable
                .Locator("tbody tr")
                .Filter(new()
                {
                    Has = _page.Locator(".emp-name").GetByText(name, new() { Exact = true })
                });

            await employeeRow.WaitForAsync(new() { State = WaitForSelectorState.Visible });

            var editButton = employeeRow.Locator(".action-group a.btn-edit");
            await editButton.ClickAsync();
        }

        public async Task EditFullname(string name)
        {
            await FullNameInput.ClickAsync();
            await FullNameInput.ClearAsync();
            await FullNameInput.FillAsync(name);
        }

        public async Task EditEditAge(string age)
        {
            await EditAge.ClickAsync();
            await EditAge.ClearAsync();
            await EditAge.FillAsync(age);
        }

        public async Task EditSalaryInput(string salary)
        {
            await EditSalary.ClickAsync();
            await EditSalary.ClearAsync();
            await EditSalary.FillAsync(salary);
        }

        public async Task EditDurationwork(string durationWorked)
        {
            await DurationWorked.ClickAsync();
            await DurationWorked.ClearAsync();
            await DurationWorked.FillAsync(durationWorked);
        }

        public async Task EditGradeSelectDropdownOptionAsync(string selector, string label)
        {
            await _page.SelectOptionAsync(selector, new SelectOptionValue
            {
                Label = label
            });
        }

        public async Task EditEmailInput(string email)
        {
            await EditEmail.ClickAsync();
            await EditEmail.ClearAsync();
            await EditEmail.FillAsync(email);
        }

        public async Task ClickonSaveChangesButton()
        {
            await SaveChaangesButton.ClickAsync();
        }

        
    }
}
