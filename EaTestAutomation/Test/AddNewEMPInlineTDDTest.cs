using EaTestAutomation.Base;
using EaTestAutomation.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EaTestAutomation.Test
{
    public class AddNewEMPInlineTDDTest : BaseTest
    {
        //[Fact]
        [Theory]
        [InlineData("Raj1", 28, 23000, 4, "raju@1gmail.com")]
        [InlineData("Raj2", 28, 23000, 4, "raju@2gmail.com")]
        [InlineData("Raj3", 28, 23000, 4, "raju@3gmail.com")]
        [InlineData("Raj4", 28, 23000, 4, "raju@4gmail.com")]
        public async Task AddNewEmployeeInlineTDDTest(string name, int age,int salary , int durationworked, string email)
        {

            AddNewEmployeeInlineTDDPage _AddNewEmployeeInlineTDDPage = new AddNewEmployeeInlineTDDPage(Page);
            await Page.GotoAsync(_testSettings.Applicationurl);

            await _AddNewEmployeeInlineTDDPage.ClickonEmployeeButton();          
            await _AddNewEmployeeInlineTDDPage.ClickonNewEmployeeButton();          
            await _AddNewEmployeeInlineTDDPage.FillFullname(name);          
            await _AddNewEmployeeInlineTDDPage.FillAge(age);          
            await _AddNewEmployeeInlineTDDPage.FillSalaryInput(salary);          
            await _AddNewEmployeeInlineTDDPage.FillDurationWorkedInput(durationworked);          
            await _AddNewEmployeeInlineTDDPage.SelectDropdownOptionAsync("#Grade", "Middle");          
            await _AddNewEmployeeInlineTDDPage.FillEmailInput(email);          
            await _AddNewEmployeeInlineTDDPage.ClickonCreateEmployeeButton();          
        }



    }
}