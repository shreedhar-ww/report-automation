namespace DbComparisonApp.Helpers;

public static class CardCountQueryHelper
{
    public static string GetQuery()
    {
        return @"
SELECT ""WorkOrderNumber"",
COALESCE(""RoutineT35"", 0) AS ""RoutineT35"", 
COALESCE(""RoutineT35ToT15Added"", 0) AS ""RoutineT35ToT15Added"", 
COALESCE(""RoutineT35ToT15Removed"", 0) AS ""RoutineT35ToT15Removed"", 
COALESCE(""RoutineT15ToEndAdded"", 0) AS ""RoutineT15ToEndAdded"", 
COALESCE(""RoutineT15ToEndRemoved"", 0) AS ""RoutineT15ToEndRemoved"", 
""RecordCreatedAt"", ""RecordModifiedAt""
	FROM public.""OnSiteTaskCard"" 
	  
	ORDER BY ""RecordModifiedAt"" DESC

	;
";
    }
}
