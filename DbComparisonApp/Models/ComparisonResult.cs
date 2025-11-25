namespace DbComparisonApp.Models;

public class ComparisonResult<T> where T : IReportData
{
    public List<T> MatchingRecords { get; set; } = new();
    public List<T> OnlyInDb1 { get; set; } = new();
    public List<T> OnlyInDb2 { get; set; } = new();
    public List<RecordDifference<T>> RecordsWithDifferences { get; set; } = new();
}
