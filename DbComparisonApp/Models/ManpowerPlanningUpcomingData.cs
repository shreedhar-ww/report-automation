namespace DbComparisonApp.Models;

public class ManpowerPlanningUpcomingData : IReportData
{
    public string WorkOrderNumber { get; set; } = string.Empty;
    public string Aircraft { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime? PlanningStartDue { get; set; }
    public DateTime? StaffingConfirmDue { get; set; }
    public DateTime? FinalPrepDue { get; set; }
    public DateTime? Next10Days { get; set; }
    public DateTime? Today { get; set; }
    public int StartPlanningNow { get; set; }
    public int ConfirmStaffingNow { get; set; }
    public int FinalPrepNow { get; set; }

    public string GetUniqueKey() => WorkOrderNumber;
}
