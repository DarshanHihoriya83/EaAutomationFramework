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
    public class AddNewEmpTest : BaseTest
    {
        [Fact]
        public async Task AddNewEmployeeTest()
        {

            AddNewEmployeePage _AddNewEmployeePage = new AddNewEmployeePage(Page);
            await Page.GotoAsync(_testSettings.Applicationurl);

            await _AddNewEmployeePage.ClickonEmployeeButton();
            await _AddNewEmployeePage.ClickonNewEmployeeButton();
            await _AddNewEmployeePage.FillFullname(name :"yash2");
            await _AddNewEmployeePage.FillAge(age:"22");
            await _AddNewEmployeePage.FillSalaryInput(salary:"22000");
            await _AddNewEmployeePage.FillDurationWorkedInput(durationworked :"3");
            await _AddNewEmployeePage.SelectDropdownOptionAsync("#Grade", "Middle");
            await _AddNewEmployeePage.FillEmailInput(email : "yash2@gmail.com");
            await _AddNewEmployeePage.ClickonCreateEmployeeButton();
        }
    }
}
