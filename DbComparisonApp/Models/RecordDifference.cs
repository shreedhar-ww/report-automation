namespace DbComparisonApp.Models;

public class RecordDifference<T> where T : IReportData
{
    public string Key { get; set; } = string.Empty;
    public T Db1Record { get; set; } = default!;
    public T Db2Record { get; set; } = default!;
    public List<string> DifferingFields { get; set; } = new();
}
