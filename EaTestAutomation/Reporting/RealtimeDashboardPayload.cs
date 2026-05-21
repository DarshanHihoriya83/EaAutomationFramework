namespace EaTestAutomation.Reporting
{
    /// <summary>
    /// JSON payload for <c>DashboardReport/index.html</c> (embedded + API).
    /// </summary>
    public sealed class RealtimeDashboardPayload
    {
        public DateTime UpdatedUtc { get; set; }
        public string DashboardUrl { get; set; } = "";
        public int Total { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public int Unknown { get; set; }
        public int HealingTotal { get; set; }
        public List<DashboardRunRow> Runs { get; set; } = new();
        public List<TimelinePoint> Timeline { get; set; } = new();
    }

    public sealed class DashboardRunRow
    {
        public string RunId { get; set; } = "";
        public string TestName { get; set; } = "";
        public string Status { get; set; } = "Unknown";
        public DateTime? StartedUtc { get; set; }
        public DateTime? FinishedUtc { get; set; }
        public long DurationMs { get; set; }
        public string DurationDisplay { get; set; } = "";
        public string ArtifactFolder { get; set; } = "";
        public int HealingMappingCount { get; set; }
        public DashboardRunLinks Links { get; set; } = new();
    }

    public sealed class TimelinePoint
    {
        public string TestName { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime StartedUtc { get; set; }
        public DateTime FinishedUtc { get; set; }
        public long DurationMs { get; set; }
    }

    public sealed class DashboardRunLinks
    {
        public string? Video { get; set; }
        public string? Trace { get; set; }
        public string? Screenshot { get; set; }
        public string? Logs { get; set; }
        public string? Healing { get; set; }
    }
}
