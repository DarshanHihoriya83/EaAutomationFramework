// Example: FreeGood-style Excel data loading (use the same pattern in every test class).
//
// private const string WorksheetName = "FreeGood";
//
// public static IEnumerable<object[]> LoadFreeGoodsData()
// {
//     foreach (ExcelDataLoader.ExcelDataRow row in ExcelDataLoader.ReadRows(WorksheetName))
//     {
//         string environment = row.Cell(1);
//         string entity = row.Cell(2);
//         string customerAccount = row.Cell(3);
//         // ... row.Cell(12) for each column in your sheet
//
//         string testName = $"FreeGoods_{entity}_R{row.RowIndex}";
//         ExcelTestTracker.TrackTestRow(testName, row.RowIndex, entity);
//
//         yield return new object[]
//         {
//             environment, entity, customerAccount /*, ... */, testName
//         };
//     }
// }
