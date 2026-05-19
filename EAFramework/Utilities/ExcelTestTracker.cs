using ClosedXML.Excel;
using System.Collections.Generic;

namespace EaTestAutomation.Utilities
{
    public static class ExcelTestTracker
    {
        private static readonly Dictionary<string, int> TestRowMapping = new();

        public static void TrackTestRow(string testName, int rowIndex)
        {
            TestRowMapping[testName] = rowIndex;
        }

        public static void WriteTestResult(
            string worksheetName,
            string testName,
            bool passed,
            string errorMessage = "")
        {
            if (!TestRowMapping.ContainsKey(testName))
                return;

            int rowIndex = TestRowMapping[testName];

            string path = SpecialExtensions.GetExcelPath();

            using var workbook = new XLWorkbook(path);

            var worksheet = workbook.Worksheet(worksheetName);

            int resultColumn = worksheet.LastColumnUsed().ColumnNumber() + 1;

            worksheet.Cell(1, resultColumn).Value = "TestResult";
            worksheet.Cell(1, resultColumn + 1).Value = "ErrorMessage";

            worksheet.Cell(rowIndex + 1, resultColumn).Value = passed ? "PASS" : "FAIL";
            worksheet.Cell(rowIndex + 1, resultColumn + 1).Value = errorMessage;

            workbook.Save();
        }
    }
}