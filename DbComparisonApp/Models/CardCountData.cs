using System.ComponentModel.DataAnnotations.Schema;
using DbComparisonApp.Attributes;

namespace DbComparisonApp.Models;

public class CardCountData : IReportData
{
    [Column("WorkOrderNumber")]
    public string WorkOrderNumber { get; set; } = string.Empty;

    [Column("RoutineT35")]
    public int? RoutineT35 { get; set; }

    [Column("RoutineT35ToT15Added")]
    public int? RoutineT35ToT15Added { get; set; }

    [Column("RoutineT35ToT15Removed")]
    public int? RoutineT35ToT15Removed { get; set; }

    [Column("RoutineT15ToEndAdded")]
    public int? RoutineT15ToEndAdded { get; set; }

    [Column("RoutineT15ToEndRemoved")]
    public int? RoutineT15ToEndRemoved { get; set; }

    [Column("RecordCreatedAt")]
    [CompareIgnore]
    public DateTime? RecordCreatedAt { get; set; }

    [Column("RecordModifiedAt")]
    [CompareIgnore]
    public DateTime? RecordModifiedAt { get; set; }

    [Column("ScheduleStartDate")]
    [CompareIgnore]
    public DateTime? ScheduleStartDate { get; set; }

    [Column("ActualStartDate")]
    [CompareIgnore]
    public DateTime? ActualStartDate { get; set; }

    public string GetUniqueKey()
    {
        return WorkOrderNumber;
    }
}
