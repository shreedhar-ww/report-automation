namespace DbComparisonApp.Models;

public class WorkOrderData : IReportData
{
    public string WorkOrderNumber { get; set; } = string.Empty;
    public string? PostStatus { get; set; }
    public int? InductionDayOfPeriod { get; set; }
    public int? CheckStatus { get; set; }
    public string? EventType { get; set; }
    public string? AirCraft { get; set; }
    public string? AircraftType { get; set; }
    public string? Location { get; set; }
    public string? WorkOrderDescription { get; set; }
    public DateTime? InductionDate { get; set; }
    public DateTime? EstCompletionDate { get; set; }
    public DateTime? ActualCompletionDate { get; set; }
    public string? VendorName { get; set; }
    public string? SiteManager { get; set; }
    public string? SiteManagerPhone { get; set; }
    public string? AircraftCheckControlRepresentative { get; set; }
    public string? AircraftCheckControlRepresentativePhone { get; set; }
    public int? GANTTurnAroundTime { get; set; }
    public int? AgreedTurnAroundTime { get; set; }
    public int? RevisedTurnAroundTime { get; set; }
    public long? TotalTatChanges { get; set; }
    public string? LatestComment { get; set; }
    public string? LatestReason { get; set; }
    public decimal? TotalTasks { get; set; }
    public decimal? OpenTasks { get; set; }
    public decimal? DayShiftHC { get; set; }
    public decimal? AfternoonShiftHC { get; set; }
    public decimal? NightShiftHC { get; set; }
    public int? RiskLevelId { get; set; }
    public string? RiskLevelName { get; set; }
    public string? RiskDescription { get; set; }
    public string? RiskComment { get; set; }

    public string GetUniqueKey()
    {
        return WorkOrderNumber;
    }
}
