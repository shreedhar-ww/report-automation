namespace DbComparisonApp.Helpers;

public static class CardCountQueryHelper
{
    public static string GetQuery()
    {
        return @"
SELECT 
    o.""WorkOrderNumber"",
    w.""ScheduleStartDate"",
    w.""ActualStartDate"",
    COALESCE(o.""RoutineT35"", 0) AS ""RoutineT35"",
    COALESCE(o.""RoutineT35ToT15Added"", 0) AS ""RoutineT35ToT15Added"",
    COALESCE(o.""RoutineT35ToT15Removed"", 0) AS ""RoutineT35ToT15Removed"",
    COALESCE(o.""RoutineT15ToEndAdded"", 0) AS ""RoutineT15ToEndAdded"",
    COALESCE(o.""RoutineT15ToEndRemoved"", 0) AS ""RoutineT15ToEndRemoved"",
    o.""RecordCreatedAt"",
    o.""RecordModifiedAt""
FROM public.""OnSiteTaskCard"" o
LEFT JOIN public.""WorkOrder"" w
    ON o.""WorkOrderNumber"" = w.""WorkOrderNumber""
ORDER BY o.""RecordModifiedAt"" DESC;
";
    }
}
