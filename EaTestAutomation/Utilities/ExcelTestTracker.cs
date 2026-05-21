using ClosedXML.Excel;

namespace EaTestAutomation.Utilities
{
    /// <summary>
    /// Maps test names to Excel rows and writes Pass/Fail results back to fixed columns.
    /// </summary>
    public static class ExcelTestTracker
    {
        private static readonly object FileLock = new();
        private static readonly Dictionary<string, TrackedRow> TestRowMapping = new(StringComparer.OrdinalIgnoreCase);

        public const string ResultHeader = "TestResult";
        public const string ReferenceHeader = "Reference";
        public const string MessageHeader = "Message";

        public sealed class TrackedRow
        {
            public int RowIndex { get; init; }
            public string Reference { get; init; } = "";
        }

        public static void TrackTestRow(string testName, int rowIndex, string reference = "")
        {
            lock (TestRowMapping)
            {
                TestRowMapping[testName] = new TrackedRow
                {
                    RowIndex = rowIndex,
                    Reference = reference ?? ""
                };
            }
        }

        /// <summary>
        /// Writes TestResult, Reference, and Message for the tracked row (thread-safe).
        /// </summary>
        public static void WriteTestResult(
            string worksheetName,
            string testName,
            bool passed,
            string message = "",
            string? reference = null)
        {
            lock (FileLock)
            {
                if (!TestRowMapping.TryGetValue(testName, out TrackedRow? tracked))
                {
                    return;
                }

                foreach (string path in GetWritableExcelPaths())
                {
                    WriteResultToFile(
                        path,
                        worksheetName,
                        tracked.RowIndex,
                        passed,
                        string.IsNullOrWhiteSpace(reference) ? tracked.Reference : reference,
                        message);
                }
            }
        }

        private static IEnumerable<string> GetWritableExcelPaths()
        {
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                SpecialExtensions.GetExcelPath()
            };

            string binCopy = Path.Combine(AppContext.BaseDirectory, "TestData", "TestData.xlsx");

            if (File.Exists(binCopy))
            {
                paths.Add(Path.GetFullPath(binCopy));
            }

            return paths;
        }

        private static void WriteResultToFile(
            string path,
            string worksheetName,
            int rowIndex,
            bool passed,
            string reference,
            string message)
        {
            using var workbook = new XLWorkbook(path);
            var worksheet = workbook.Worksheet(worksheetName);

            (int resultCol, int referenceCol, int messageCol) =
                EnsureResultColumns(worksheet);

            int excelRow = rowIndex + 1;

            worksheet.Cell(excelRow, resultCol).Value = passed ? "Pass" : "Fail";
            worksheet.Cell(excelRow, referenceCol).Value = reference;
            worksheet.Cell(excelRow, messageCol).Value = Truncate(message, 500);

            workbook.Save();
        }

        private static (int resultCol, int referenceCol, int messageCol) EnsureResultColumns(IXLWorksheet worksheet)
        {
            int lastDataCol = worksheet.LastColumnUsed()?.ColumnNumber() ?? 6;
            int startCol = Math.Max(7, lastDataCol + 1);

            int resultCol = FindOrCreateHeaderColumn(worksheet, ResultHeader, startCol);
            int referenceCol = FindOrCreateHeaderColumn(worksheet, ReferenceHeader, resultCol + 1);
            int messageCol = FindOrCreateHeaderColumn(worksheet, MessageHeader, referenceCol + 1);

            return (resultCol, referenceCol, messageCol);
        }

        private static int FindOrCreateHeaderColumn(IXLWorksheet worksheet, string header, int preferredCol)
        {
            int? existing = FindHeaderColumn(worksheet, header);

            if (existing.HasValue)
            {
                return existing.Value;
            }

            worksheet.Cell(1, preferredCol).Value = header;
            return preferredCol;
        }

        private static int? FindHeaderColumn(IXLWorksheet worksheet, string header)
        {
            int lastCol = worksheet.LastColumnUsed()?.ColumnNumber() ?? 1;

            for (int col = 1; col <= lastCol; col++)
            {
                string cellText = worksheet.Cell(1, col).GetString().Trim();

                if (cellText.Equals(header, StringComparison.OrdinalIgnoreCase))
                {
                    return col;
                }
            }

            return null;
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            {
                return value;
            }

            return value[..maxLength];
        }
    }
}
