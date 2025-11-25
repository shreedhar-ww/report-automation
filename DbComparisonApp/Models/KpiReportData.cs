using System;

namespace DbComparisonApp.Models
{
    public class KpiReportData : IReportData
    {
        public string VendorName { get; set; }
        public string Station { get; set; }
        public int Year { get; set; }
        public string Month { get; set; }
        public decimal TotalDuration { get; set; }
        public int TotalWorkOrders { get; set; }
        public decimal TotalSavedCost { get; set; }
        public int AvgContractualInspectionPercentage { get; set; }
        public int AIRTURNBACKS { get; set; }
        public int QUALITYESCAPES { get; set; }
        public int CARSISSUED { get; set; }
        public int MRODAMAGEEVENTS { get; set; }
        public int RII_LEVEL_I { get; set; }
        public int RII_LEVEL_II { get; set; }
        public decimal TotalNonExcusableDays { get; set; }
        public decimal TotalExcusableDays { get; set; }
        public int TAT_Adjustment_Count { get; set; }
        public string ACORS_LEVEL { get; set; }

        public string GetUniqueKey()
        {
            return $"{VendorName}_{Station}_{Year}_{Month}";
        }
    }
}
