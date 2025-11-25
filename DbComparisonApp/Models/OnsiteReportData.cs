using System;

namespace DbComparisonApp.Models
{
    public class OnsiteReportData : IReportData
    {
        public string WorkOrderNumber { get; set; }
        public string CheckStatus { get; set; }
        public string EventReportType { get; set; }
        public string AirCraft { get; set; }
        public string AircraftType { get; set; }
        public string Location { get; set; }
        public string Site { get; set; }
        public string VendorName { get; set; }
        public string WorkOrderDescription { get; set; }
        public string ContractualInspection { get; set; }
        public decimal? ContractualInspectionPercentage { get; set; }
        public decimal? TurnAroundTimeConfirmedAtFifty { get; set; }
        public decimal? InspectionDays { get; set; }
        public decimal? ConfirmedTurnAroundTimeDays { get; set; }
        
        // Cost Avoidance
        public decimal? CA_HOURS_REQUESTED { get; set; }
        public decimal? CA_HOURS_APPROVED { get; set; }
        public decimal? CA_HOURS_SAVED { get; set; }
        public decimal? LABOUR_SAVED { get; set; }
        public decimal? CA_MATERIAL_REQUESTED { get; set; }
        public decimal? CA_MATERIAL_APPROVED { get; set; }
        public decimal? MATERIAL_SAVED { get; set; }
        public decimal? INVOICE_SUBMITTED { get; set; }
        public decimal? INVOICE_APPROVED { get; set; }
        public decimal? INVOICE_SAVED { get; set; }
        public decimal? TOTAL_SAVED { get; set; }
        
        public DateTime? ActualStartDateTime { get; set; }
        public DateTime? RTS_DATE { get; set; }
        public decimal? BudgetTat { get; set; }
        public decimal? GanttTAT { get; set; }
        public decimal? AgreedTurnAroundTime { get; set; }
        public decimal? CurrentTAT_inDays { get; set; }
        public decimal? TAT_vs_Budget { get; set; }
        public decimal? TAT_vs_Agreed { get; set; }
        public decimal? TotalTatAdjustment_inDays { get; set; }
        public decimal? TotalNonExcusableDays_inDays { get; set; }
        public decimal? TotalExcusableDays_inDays { get; set; }
        
        // Task Card
        public decimal? RoutineT35 { get; set; }
        public decimal? RoutineT35ToT15Added { get; set; }
        public decimal? RoutineT35ToT15Removed { get; set; }
        public decimal? RoutineT15ToEndAdded { get; set; }
        public decimal? RoutineT15ToEndRemoved { get; set; }
        public decimal? TOTAL_NRs { get; set; }
        public decimal? NON_ROUTINES_GENERATED { get; set; }
        
        // Manpower
        public decimal? TotalDayManHours { get; set; }
        public decimal? TotalAfternoonManHours { get; set; }
        public decimal? TotalNightManHours { get; set; }
        public decimal? OverallTotalManHours { get; set; }

        public string GetUniqueKey()
        {
            return WorkOrderNumber;
        }
    }
}
