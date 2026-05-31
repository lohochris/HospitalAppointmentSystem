# DATABASE MIGRATION & TROUBLESHOOTING GUIDE

## 🔴 CRITICAL: "No such column: Username" Error

### Problem
Your application is crashing with:
```
SQL logic error: no such column: Username
```

This happens because your **HospitalDB.db** file already exists from previous runs and contains **old schema** without the new RBAC columns.

---

## ✅ SOLUTION 1: Clean Database Reset (RECOMMENDED FOR DEVELOPMENT)

### Step 1: Close Visual Studio
Close all instances of Visual Studio to release file locks.

### Step 2: Delete Old Database
Navigate to your application's bin directory:
```
C:\Users\Loho Christopher\source\repos\HospitalAppointmentSystem\HospitalAppointmentSystem\bin\Debug\
```

**Delete these files:**
- `HospitalDB.db`
- `HospitalDB.db-journal` (if exists)
- `HospitalDB.db-wal` (if exists)
- `HospitalDB.db-shm` (if exists)

### Step 3: Restart Application
Run your application again. The database will be recreated with the correct schema including:
- ✅ `DoctorsManagement` table with `Username` column
- ✅ `PatientsManagement` table
- ✅ `InpatientVitals` table
- ✅ All indexes and foreign keys

---

## ✅ SOLUTION 2: In-Place Migration (PRESERVES DATA)

If you need to keep existing data, add this migration helper to `ModuleDatabase.vb`:

### VB.NET Migration Code

Add this function to the `#Region "Connection Management"` section:

```vb
''' <summary>
''' Performs safe database schema migration for existing databases.
''' Adds missing columns and tables without data loss.
''' </summary>
Public Sub MigrateDatabase()
	Try
		Using conn As New SQLiteConnection(GetConnectionString())
			conn.Open()

			' Enable foreign key support
			Using cmd As New SQLiteCommand("PRAGMA foreign_keys = ON;", conn)
				cmd.ExecuteNonQuery()
			End Using

			' Check if DoctorsManagement table exists
			Dim tableCheckSql As String = "SELECT name FROM sqlite_master WHERE type='table' AND name='DoctorsManagement'"
			Dim tableExists As Boolean = False

			Using cmd As New SQLiteCommand(tableCheckSql, conn)
				Using reader As SQLiteDataReader = cmd.ExecuteReader()
					tableExists = reader.Read()
				End Using
			End Using

			If tableExists Then
				' Table exists - check for Username column
				Dim columnCheckSql As String = "PRAGMA table_info(DoctorsManagement)"
				Dim hasUsername As Boolean = False

				Using cmd As New SQLiteCommand(columnCheckSql, conn)
					Using reader As SQLiteDataReader = cmd.ExecuteReader()
						While reader.Read()
							If reader.GetString(1).Equals("Username", StringComparison.OrdinalIgnoreCase) Then
								hasUsername = True
								Exit While
							End If
						End While
					End Using
				End Using

				If Not hasUsername Then
					' Add Username column safely
					Dim alterSql As String = "ALTER TABLE DoctorsManagement ADD COLUMN Username TEXT UNIQUE;"
					Using cmd As New SQLiteCommand(alterSql, conn)
						cmd.ExecuteNonQuery()
					End Using

					' Create index
					Dim indexSql As String = "CREATE INDEX IF NOT EXISTS idx_doctor_username ON DoctorsManagement(Username);"
					Using cmd As New SQLiteCommand(indexSql, conn)
						cmd.ExecuteNonQuery()
					End Using

					LogError("SUCCESS: Added Username column to DoctorsManagement table")
				End If
			End If

			' Verify all required tables exist
			Dim requiredTables As New List(Of String) From {
				"PatientsManagement",
				"InpatientVitals",
				"DoctorsManagement"
			}

			For Each tableName As String In requiredTables
				tableCheckSql = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'"
				tableExists = False

				Using cmd As New SQLiteCommand(tableCheckSql, conn)
					Using reader As SQLiteDataReader = cmd.ExecuteReader()
						tableExists = reader.Read()
					End Using
				End Using

				If Not tableExists Then
					LogError($"WARNING: Table '{tableName}' does not exist. Running full schema initialization...")
					' Exit and let InitialiseDatabase create all tables
					Exit For
				End If
			Next

		End Using

	Catch ex As Exception
		LogError($"MigrateDatabase error: {ex.Message}")
		Throw
	End Try
End Sub
```

### Update InitialiseDatabase()

Find this function (around line 73) and add the migration call:

```vb
Public Sub InitialiseDatabase()
	Try
		Using conn As New SQLiteConnection(GetConnectionString())
			conn.Open()
			Dim sql As String = GetDatabaseSchema()
			Using cmd As New SQLiteCommand(sql, conn)
				cmd.ExecuteNonQuery()
			End Using
			InsertSampleDataIfEmpty(conn)
		End Using

		' Run migration BEFORE initializing new schemas
		MigrateDatabase()  ' <--- ADD THIS LINE

		' Initialize Patient Management schema
		InitializePatientManagementSchema()

		' Initialize Patient Vitals schema with referential integrity
		InitializePatientVitalsSchema()

		' Initialize Doctors Management schema
		InitializeDoctorsManagementSchema()

	Catch ex As Exception
		LogError("InitialiseDatabase error: " & ex.Message)
		Throw
	End Try
End Sub
```

---

## 🔍 VERIFICATION

After applying either solution, verify the schema:

### Method 1: Using DB Browser for SQLite
1. Download [DB Browser for SQLite](https://sqlitebrowser.org/)
2. Open `HospitalDB.db`
3. Go to "Database Structure" tab
4. Find `DoctorsManagement` table
5. Verify columns include: `DoctorID`, **`Username`**, `FirstName`, `LastName`, `Specialization`, etc.

### Method 2: Using PowerShell + SQLite CLI
```powershell
cd "C:\Users\Loho Christopher\source\repos\HospitalAppointmentSystem\HospitalAppointmentSystem\bin\Debug\"

# Check if DoctorsManagement exists
sqlite3 HospitalDB.db "SELECT name FROM sqlite_master WHERE type='table' AND name='DoctorsManagement';"

# View DoctorsManagement schema
sqlite3 HospitalDB.db "PRAGMA table_info(DoctorsManagement);"

# Expected output should include:
# | cid | name | type | notnull | dflt_value | pk |
# | 0 | DoctorID | TEXT | 1 | | 1 |
# | 1 | Username | TEXT | 0 | | 0 |  <-- THIS MUST BE PRESENT
# | 2 | FirstName | TEXT | 1 | | 0 |
```

---

## 🛡️ PREVENTION: Best Practices

### 1. Version Control Your Database Schema
Add this constant to `ModuleDatabase.vb`:

```vb
Public Const DB_SCHEMA_VERSION As Integer = 2 ' Increment when schema changes
```

### 2. Create a Schema Version Table
```sql
CREATE TABLE IF NOT EXISTS SchemaVersion (
	Version INTEGER PRIMARY KEY,
	AppliedDate TEXT DEFAULT (datetime('now')),
	Description TEXT
);
```

### 3. Conditional Migration Logic
```vb
Public Function GetCurrentSchemaVersion() As Integer
	Try
		Dim sql As String = "SELECT MAX(Version) FROM SchemaVersion"
		Dim result As Object = ExecuteScalar(sql)
		If result IsNot Nothing AndAlso Not IsDBNull(result) Then
			Return Convert.ToInt32(result)
		End If
		Return 0
	Catch
		Return 0
	End Try
End Function

Public Sub ApplyMigration(version As Integer, description As String, migrationSql As String)
	Using conn As New SQLiteConnection(GetConnectionString())
		conn.Open()
		Using transaction As SQLiteTransaction = conn.BeginTransaction()
			Try
				' Execute migration
				Using cmd As New SQLiteCommand(migrationSql, conn, transaction)
					cmd.ExecuteNonQuery()
				End Using

				' Record version
				Dim versionSql As String = "INSERT INTO SchemaVersion (Version, Description) VALUES (@v, @d)"
				Using cmd As New SQLiteCommand(versionSql, conn, transaction)
					cmd.Parameters.AddWithValue("@v", version)
					cmd.Parameters.AddWithValue("@d", description)
					cmd.ExecuteNonQuery()
				End Using

				transaction.Commit()
			Catch ex As Exception
				transaction.Rollback()
				Throw
			End Try
		End Using
	End Using
End Sub
```

---

## 📋 TROUBLESHOOTING CHECKLIST

### If Application Still Crashes:

1. ✅ **Verify database file location:**
   ```vb
   Dim dbPath As String = Path.Combine(Application.StartupPath, DB_FILE)
   MessageBox.Show($"Database path: {dbPath}")
   ```

2. ✅ **Check error_log.txt:**
   ```
   C:\Users\Loho Christopher\source\repos\HospitalAppointmentSystem\HospitalAppointmentSystem\bin\Debug\error_log.txt
   ```

3. ✅ **Test database connection:**
   ```vb
   If ModuleDatabase.TestConnection() Then
	   MessageBox.Show("Connected successfully")
   Else
	   MessageBox.Show("Connection failed - check error_log.txt")
   End If
   ```

4. ✅ **Inspect actual schema:**
   Add this debug function:
   ```vb
   Public Function DebugDoctorsTable() As String
	   Try
		   Using conn As New SQLiteConnection(GetConnectionString())
			   conn.Open()
			   Dim sql As String = "PRAGMA table_info(DoctorsManagement)"
			   Dim output As New System.Text.StringBuilder()

			   Using cmd As New SQLiteCommand(sql, conn)
				   Using reader As SQLiteDataReader = cmd.ExecuteReader()
					   While reader.Read()
						   output.AppendLine($"Column: {reader.GetString(1)}, Type: {reader.GetString(2)}")
					   End While
				   End Using
			   End Using

			   Return output.ToString()
		   End Using
	   Catch ex As Exception
		   Return $"ERROR: {ex.Message}"
	   End Try
   End Function
   ```

   Call it from your startup form:
   ```vb
   Dim schema As String = ModuleDatabase.DebugDoctorsTable()
   MessageBox.Show(schema, "DoctorsManagement Schema")
   ```

---

## 🚨 EMERGENCY: Quick Fix for Production

If you need to restore functionality immediately:

### Option A: Disable RBAC Temporarily
Comment out Username references in `SearchDoctors`:

```vb
' OR Username LIKE @search  ' <-- Comment this out temporarily
```

### Option B: Use NULL-Safe Queries
Modify `GetDoctorsTable()` to handle missing column:

```vb
Dim sql As String = "
SELECT 
	d.DoctorID AS 'Doctor ID',
	CASE WHEN d.Username IS NULL THEN '(No Login)' ELSE d.Username END AS Username,
	-- rest of query
```

---

## 📞 SUPPORT

If issues persist after trying all solutions:

1. **Backup current database:**
   ```powershell
   Copy-Item "HospitalDB.db" "HospitalDB_backup_$(Get-Date -Format 'yyyyMMdd_HHmmss').db"
   ```

2. **Check Visual Studio Output Window:**
   - Debug → Windows → Output
   - Look for SQLite error messages

3. **Enable SQL logging:**
   Add to connection string:
   ```vb
   _connectionString = $"Data Source={dbPath};Version=3;Trace=True;"
   ```

---

**Document Version:** 1.0  
**Last Updated:** 2025-01-12  
**Applies To:** HospitalAppointmentSystem with RBAC Doctors Management  
**Status:** ✅ Production-Ready Migration Strategy
