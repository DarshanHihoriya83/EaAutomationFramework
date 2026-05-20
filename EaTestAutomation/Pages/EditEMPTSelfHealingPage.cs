using EAFramework.Base;
using EAFramework.Extension;
using Microsoft.Playwright;

namespace EaTestAutomation.Pages
{
    /// <summary>
    /// Edit Employee page using a single selector per control. All actions go through
    /// <see cref="PageBase"/> so <see cref="EAFramework.AIHealing.SelfHealingEngine"/> can heal failed locators
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
            ".form-card-body .form-row-2 #Email12";

        private const string SaveChangesSelector =
            ".form-card-body .form-actions button:has-text('Save Changes')";

        private const string GradeSelector = "#Grade";

        public EditEMPTSelfHealingPage(IPage page) : base(page)
        {
        }

        /// <summary>XPath literal for <c>normalize-space()=...</c> comparisons.</summary>
        private static string XPathStringLiteral(string value)
        {
            if (value.IndexOf('\'', StringComparison.Ordinal) < 0)
            {
                return "'" + value + "'";
            }

            string[] parts = value.Split('\'');
            var sb = new System.Text.StringBuilder("concat(");

            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(", \"'\", ");
                }

                sb.Append('\'');
                sb.Append(parts[i]);
                sb.Append('\'');
            }

            sb.Append(')');
            return sb.ToString();
        }

        private static string EditEmployeeButtonSelector(string employeeName)
        {
            string lit = XPathStringLiteral(employeeName);

            return "xpath=//*[contains(@class,'employee-table-card')]"
                   + "//tbody//tr[.//*[contains(@class,'emp-name')"
                   + " and normalize-space()=" + lit + "]]"
                   + "//a[contains(@class,'btn-edit')]";
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
            await JsFillAsync(SearchInputSelector, name);
        }

        public async Task ClickSearchButton()
        {
            await ClickAsync(SearchButtonSelector);
            await _page.WaitForNetworkIdleAsync();
        }

        public async Task FilterUserandEditUser(string name)
        {
            await ClickAsync(EditEmployeeButtonSelector(name));
        }

        public async Task EditFullname(string name)
        {
            await JsFillAsync(FullNameSelector, name);
        }

        public async Task EditEditAge(string age)
        {
            await JsFillAsync(AgeSelector, age);
        }

        public async Task EditSalaryInput(string salary)
        {
            await JsFillAsync(SalarySelector, salary);
        }

        public async Task EditDurationwork(string durationWorked)
        {
            await JsFillAsync(DurationSelector, durationWorked);
        }

        public async Task EditGradeSelectDropdownOptionAsync(string grade)
        {
            await SelectDropdownAsync(GradeSelector, grade);
        }

        public async Task EditEmailInput(string email)
        {
            await JsFillAsync(EmailSelector, email);
        }

        public async Task ClickonSaveChangesButton()
        {
            await ClickAsync(SaveChangesSelector);
        }
    }
}
