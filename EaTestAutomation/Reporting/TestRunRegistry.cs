using System.Text.Json;

namespace EaTestAutomation.Reporting
{
    public sealed class TestRunRecord
    {
        public string TestName { get; set; } = "";
        public string ArtifactFolder { get; set; } = "";
        public string Status { get; set; } = "Unknown";
        public DateTime FinishedUtc { get; set; }
        public bool HasVideo { get; set; }
        public bool HasTrace { get; set; }
        public bool HasScreenshot { get; set; }
        public int HealingMappingCount { get; set; }
    }

    /// <summary>
    /// Persists test outcomes for the master dashboard charts.
    /// </summary>
    public static class TestRunRegistry
    {
        private static readonly object Gate = new();

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static void Record(TestRunRecord record)
        {
            lock (Gate)
            {
                string path = DashboardPaths.GetTestResultsJsonPath();
                string? dir = Path.GetDirectoryName(path);

                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                List<TestRunRecord> list = Load(path);
                list.Add(record);

                if (list.Count > 500)
                {
                    list = list.Skip(list.Count - 500).ToList();
                }

                File.WriteAllText(path, JsonSerializer.Serialize(list, JsonOptions));
            }
        }

        public static List<TestRunRecord> LoadAll()
        {
            lock (Gate)
            {
                return Load(DashboardPaths.GetTestResultsJsonPath());
            }
        }

        private static List<TestRunRecord> Load(string path)
        {
            if (!File.Exists(path))
            {
                return new List<TestRunRecord>();
            }

            try
            {
                string json = File.ReadAllText(path);

                return JsonSerializer.Deserialize<List<TestRunRecord>>(json, JsonOptions)
                       ?? new List<TestRunRecord>();
            }
            catch
            {
                return new List<TestRunRecord>();
            }
        }
    }
}
