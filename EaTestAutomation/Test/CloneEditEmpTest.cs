//using DocumentFormat.OpenXml.Spreadsheet;
//using EAFramework.Config;
//using EaTestAutomation.Base;
//using EaTestAutomation.Pages;
//using EaTestAutomation.Utilities;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace EaTestAutomation.Test
//{
//    public class CloneEditEmpTest :BaseTest
//    {
//        private const string WorksheetName = "AddEditEmployee";

//        public static IEnumerable<object[]> LoadEditEmployeeData()
//        {
//            foreach (ExcelDataLoader.EmployeeExcelRow row in ExcelDataLoader.LoadEmployeeRows(WorksheetName))
//            {
//                string testName = $"EditEmployee_{row.Name}_R{row.RowIndex}";

//                ExcelTestTracker.TrackTestRow(testName, row.RowIndex, row.Reference);

//                yield return new object[]
//                {
//                    row.Name,
//                    row.Age,
//                    row.Salary,
//                    row.DurationWorked,
//                    row.Grade,
//                    row.Email,
//                    testName,
//                    row.Reference
//                };
//            }
//        }

//        [Theory]
//        [MemberData(nameof(LoadEditEmployeeData))]
//        public async Task AddEditEmployeeTest(
//            string name,
//            string age,
//            string salary,
//            string durationWorked,
//            string grade,
//            string email,
//            string testName,
//            string reference)
//        {
//            bool testResult = false;
//            string message = "";

//            var _CloneEditEmpPage = new CloneEditEmpPage(Page);

//            try
//            {
//                await Page.GotoAsync(_testSettings.Applicationurl);

//                await _CloneEditEmpPage.ClickEmployees();
//                await _CloneEditEmpPage.FillSearchInput(name);
//                await _CloneEditEmpPage.ClickSearchButton();
//                await _CloneEditEmpPage.FilterUserandEditUser(name);
//                await _CloneEditEmpPage.EditFullname("Raj");
//                await _CloneEditEmpPage.EditEditAge(age);
//                await _CloneEditEmpPage.EditSalaryInput(salary);
//                await _CloneEditEmpPage.EditDurationwork(durationWorked);
//                await _CloneEditEmpPage.EditGradeSelectDropdownOptionAsync("#Grade", grade);
//                await _CloneEditEmpPage.EditEmailInput(email);
//                await _CloneEditEmpPage.ClickonSaveChangesButton();


//                testResult = true;
//            }
//            catch (Exception ex)
//            {
//                message = ex.Message;
//                testResult = false;
//                throw;
//            }
//            finally
//            {
//                MarkTestPassed(testResult);

//                ExcelTestTracker.WriteTestResult(
//                    WorksheetName,
//                    testName,
//                    testResult,
//                    message,
//                    reference);
//            }
//        }
//    }
//}


using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using EAFramework.Config;
using EaTestAutomation.Base;
using EaTestAutomation.Pages;
using EaTestAutomation.Utilities;

//Here is the complete updated test file code that reads:

//* `Name` from `"AddNewEmployee"` sheet
//* Remaining fields from `"AddEditEmployee"` sheet

//```csharp
using DocumentFormat.OpenXml.Spreadsheet;
using EAFramework.Config;
using EaTestAutomation.Base;
using EaTestAutomation.Pages;
using EaTestAutomation.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EaTestAutomation.Test
{
    public class CloneEditEmpTest : BaseTest
    {
        // Sheet Names
        private const string AddNewEmployeeSheet = "AddNewEmployee";
        private const string AddEditEmployeeSheet = "AddEditEmployee";

        public static IEnumerable<object[]> LoadEditEmployeeData()
        {
            var addNewEmployeeRows = ExcelDataLoader.ReadRows(AddNewEmployeeSheet).ToList();
            var addEditEmployeeRows = ExcelDataLoader.ReadRows(AddEditEmployeeSheet).ToList();

            for (int i = 0; i < Math.Min(addNewEmployeeRows.Count, addEditEmployeeRows.Count); i++)
            {
                ExcelDataLoader.ExcelDataRow addNewRow = addNewEmployeeRows[i];
                ExcelDataLoader.ExcelDataRow editRow = addEditEmployeeRows[i];

                string name = addNewRow.Cell(1);
                string age = editRow.Cell(2);
                string salary = editRow.Cell(3);
                string durationWorked = editRow.Cell(4);
                string grade = editRow.Cell(5);
                string email = editRow.Cell(6);
                string reference = editRow.Reference;

                string testName = $"EditEmployee_{name}_R{editRow.RowIndex}";

                ExcelTestTracker.TrackTestRow(testName, editRow.RowIndex, reference);

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
        [MemberData(nameof(LoadEditEmployeeData))]
        public async Task AddEditEmployeeTest(
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

            var cloneEditEmpPage = new CloneEditEmpPage(Page);

            try
            {
                // Navigate Application
                await Page.GotoAsync(_testSettings.Applicationurl);

                // Employee Navigation
                await cloneEditEmpPage.ClickEmployees();

                // Search Employee
                await cloneEditEmpPage.FillSearchInput(name);
                await cloneEditEmpPage.ClickSearchButton();

                // Filter and Edit User
                await cloneEditEmpPage.FilterUserandEditUser(name);

                // Edit Employee Details
                await cloneEditEmpPage.EditFullname("Raj");
                await cloneEditEmpPage.EditEditAge(age);
                await cloneEditEmpPage.EditSalaryInput(salary);
                await cloneEditEmpPage.EditDurationwork(durationWorked);
                await cloneEditEmpPage.EditGradeSelectDropdownOptionAsync("#Grade", grade);
                await cloneEditEmpPage.EditEmailInput(email);

                // Save Changes
                await cloneEditEmpPage.ClickonSaveChangesButton();

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
                // Mark Test Status
                MarkTestPassed(testResult);

                // Write Result in Excel
                ExcelTestTracker.WriteTestResult(
                    AddEditEmployeeSheet,
                    testName,
                    testResult,
                    message,
                    reference);
            }
        }
    }
}

