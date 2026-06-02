using EAFramework.Utilities;
using EaTestAutomation.Base;
using EaTestAutomation.Pages;
using EaTestAutomation.Utilities;
using Xunit;

namespace EaTestAutomation.Test
{
    /// <summary>
    /// Registration tests from Excel <c>RegistrationEmp</c> using chained locators with automatic AI self-healing.
    /// </summary>
    public class RegisterEmpTest : BaseTest
    {
        private const string WorksheetName = "RegistrationEmp";

        public static IEnumerable<object[]> LoadRegistrationEmpTestData()
        {
            if (!ExcelDataLoader.WorksheetExists(WorksheetName))
            {
                yield break;
            }

            foreach (ExcelDataLoader.ExcelDataRow row in ExcelDataLoader.ReadRows(WorksheetName))
            {
                string username = row.Cell(1);
                string email = row.Cell(2);
                string password = row.Cell(3);
                string confirmPassword = row.Cell(4);
                string reference = string.IsNullOrWhiteSpace(email) ? username : email;

                string testName = $"RegisterEmp_{username}_R{row.RowIndex}";

                ExcelTestTracker.TrackTestRow(testName, row.RowIndex, reference);

                yield return new object[]
                {
                    username,
                    email,
                    password,
                    confirmPassword,
                    testName,
                    reference
                };
            }
        }

        [Theory]
        [MemberData(nameof(LoadRegistrationEmpTestData))]
        public async Task RegistrationEmployeeTest(
            string username,
            string email,
            string password,
            string confirmPassword,
            string testName,
            string reference)
        {
            bool testResult = false;
            string message = "";

            BindArtifactToTestCase(testName);

            var registerPage = new RegisterEmpPage(Page);

            try
            {
                (string uniqueUsername, string uniqueEmail) =
                    UniqueTestDataGenerator.CreateUniqueRegistrationPair(username, email);

                await Page.GotoAsync(_testSettings.Applicationurl);

                await registerPage.ClickonRegistationButton();
                await registerPage.EnterUsername(uniqueUsername);
                await registerPage.EnterEmail(uniqueEmail);
                await registerPage.EnterPassword(password);
                await registerPage.EnterConfirmPassword(confirmPassword);
                await registerPage.ClickonCreateAccountButton();

                if (!await registerPage.WaitForRegisteredSessionAsync())
                {
                    throw new InvalidOperationException(
                        "Registration did not complete — profile link (nav a[title='Manage']) was not found. "
                        + $"Username: {uniqueUsername}, Email: {uniqueEmail}. "
                        + "Check username/email rules or app validation messages.");
                }

                await registerPage.profileButton();
                await registerPage.ClickonLogoutButton();

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
