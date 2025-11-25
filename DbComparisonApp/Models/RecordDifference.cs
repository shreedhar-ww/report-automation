namespace DbComparisonApp.Models;

public class RecordDifference
{
    public string WorkOrderNumber { get; set; } = string.Empty;
    public WorkOrderData Db1Record { get; set; } = new();
    public WorkOrderData Db2Record { get; set; } = new();
    public List<string> DifferingFields { get; set; } = new();
}
