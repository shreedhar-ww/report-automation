using ClosedXML.Excel;
using DbComparisonApp.Models;
using System.Reflection;

namespace DbComparisonApp.Services;

public class ExcelReportService
{
    public void GenerateReport<T>(ComparisonResult<T> comparisonResult, string outputPath) where T : IReportData
    {
        using var workbook = new XLWorkbook();

        // Create Summary Sheet
        CreateSummarySheet(workbook, comparisonResult);

        // Create Matching Records Sheet
        if (comparisonResult.MatchingRecords.Count > 0)
        {
            CreateDataSheet(workbook, "Matching Records", comparisonResult.MatchingRecords, XLColor.LightGreen);
        }

        // Create Missing in DB2 Sheet (Only in DB1) - GRAY
        if (comparisonResult.OnlyInDb1.Count > 0)
        {
            CreateDataSheet(workbook, "Missing in DB2", comparisonResult.OnlyInDb1, XLColor.Gray);
        }

        // Create Missing in DB1 Sheet (Only in DB2) - PINK
        if (comparisonResult.OnlyInDb2.Count > 0)
        {
            CreateDataSheet(workbook, "Missing in DB1", comparisonResult.OnlyInDb2, XLColor.Pink);
        }

        // Create Records with Differences Sheet
        if (comparisonResult.RecordsWithDifferences.Count > 0)
        {
            CreateDifferencesSheet(workbook, comparisonResult.RecordsWithDifferences);
        }

        // Create Detailed Comparison Sheet (DB1 Source vs DB2 Target)
        CreateDetailedComparisonSheet(workbook, comparisonResult);

        workbook.SaveAs(outputPath);
        Console.WriteLine($"\nExcel report generated successfully: {outputPath}");
    }

    private void CreateSummarySheet<T>(XLWorkbook workbook, ComparisonResult<T> result) where T : IReportData
    {
        var worksheet = workbook.Worksheets.Add("Summary");

        // Title
        worksheet.Cell(1, 1).Value = $"Database Comparison Summary ({typeof(T).Name})";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 16;

        // Summary data
        worksheet.Cell(3, 1).Value = "Category";
        worksheet.Cell(3, 2).Value = "Count";
        worksheet.Range(3, 1, 3, 2).Style.Font.Bold = true;
        worksheet.Range(3, 1, 3, 2).Style.Fill.BackgroundColor = XLColor.LightGray;

        worksheet.Cell(4, 1).Value = "Matching Records";
        worksheet.Cell(4, 2).Value = result.MatchingRecords.Count;
        worksheet.Range(4, 1, 4, 2).Style.Fill.BackgroundColor = XLColor.LightGreen;

        worksheet.Cell(5, 1).Value = "Only in DB1 (Missing in DB2)";
        worksheet.Cell(5, 2).Value = result.OnlyInDb1.Count;
        worksheet.Range(5, 1, 5, 2).Style.Fill.BackgroundColor = XLColor.Gray;

        worksheet.Cell(6, 1).Value = "Only in DB2 (Missing in DB1)";
        worksheet.Cell(6, 2).Value = result.OnlyInDb2.Count;
        worksheet.Range(6, 1, 6, 2).Style.Fill.BackgroundColor = XLColor.Pink;

        worksheet.Cell(7, 1).Value = "Records with Differences";
        worksheet.Cell(7, 2).Value = result.RecordsWithDifferences.Count;
        worksheet.Range(7, 1, 7, 2).Style.Fill.BackgroundColor = XLColor.Red;

        worksheet.Cell(9, 1).Value = "Total Records in DB1";
        worksheet.Cell(9, 2).Value = result.MatchingRecords.Count + result.OnlyInDb1.Count + result.RecordsWithDifferences.Count;

        worksheet.Cell(10, 1).Value = "Total Records in DB2";
        worksheet.Cell(10, 2).Value = result.MatchingRecords.Count + result.OnlyInDb2.Count + result.RecordsWithDifferences.Count;

        worksheet.Columns().AdjustToContents();
    }

    private void CreateDataSheet<T>(XLWorkbook workbook, string sheetName, List<T> data, XLColor highlightColor)
    {
        var worksheet = workbook.Worksheets.Add(sheetName);

        // Get properties
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Add headers
        for (int i = 0; i < properties.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = properties[i].Name;
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        // Add data
        for (int row = 0; row < data.Count; row++)
        {
            for (int col = 0; col < properties.Length; col++)
            {
                var value = properties[col].GetValue(data[row]);
                worksheet.Cell(row + 2, col + 1).Value = value?.ToString() ?? "";
                
                // Highlight the entire row
                worksheet.Cell(row + 2, col + 1).Style.Fill.BackgroundColor = highlightColor;
            }
        }

        // Freeze header row
        worksheet.SheetView.FreezeRows(1);

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();
    }

    private void CreateDifferencesSheet<T>(XLWorkbook workbook, List<RecordDifference<T>> differences) where T : IReportData
    {
        var worksheet = workbook.Worksheets.Add("Records with Differences");

        // Get properties
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Create headers with DB1 and DB2 columns
        worksheet.Cell(1, 1).Value = "Field Name";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;

        int currentRow = 2;

        foreach (var diff in differences)
        {
            // Add Key header
            worksheet.Cell(currentRow, 1).Value = $"Key: {diff.Key}";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
            
            worksheet.Cell(currentRow, 2).Value = "DB1 Value";
            worksheet.Cell(currentRow, 2).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 2).Style.Fill.BackgroundColor = XLColor.LightGray;
            
            worksheet.Cell(currentRow, 3).Value = "DB2 Value";
            worksheet.Cell(currentRow, 3).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 3).Style.Fill.BackgroundColor = XLColor.LightGray;
            
            worksheet.Cell(currentRow, 4).Value = "Status";
            worksheet.Cell(currentRow, 4).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 4).Style.Fill.BackgroundColor = XLColor.LightGray;

            currentRow++;

            // Add all fields
            foreach (var property in properties)
            {
                var db1Value = property.GetValue(diff.Db1Record);
                var db2Value = property.GetValue(diff.Db2Record);
                bool isDifferent = diff.DifferingFields.Contains(property.Name);

                worksheet.Cell(currentRow, 1).Value = property.Name;
                worksheet.Cell(currentRow, 2).Value = db1Value?.ToString() ?? "";
                worksheet.Cell(currentRow, 3).Value = db2Value?.ToString() ?? "";
                worksheet.Cell(currentRow, 4).Value = isDifferent ? "DIFFERENT" : "Same";

                // Highlight differing cells
                if (isDifferent)
                {
                    worksheet.Cell(currentRow, 2).Style.Fill.BackgroundColor = XLColor.Red;
                    worksheet.Cell(currentRow, 3).Style.Fill.BackgroundColor = XLColor.Red;
                    worksheet.Cell(currentRow, 4).Style.Fill.BackgroundColor = XLColor.Red;
                    worksheet.Cell(currentRow, 4).Style.Font.Bold = true;
                }

                currentRow++;
            }

            // Add spacing between records
            currentRow++;
        }

        // Freeze header row
        worksheet.SheetView.FreezeRows(1);

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();
    }

    private void CreateDetailedComparisonSheet<T>(XLWorkbook workbook, ComparisonResult<T> comparisonResult) where T : IReportData
    {
        var worksheet = workbook.Worksheets.Add("Detailed Comparison");

        // Get properties
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Create header row with Source column first
        worksheet.Cell(1, 1).Value = "Source";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.DarkGray;
        worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.White;

        for (int i = 0; i < properties.Length; i++)
        {
            worksheet.Cell(1, i + 2).Value = properties[i].Name;
            worksheet.Cell(1, i + 2).Style.Font.Bold = true;
            worksheet.Cell(1, i + 2).Style.Fill.BackgroundColor = XLColor.DarkGray;
            worksheet.Cell(1, i + 2).Style.Font.FontColor = XLColor.White;
        }

        int currentRow = 2;

        // Process matching records first
        foreach (var record in comparisonResult.MatchingRecords)
        {
            // DB1 (Source) row
            worksheet.Cell(currentRow, 1).Value = "DB1";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.LightGreen;

            for (int i = 0; i < properties.Length; i++)
            {
                var value = properties[i].GetValue(record);
                worksheet.Cell(currentRow, i + 2).Value = value?.ToString() ?? "";
                worksheet.Cell(currentRow, i + 2).Style.Fill.BackgroundColor = XLColor.LightGreen;
            }
            currentRow++;

            // DB2 (Target) row - same as DB1 for matching records
            worksheet.Cell(currentRow, 1).Value = "DB2";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.LightGreen;

            for (int i = 0; i < properties.Length; i++)
            {
                var value = properties[i].GetValue(record);
                worksheet.Cell(currentRow, i + 2).Value = value?.ToString() ?? "";
                worksheet.Cell(currentRow, i + 2).Style.Fill.BackgroundColor = XLColor.LightGreen;
            }
            currentRow++; // No extra spacing for matching records
        }

        // Process records with differences
        foreach (var diff in comparisonResult.RecordsWithDifferences)
        {
            // DB1 (Source) row
            worksheet.Cell(currentRow, 1).Value = "DB1";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;

            for (int i = 0; i < properties.Length; i++)
            {
                var value = properties[i].GetValue(diff.Db1Record);
                var cell = worksheet.Cell(currentRow, i + 2);
                cell.Value = value?.ToString() ?? "";
                
                // Highlight if this field differs
                if (diff.DifferingFields.Contains(properties[i].Name))
                {
                    bool isCardCount = typeof(T).Name == "CardCountData";
                    bool applyBlue = false;

                    if (isCardCount)
                    {
                        var db2Value = properties[i].GetValue(diff.Db2Record);
                        if (double.TryParse(value?.ToString(), out double db1Val) && 
                            double.TryParse(db2Value?.ToString(), out double db2Val))
                        {
                            if (db1Val > db2Val)
                            {
                                applyBlue = true;
                            }
                        }
                    }

                    if (applyBlue)
                    {
                        cell.Style.Fill.BackgroundColor = XLColor.Blue;
                        cell.Style.Font.Bold = true;
                        cell.Style.Font.FontColor = XLColor.White;
                    }
                    else
                    {
                        cell.Style.Fill.BackgroundColor = XLColor.Red;
                        cell.Style.Font.Bold = true;
                        cell.Style.Font.FontColor = XLColor.White;
                    }
                }
                else
                {
                    cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
                }
            }
            currentRow++;

            // DB2 (Target) row
            worksheet.Cell(currentRow, 1).Value = "DB2";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.LightYellow;

            for (int i = 0; i < properties.Length; i++)
            {
                var value = properties[i].GetValue(diff.Db2Record);
                var cell = worksheet.Cell(currentRow, i + 2);
                cell.Value = value?.ToString() ?? "";
                
                // Highlight if this field differs
                if (diff.DifferingFields.Contains(properties[i].Name))
                {
                    bool isCardCount = typeof(T).Name == "CardCountData";
                    bool applyBlue = false;

                    if (isCardCount)
                    {
                        var db1Value = properties[i].GetValue(diff.Db1Record);
                        if (double.TryParse(db1Value?.ToString(), out double db1Val) && 
                            double.TryParse(value?.ToString(), out double db2Val))
                        {
                            if (db1Val > db2Val)
                            {
                                applyBlue = true;
                            }
                        }
                    }

                    if (applyBlue)
                    {
                        cell.Style.Fill.BackgroundColor = XLColor.Blue;
                        cell.Style.Font.Bold = true;
                        cell.Style.Font.FontColor = XLColor.White;
                    }
                    else
                    {
                        cell.Style.Fill.BackgroundColor = XLColor.Orange;
                        cell.Style.Font.Bold = true;
                        cell.Style.Font.FontColor = XLColor.White;
                    }
                }
                else
                {
                    cell.Style.Fill.BackgroundColor = XLColor.LightYellow;
                }
            }
            currentRow++; // No extra spacing
        }

        // Process records only in DB1 (missing in DB2) - at the bottom
        foreach (var record in comparisonResult.OnlyInDb1)
        {
            // DB1 (Source) row
            worksheet.Cell(currentRow, 1).Value = "DB1";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.Gray;

            for (int i = 0; i < properties.Length; i++)
            {
                var value = properties[i].GetValue(record);
                worksheet.Cell(currentRow, i + 2).Value = value?.ToString() ?? "";
                worksheet.Cell(currentRow, i + 2).Style.Fill.BackgroundColor = XLColor.Gray;
            }
            currentRow++;

            // DB2 (Target) row - MISSING
            worksheet.Cell(currentRow, 1).Value = "DB2";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.Pink;
            // Keep font black for readability

            for (int i = 0; i < properties.Length; i++)
            {
                worksheet.Cell(currentRow, i + 2).Value = "MISSING";
                worksheet.Cell(currentRow, i + 2).Style.Fill.BackgroundColor = XLColor.Pink;
                worksheet.Cell(currentRow, i + 2).Style.Font.Bold = true;
            }
            currentRow++;
        }

        // Process records only in DB2 (missing in DB1) - at the bottom
        foreach (var record in comparisonResult.OnlyInDb2)
        {
            // DB1 (Source) row - MISSING
            worksheet.Cell(currentRow, 1).Value = "DB1";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.Gray;
            // Keep font black for readability

            for (int i = 0; i < properties.Length; i++)
            {
                worksheet.Cell(currentRow, i + 2).Value = "MISSING";
                worksheet.Cell(currentRow, i + 2).Style.Fill.BackgroundColor = XLColor.Gray;
                worksheet.Cell(currentRow, i + 2).Style.Font.Bold = true;
            }
            currentRow++;

            // DB2 (Target) row
            worksheet.Cell(currentRow, 1).Value = "DB2";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.Pink;

            for (int i = 0; i < properties.Length; i++)
            {
                var value = properties[i].GetValue(record);
                worksheet.Cell(currentRow, i + 2).Value = value?.ToString() ?? "";
                worksheet.Cell(currentRow, i + 2).Style.Fill.BackgroundColor = XLColor.Pink;
            }
            currentRow++;
        }

        // Freeze header row and Source column
        worksheet.SheetView.FreezeRows(1);
        worksheet.SheetView.FreezeColumns(1);

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();
    }
}
