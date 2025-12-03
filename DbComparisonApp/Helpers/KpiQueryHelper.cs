namespace DbComparisonApp.Helpers;

public static class KpiQueryHelper
{
    public static string GetQuery(string startDate, string endDate)
    {
        return $@"
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
        WO.""ActualCompletionDate"" BETWEEN TO_DATE('{startDate}', 'YYYY/MM/DD')
                                      AND TO_DATE('{endDate}', 'YYYY/MM/DD')
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
        QF.""OccurredAt"" BETWEEN TO_DATE('{startDate}', 'YYYY/MM/DD') 
                             AND TO_DATE('{endDate}', 'YYYY/MM/DD')
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
        w.""ActualCompletionDate"" BETWEEN TO_DATE('{startDate}', 'YYYY/MM/DD')
                                     AND TO_DATE('{endDate}', 'YYYY/MM/DD')
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
        EL.""ActualCompletionDate"" BETWEEN TO_DATE('{startDate}', 'YYYY/MM/DD')
                                      AND TO_DATE('{endDate}', 'YYYY/MM/DD')
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
}
