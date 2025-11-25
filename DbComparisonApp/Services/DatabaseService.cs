using Npgsql;
using DbComparisonApp.Models;

namespace DbComparisonApp.Services;

public class DatabaseService
{
    public async Task<List<WorkOrderData>> ExecuteQueryAsync(string connectionString, string query)
    {
        var results = new List<WorkOrderData>();

        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var workOrder = new WorkOrderData
                {
                    WorkOrderNumber = GetStringValue(reader, "WorkOrderNumber"),
                    PostStatus = GetStringValue(reader, "PostStatus"),
                    InductionDayOfPeriod = GetNullableInt(reader, "InductionDayOfPeriod"),
                    CheckStatus = GetNullableInt(reader, "CheckStatus"),
                    EventType = GetStringValue(reader, "EventType"),
                    AirCraft = GetStringValue(reader, "AirCraft"),
                    AircraftType = GetStringValue(reader, "AircraftType"),
                    Location = GetStringValue(reader, "Location"),
                    WorkOrderDescription = GetStringValue(reader, "WorkOrderDescription"),
                    InductionDate = GetNullableDateTime(reader, "InductionDate"),
                    EstCompletionDate = GetNullableDateTime(reader, "EstCompletionDate"),
                    ActualCompletionDate = GetNullableDateTime(reader, "ActualCompletionDate"),
                    VendorName = GetStringValue(reader, "VendorName"),
                    SiteManager = GetStringValue(reader, "SiteManager"),
                    SiteManagerPhone = GetStringValue(reader, "SiteManagerPhone"),
                    AircraftCheckControlRepresentative = GetStringValue(reader, "AircraftCheckControlRepresentative"),
                    AircraftCheckControlRepresentativePhone = GetStringValue(reader, "AircraftCheckControlRepresentativePhone"),
                    GANTTurnAroundTime = GetNullableInt(reader, "GANTTurnAroundTime"),
                    AgreedTurnAroundTime = GetNullableInt(reader, "AgreedTurnAroundTime"),
                    RevisedTurnAroundTime = GetNullableInt(reader, "RevisedTurnAroundTime"),
                    TotalTatChanges = GetNullableLong(reader, "TotalTatChanges"),
                    LatestComment = GetStringValue(reader, "LatestComment"),
                    LatestReason = GetStringValue(reader, "LatestReason"),
                    TotalTasks = GetNullableDecimal(reader, "TotalTasks"),
                    OpenTasks = GetNullableDecimal(reader, "OpenTasks"),
                    DayShiftHC = GetNullableDecimal(reader, "DayShiftHC"),
                    AfternoonShiftHC = GetNullableDecimal(reader, "AfternoonShiftHC"),
                    NightShiftHC = GetNullableDecimal(reader, "NightShiftHC"),
                    RiskLevelId = GetNullableInt(reader, "RiskLevelId"),
                    RiskLevelName = GetStringValue(reader, "RiskLevelName"),
                    RiskDescription = GetStringValue(reader, "riskdescription"),
                    RiskComment = GetStringValue(reader, "RiskComment")
                };

                results.Add(workOrder);
            }

            Console.WriteLine($"Successfully retrieved {results.Count} records from database.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing query: {ex.Message}");
            throw;
        }

        return results;
    }

    private string GetStringValue(NpgsqlDataReader reader, string columnName)
    {
        try
        {
            var ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal))
                return string.Empty;
            
            return reader.GetString(ordinal);
        }
        catch
        {
            return string.Empty;
        }
    }

    private int? GetNullableInt(NpgsqlDataReader reader, string columnName)
    {
        try
        {
            var ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal))
                return null;

            // Try to get as int first
            var value = reader.GetValue(ordinal);
            
            if (value is int intValue)
                return intValue;
            
            if (value is long longValue)
                return (int)longValue;
            
            if (value is string strValue && int.TryParse(strValue, out int parsed))
                return parsed;
            
            // Try direct conversion
            return Convert.ToInt32(value);
        }
        catch
        {
            return null;
        }
    }

    private long? GetNullableLong(NpgsqlDataReader reader, string columnName)
    {
        try
        {
            var ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal))
                return null;

            var value = reader.GetValue(ordinal);
            
            if (value is long longValue)
                return longValue;
            
            if (value is int intValue)
                return intValue;
            
            if (value is string strValue && long.TryParse(strValue, out long parsed))
                return parsed;
            
            return Convert.ToInt64(value);
        }
        catch
        {
            return null;
        }
    }

    private decimal? GetNullableDecimal(NpgsqlDataReader reader, string columnName)
    {
        try
        {
            var ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal))
                return null;

            var value = reader.GetValue(ordinal);
            
            if (value is decimal decValue)
                return decValue;
            
            if (value is double dblValue)
                return (decimal)dblValue;
            
            if (value is int intValue)
                return intValue;
            
            if (value is long longValue)
                return longValue;
            
            if (value is string strValue && decimal.TryParse(strValue, out decimal parsed))
                return parsed;
            
            return Convert.ToDecimal(value);
        }
        catch
        {
            return null;
        }
    }

    private DateTime? GetNullableDateTime(NpgsqlDataReader reader, string columnName)
    {
        try
        {
            var ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal))
                return null;

            var value = reader.GetValue(ordinal);
            
            if (value is DateTime dtValue)
                return dtValue;
            
            if (value is string strValue && DateTime.TryParse(strValue, out DateTime parsed))
                return parsed;
            
            return Convert.ToDateTime(value);
        }
        catch
        {
            return null;
        }
    }
}
