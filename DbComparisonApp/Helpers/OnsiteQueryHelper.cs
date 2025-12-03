namespace DbComparisonApp.Helpers;

public static class OnsiteQueryHelper
{
    public static string GetQuery(string startDate, string endDate, string excludeWorkOrderNumber)
    {
        return $@"
SELECT 
    wo.""WorkOrderNumber"",
    CASE wo.""CheckStatus""
        WHEN 1 THEN 'COMPLETED'
        WHEN 2 THEN 'CHECK'
        WHEN 3 THEN 'T-15 to T0'
        WHEN 4 THEN 'T-35 to T-15'
        WHEN 5 THEN 'OPEN'
        ELSE 'UNKNOWN'
    END AS ""CheckStatus"",
    wo.""EventReportType"",
    wo.""AirCraft"",
    wo.""AircraftType"",
    wo.""Location"",
    wo.""Site"",
    wo.""VendorName"",
    wo.""WorkOrderDescription"",
    tat.""ContractualInspection"",
    tat.""ContractualInspectionPercentage"",
    tat.""TurnAroundTimeConfirmedAtFifty"",
    tat.""InspectionDays"",
    tat.""ConfirmedTurnAroundTimeDays"",

    ca.""LabourRequestedNumberOfHours""   AS ""CA_HOURS_REQUESTED"",
    ca.""LabourApprovedNumberOfHours""    AS ""CA_HOURS_APPROVED"",
    ca.""LabourSavedHours""               AS ""CA_HOURS_SAVED"",
    ca.""LabourSavedAmount""              AS ""LABOUR_SAVED"",
    ca.""MaterialRequestedCost""          AS ""CA_MATERIAL_REQUESTED"",
    ca.""MaterialApprovedCost""           AS ""CA_MATERIAL_APPROVED"",
    ca.""MaterialSavedCost""              AS ""MATERIAL_SAVED"",
    ca.""InvoiceSubmittedValue""          AS ""INVOICE_SUBMITTED"",
    ca.""InvoiceApprovedValue""           AS ""INVOICE_APPROVED"",
    ca.""InvoiceSavedValue""              AS ""INVOICE_SAVED"",
    ca.""TotalSavedCost""                 AS ""TOTAL_SAVED"",
	
    (wo.""ActualStartDate"" 
     + (wo.""ActualStartHour"" || ' hours')::interval 
     + (wo.""ActualStartMinute"" || ' minutes')::interval) AS ""ActualStartDateTime"",

    (
        (wo.""ActualStartDate"" 
         + (wo.""ActualStartHour"" || ' hours')::interval
         + (wo.""ActualStartMinute"" || ' minutes')::interval)
        +
        (COALESCE(tat.""AgreedTurnAroundTime""::numeric, wo.""Duration""::numeric) * interval '1 day')
        +
        (
            CASE 
                WHEN tat.""TotalTatAdjustmentDays"" IS NOT NULL THEN
                    (regexp_replace(
                        regexp_replace(
                            regexp_replace(tat.""TotalTatAdjustmentDays"", '(\d+)d', '\1 day', 'g'),
                        '(\d+)h', '\1 hours', 'g'),
                    '(\d+)m', '\1 minutes', 'g'))::interval
                ELSE interval '0'
            END
        )
    ) AS ""RTS_DATE"",

    tat.""BudgetTurnAroundTime"" AS ""BudgetTat"",
    tat.""GANTTurnAroundTime"" AS ""GanttTAT"",
    COALESCE(tat.""AgreedTurnAroundTime""::numeric, 0) AS ""AgreedTurnAroundTime"",
 
    ROUND(
        COALESCE((regexp_match(tat.""CurrentTurnAroundTime"", '(\d+)d'))[1]::numeric, 0)
      + COALESCE((regexp_match(tat.""CurrentTurnAroundTime"", '(\d+)h'))[1]::numeric / 24, 0)
      + COALESCE((regexp_match(tat.""CurrentTurnAroundTime"", '(\d+)m'))[1]::numeric / 1440, 0)
    , 1) AS ""CurrentTAT_inDays"",

    ROUND(
        (
            COALESCE((regexp_match(tat.""CurrentTurnAroundTime"", '(\d+)d'))[1]::numeric, 0)
          + COALESCE((regexp_match(tat.""CurrentTurnAroundTime"", '(\d+)h'))[1]::numeric / 24, 0)
          + COALESCE((regexp_match(tat.""CurrentTurnAroundTime"", '(\d+)m'))[1]::numeric / 1440, 0)
        )  - COALESCE(tat.""BudgetTurnAroundTime""::numeric, 0), 1
    ) AS ""TAT_vs_Budget"",

    ROUND(
        (
            COALESCE((regexp_match(tat.""CurrentTurnAroundTime"", '(\d+)d'))[1]::numeric, 0)
          + COALESCE((regexp_match(tat.""CurrentTurnAroundTime"", '(\d+)h'))[1]::numeric / 24, 0)
          + COALESCE((regexp_match(tat.""CurrentTurnAroundTime"", '(\d+)m'))[1]::numeric / 1440, 0)
        ) - COALESCE(tat.""AgreedTurnAroundTime""::numeric, 0), 1
    ) AS ""TAT_vs_Agreed"",

    ROUND(
        COALESCE((regexp_match(tat.""TotalTatAdjustmentDays"", '(-?\d+)d'))[1]::numeric, 0)
      + COALESCE((regexp_match(tat.""TotalTatAdjustmentDays"", '(-?\d+)h'))[1]::numeric / 24, 0)
      + COALESCE((regexp_match(tat.""TotalTatAdjustmentDays"", '(-?\d+)m'))[1]::numeric / 1440, 0)
    , 1) AS ""TotalTatAdjustment_inDays"",
    ROUND(
        COALESCE((regexp_match(tat.""TotalNonExcusableDays"", '(-?\d+)d'))[1]::numeric, 0)
      + COALESCE((regexp_match(tat.""TotalNonExcusableDays"", '(-?\d+)h'))[1]::numeric / 24, 0)
      + COALESCE((regexp_match(tat.""TotalNonExcusableDays"", '(-?\d+)m'))[1]::numeric / 1440, 0)
    , 1) AS ""TotalNonExcusableDays_inDays"",
    ROUND(
        COALESCE((regexp_match(tat.""TotalExcusableDays"", '(-?\d+)d'))[1]::numeric, 0)
      + COALESCE((regexp_match(tat.""TotalExcusableDays"", '(-?\d+)h'))[1]::numeric / 24, 0)
      + COALESCE((regexp_match(tat.""TotalExcusableDays"", '(-?\d+)m'))[1]::numeric / 1440, 0)
    , 1) AS ""TotalExcusableDays_inDays"",

    t.""RoutineT35"",
    t.""RoutineT35ToT15Added"",
    t.""RoutineT35ToT15Removed"",
    t.""RoutineT15ToEndAdded"",
    t.""RoutineT15ToEndRemoved"",
    (COALESCE(t.""OpenNonRoutine"", 0) 
     + COALESCE(t.""ClosedNonRoutine"", 0) 
     + COALESCE(t.""CancelNonRoutine"", 0)) AS ""TOTAL_NRs"",  
    (COALESCE(t.""VendorOpenNonRoutine"", 0) 
     + COALESCE(t.""VendorClosedNonRoutine"", 0)) AS ""NON_ROUTINES_GENERATED"",

    mp_agg.""TotalDayManHours"",
    mp_agg.""TotalAfternoonManHours"",
    mp_agg.""TotalNightManHours"",
    mp_agg.""OverallTotalManHours""

FROM ""WorkOrder"" wo
LEFT JOIN ""OnSiteTurnAroundTime"" tat 
    ON wo.""WorkOrderNumber"" = tat.""WorkOrderNumber""
LEFT JOIN ""OnsiteCostAvoidance"" ca 
    ON wo.""WorkOrderNumber"" = ca.""WorkOrderNumber""
LEFT JOIN ""OnSiteTaskCard"" t
    ON wo.""WorkOrderNumber"" = t.""WorkOrderNumber""
LEFT JOIN (
    SELECT
        ""WorkOrderNumber"",
        SUM(""DayTotalManHours"")       AS ""TotalDayManHours"",
        SUM(""AfternoonTotalManHours"") AS ""TotalAfternoonManHours"",
        SUM(""NightTotalManHours"")     AS ""TotalNightManHours"",
        SUM(""TotalManHours"")          AS ""OverallTotalManHours""
    FROM ""OnSiteManPower""
    WHERE ""IsDeleted"" = false
    GROUP BY ""WorkOrderNumber""
) mp_agg
    ON wo.""WorkOrderNumber"" = mp_agg.""WorkOrderNumber""   

WHERE
  wo.""ActualCompletionDate"" >= TO_DATE('{startDate}', 'YYYY/MM/DD')
  AND wo.""WorkOrderNumber"" <> '{excludeWorkOrderNumber}'
  AND wo.""ActualCompletionDate"" <= TO_DATE('{endDate}', 'YYYY/MM/DD')
  AND wo.""PostStatus"" = 'ACTIVE'
  AND COALESCE(wo.""EventType"", 'N/A') NOT IN ('OOS','PRK','HML');
";
    }
}
