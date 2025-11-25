using Npgsql;
using DbComparisonApp.Models;
using System.Reflection;

namespace DbComparisonApp.Services;

public class DatabaseService
{
    public async Task<List<WorkOrderData>> ExecuteQueryAsync(string connectionString, string query)
    {
        return await ExecuteQueryGenericAsync<WorkOrderData>(connectionString, query);
    }

    public async Task<List<T>> ExecuteQueryGenericAsync<T>(string connectionString, string query) where T : new()
    {
        var results = new List<T>();

        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var columnMap = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);

            // Map properties to columns
            foreach (var prop in properties)
            {
                columnMap[prop.Name] = prop;
            }

            while (await reader.ReadAsync())
            {
                var item = new T();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var columnName = reader.GetName(i);
                    if (columnMap.TryGetValue(columnName, out var prop))
                    {
                        var value = reader.GetValue(i);
                        if (value != DBNull.Value)
                        {
                            // Handle type conversion if needed
                            var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                            
                            try 
                            {
                                if (targetType == typeof(int) && value is long l)
                                    prop.SetValue(item, (int)l);
                                else if (targetType == typeof(decimal) && value is double d)
                                    prop.SetValue(item, (decimal)d);
                                else
                                    prop.SetValue(item, Convert.ChangeType(value, targetType));
                            }
                            catch
                            {
                                // Fallback or ignore if conversion fails
                            }
                        }
                    }
                }
                results.Add(item);
            }

            Console.WriteLine($"Successfully retrieved {results.Count} records for {typeof(T).Name}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing query for {typeof(T).Name}: {ex.Message}");
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
