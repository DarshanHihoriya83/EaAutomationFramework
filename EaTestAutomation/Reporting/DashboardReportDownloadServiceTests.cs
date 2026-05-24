using System.IO.Compression;

namespace EaTestAutomation.Reporting
{
    public class DashboardReportDownloadServiceTests
    {
        [Fact]
        public void SanitizeRunId_RejectsPathTraversal()
        {
            Assert.Equal("", DashboardReportDownloadService.SanitizeRunId("../secrets"));
            Assert.Equal("", DashboardReportDownloadService.SanitizeRunId("..\\x"));
        }

        [Fact]
        public void SanitizeRunId_AllowsNormalFolderName()
        {
            const string id = "EditEMPTest_AddEdit_20260521_abc12345";
            Assert.Equal(id, DashboardReportDownloadService.SanitizeRunId(id));
        }

        [Fact]
        public void CreateWholeExecutionZip_WritesExpectedEntries()
        {
            using var stream = new MemoryStream();
            DashboardReportDownloadService.CreateWholeExecutionZip(stream);

            using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
            HashSet<string> names = zip.Entries.Select(e => e.FullName).ToHashSet(StringComparer.OrdinalIgnoreCase);

            Assert.Contains("execution-report.json", names);
            Assert.Contains("dashboard-data.json", names);
            Assert.Contains("execution-summary.html", names);
        }

        [Fact]
        public async Task DownloadApi_ReturnsZip_WhenServerRunning()
        {
            MasterDashboardGenerator.Generate();
            DashboardReportServer.EnsureStarted();

            if (!DashboardReportServer.IsRunning)
            {
                return;
            }

            using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };

            using HttpResponseMessage whole = await client.GetAsync(
                DashboardReportServer.BaseUrl + "api/download/execution-report");

            Assert.True(whole.IsSuccessStatusCode);
            Assert.Equal("application/zip", whole.Content.Headers.ContentType?.MediaType);
            Assert.True((await whole.Content.ReadAsByteArrayAsync()).Length > 100);

            RealtimeDashboardPayload payload = MasterDashboardGenerator.RefreshLivePayload();
            string? runId = payload.Runs.FirstOrDefault()?.RunId;

            if (string.IsNullOrWhiteSpace(runId))
            {
                return;
            }

            using HttpResponseMessage single = await client.GetAsync(
                DashboardReportServer.BaseUrl + "api/download/run/" + Uri.EscapeDataString(runId));

            Assert.True(single.IsSuccessStatusCode);
            Assert.Equal("application/zip", single.Content.Headers.ContentType?.MediaType);
        }
    }
}
