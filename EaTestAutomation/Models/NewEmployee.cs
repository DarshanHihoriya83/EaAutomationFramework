using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EaTestAutomation.Models
{
    public class NewEmployee
    {
        public string name { get; set; }
        public int age { get; set; }
        public int salary { get; set; }
        public int durationworked { get; set; }
        public string email { get; set; }
        public EmployeeList EmployeeList { get; set; }

    }

    public enum EmployeeList
    {
        Junior,
        Middle,
        Senior
    }
}


