namespace DbComparisonApp.Models;

public class CostAvoidanceReviewData : IReportData
{
    public string WorkOrderNumber { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public decimal TotalSavedCost { get; set; }
    public DateTime? InitialReviewDue { get; set; }
    public DateTime? FinalApprovalDue { get; set; }
    public DateTime? Next5Days { get; set; }
    public DateTime? Today { get; set; }
    public int InitialReviewUpcoming { get; set; }
    public int FinalApprovalUpcoming { get; set; }

    public string GetUniqueKey() => WorkOrderNumber;
}
