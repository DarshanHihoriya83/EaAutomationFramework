using ClosedXML.Excel;
using EaTestAutomation.Base;
using EaTestAutomation.Pages;
using EaTestAutomation.Utilities;
using Xunit;

namespace EaTestAutomation.Test
{
    public class EditEMPTest : BaseTest
    {
        private const string WORKSHEET_NAME = "AddEditEmployee";

        public static IEnumerable<object[]> LoadEditEmployeeData()
        {
            string path = SpecialExtensions.GetExcelPath();

            using var workbook = new XLWorkbook(path);

            var worksheet = workbook.Worksheet(WORKSHEET_NAME);

            var rows = worksheet.RowsUsed().Skip(1).ToList();

            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];

                string name = row.Cell(1).GetString();
                string age = row.Cell(2).GetString();
                string salary = row.Cell(3).GetString();
                string durationWorked = row.Cell(4).GetString();
                string grade = row.Cell(5).GetString();
                string email = row.Cell(6).GetString();

                string testName = $"EditEmployee_{name}";

                ExcelTestTracker.TrackTestRow(testName, i + 1);

                yield return new object[]
                {
                    name,
                    age,
                    salary,
                    durationWorked,
                    grade,
                    email,
                    testName
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
            string testName)
        {
            bool testResult = false;
            string errorMessage = "";

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
                errorMessage = ex.Message;

                testResult = false;

                throw;
            }
            finally
            {
                ExcelTestTracker.WriteTestResult(
                    WORKSHEET_NAME,
                    testName,
                    testResult,
                    errorMessage);
            }
        }
    }
}