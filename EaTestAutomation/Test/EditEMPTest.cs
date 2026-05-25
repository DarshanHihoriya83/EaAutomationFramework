using EaTestAutomation.Base;
using EaTestAutomation.Pages;
using EaTestAutomation.Utilities;
using Xunit;

namespace EaTestAutomation.Test
{
    public class EditEMPTest : BaseTest
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

                string testName = $"EditEmployee_{name}_R{row.RowIndex}";

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
        public async Task AddEditEmployeeTest(
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

            var _EditEMPPage = new EditEMPPage(Page);

            try
            {
                await Page.GotoAsync(_testSettings.Applicationurl);

                await _EditEMPPage.ClickEmployees();
                await _EditEMPPage.FillSearchInput(name);
                await _EditEMPPage.ClickSearchButton();
                await _EditEMPPage.FilterUserandEditUser(name);
                await _EditEMPPage.EditFullname("Raj");
                await _EditEMPPage.EditEditAge(age);
                await _EditEMPPage.EditSalaryInput(salary);
                await _EditEMPPage.EditDurationwork(durationWorked);
                await _EditEMPPage.EditGradeSelectDropdownOptionAsync("#Grade", grade);
                await _EditEMPPage.EditEmailInput(email);
                await _EditEMPPage.ClickonSaveChangesButton();

                testResult = true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                testResult = false;
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
    }
}
