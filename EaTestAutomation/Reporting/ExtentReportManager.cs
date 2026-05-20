using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace EaTestAutomation.Reporting
{
    /// <summary>
    /// Process-wide Extent Spark reporter (single HTML under <c>Artifacts/ExtentReport</c>).
    /// </summary>
    public static class ExtentReportManager
    {
        private static readonly object Gate = new();
        private static ExtentReports? _extent;
        private static readonly string ReportPath = GetReportPath();

        private static string GetReportPath()
        {
            string dir = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Artifacts",
                "ExtentReport");

            Directory.CreateDirectory(dir);

            return Path.Combine(dir, "ExtentDashboard.html");
        }

        public static ExtentReports Instance
        {
            get
            {
                lock (Gate)
                {
                    if (_extent != null)
                    {
                        return _extent;
                    }

                    var spark = new ExtentSparkReporter(ReportPath);
                    _extent = new ExtentReports();
                    _extent.AttachReporter(spark);
                    _extent.AddSystemInfo("CLR", Environment.Version.ToString());
                    _extent.AddSystemInfo("OS", Environment.OSVersion.ToString());
                    _extent.AddSystemInfo("StartedUtc", DateTime.UtcNow.ToString("O"));
                    return _extent;
                }
            }
        }

        public static ExtentTest CreateTest(string name) =>
            Instance.CreateTest(name);

        public static void Flush()
        {
            lock (Gate)
            {
                _extent?.Flush();
            }
        }
    }
}
