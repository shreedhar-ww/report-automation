namespace DbComparisonApp.Models;

public class QualityFindingUpcomingData : IReportData
{
    public string FindingId { get; set; } = string.Empty;
    public string WorkOrderNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime? WeekBeforeDue { get; set; }
    public DateTime? ThreeDaysDue { get; set; }
    public DateTime? FinalDeadline { get; set; }
    public DateTime? Next14Days { get; set; }
    public DateTime? Today { get; set; }
    public int WeekWarningUpcoming { get; set; }
    public int UrgentWarningUpcoming { get; set; }

    public string GetUniqueKey()
    {
        if (string.IsNullOrWhiteSpace(FindingId))
        {
            // Fallback for empty/null ID to prevent crash
            // Appending Guid to ensure uniqueness since data seems to have duplicates
            return $"{WorkOrderNumber}_{Description}_{Severity}_{WeekBeforeDue}_{Guid.NewGuid()}";
        }
        return FindingId;
    }
}
