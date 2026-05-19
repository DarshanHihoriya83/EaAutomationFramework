using AutoFixture.Xunit2;
using EaTestAutomation.Base;
using EaTestAutomation.Models;
using EaTestAutomation.Pages;
using Xunit;

namespace EaTestAutomation.Test
{
    public class AddNewEmpAutoTestDataCreationTest : BaseTest
    {
        [Theory]
        [AutoData]
        public async Task AddNewEmployeeAutoTestDataCreationTest(string name, int age, int salary, int durationworked, string email)
        {
            AddNewEmpAutoTestDataCreationPage _AddNewEmpAutoTestDataCreationPage = new AddNewEmpAutoTestDataCreationPage(Page);

            // Prevent invalid negative numbers
            age = Math.Abs(age % 60) + 18;
            salary = Math.Abs(salary % 100000) + 10000;
            durationworked = Math.Abs(durationworked % 20) + 1;

            email = $"test{Guid.NewGuid():N}@gmail.com";

            await Page.GotoAsync(_testSettings.Applicationurl);

            await _AddNewEmpAutoTestDataCreationPage.ClickonEmployeeButton();

            await _AddNewEmpAutoTestDataCreationPage.ClickonNewEmployeeButton();

            await _AddNewEmpAutoTestDataCreationPage.FillFullname(name);

            await _AddNewEmpAutoTestDataCreationPage.FillAge(age);

            await _AddNewEmpAutoTestDataCreationPage.FillSalaryInput(salary);

            await _AddNewEmpAutoTestDataCreationPage.FillDurationWorkedInput(durationworked);

            await _AddNewEmpAutoTestDataCreationPage.SelectDropdownOptionAsync(
                "#Grade", "Middle");

            await _AddNewEmpAutoTestDataCreationPage.FillEmailInput(email);

            await _AddNewEmpAutoTestDataCreationPage.ClickonCreateEmployeeButton();
        }
    }
}