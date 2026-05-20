using EAFramework.Extension;
using EAFramework.Base;
using Microsoft.Playwright;

namespace EaTestAutomation.Pages
{
    /// <summary>
    /// Edit Employee page — same locators as <see cref="EditEMPPage"/>,
    /// with AI self-healing fallback when primary locators fail.
    /// </summary>
    public class EditEMPTSelfHealingPage : PageBase
    {
        // Primary locator strings (used by SelfHealingEngine when ILocator path fails)
        private const string EmployeesLinkSelector =
            "nav .container a:has-text('Employees')";

        private const string SearchInputSelector =
            "form.search-card input[placeholder='Search by name...']";

        private const string SearchButtonSelector =
            "form.search-card button.btn-search";

        private const string FullNameSelector =
            ".form-card-body >> xpath=//input[@name='Name']";

        private const string AgeSelector =
            ".form-card-body .form-row-2 >> xpath=//input[@name='age1']";

        private const string SalarySelector =
            ".form-card-body .form-row-2 >> #Salary";

        private const string DurationSelector =
            ".form-card-body .form-row-2 >> xpath=//input[@name='DurationWorked']";

        private const string EmailSelector =
            ".form-card-body .form-row-2 >> #Email";

        private const string SaveChangesSelector =
            ".form-card-body .form-actions >> button:has-text('Save Changes')";

        private const string GradeSelector = "#Grade";

        public EditEMPTSelfHealingPage(IPage page) : base(page)
        {
        }

        // ----- Original EditEMPPage locator chains (reference locators) -----

        private ILocator NavigationBar => _page.Locator("nav");

        private ILocator EmployeesDiv => NavigationBar.Locator(".container");

        private ILocator EmployeesLink =>
            EmployeesDiv.Locator("a:has-text('Employees')");

        private ILocator SearchFormDiv => _page.Locator("form.search-card");

        private ILocator SearchInput =>
            SearchFormDiv.GetByPlaceholder("Search by name...");

        private ILocator SearchButton =>
            SearchFormDiv.Locator("button.btn-search");

        private ILocator EmployeeTable =>
            _page.Locator(".employee-table-card table");

        private ILocator EditForm => _page.Locator(".form-card-body");

        private ILocator Formrow2Input => EditForm.Locator(".form-row-2");

        private ILocator FullNameInput =>
            EditForm.Locator("//input[@name='Name']");

        private ILocator EditAge =>
            Formrow2Input.Locator("//input[@name='age1']");

        private ILocator EditSalary => Formrow2Input.Locator("#Salary");

        private ILocator DurationWorked =>
            Formrow2Input.Locator("//input[@name='DurationWorked']");

        private ILocator EditEmail => Formrow2Input.Locator("#Email");

        private ILocator SaveChangesButtonDiv =>
            EditForm.Locator(".form-actions");

        private ILocator SaveChangesButton =>
            SaveChangesButtonDiv.Locator("button:has-text('Save Changes')");

        public async Task ClickEmployees()
        {
            try
            {
                await EmployeesLink.ClickExAsync();
            }
            catch
            {
                await ClickAsync(EmployeesLinkSelector);
            }

            await SearchFormDiv.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });
        }

        public async Task FillSearchInput(string name)
        {
            try
            {
                await SearchInput.WaitForAsync(new()
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 10000
                });

                await SearchInput.ClearAsync();
                await SearchInput.FillAsync(name);
            }
            catch
            {
                await FillAsync(SearchInputSelector, name);
            }
        }

        public async Task ClickSearchButton()
        {
            try
            {
                await SearchButton.ClickAsync();
            }
            catch
            {
                await ClickAsync(SearchButtonSelector);
            }

            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        public async Task FilterUserandEditUser(string name)
        {
            var employeeRow = EmployeeTable
                .Locator("tbody tr")
                .Filter(new()
                {
                    Has = _page.Locator(".emp-name")
                        .GetByText(name, new() { Exact = true })
                });

            if (await employeeRow.CountAsync() == 0)
            {
                employeeRow = EmployeeTable
                    .Locator("tbody tr")
                    .Filter(new()
                    {
                        Has = _page.Locator(".emp-name")
                            .GetByText(name, new() { Exact = false })
                    });
            }

            await employeeRow.First.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = 30000
            });

            var editButton = employeeRow.First.Locator(".action-group a.btn-edit");

            try
            {
                await editButton.ClickAsync();
            }
            catch
            {
                await ClickAsync(editButton);
            }
        }

        public async Task EditFullname(string name)
        {
            await FillFieldAsync(FullNameInput, FullNameSelector, name);
        }

        public async Task EditEditAge(string age)
        {
            await FillFieldAsync(EditAge, AgeSelector, age);
        }

        public async Task EditSalaryInput(string salary)
        {
            await FillFieldAsync(EditSalary, SalarySelector, salary);
        }

        public async Task EditDurationwork(string durationWorked)
        {
            await FillFieldAsync(DurationWorked, DurationSelector, durationWorked);
        }

        public async Task EditGradeSelectDropdownOptionAsync(string grade)
        {
            try
            {
                await _page.SelectOptionAsync(
                    GradeSelector,
                    new SelectOptionValue { Label = grade });
            }
            catch
            {
                await SelectDropdownAsync(GradeSelector, grade);
            }
        }

        public async Task EditEmailInput(string email)
        {
            await FillFieldAsync(EditEmail, EmailSelector, email);
        }

        public async Task ClickonSaveChangesButton()
        {
            try
            {
                await SaveChangesButton.ClickAsync();
            }
            catch
            {
                await ClickAsync(SaveChangesSelector);
            }
        }

        /// <summary>
        /// Uses original EditEMPPage locator first; falls back to AI self-healing selector.
        /// </summary>
        private async Task FillFieldAsync(
            ILocator primaryLocator,
            string healingSelector,
            string value)
        {
            try
            {
                await primaryLocator.ClickAsync();
                await primaryLocator.ClearAsync();
                await primaryLocator.FillAsync(value);
            }
            catch
            {
                await ClearTextAsync(healingSelector);
                await FillAsync(healingSelector, value);
            }
        }
    }
}
