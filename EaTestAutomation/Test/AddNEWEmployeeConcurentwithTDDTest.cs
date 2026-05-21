using EAFramework.Reporting;
using EaTestAutomation.Base;
using EaTestAutomation.Models;
using EaTestAutomation.Pages;
using EaTestAutomation.Parallel;
using EaTestAutomation.TestData;
using Microsoft.Playwright;

namespace EaTestAutomation.Test
{
    /// <summary>
    /// Data-driven employee creation. When <c>EnableParallelExecution</c> is true,
    /// opens up to <c>MaxParallelBrowsers</c> Chrome windows at the same time.
    /// </summary>
    public class AddNEWEmployeeConcurentwithTDDTest : BaseTest
    {
        [Fact]
        public async Task AddNEWEmpConcurentwithTDDTest()
        {
            List<NewEmployee> employees = NewEmployeeData.EmployeeData
                .Select(row => (NewEmployee)row[0])
                .ToList();

            if (_testSettings.EnableParallelExecution)
            {
                await ParallelTestOrchestrator.RunEmployeeCasesAsync(
                    _testSettings,
                    nameof(AddNEWEmpConcurentwithTDDTest),
                    employees,
                    (employee, _) => ArtifactDirectoryBuilder.Sanitize(employee.name),
                    ExecuteEmployeeFlowAsync);
            }
            else
            {
                foreach (NewEmployee employee in employees)
                {
                    await ExecuteEmployeeFlowAsync(Page, employee);
                }
            }

            MarkTestPassed(true);
        }

        private async Task ExecuteEmployeeFlowAsync(IPage page, NewEmployee employee)
        {
            var pageObject = new AddNEWEmployeeConcurentwithTDD(page);

            await page.GotoAsync(_testSettings.Applicationurl);

            await pageObject.ClickonEmployeeButton();
            await pageObject.ClickonNewEmployeeButton();
            await pageObject.FillFullname(employee.name);
            await pageObject.FillAge(employee.age);
            await pageObject.FillSalaryInput(employee.salary);
            await pageObject.FillDurationWorkedInput(employee.durationworked);
            await pageObject.SelectDropdownOptionAsync(
                "#Grade",
                employee.EmployeeList.ToString());
            await pageObject.FillEmailInput(employee.email);
            await pageObject.ClickonCreateEmployeeButton();
        }
    }
}
