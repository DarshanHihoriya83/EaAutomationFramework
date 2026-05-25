using EaTestAutomation.Base;
using EaTestAutomation.Pages;
using EaTestAutomation.Utilities;
using Xunit;

namespace EaTestAutomation.Test
{
    public class RegisterEmpTest : BaseTest
    {
        private const string WorksheetName = "RegistrationEmp";

        public static IEnumerable<object[]> LoadRegistrationEmpTestData()
        {
            foreach (ExcelDataLoader.ExcelDataRow row in ExcelDataLoader.ReadRows(WorksheetName))
            {
                string username = row.Cell(1);
                string email = row.Cell(2);
                string password = row.Cell(3);
                string Confirmpassword = row.Cell(4);
                string reference = row.Reference;


                string testName = $"EditEmployee_{username}_R{row.RowIndex}";

                ExcelTestTracker.TrackTestRow(testName, row.RowIndex, reference);

                yield return new object[]
                {
                    username,email,password,Confirmpassword,testName,reference
                };
            }
        }

        [Theory]
        [MemberData(nameof(LoadRegistrationEmpTestData))]
        public async Task RegistrationEmployeeTest(
            string username, string email, string password, string Confirmpassword,string testName,string reference)
        {
            bool testResult = false;
            string message = "";

            var _RegisterEmpPage = new RegisterEmpPage(Page);

            try
            {
                await Page.GotoAsync(_testSettings.Applicationurl);

                await _RegisterEmpPage.ClickonRegistationButton();
                await _RegisterEmpPage.EnterUsername(username);
                await _RegisterEmpPage.EnterEmail(email);
                await _RegisterEmpPage.EnterPassword(password);
                await _RegisterEmpPage.EnterConfirmPassword(Confirmpassword);
                await _RegisterEmpPage.ClickonCreateAccountButton();            
                await _RegisterEmpPage.profileButton();
                await _RegisterEmpPage.ClickonLogoutButton();
                
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
