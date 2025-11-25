# Detailed Comparison Sheet - Quick Guide

## New Format (Updated)

The **Detailed Comparison** sheet now has an improved layout for easier comparison:

### Layout
- **Column 1**: "Source" - Shows "DB1" or "DB2" to identify which database the row is from
- **Columns 2+**: All data fields (WorkOrderNumber, PostStatus, etc.)
- **Rows**: Consecutive pairs - DB1 row followed immediately by DB2 row for the same record

### Color Coding

| Color | Meaning |
|-------|---------|
| 🟢 **Light Green** | Matching records – both DB1 and DB2 rows are identical |
| 🔴 **Red with white text** | Fields in DB1 that differ from DB2 |
| 🟠 **Orange with white text** | Fields in DB2 that differ from DB1 |
| 🟡 **Yellow** | Record exists in DB1 but missing in DB2 *(now shown as **Gray** for DB1 rows and **Pink** for DB2 rows)* |
| 🔴 **Red "MISSING"** | Record doesn't exist in the other database *(now Gray for DB1 missing rows and Pink for DB2 missing rows)* |

*Note: The actual sheet uses **Gray** for DB1‑only rows and **Pink** for DB2‑only rows.*

### How to Read

**Example:**
```
Row 2:  DB1  | 1107819 | ACTIVE  | ...
Row 3:  DB2  | 1107819 | PENDING | ...
                        ^^^^^^^ (Orange - Different!)
```

- Row 2 shows DB1 data
- Row 3 shows DB2 data for the same WorkOrderNumber
- "ACTIVE" vs "PENDING" difference is highlighted in red (DB1) and orange (DB2)

### Features
✅ **Source column frozen** - Always visible when scrolling
✅ **Header row frozen** - Column names always visible
✅ **White text on highlights** - Better readability on red/orange backgrounds
✅ **No extra spacing** - Compact view, easy to scan

### Tips
- Filter by "Source" column to show only DB1 or DB2 rows
- Scroll horizontally to see all fields while Source column stays visible
- Red/Orange cells = Data accuracy issues that need attention
- "MISSING" rows = Synchronization needed
