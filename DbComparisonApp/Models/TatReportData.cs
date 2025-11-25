using System;

namespace DbComparisonApp.Models
{
    public class TatReportData : IReportData
    {
        public int? CheckStatus { get; set; }
        public string WO { get; set; }
        public string FinNumber { get; set; } // Mapped from "FIN#"
        public string Fleet { get; set; }
        public string AcCheck { get; set; } // Mapped from "A/C Check"
        public DateTime? ActualStartDateTime { get; set; }
        public decimal? Duration { get; set; }
        public int? AgreedTurnAroundTime { get; set; }
        public decimal? Days { get; set; }
        public decimal? Hours { get; set; }
        public decimal? Minutes { get; set; }
        public long CountEstChanges { get; set; } // Mapped from "Count Est. Changes"
        public string VendorCode { get; set; }
        public string VendorName { get; set; }
        public string Reason { get; set; }
        public bool? IsGain { get; set; }
        public string Responsibility { get; set; }
        public int? ResponsibleId { get; set; }
        public decimal? TotalDays { get; set; }
        public decimal? ExecutableDays { get; set; }
        public decimal? NonExecutableDays { get; set; }
        public int? GANTTurnAroundTime { get; set; }
        public DateTime? ChangeDate { get; set; }
        public string Comment { get; set; }
        public bool? IsDeleted { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }

        public string GetUniqueKey()
        {
            // Unique key based on Work Order and Sequence Number
            return $"{WO}_{CountEstChanges}";
        }
    }
}
