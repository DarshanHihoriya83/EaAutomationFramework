using ClosedXML.Excel;

namespace EaTestAutomation.Utilities
{
    /// <summary>
    /// Reads Excel worksheets row-by-row with explicit column access (same pattern as FreeGoodTest).
    /// </summary>
    public static class ExcelDataLoader
    {
        /// <summary>
        /// One data row from a worksheet (header row skipped). Use <see cref="Cell"/> for column values.
        /// </summary>
        public sealed class ExcelDataRow
        {
            private readonly Dictionary<int, string> _cells;

            internal ExcelDataRow(IXLRow row, int rowIndex)
            {
                RowIndex = rowIndex;
                _cells = new Dictionary<int, string>();

                int lastColumn = row.LastCellUsed()?.Address.ColumnNumber ?? 1;

                for (int column = 1; column <= lastColumn; column++)
                {
                    _cells[column] = row.Cell(column).GetString().Trim();
                }
            }

            /// <summary>1-based data row index used by <see cref="ExcelTestTracker"/>.</summary>
            public int RowIndex { get; }

            /// <summary>Gets trimmed cell text for a 1-based column index.</summary>
            public string Cell(int columnNumber) =>
                _cells.TryGetValue(columnNumber, out string? value) ? value : string.Empty;

            public string Reference =>
                string.IsNullOrWhiteSpace(Cell(6)) ? Cell(1) : Cell(6);
        }

        public static bool WorksheetExists(string worksheetName)
        {
            string path = SpecialExtensions.GetExcelPath();

            using var workbook = new XLWorkbook(path);

            return workbook.Worksheets.Contains(worksheetName);
        }

        /// <summary>
        /// Opens <c>TestData.xlsx</c> and yields non-empty data rows from the named worksheet.
        /// </summary>
        /// <param name="worksheetName">Worksheet tab name.</param>
        /// <param name="keyColumn">First column to check; blank rows are skipped (default: column 1).</param>
        public static IEnumerable<ExcelDataRow> ReadRows(
            string worksheetName,
            int keyColumn = 1)
        {
            string path = SpecialExtensions.GetExcelPath();

            using var workbook = new XLWorkbook(path);
            var worksheet = workbook.Worksheet(worksheetName);
            var rows = worksheet.RowsUsed().Skip(1).ToList();

            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];

                if (string.IsNullOrWhiteSpace(row.Cell(keyColumn).GetString().Trim()))
                {
                    continue;
                }

                yield return new ExcelDataRow(row, i + 1);
            }
        }
    }
}
