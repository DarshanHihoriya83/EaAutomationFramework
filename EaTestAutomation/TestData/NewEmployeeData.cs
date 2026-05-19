using EaTestAutomation.Models;
using System.Collections.Generic;

namespace EaTestAutomation.TestData
{
    public class NewEmployeeData
    {
        public static IEnumerable<object[]> EmployeeData =>
            new List<object[]>
            {                
                new object[]
                {
                    new NewEmployee
                    {
                        name = "Raj1",
                        age = 28,
                        salary = 23000,
                        durationworked = 4,
                        email = "raju1@gmail.com",
                        EmployeeList = EmployeeList.Middle
                    }
                },

                new object[]
                {
                    new NewEmployee
                    {
                        name = "Raj2",
                        age = 30,
                        salary = 25000,
                        durationworked = 5,
                        email = "raju2@gmail.com",
                        EmployeeList = EmployeeList.Middle
                    }
                },

                new object[]
                {
                    new NewEmployee
                    {
                        name = "Raj3",
                        age = 32,
                        salary = 27000,
                        durationworked = 6,
                        email = "raju3@gmail.com",
                        EmployeeList = EmployeeList.Senior
                    }
                },

                new object[]
                {
                    new NewEmployee
                    {
                        name = "Raj4",
                        age = 35,
                        salary = 30000,
                        durationworked = 8,
                        email = "raju4@gmail.com",
                        EmployeeList = EmployeeList.Senior
                    }
                }
            };
    }
}