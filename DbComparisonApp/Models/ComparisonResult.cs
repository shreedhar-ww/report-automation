namespace DbComparisonApp.Models;

public class ComparisonResult
{
    public List<WorkOrderData> MatchingRecords { get; set; } = new();
    public List<WorkOrderData> OnlyInDb1 { get; set; } = new();
    public List<WorkOrderData> OnlyInDb2 { get; set; } = new();
    public List<RecordDifference> RecordsWithDifferences { get; set; } = new();
}
