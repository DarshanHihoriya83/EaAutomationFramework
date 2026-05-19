using ClosedXML.Excel;
using EaTestAutomation.Base;
using EaTestAutomation.Pages;
using EaTestAutomation.Utilities;
using Xunit;

namespace EaTestAutomation.Test
{
    public class AddNewEMPExcelReaderTest : BaseTest
    {
        private const string WORKSHEET_NAME = "AddNewEmployee";

        public static IEnumerable<object[]> LoadEmployeeData()
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

                string testName = $"AddEmployee_{name}";

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
        [MemberData(nameof(LoadEmployeeData))]
        public async Task AddNewEmployeeTest(
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

            var employeePage = new AddNewEMPExcelReaderPage(Page);

            try
            {
                await Page.GotoAsync(_testSettings.Applicationurl);

                await employeePage.ClickonEmployeeButton();

                await employeePage.ClickonNewEmployeeButton();

                await employeePage.FillFullname(name);

                await employeePage.FillAge(age);

                await employeePage.FillSalaryInput(salary);

                await employeePage.FillDurationWorkedInput(durationWorked);

                await employeePage.SelectDropdownOptionAsync("#Grade", grade);

                await employeePage.FillEmailInput(email);

                await employeePage.ClickonCreateEmployeeButton();

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