using EaTestAutomation.Base;
using EaTestAutomation.Pages;
using EaTestAutomation.Utilities;
using Xunit;

namespace EaTestAutomation.Test
{
    /// <summary>
    /// Edit Employee test using AI self-healing — runs every row from Excel <c>AddEditEmployee</c>.
    /// </summary>
    public class AISelfhealingEditEMPTTest : BaseTest
    {
        private const string WorksheetName = "AddEditEmployee";

        public static IEnumerable<object[]> LoadEditEmployeeData()
        {
            foreach (ExcelDataLoader.EmployeeExcelRow row in ExcelDataLoader.LoadEmployeeRows(WorksheetName))
            {
                string testName = $"AISelfHeal_EditEmployee_{row.Name}_R{row.RowIndex}";

                ExcelTestTracker.TrackTestRow(testName, row.RowIndex, row.Reference);

                yield return new object[]
                {
                    row.Name,
                    row.Age,
                    row.Salary,
                    row.DurationWorked,
                    row.Grade,
                    row.Email,
                    testName,
                    row.Reference
                };
            }
        }

        [Theory]
        [MemberData(nameof(LoadEditEmployeeData))]
        public async Task AddEditEmployeeWithSelfHealingTest(
            string name,
            string age,
            string salary,
            string durationWorked,
            string grade,
            string email,
            string testName,
            string reference)
        {
            bool testResult = false;
            string message = "";

            BindArtifactToTestCase(testName);

            var editPage = new EditEMPTSelfHealingPage(Page);

            try
            {
                await editPage.NavigateAsync(
                    $"{_testSettings.Applicationurl.TrimEnd('/')}/Employee");

                await editPage.ClickEmployees();

                await editPage.FillAsync(
                    "form.search-card input[name='searchTerm']xxx",
                    name);

                await editPage.FillSearchInput(name);
                await editPage.ClickSearchButton();
                await editPage.FilterUserandEditUser(name);
                await editPage.EditFullname("Raj");
                await editPage.EditEditAge(age);
                await editPage.EditSalaryInput(salary);
                await editPage.EditDurationwork(durationWorked);
                await editPage.EditGradeSelectDropdownOptionAsync(grade);
                await editPage.EditEmailInput(email);
                await editPage.ClickonSaveChangesButton();

                testResult = true;
            }
            catch (Exception ex)
            {
                message = FormatError(ex);
                throw;
            }
            finally
            {
                MarkTestPassed(testResult);

                ExcelTestTracker.WriteTestResult(
                    WorksheetName,
                    testName,
                    testResult,
                    message,
                    reference);
            }
        }

        private static string FormatError(Exception ex)
        {
            if (ex.InnerException == null)
            {
                return ex.Message;
            }

            return $"{ex.Message} | {ex.InnerException.Message}";
        }
    }
}
