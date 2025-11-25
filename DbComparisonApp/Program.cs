using DbComparisonApp.Services;
using DbComparisonApp.Models;
using Microsoft.Extensions.Configuration;

namespace DbComparisonApp;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Database Comparison Tool ===\n");

        try
        {
            // Load configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var db1ConnectionString = configuration.GetConnectionString("Database1") 
                ?? throw new Exception("Database1 connection string not found");
            var db2ConnectionString = configuration.GetConnectionString("Database2") 
                ?? throw new Exception("Database2 connection string not found");

            var startDate = configuration["QueryParameters:StartDate"] ?? "2025-06-27";
            var endDate = configuration["QueryParameters:EndDate"] ?? "2026-02-12";
            var workOrderNumber = configuration["QueryParameters:WorkOrderNumber"] ?? "1107819";
            var excludeWorkOrderNumber = configuration["QueryParameters:ExcludeWorkOrderNumber"] ?? "XXXXXXX";

            // Get today's date for manpower calculation
            var todayDate = DateTime.Now.ToString("yyyy-MM-dd");

            // Build the SQL query with today's date
            var query = BuildQuery(startDate, endDate, workOrderNumber, excludeWorkOrderNumber, todayDate);

            Console.WriteLine("Configuration loaded successfully.");
            Console.WriteLine($"Using date for manpower calculation: {todayDate}\n");

            // Initialize services
            // Initialize services
            var dbService = new DatabaseService();
            var comparisonService = new ComparisonService();
            var excelService = new ExcelReportService();

            // Create Reports directory
            var reportsDir = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            if (!Directory.Exists(reportsDir))
            {
                Directory.CreateDirectory(reportsDir);
                Console.WriteLine($"Created directory: {reportsDir}");
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // --- Work Order Report ---
            Console.WriteLine("--- Starting Work Order Report ---");
            Console.WriteLine("Executing query on Database 1...");
            var db1Data = await dbService.ExecuteQueryGenericAsync<WorkOrderData>(db1ConnectionString, query);

            Console.WriteLine("Executing query on Database 2...");
            var db2Data = await dbService.ExecuteQueryGenericAsync<WorkOrderData>(db2ConnectionString, query);

            // Compare data
            Console.WriteLine("\nComparing Work Order data...");
            var comparisonResult = comparisonService.CompareData(db1Data, db2Data);

            // Generate Excel report
            var outputFileName = $"WorkOrderReport_{timestamp}.xlsx";
            var outputPath = Path.Combine(reportsDir, outputFileName);

            Console.WriteLine("\nGenerating Work Order Excel report...");
            excelService.GenerateReport(comparisonResult, outputPath);
            Console.WriteLine($"Work Order Report saved to: {outputPath}");

            // --- KPI Report ---
            Console.WriteLine("\n--- Starting KPI Report ---");
            var kpiQuery = GetKpiQuery(); 
            
            Console.WriteLine("Executing KPI query on Database 1...");
            var db1KpiData = await dbService.ExecuteQueryGenericAsync<KpiReportData>(db1ConnectionString, kpiQuery);

            Console.WriteLine("Executing KPI query on Database 2...");
            var db2KpiData = await dbService.ExecuteQueryGenericAsync<KpiReportData>(db2ConnectionString, kpiQuery);

            Console.WriteLine("\nComparing KPI data...");
            var kpiComparisonResult = comparisonService.CompareData(db1KpiData, db2KpiData);

            var kpiOutputFileName = $"KpiReport_{timestamp}.xlsx";
            var kpiOutputPath = Path.Combine(reportsDir, kpiOutputFileName);

            Console.WriteLine("\nGenerating KPI Excel report...");
            excelService.GenerateReport(kpiComparisonResult, kpiOutputPath);
            Console.WriteLine($"KPI Report saved to: {kpiOutputPath}");

            // --- TAT Report ---
            Console.WriteLine("\n--- Starting TAT Report ---");
            var tatQuery = GetTatQuery();

            Console.WriteLine("Executing TAT query on Database 1...");
            var db1TatData = await dbService.ExecuteQueryGenericAsync<TatReportData>(db1ConnectionString, tatQuery);

            Console.WriteLine("Executing TAT query on Database 2...");
            var db2TatData = await dbService.ExecuteQueryGenericAsync<TatReportData>(db2ConnectionString, tatQuery);

            Console.WriteLine("\nComparing TAT data...");
            var tatComparisonResult = comparisonService.CompareData(db1TatData, db2TatData);

            var tatOutputFileName = $"TatReport_{timestamp}.xlsx";
            var tatOutputPath = Path.Combine(reportsDir, tatOutputFileName);

            Console.WriteLine("\nGenerating TAT Excel report...");
            excelService.GenerateReport(tatComparisonResult, tatOutputPath);
            Console.WriteLine($"TAT Report saved to: {tatOutputPath}");

            // --- Onsite Report ---
            Console.WriteLine("\n--- Starting Onsite Report ---");
            var onsiteStartDate = configuration["QueryParameters:Onsite:StartDate"] ?? "2025-07-26";
            var onsiteEndDate = configuration["QueryParameters:Onsite:EndDate"] ?? "2026-01-12";
            var onsiteExclude = configuration["QueryParameters:Onsite:ExcludeWorkOrderNumber"] ?? "0";
            var onsiteQuery = GetOnsiteQuery(onsiteStartDate, onsiteEndDate, onsiteExclude);

            Console.WriteLine("Executing Onsite query on Database 1...");
            var db1OnsiteData = await dbService.ExecuteQueryGenericAsync<OnsiteReportData>(db1ConnectionString, onsiteQuery);

            Console.WriteLine("Executing Onsite query on Database 2...");
            var db2OnsiteData = await dbService.ExecuteQueryGenericAsync<OnsiteReportData>(db2ConnectionString, onsiteQuery);

            Console.WriteLine("\nComparing Onsite data...");
            var onsiteComparisonResult = comparisonService.CompareData(db1OnsiteData, db2OnsiteData);

            var onsiteOutputFileName = $"OnsiteReport_{timestamp}.xlsx";
            var onsiteOutputPath = Path.Combine(reportsDir, onsiteOutputFileName);

            Console.WriteLine("\nGenerating Onsite Excel report...");
            excelService.GenerateReport(onsiteComparisonResult, onsiteOutputPath);
            Console.WriteLine($"Onsite Report saved to: {onsiteOutputPath}");

            Console.WriteLine("\n=== Process Completed Successfully ===");
            Console.WriteLine($"Work Order Report: {outputPath}");
            Console.WriteLine($"KPI Report: {kpiOutputPath}");
            Console.WriteLine($"TAT Report: {tatOutputPath}");
            Console.WriteLine($"Onsite Report: {onsiteOutputPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n!!! ERROR !!!");
            Console.WriteLine($"Message: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            Environment.Exit(1);
        }
    }

    static string BuildQuery(string startDate, string endDate, string workOrderNumber, string excludeWorkOrderNumber, string todayDate)
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

    static string GetKpiQuery()
    {
        return @"
WITH WorkOrderSummary AS (
    SELECT
        WO.""VendorName"" AS ""VendorName"",
        WO.""Location"" AS ""Station"",
        EXTRACT(YEAR FROM WO.""ActualCompletionDate"") AS ""Year"",
        TO_CHAR(WO.""ActualCompletionDate"", 'Mon') AS ""Month"",
        SUM(WO.""Duration"") AS ""TotalDuration"",
        COUNT(WO.""WorkOrderNumber"") AS ""TotalWorkOrders"",
        COALESCE(SUM(OCA.""TotalSavedCost""), 0) AS ""TotalSavedCost"",
        ROUND(COALESCE(AVG(OTAT.""ContractualInspectionPercentage""), 0), 0) AS ""AvgContractualInspectionPercentage""
    FROM 
        public.""WorkOrder"" WO
    LEFT JOIN 
        public.""OnsiteCostAvoidance"" OCA
        ON WO.""WorkOrderNumber"" = OCA.""WorkOrderNumber""
    LEFT JOIN 
        public.""OnSiteTurnAroundTime"" OTAT
        ON WO.""WorkOrderNumber"" = OTAT.""WorkOrderNumber""
    WHERE
        WO.""ActualCompletionDate"" BETWEEN TO_DATE('2025/08/01', 'YYYY/MM/DD')
                                      AND TO_DATE('2025/09/30', 'YYYY/MM/DD')
        AND WO.""WorkOrderNumber"" <> 'XXXXXXX'
        AND WO.""PostStatus"" = 'ACTIVE'
        AND COALESCE(WO.""VendorName"", '') NOT IN (' - NO VENDOR - ')
        AND COALESCE(WO.""EventReportType"", 'N/A') NOT IN 
            ('OOS', 'PRK', 'HML', 'EMS', 'AOG', 'EXIT', 'STC', 'RTS')
    GROUP BY
        WO.""VendorName"",
        WO.""Location"",
        EXTRACT(YEAR FROM WO.""ActualCompletionDate""),
        TO_CHAR(WO.""ActualCompletionDate"", 'Mon')
),

QualitySummary AS (
    SELECT    
        WO.""VendorName"" AS ""VendorName"",
        WO.""Location"" AS ""Station"",
        EXTRACT(YEAR FROM QF.""OccurredAt"") AS ""Year"",
        TO_CHAR(QF.""OccurredAt"", 'Mon') AS ""Month"",

        COUNT(QF.""QualityFindingId"") FILTER (WHERE QRC.""ReasonCode"" = 'Air Turn Back') AS ""AIRTURNBACKS"",
        COUNT(QF.""QualityFindingId"") FILTER (WHERE QRC.""ReasonCode"" = 'Quality Escape After RTS') AS ""QUALITYESCAPES"",
        COUNT(QF.""CARNumber"") FILTER (WHERE QF.""CARNumber"" IS NOT NULL AND QF.""CARNumber"" NOT IN ('null', 'N/A', ' N/A')) AS ""CARSISSUED"",
        COUNT(QF.""QualityFindingId"") FILTER (WHERE QRC.""ReasonCode"" = 'Aircraft Damaged by MRO') AS ""MRODAMAGEEVENTS"",
        COUNT(QF.""QualityFindingId"") FILTER (WHERE QRC.""ReasonCode"" = 'Required Inspection Item (RII Level I)') AS ""RII_LEVEL_I"",
        COUNT(QF.""QualityFindingId"") FILTER (WHERE QRC.""ReasonCode"" = 'Required Inspection Item (RII Level II)') AS ""RII_LEVEL_II""
    FROM public.""QualityFinding"" QF
    INNER JOIN public.""WorkOrder"" WO
        ON QF.""WorkOrderNumber"" = WO.""WorkOrderNumber""
    LEFT JOIN public.""QualityReasonCode"" QRC
        ON QF.""QualityReasonCodeId"" = QRC.""id""
    WHERE 
        QF.""OccurredAt"" BETWEEN TO_DATE('2025/08/01', 'YYYY/MM/DD') 
                             AND TO_DATE('2025/09/30', 'YYYY/MM/DD')
        AND WO.""PostStatus"" = 'ACTIVE'
        AND WO.""WorkOrderNumber"" <> 'XXXXXXX'
        AND COALESCE(WO.""VendorName"", '') NOT IN (' - NO VENDOR - ')
        AND COALESCE(WO.""EventReportType"", 'N/A') NOT IN 
            ('OOS', 'PRK', 'HML', 'EMS', 'AOG', 'EXIT', 'STC', 'RTS')
        AND QF.""IsDeleted"" = false
    GROUP BY 
        WO.""VendorName"", 
        WO.""Location"",
        EXTRACT(YEAR FROM QF.""OccurredAt""),
        TO_CHAR(QF.""OccurredAt"", 'Mon')
),

TAT AS (
    SELECT
        w.""VendorName"" AS ""VendorName"",
        w.""Location"" AS ""Station"",
        EXTRACT(YEAR FROM w.""ActualCompletionDate"") AS ""Year"",
        TO_CHAR(w.""ActualCompletionDate"", 'Mon') AS ""Month"",
        ROUND(SUM(COALESCE(d.days,0) + COALESCE(d.hours,0)/24 + COALESCE(d.minutes,0)/1440), 1) AS ""TotalNonExcusableDays"",
        ROUND(SUM(COALESCE(t.days,0) + COALESCE(t.hours,0)/24 + COALESCE(t.minutes,0)/1440), 1) AS ""TotalExcusableDays""
    FROM public.""WorkOrder"" w
    LEFT JOIN LATERAL (
        SELECT
            (REGEXP_MATCHES(o.""TotalNonExcusableDays"", '(-?\d+)d'))[1]::NUMERIC AS days,
            (REGEXP_MATCHES(o.""TotalNonExcusableDays"", '(-?\d+)h'))[1]::NUMERIC AS hours,
            (REGEXP_MATCHES(o.""TotalNonExcusableDays"", '(-?\d+)m'))[1]::NUMERIC AS minutes
        FROM public.""OnSiteTurnAroundTime"" o
        WHERE o.""WorkOrderNumber"" = w.""WorkOrderNumber""
    ) d ON TRUE
    LEFT JOIN LATERAL (
        SELECT
            (REGEXP_MATCHES(o.""TotalExcusableDays"", '(-?\d+)d'))[1]::NUMERIC AS days,
            (REGEXP_MATCHES(o.""TotalExcusableDays"", '(-?\d+)h'))[1]::NUMERIC AS hours,
            (REGEXP_MATCHES(o.""TotalExcusableDays"", '(-?\d+)m'))[1]::NUMERIC AS minutes
        FROM public.""OnSiteTurnAroundTime"" o
        WHERE o.""WorkOrderNumber"" = w.""WorkOrderNumber""
    ) t ON TRUE
    WHERE
        w.""ActualCompletionDate"" BETWEEN TO_DATE('2025/08/01', 'YYYY/MM/DD')
                                     AND TO_DATE('2025/09/30', 'YYYY/MM/DD')
        AND w.""WorkOrderNumber"" <> 'XXXXXXX'
        AND w.""PostStatus"" = 'ACTIVE'
        AND COALESCE(w.""VendorName"", '') NOT IN (' - NO VENDOR - ')
        AND COALESCE(w.""EventReportType"", 'N/A') NOT IN 
            ('OOS', 'PRK', 'HML', 'EMS', 'AOG', 'EXIT', 'STC', 'RTS')
    GROUP BY
        w.""VendorName"", w.""Location"",
        EXTRACT(YEAR FROM w.""ActualCompletionDate""), TO_CHAR(w.""ActualCompletionDate"", 'Mon')
),

TAT_Adjustments AS (
    SELECT
        EL.""VendorName"" AS ""VendorName"",
        EL.""Location"" AS ""Station"",
        EXTRACT(YEAR FROM EL.""ActualCompletionDate"") AS ""Year"",
        TO_CHAR(EL.""ActualCompletionDate"", 'Mon') AS ""Month"",
        COUNT(
            CASE 
                WHEN adj.""IsDeleted"" = false 
                 AND adj.""ChangeDate"" >= EL.""ActualCompletionDate"" - INTERVAL '2 days'
                 AND rea.""IsGain"" = false
                THEN adj.""WorkOrderNumber""
            END
        ) AS ""TAT_Adjustment_Count""
    FROM 
        public.""WorkOrder"" AS EL
    LEFT JOIN public.""OnSiteTurnAroundTimeAdjustment"" AS adj
        ON EL.""WorkOrderNumber"" = adj.""WorkOrderNumber""
    LEFT JOIN public.""OnSiteTurnAroundTimeReason"" AS rea
        ON adj.""ReasonId"" = rea.""id""
    WHERE 
        EL.""ActualCompletionDate"" BETWEEN TO_DATE('2025/08/01', 'YYYY/MM/DD')
                                      AND TO_DATE('2025/09/30', 'YYYY/MM/DD')
        AND EL.""WorkOrderNumber"" <> 'XXXXXXX'
        AND COALESCE(EL.""VendorName"", '') NOT IN (' - NO VENDOR - ')
        AND COALESCE(EL.""EventReportType"", 'N/A') NOT IN 
            ('OOS', 'PRK', 'HML', 'EMS', 'AOG', 'EXIT', 'STC', 'RTS')
    GROUP BY
        EL.""VendorName"",
        EL.""Location"",
        EXTRACT(YEAR FROM EL.""ActualCompletionDate""),
        TO_CHAR(EL.""ActualCompletionDate"", 'Mon')
),

LatestKpiPackage AS (
    SELECT DISTINCT ON (""Vendor"", ""Location"")
        ""Vendor"",
        ""Location"" AS ""Station"",
        ""Level"",
        ""ChangeDate""
    FROM public.""KpiPackage""
    WHERE ""ChangeDate"" IS NOT NULL
    ORDER BY ""Vendor"", ""Location"", ""ChangeDate"" DESC
)

SELECT 
    COALESCE(WS.""VendorName"", QS.""VendorName"", T.""VendorName"", TA.""VendorName"") AS ""VendorName"",
    COALESCE(WS.""Station"", QS.""Station"", T.""Station"", TA.""Station"") AS ""Station"",
    COALESCE(WS.""Year"", QS.""Year"", T.""Year"", TA.""Year"") AS ""Year"",
    COALESCE(WS.""Month"", QS.""Month"", T.""Month"", TA.""Month"") AS ""Month"",

    WS.""TotalDuration"",
    WS.""TotalWorkOrders"",

    LKP.""Level"" AS ""ACORS_LEVEL"",
    COALESCE(QS.""AIRTURNBACKS"", 0) AS ""AIRTURNBACKS"",
    COALESCE(QS.""QUALITYESCAPES"", 0) AS ""QUALITYESCAPES"",
    COALESCE(QS.""CARSISSUED"", 0) AS ""CARSISSUED"",
    COALESCE(QS.""MRODAMAGEEVENTS"", 0) AS ""MRODAMAGEEVENTS"",
    COALESCE(QS.""RII_LEVEL_I"", 0) AS ""RII_LEVEL_I"",
    COALESCE(QS.""RII_LEVEL_II"", 0) AS ""RII_LEVEL_II"",

    COALESCE(T.""TotalNonExcusableDays"", 0) AS ""TotalNonExcusableDays"",
    COALESCE(T.""TotalExcusableDays"", 0) AS ""TotalExcusableDays"",
    COALESCE(TA.""TAT_Adjustment_Count"", 0) AS ""TAT_Adjustment_Count"",
    WS.""TotalSavedCost"",
    WS.""AvgContractualInspectionPercentage""
FROM WorkOrderSummary WS
FULL OUTER JOIN QualitySummary QS
   ON WS.""VendorName"" = QS.""VendorName""
  AND WS.""Station"" = QS.""Station""
  AND WS.""Year"" = QS.""Year""
  AND WS.""Month"" = QS.""Month""
FULL OUTER JOIN TAT T
   ON COALESCE(WS.""VendorName"", QS.""VendorName"") = T.""VendorName""
  AND COALESCE(WS.""Station"", QS.""Station"") = T.""Station""
  AND COALESCE(WS.""Year"", QS.""Year"") = T.""Year""
  AND COALESCE(WS.""Month"", QS.""Month"") = T.""Month""
FULL OUTER JOIN TAT_Adjustments TA
   ON COALESCE(WS.""VendorName"", QS.""VendorName"", T.""VendorName"") = TA.""VendorName""
  AND COALESCE(WS.""Station"", QS.""Station"", T.""Station"") = TA.""Station""
  AND COALESCE(WS.""Year"", QS.""Year"", T.""Year"") = TA.""Year""
  AND COALESCE(WS.""Month"", QS.""Month"", T.""Month"") = TA.""Month""
LEFT JOIN LatestKpiPackage LKP
   ON LKP.""Vendor"" = COALESCE(WS.""VendorName"", QS.""VendorName"", T.""VendorName"", TA.""VendorName"")
  AND LKP.""Station"" = COALESCE(WS.""Station"", QS.""Station"", T.""Station"", TA.""Station"")
ORDER BY 
    ""VendorName"",
    ""Station"",
    ""Year"";
";
    }

    static string GetTatQuery()
    {
        return @"
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
    w.""VendorName"",  -- Using VendorName directly from WorkOrder
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
    AND w.""ActualCompletionDate"" >= TO_DATE('2025/07/04', 'YYYY/MM/DD')
    AND w.""ActualCompletionDate"" <= TO_DATE('2025/12/20', 'YYYY/MM/DD')
    AND w.""ExternalReference"" <> 'XXXXXXX'
    AND w.""PostStatus"" = 'ACTIVE'
    AND COALESCE(w.""EventReportType"", 'N/A') NOT IN ('OOS','PRK','HML','EMS','AOG','EXIT','STC','RTS')

ORDER BY w.""ExternalReference"", a.""RecordCreatedAt"";
";
    }

    static string GetOnsiteQuery(string startDate, string endDate, string excludeWorkOrderNumber)
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
