namespace DbComparisonApp.Helpers;

public static class UpcomingReportsQueryHelper
{
    public static string GetQualityFindingUpcomingQuery(int lookaheadDays, string startDate, string endDate)
    {
        return $@"
SELECT 
    QF.""QualityFindingId"" AS ""FindingId"",
    QF.""WorkOrderNumber"",
    COALESCE(QRC.""ReasonCode"", 'N/A') AS ""Description"",
    COALESCE(QF.""CARNumber"", 'N/A') AS ""Severity"",
    
    -- Milestones before occurred date (using OccurredAt as reference)
    QF.""OccurredAt"" + INTERVAL '7 day' AS ""WeekBeforeDue"",
    QF.""OccurredAt"" + INTERVAL '14 day' AS ""ThreeDaysDue"",
    QF.""OccurredAt"" + INTERVAL '30 day' AS ""FinalDeadline"",
    
    -- Time references
    CURRENT_DATE + INTERVAL '{lookaheadDays} day' AS ""Next14Days"",
    CURRENT_DATE AS ""Today"",
    
    -- Upcoming flags (check if follow-up dates are coming up)
    CASE 
        WHEN QF.""OccurredAt"" + INTERVAL '7 day' 
             BETWEEN CURRENT_DATE AND CURRENT_DATE + INTERVAL '{lookaheadDays} day' 
        THEN 1 
        ELSE 0 
    END AS ""WeekWarningUpcoming"",
    
    CASE 
        WHEN QF.""OccurredAt"" + INTERVAL '14 day' 
             BETWEEN CURRENT_DATE AND CURRENT_DATE + INTERVAL '{lookaheadDays} day' 
        THEN 1 
        ELSE 0 
    END AS ""UrgentWarningUpcoming""
    
FROM ""QualityFinding"" QF
LEFT JOIN ""QualityReasonCode"" QRC ON QF.""QualityReasonCodeId"" = QRC.""id""
WHERE
    QF.""OccurredAt"" >= TO_DATE('{startDate}', 'YYYY-MM-DD')
    AND QF.""OccurredAt"" <= TO_DATE('{endDate}', 'YYYY-MM-DD')
    AND QF.""IsDeleted"" = false
ORDER BY QF.""OccurredAt"" DESC;
";
    }

    public static string GetManpowerPlanningUpcomingQuery(int lookaheadDays, string startDate, string endDate)
    {
        return $@"
SELECT
    WO.""WorkOrderNumber"",
    WO.""AirCraft"" AS ""Aircraft"",
    COALESCE(WO.""VendorName"", 'N/A') AS ""VendorName"",
    COALESCE(WO.""Location"", 'N/A') AS ""Location"",
    
    -- Milestones for manpower planning (using ActualStartDate since it exists)
    WO.""ActualStartDate"" - INTERVAL '30 day' AS ""PlanningStartDue"",
    WO.""ActualStartDate"" - INTERVAL '14 day' AS ""StaffingConfirmDue"",
    WO.""ActualStartDate"" - INTERVAL '7 day' AS ""FinalPrepDue"",
    
    -- Time window
    CURRENT_DATE + INTERVAL '{lookaheadDays} day' AS ""Next10Days"",
    CURRENT_DATE AS ""Today"",
    
    -- Upcoming action flags
    CASE 
        WHEN WO.""ActualStartDate"" - INTERVAL '30 day' 
             BETWEEN CURRENT_DATE AND CURRENT_DATE + INTERVAL '{lookaheadDays} day' 
        THEN 1 
        ELSE 0 
    END AS ""StartPlanningNow"",
    
    CASE 
        WHEN WO.""ActualStartDate"" - INTERVAL '14 day' 
             BETWEEN CURRENT_DATE AND CURRENT_DATE + INTERVAL '{lookaheadDays} day' 
        THEN 1 
        ELSE 0 
    END AS ""ConfirmStaffingNow"",
    
    CASE 
        WHEN WO.""ActualStartDate"" - INTERVAL '7 day' 
             BETWEEN CURRENT_DATE AND CURRENT_DATE + INTERVAL '{lookaheadDays} day' 
        THEN 1 
        ELSE 0 
    END AS ""FinalPrepNow""
    
FROM ""WorkOrder"" WO
WHERE
    WO.""ActualStartDate"" >= TO_DATE('{startDate}', 'YYYY-MM-DD')
    AND WO.""ActualStartDate"" <= TO_DATE('{endDate}', 'YYYY-MM-DD')
    AND WO.""PostStatus"" = 'ACTIVE'
    AND WO.""CheckStatus"" IN (2, 3, 4, 5)
ORDER BY WO.""ActualStartDate"";
";
    }

    public static string GetCostAvoidanceReviewQuery(int lookaheadDays, string startDate, string endDate, decimal minimumSavings)
    {
        return $@"
SELECT
    CA.""WorkOrderNumber"",
    COALESCE(WO.""VendorName"", 'N/A') AS ""VendorName"",
    COALESCE(CA.""TotalSavedCost"", 0) AS ""TotalSavedCost"",
    
    -- Review milestones
    WO.""ActualCompletionDate"" + INTERVAL '7 day' AS ""InitialReviewDue"",
    WO.""ActualCompletionDate"" + INTERVAL '30 day' AS ""FinalApprovalDue"",
    
    -- Time window
    CURRENT_DATE + INTERVAL '{lookaheadDays} day' AS ""Next5Days"",
    CURRENT_DATE AS ""Today"",
    
    -- Upcoming review flags
    CASE 
        WHEN WO.""ActualCompletionDate"" + INTERVAL '7 day' 
             BETWEEN CURRENT_DATE AND CURRENT_DATE + INTERVAL '{lookaheadDays} day' 
        THEN 1 
        ELSE 0 
    END AS ""InitialReviewUpcoming"",
    
    CASE 
        WHEN WO.""ActualCompletionDate"" + INTERVAL '30 day' 
             BETWEEN CURRENT_DATE AND CURRENT_DATE + INTERVAL '{lookaheadDays} day' 
        THEN 1 
        ELSE 0 
    END AS ""FinalApprovalUpcoming""
    
FROM ""OnsiteCostAvoidance"" CA
JOIN ""WorkOrder"" WO ON CA.""WorkOrderNumber"" = WO.""WorkOrderNumber""
WHERE
    WO.""ActualCompletionDate"" IS NOT NULL
    AND WO.""ActualCompletionDate"" >= TO_DATE('{startDate}', 'YYYY-MM-DD')
    AND WO.""ActualCompletionDate"" <= TO_DATE('{endDate}', 'YYYY-MM-DD')
    AND COALESCE(CA.""TotalSavedCost"", 0) >= {minimumSavings}
    AND WO.""PostStatus"" = 'ACTIVE'
ORDER BY WO.""ActualCompletionDate"" DESC;
";
    }
}
