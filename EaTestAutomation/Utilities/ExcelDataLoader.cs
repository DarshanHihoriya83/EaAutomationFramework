using ClosedXML.Excel;

namespace EaTestAutomation.Utilities
{
    /// <summary>
    /// Reads data rows from framework Excel test data worksheets.
    /// </summary>
    public static class ExcelDataLoader
    {
        public sealed class EmployeeExcelRow
        {
            public int RowIndex { get; init; }
            public string Name { get; init; } = "";
            public string Age { get; init; } = "";
            public string Salary { get; init; } = "";
            public string DurationWorked { get; init; } = "";
            public string Grade { get; init; } = "";
            public string Email { get; init; } = "";
            public string Reference => string.IsNullOrWhiteSpace(Email) ? Name : Email;
        }

        /// <summary>
        /// Returns all non-empty data rows (skips header row 1).
        /// </summary>
        public static List<EmployeeExcelRow> LoadEmployeeRows(string worksheetName)
        {
            string path = SpecialExtensions.GetExcelPath();
            using var workbook = new XLWorkbook(path);
            var worksheet = workbook.Worksheet(worksheetName);

            var rows = worksheet.RowsUsed().Skip(1).ToList();
            var result = new List<EmployeeExcelRow>();

            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                string name = row.Cell(1).GetString().Trim();

                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                result.Add(new EmployeeExcelRow
                {
                    RowIndex = i + 1,
                    Name = name,
                    Age = row.Cell(2).GetString().Trim(),
                    Salary = row.Cell(3).GetString().Trim(),
                    DurationWorked = row.Cell(4).GetString().Trim(),
                    Grade = row.Cell(5).GetString().Trim(),
                    Email = row.Cell(6).GetString().Trim()
                });
            }

            return result;
        }
    }
}
