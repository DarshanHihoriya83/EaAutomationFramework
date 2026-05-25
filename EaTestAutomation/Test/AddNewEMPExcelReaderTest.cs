using EaTestAutomation.Base;
using EaTestAutomation.Pages;
using EaTestAutomation.Utilities;
using Xunit;

namespace EaTestAutomation.Test
{
    public class AddNewEMPExcelReaderTest : BaseTest
    {
        private const string WorksheetName = "AddNewEmployee";

        public static IEnumerable<object[]> LoadEmployeeData()
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

                string testName = $"AddEmployee_{name}_R{row.RowIndex}";

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
        [MemberData(nameof(LoadEmployeeData))]
        public async Task AddNewEmployeeTest(
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
