using ClosedXML.Excel;
using EaTestAutomation.Base;
using EaTestAutomation.Pages;
using EaTestAutomation.Utilities;
using Xunit;

namespace EaTestAutomation.Test
{
    /// <summary>
    /// Edit Employee test using AI self-healing page object.
    /// </summary>
    public class AISelfhealingEditEMPTTest : BaseTest
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

                string testName = $"AISelfHeal_EditEmployee_{name}";

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
        public async Task AddEditEmployeeWithSelfHealingTest(
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

            var editPage = new EditEMPTSelfHealingPage(Page);

            try
            {
                await Page.GotoAsync(
                    $"{_testSettings.Applicationurl.TrimEnd('/')}/Employee");

                await editPage.ClickEmployees();
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
