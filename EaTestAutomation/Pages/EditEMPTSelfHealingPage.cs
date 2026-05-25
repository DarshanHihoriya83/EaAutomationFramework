using EAFramework.Base;
using EAFramework.Extension;
using Microsoft.Playwright;

namespace EaTestAutomation.Pages
{
    /// <summary>
    /// Edit Employee page using a single selector per control.All actions go through
    /// <see cref = "PageBase" /> so < see cref= "EAFramework.AIHealing.SelfHealingEngine" /> can heal failed locators
    /// and persist mappings to <c>FailedLocatorStore.json</c>.
    /// </summary>
    public class EditEMPTSelfHealingPage : PageBase
    {
        private const string EmployeesLinkSelector =
            "nav .container a:has-text('Employees')";

        private const string SearchFormSelector = "form.search-card";

        private const string SearchInputSelector =
            "form.search-card input[name='searchTerm']";

        private const string SearchButtonSelector =
            "form.search-card button.btn-search";

        private const string FullNameSelector =
            ".form-card-body input[name='Name']";

        /// <summary>
        /// Must match the live DOM / <see cref="EditEMPPage"/> (<c>Name="Age"</c>, not <c>age1</c>).
        /// </summary>
        private const string AgeSelector =
            ".form-card-body .form-row-2 input[name='Age']";

        private const string SalarySelector =
            ".form-card-body .form-row-2 #Salary";

        private const string DurationSelector =
            ".form-card-body .form-row-2 input[name='DurationWorked']";

        private const string EmailSelector =
            ".form-card-body .form-row-2 #Email";

        private const string SaveChangesSelector =
            ".form-card-body .form-actions button:has-text('Save Changes')";

        private const string GradeSelector = "#Grade";

        private const string EmployeeTableSelector =
            ".employee-table-card table";

        public EditEMPTSelfHealingPage(IPage page) : base(page)
        {
        }

        public async Task ClickEmployees()
        {
            await ClickAsync(EmployeesLinkSelector);

            ILocator searchForm = await FindAsync(SearchFormSelector);

            await searchForm.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });
        }

        public async Task FillSearchInput(string name)
        {
            await FillExAsync(SearchInputSelector, name);
        }

        public async Task ClickSearchButton()
        {
            await ClickExAsync(SearchButtonSelector);
            await _page.WaitForNetworkIdleAsync();
        }

        public async Task FilterUserandEditUser(string name, string reference = "")
        {
            ILocator table = await FindAsync(EmployeeTableSelector);

            await table.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });

            ILocator employeeRow = await ResolveEmployeeRowAsync(table, name, reference);

            if (await employeeRow.CountAsync() != 1
                && !string.IsNullOrWhiteSpace(reference)
                && reference.Contains('@', StringComparison.Ordinal))
            {
                await JsFillAsync(SearchInputSelector, string.Empty);
                await ClickAsync(SearchButtonSelector);
                await _page.WaitForNetworkIdleAsync();

                table = await FindAsync(EmployeeTableSelector);
                employeeRow = await ResolveEmployeeRowAsync(table, name, reference);
            }

            if (await employeeRow.CountAsync() != 1)
            {
                throw new InvalidOperationException(
                    $"Expected exactly one employee row for name '{name}'"
                    + (string.IsNullOrWhiteSpace(reference)
                        ? "."
                        : $" and reference '{reference}'."));
            }

            await employeeRow.First.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });

            ILocator editButton = employeeRow.Locator(".action-group a.btn-edit");
            await ClickAsync(editButton);
        }

        private async Task<ILocator> ResolveEmployeeRowAsync(
            ILocator table,
            string name,
            string reference)
        {
            if (!string.IsNullOrWhiteSpace(reference)
                && reference.Contains('@', StringComparison.Ordinal))
            {
                ILocator rowByEmail = table
                    .Locator("tbody tr")
                    .Filter(new()
                    {
                        Has = _page.Locator(".emp-email")
                            .GetByText(reference, new() { Exact = true })
                    });

                if (await rowByEmail.CountAsync() > 0)
                {
                    return rowByEmail;
                }
            }

            ILocator rowByName = table
                .Locator("tbody tr")
                .Filter(new()
                {
                    Has = _page.Locator(".emp-name")
                        .GetByText(name, new() { Exact = true })
                });

            if (await rowByName.CountAsync() > 0)
            {
                return rowByName;
            }

            if (string.IsNullOrWhiteSpace(reference)
                || !reference.Contains('@', StringComparison.Ordinal))
            {
                ILocator rowByPartialName = table
                    .Locator("tbody tr")
                    .Filter(new()
                    {
                        Has = _page.Locator(".emp-name")
                            .GetByText(name, new() { Exact = false })
                    });

                if (await rowByPartialName.CountAsync() == 1)
                {
                    return rowByPartialName;
                }
            }

            return table.Locator("tbody tr")
                .Filter(new() { Has = _page.Locator("[data-ea-no-match]") });
        }

        public async Task EditFullname(string name)
        {
            await FillExAsync(FullNameSelector, name);
        }

        public async Task EditEditAge(string age)
        {
            await FillExAsync(AgeSelector, age);
        }

        public async Task EditSalaryInput(string salary)
        {
            await FillExAsync(SalarySelector, salary);
        }

        public async Task EditDurationwork(string durationWorked)
        {
            await FillExAsync(DurationSelector, durationWorked);
        }

        public async Task EditGradeSelectDropdownOptionAsync(string grade)
        {
            await SelectDropdownAsync(GradeSelector, grade);
        }

        public async Task EditEmailInput(string email)
        {
            await FillExAsync(EmailSelector, email);
        }

        public async Task ClickonSaveChangesButton()
        {
            await ClickExAsync(SaveChangesSelector);
        }
    }
}
