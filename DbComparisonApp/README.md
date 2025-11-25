# Database Comparison Tool

A .NET Core 8.0 console application that connects to two PostgreSQL databases, executes queries, compares the results, and generates an Excel report with color-coded highlighting.

## Features

- Connects to two PostgreSQL databases
- Executes complex queries with lateral joins
- Compares data and categorizes results into:
  - **Matching Records** (Green highlight)
  - **Missing in DB2** (Yellow highlight)
  - **Missing in DB1** (Orange highlight)
  - **Records with Differences** (Red highlight with cell-level highlighting)
- Generates comprehensive Excel reports using ClosedXML
- Uses today's date dynamically for manpower calculations

## Prerequisites

- .NET 8.0 SDK
- Access to two PostgreSQL databases
- Connection credentials for both databases

## Configuration

Edit `appsettings.json` to configure your database connections and query parameters:

```json
{
  "ConnectionStrings": {
    "Database1": "Host=your_host;Port=5432;Database=db1;Username=your_user;Password=your_password",
    "Database2": "Host=your_host;Port=5432;Database=db2;Username=your_user;Password=your_password"
  },
  "QueryParameters": {
    "StartDate": "2025-06-27",
    "EndDate": "2026-02-12",
    "WorkOrderNumber": "1107819",
    "ExcludeWorkOrderNumber": "XXXXXXX"
  },
  "OutputSettings": {
    "ExcelFileName": "DatabaseComparison_{timestamp}.xlsx"
  }
}
```

### Connection String Format

PostgreSQL connection string format:
```
Host=hostname;Port=5432;Database=database_name;Username=username;Password=password
```

Optional parameters:
- `SSL Mode=Require` - for SSL connections
- `Timeout=30` - connection timeout in seconds

## Installation

1. Clone or download this repository
2. Navigate to the project directory
3. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

## Usage

1. Update `appsettings.json` with your database connection strings
2. Adjust query parameters as needed
3. Run the application:
   ```bash
   dotnet run
   ```

The application will:
1. Connect to both databases
2. Execute the query on each database
3. Compare the results
4. Generate an Excel report in the current directory

## Excel Report Structure

The generated Excel file contains multiple worksheets:

### Summary Sheet
- Overview of comparison results
- Count of records in each category
- Color-coded summary table

### Matching Records Sheet
- Records that exist in both databases with identical values
- Highlighted in **green**

### Missing in DB2 Sheet
- Records that exist only in Database 1
- Highlighted in **yellow**

### Missing in DB1 Sheet
- Records that exist only in Database 2
- Highlighted in **orange**

### Records with Differences Sheet
- Records that exist in both databases but have different values
- Shows DB1 and DB2 values side by side
- Differing cells are highlighted in **red**
- "DIFFERENT" status indicator for changed fields

### Detailed Comparison Sheet (NEW!)
- **Stacked record format** - Each record shows DB1 (Source) and DB2 (Target) on consecutive rows
- **DB1 is the Source of Truth** - Used to validate DB2 data accuracy
- **Cell-level highlighting** - Only differing fields are highlighted
- **Color coding:**
  - 🟢 Green = Matching records
  - 🔴 Red cells on DB1 = Fields that differ from DB2
  - 🟠 Orange cells on DB2 = Fields that differ from DB1
  - 🟡 Yellow = Record in DB1 but missing in DB2
  - 🔴 Red "MISSING" = Record doesn't exist in target database
- **Missing records at bottom** - Easy to find records that need synchronization
- See [DETAILED_COMPARISON_GUIDE.md](DETAILED_COMPARISON_GUIDE.md) for complete usage instructions

## Query Details

The application executes a complex SQL query that retrieves work order information including:
- Work order details
- On-site basic information
- Turn-around time data
- TAT changes and adjustments
- Task summaries
- Manpower data (using today's date)
- Risk information

## Troubleshooting

### Connection Issues
- Verify database host and port are correct
- Ensure firewall allows connections to PostgreSQL
- Check username and password
- Verify database name exists

### Query Errors
- Ensure all tables referenced in the query exist in both databases
- Check that column names match exactly (PostgreSQL is case-sensitive with quoted identifiers)
- Verify the WorkOrderNumber exists in the databases

### Excel Generation Issues
- Ensure you have write permissions in the output directory
- Check that the output file is not already open in Excel

## Dependencies

- **Npgsql** (10.0.0) - PostgreSQL data provider
- **ClosedXML** (0.105.0) - Excel file generation
- **Microsoft.Extensions.Configuration.Json** (10.0.0) - Configuration management

## License

This project is provided as-is for database comparison purposes.

## Notes

- The application uses today's date for manpower calculations automatically
- The WorkOrderNumber is used as the primary key for comparison
- All date/time comparisons have a 1-second tolerance to handle precision differences
- The Excel file name includes a timestamp to prevent overwriting previous reports
