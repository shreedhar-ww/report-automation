namespace DbComparisonApp.Helpers;

public static class TatQueryHelper
{
    public static string GetQuery(string startDate, string endDate)
    {
        return $@"
SELECT 
    w.""CheckStatus"",
    w.""WorkOrderNumber"" AS ""WO"",
    w.""AirCraft"" AS ""FinNumber"",
    w.""AircraftType"" AS ""Fleet"",
    w.""WorkOrderDescription"" AS ""AcCheck"",

    -- Concatenated Actual Start DateTime
    (
        w.""ActualStartDate""
        + make_interval(hours => w.""ActualStartHour"", mins => w.""ActualStartMinute"")
    ) AS ""ActualStartDateTime"",
    
    w.""Duration"",
    tat.""AgreedTurnAroundTime"",
    a.""Days"",
    a.""Hours"",
    a.""Minutes"",

    -- Adjustment sequence number per WorkOrder
    ROW_NUMBER() OVER (PARTITION BY w.""WorkOrderNumber"" ORDER BY a.""RecordCreatedAt"") AS ""CountEstChanges"",

    w.""VendorCode"",
    w.""VendorName"",
    r.""Reason"",
    r.""IsGain"",
    tr.""Responsibility"",
    tr.""ResponsibleId"",

    -- Total in days with IsGain sign
    ROUND(
        ((a.""Days"") + (a.""Hours"" / 24.0) + (a.""Minutes"" / 1440.0))
        * CASE WHEN r.""IsGain"" = true THEN -1 ELSE 1 END, 
        1
    ) AS ""TotalDays"",

    -- Executable column
    ROUND(
        CASE 
            WHEN tr.""ResponsibleId"" IN (1,2,4) 
                THEN ((a.""Days"") + (a.""Hours"" / 24.0) + (a.""Minutes"" / 1440.0))
                     * CASE WHEN r.""IsGain"" = true THEN -1 ELSE 1 END
            ELSE 0 
        END, 1
    ) AS ""ExecutableDays"",

    -- Non-Executable column
    ROUND(
        CASE 
            WHEN tr.""ResponsibleId"" IN (5,6) 
                THEN ((a.""Days"") + (a.""Hours"" / 24.0) + (a.""Minutes"" / 1440.0))
                     * CASE WHEN r.""IsGain"" = true THEN -1 ELSE 1 END
            ELSE 0 
        END, 1
    ) AS ""NonExecutableDays"",

    -- GANT Turn Around Time
    tat.""GANTTurnAroundTime"",

    a.""ChangeDate"",    
    a.""Comment"",
    a.""IsDeleted"", 
    a.""CreatedBy"",
    a.""ModifiedBy""

FROM public.""WorkOrder"" w
LEFT JOIN public.""OnSiteTurnAroundTimeAdjustment"" a 
       ON a.""WorkOrderNumber"" = w.""WorkOrderNumber""
LEFT JOIN public.""OnSiteTurnAroundTimeReason"" r
       ON a.""ReasonId"" = r.id
LEFT JOIN public.""OnSiteTurnAroundTimeResponsibility"" tr
       ON a.""ResponsiblePartyId"" = tr.id
LEFT JOIN public.""OnSiteTurnAroundTime"" tat 
       ON tat.""WorkOrderNumber"" = w.""WorkOrderNumber""

WHERE   
    w.""CheckStatus"" = 1
    AND a.""IsDeleted"" = false
    AND w.""ActualCompletionDate"" >= TO_DATE('{startDate}', 'YYYY/MM/DD')
    AND w.""ActualCompletionDate"" <= TO_DATE('{endDate}', 'YYYY/MM/DD')
    AND w.""ExternalReference"" <> 'XXXXXXX'
    AND w.""PostStatus"" = 'ACTIVE'
    AND COALESCE(w.""EventReportType"", 'N/A') NOT IN ('OOS','PRK','HML','EMS','AOG','EXIT','STC','RTS')

ORDER BY w.""ExternalReference"", a.""RecordCreatedAt"";
";
    }
}
