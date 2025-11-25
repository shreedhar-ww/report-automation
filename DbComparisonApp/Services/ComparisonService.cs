using DbComparisonApp.Models;
using System.Reflection;

namespace DbComparisonApp.Services;

public class ComparisonService
{
    public ComparisonResult CompareData(List<WorkOrderData> db1Data, List<WorkOrderData> db2Data)
    {
        var result = new ComparisonResult();

        // Create dictionaries for faster lookup
        var db1Dict = db1Data.ToDictionary(x => x.WorkOrderNumber);
        var db2Dict = db2Data.ToDictionary(x => x.WorkOrderNumber);

        // Find records only in DB1
        foreach (var record in db1Data)
        {
            if (!db2Dict.ContainsKey(record.WorkOrderNumber))
            {
                result.OnlyInDb1.Add(record);
            }
        }

        // Find records only in DB2
        foreach (var record in db2Data)
        {
            if (!db1Dict.ContainsKey(record.WorkOrderNumber))
            {
                result.OnlyInDb2.Add(record);
            }
        }

        // Find matching records and records with differences
        foreach (var db1Record in db1Data)
        {
            if (db2Dict.TryGetValue(db1Record.WorkOrderNumber, out var db2Record))
            {
                var differingFields = GetDifferingFields(db1Record, db2Record);

                if (differingFields.Count == 0)
                {
                    // Records are identical
                    result.MatchingRecords.Add(db1Record);
                }
                else
                {
                    // Records have differences
                    result.RecordsWithDifferences.Add(new RecordDifference
                    {
                        WorkOrderNumber = db1Record.WorkOrderNumber,
                        Db1Record = db1Record,
                        Db2Record = db2Record,
                        DifferingFields = differingFields
                    });
                }
            }
        }

        Console.WriteLine($"\nComparison Summary:");
        Console.WriteLine($"  Matching Records: {result.MatchingRecords.Count}");
        Console.WriteLine($"  Only in DB1: {result.OnlyInDb1.Count}");
        Console.WriteLine($"  Only in DB2: {result.OnlyInDb2.Count}");
        Console.WriteLine($"  Records with Differences: {result.RecordsWithDifferences.Count}");

        return result;
    }

    private List<string> GetDifferingFields(WorkOrderData record1, WorkOrderData record2)
    {
        var differingFields = new List<string>();
        var properties = typeof(WorkOrderData).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var value1 = property.GetValue(record1);
            var value2 = property.GetValue(record2);

            // Compare values, handling nulls
            if (!AreEqual(value1, value2))
            {
                differingFields.Add(property.Name);
            }
        }

        return differingFields;
    }

    private bool AreEqual(object? value1, object? value2)
    {
        // Both null
        if (value1 == null && value2 == null)
            return true;

        // One is null, other is not
        if (value1 == null || value2 == null)
            return false;

        // For DateTime, compare with tolerance (to handle potential precision differences)
        if (value1 is DateTime dt1 && value2 is DateTime dt2)
        {
            return Math.Abs((dt1 - dt2).TotalSeconds) < 1;
        }

        // For decimal/numeric types, use appropriate comparison
        if (value1 is decimal d1 && value2 is decimal d2)
        {
            return d1 == d2;
        }

        // Default comparison
        return value1.Equals(value2);
    }
}
