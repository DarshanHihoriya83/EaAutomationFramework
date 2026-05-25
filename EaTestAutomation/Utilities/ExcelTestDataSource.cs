namespace EaTestAutomation.Utilities
{
    /// <summary>
    /// Builds xUnit <c>MemberData</c> rows from Excel with tracking (NUnit TestCaseData equivalent).
    /// </summary>
    public static class ExcelTestDataSource
    {
        /// <summary>
        /// Loads rows, tracks them for result write-back, and yields test argument arrays.
        /// </summary>
        public static IEnumerable<object[]> Load(
            string worksheetName,
            Func<ExcelDataLoader.ExcelDataRow, string> buildTestName,
            Func<ExcelDataLoader.ExcelDataRow, object[]> buildArguments,
            int keyColumn = 1,
            Func<ExcelDataLoader.ExcelDataRow, string>? buildReference = null)
        {
            foreach (ExcelDataLoader.ExcelDataRow row in ExcelDataLoader.ReadRows(worksheetName, keyColumn))
            {
                string testName = buildTestName(row);
                string reference = buildReference?.Invoke(row) ?? row.Reference;

                ExcelTestTracker.TrackTestRow(testName, row.RowIndex, reference);

                object[] args = buildArguments(row);
                yield return args;
            }
        }
    }
}
