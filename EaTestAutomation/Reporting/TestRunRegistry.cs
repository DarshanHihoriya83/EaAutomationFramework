using System.Text.Json;

namespace EaTestAutomation.Reporting
{
    public sealed class TestRunRecord
    {
        public string RunId { get; set; } = "";
        public string TestName { get; set; } = "";
        public string ArtifactFolder { get; set; } = "";
        public string Status { get; set; } = "Unknown";
        public DateTime StartedUtc { get; set; }
        public DateTime FinishedUtc { get; set; }
        public long DurationMs { get; set; }
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
                if (string.IsNullOrWhiteSpace(record.RunId))
                {
                    record.RunId = record.ArtifactFolder;
                }

                foreach (string path in DashboardPaths.GetAllTestResultsJsonPaths())
                {
                    AppendToFile(path, record);
                }
            }
        }

        public static List<TestRunRecord> LoadAll()
        {
            lock (Gate)
            {
                var merged = new Dictionary<string, TestRunRecord>(StringComparer.OrdinalIgnoreCase);

                foreach (string path in DashboardPaths.GetAllTestResultsJsonPaths())
                {
                    foreach (TestRunRecord row in Load(path))
                    {
                        string key = string.IsNullOrWhiteSpace(row.ArtifactFolder)
                            ? row.RunId
                            : row.ArtifactFolder;

                        if (!merged.TryGetValue(key, out TestRunRecord? existing)
                            || row.FinishedUtc >= existing.FinishedUtc)
                        {
                            merged[key] = row;
                        }
                    }
                }

                return merged.Values.ToList();
            }
        }

        public static bool RemoveByArtifactFolder(string resultsPath, string artifactFolder)
        {
            lock (Gate)
            {
                List<TestRunRecord> list = Load(resultsPath);
                int before = list.Count;

                list.RemoveAll(r =>
                    string.Equals(r.ArtifactFolder, artifactFolder, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(r.RunId, artifactFolder, StringComparison.OrdinalIgnoreCase));

                if (list.Count == before)
                {
                    return false;
                }

                string? dir = Path.GetDirectoryName(resultsPath);

                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllText(resultsPath, JsonSerializer.Serialize(list, JsonOptions));
                return true;
            }
        }

        private static void AppendToFile(string path, TestRunRecord record)
        {
            string? dir = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            List<TestRunRecord> list = Load(path);

            list.RemoveAll(r =>
                string.Equals(r.ArtifactFolder, record.ArtifactFolder, StringComparison.OrdinalIgnoreCase));

            list.Add(record);

            if (list.Count > 500)
            {
                list = list.OrderByDescending(r => r.FinishedUtc).Take(500).ToList();
            }

            File.WriteAllText(path, JsonSerializer.Serialize(list, JsonOptions));
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
