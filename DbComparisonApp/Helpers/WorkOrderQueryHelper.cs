namespace DbComparisonApp.Helpers;

public static class WorkOrderQueryHelper
{
    public static string BuildQuery(string startDate, string endDate, string workOrderNumber, string excludeWorkOrderNumber, string todayDate)
    {
        return $@"
SELECT 
    wo.""WorkOrderNumber"",
    wo.""PostStatus"",
    wo.""InductionDayOfPeriod"",
    wo.""CheckStatus"",
    wo.""EventType"",
    wo.""AirCraft"",
    wo.""AircraftType"",
    wo.""Location"",
    wo.""WorkOrderDescription"",
    wo.""ScheduleStartDate"" AS InductionDate,
    wo.""ScheduleCompletionDate"" AS EstCompletionDate,
    wo.""ActualCompletionDate"",
    wo.""VendorName"",
    ob.""SiteManager"",
    ob.""SiteManagerPhone"",
    ob.""AircraftCheckControlRepresentative"",
    ob.""AircraftCheckControlRepresentativePhone"",
    tat.""GANTTurnAroundTime"",
    tat.""AgreedTurnAroundTime"",
    tat.""RevisedTurnAroundTime"",
    tc.TotalTatChanges,
    tc.LatestComment,
    tc.LatestReason,
    ts.TotalTasks,
    ts.OpenTasks,
    mp.DayShiftHC,
    mp.AfternoonShiftHC,
    mp.NightShiftHC,
    lr.""RiskLevelId"",
    lr.""RiskLevelName"",
    lr.riskdescription,
    lr.""RiskComment""
FROM ""WorkOrder"" wo
LEFT JOIN ""OnSiteBasic"" ob 
    ON wo.""WorkOrderNumber"" = ob.""WorkOrderNumber""
LEFT JOIN ""OnSiteTurnAroundTime"" tat 
    ON wo.""WorkOrderNumber"" = tat.""WorkOrderNumber""
LEFT JOIN LATERAL (
    SELECT 
        COUNT(*) AS TotalTatChanges,
        (SELECT t.""Comment""
         FROM ""OnSiteTurnAroundTimeAdjustment"" t
         LEFT JOIN ""OnSiteTurnAroundTimeReason"" r
           ON t.""ReasonId"" = r.""id""
         WHERE t.""WorkOrderNumber"" = wo.""WorkOrderNumber""
           AND t.""IsDeleted"" = false
         ORDER BY t.""ChangeDate"" DESC, t.""RecordCreatedAt"" DESC
         LIMIT 1) AS LatestComment,
        (SELECT r.""Reason""
         FROM ""OnSiteTurnAroundTimeAdjustment"" t
         LEFT JOIN ""OnSiteTurnAroundTimeReason"" r
           ON t.""ReasonId"" = r.""id""
         WHERE t.""WorkOrderNumber"" = wo.""WorkOrderNumber""
           AND t.""IsDeleted"" = false
         ORDER BY t.""ChangeDate"" DESC, t.""RecordCreatedAt"" DESC
         LIMIT 1) AS LatestReason
    FROM ""OnSiteTurnAroundTimeAdjustment"" t
    WHERE t.""WorkOrderNumber"" = wo.""WorkOrderNumber""
      AND t.""IsDeleted"" = false
) tc ON true
LEFT JOIN LATERAL (
    SELECT 
        COALESCE(SUM(""GrandTotal""),0) AS TotalTasks,
        COALESCE(SUM(""OpenRoutine"" + ""OpenNonRoutine"" + ""VendorOpenRoutine"" + ""VendorOpenNonRoutine""),0) AS OpenTasks
    FROM ""OnSiteTaskCard""
    WHERE ""WorkOrderNumber"" = wo.""WorkOrderNumber""
) ts ON true
LEFT JOIN LATERAL (
    SELECT 
        COALESCE(SUM(""DayHeadCount""),0) AS DayShiftHC,
        COALESCE(SUM(""AfternoonHeadCount""),0) AS AfternoonShiftHC,
        COALESCE(SUM(""NightHeadCount""),0) AS NightShiftHC
    FROM ""OnSiteManPower""
    WHERE ""WorkOrderNumber"" = wo.""WorkOrderNumber""
      AND ""IsDeleted"" = false
      AND ""WorkDate"" = '{todayDate}'
) mp ON true
LEFT JOIN LATERAL (
    SELECT r.""RiskLevelId"",
           rlr.""RiskLevelName"",
           rlr.""Description"" AS riskdescription,
           r.""RiskComment"",
           r.""RiskDate"",
           r.""ConfirmedTurnAroundTimeDays""
    FROM ""OnSiteTurnAroundTimeRisk"" r
    LEFT JOIN ""OnSiteTurnAroundTimeRiskLevelReference"" rlr
      ON r.""RiskLevelId"" = rlr.""RiskID""
    WHERE r.""WorkOrderNumber"" = wo.""WorkOrderNumber""
      AND r.""IsDeleted"" = false
    ORDER BY r.""RiskDate"" DESC
    LIMIT 1
) lr ON true
WHERE wo.""ScheduleStartDate"" >= '{startDate}'
  AND wo.""ScheduleStartDate"" <= '{endDate}'
  AND wo.""WorkOrderNumber"" <> '{excludeWorkOrderNumber}'
  AND wo.""PostStatus"" = 'ACTIVE' 
  AND wo.""CheckStatus"" = 2
  AND COALESCE(wo.""EventType"", 'N/A') NOT IN ('EMS', 'OOS', 'PRK', 'HML')
  -- AND wo.""WorkOrderNumber"" = '{workOrderNumber}'
ORDER BY wo.""VendorName"";
";
    }
}
