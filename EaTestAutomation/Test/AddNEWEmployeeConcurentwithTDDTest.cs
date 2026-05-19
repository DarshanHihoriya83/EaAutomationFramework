using AutoFixture.Xunit2;
using EaTestAutomation.Base;
using EaTestAutomation.Models;
using EaTestAutomation.Pages;
using EaTestAutomation.TestData;
using Xunit;

namespace EaTestAutomation.Test
{
    public class AddNEWEmployeeConcurentwithTDDTest : BaseTest
    {
        [Theory]
        [MemberData(nameof(NewEmployeeData.EmployeeData),
         MemberType = typeof(NewEmployeeData))]
        public async Task AddNEWEmpConcurentwithTDDTest(NewEmployee employee)
        {
            AddNEWEmployeeConcurentwithTDD _AddNEWEmployeeConcurentwithTDD =  new AddNEWEmployeeConcurentwithTDD(Page);

            await Page.GotoAsync(_testSettings.Applicationurl);

            await _AddNEWEmployeeConcurentwithTDD.ClickonEmployeeButton();

            await _AddNEWEmployeeConcurentwithTDD.ClickonNewEmployeeButton();

            await _AddNEWEmployeeConcurentwithTDD.FillFullname(employee.name);

            await _AddNEWEmployeeConcurentwithTDD.FillAge(employee.age);

            await _AddNEWEmployeeConcurentwithTDD.FillSalaryInput(employee.salary);

            await _AddNEWEmployeeConcurentwithTDD.FillDurationWorkedInput(employee.durationworked);

            await _AddNEWEmployeeConcurentwithTDD.SelectDropdownOptionAsync(
                "#Grade",
                employee.EmployeeList.ToString());

            await _AddNEWEmployeeConcurentwithTDD.FillEmailInput(employee.email);

            await _AddNEWEmployeeConcurentwithTDD.ClickonCreateEmployeeButton();
        }
    }
}