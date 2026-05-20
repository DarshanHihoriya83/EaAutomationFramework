using ClosedXML.Excel;
using EAFramework.Reporting;
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
            bool testResult;
            string errorMessage;

            BindArtifactToTestCase(testName);

            var editPage = new EditEMPTSelfHealingPage(Page);

            Exception? failure = await Record.ExceptionAsync(async () =>
            {
                await editPage.NavigateAsync(
                    $"{_testSettings.Applicationurl.TrimEnd('/')}/Employee");

                await editPage.ClickEmployees();

                // Intentionally use a broken selector once so the engine persists a mapping in FailedLocatorStore.json.
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
            });

            testResult = failure is null;
            errorMessage = failure?.Message ?? string.Empty;

            TestArtifactScope.MarkPassed(testResult);

            ExcelTestTracker.WriteTestResult(
                WORKSHEET_NAME,
                testName,
                testResult,
                errorMessage);

            if (failure is not null)
            {
                throw failure;
            }
        }
    }
}
