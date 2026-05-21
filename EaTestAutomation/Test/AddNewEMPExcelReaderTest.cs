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
            foreach (ExcelDataLoader.EmployeeExcelRow row in ExcelDataLoader.LoadEmployeeRows(WorksheetName))
            {
                string testName = $"AddEmployee_{row.Name}_R{row.RowIndex}";

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