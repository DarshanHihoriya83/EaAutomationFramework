using EaTestAutomation.Base;
using EaTestAutomation.Pages;
using EaTestAutomation.Utilities;
using Xunit;

namespace EaTestAutomation.Test
{
    public class AISelfhealingEditEMPTTest : BaseTest
    {
        private const string WorksheetName = "AddEditEmployee";

        public static IEnumerable<object[]> LoadEditEmployeeData()
        {
            foreach (ExcelDataLoader.ExcelDataRow row in ExcelDataLoader.ReadRows(WorksheetName))
            {
                string name = row.Cell(1);
                string age = row.Cell(2);
                string salary = row.Cell(3);
                string durationWorked = row.Cell(4);
                string grade = row.Cell(5);
                string email = row.Cell(6);
                string reference = row.Reference;

                string testName = $"AISelfHeal_EditEmployee_{name}_R{row.RowIndex}";

                ExcelTestTracker.TrackTestRow(testName, row.RowIndex, reference);

                yield return new object[]
                {
                    name,
                    age,
                    salary,
                    durationWorked,
                    grade,
                    email,
                    testName,
                    reference
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
                await editPage.FillSearchInput(name);
                await editPage.ClickSearchButton();
                await editPage.FilterUserandEditUser(name, reference);
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
