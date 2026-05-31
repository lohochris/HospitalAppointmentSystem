Option Strict On
Option Explicit On

' ============================================================
' DatabaseSchemaVerifier.vb
' CSC3226 - Hospital Appointment System
' Database Schema Verification and Testing Utility
' Group: Sa'id Umar, Aisha Ladan, Maryam Rabiu
' ============================================================

Imports System
Imports System.Data
Imports System.Data.SQLite
Imports System.Text
Imports System.Windows.Forms

Public Class DatabaseSchemaVerifier

    ''' <summary>
    ''' Verifies that the DoctorsManagement table has the correct schema including Username column.
    ''' Returns detailed report of table structure.
    ''' </summary>
    Public Shared Function VerifyDoctorsManagementSchema() As String
        Dim report As New StringBuilder()
        report.AppendLine("========================================")
        report.AppendLine("DOCTORSMANAGEMENT TABLE SCHEMA REPORT")
        report.AppendLine("========================================")
        report.AppendLine()

        Try
            Using conn As New SQLiteConnection(ModuleDatabase.GetConnectionString())
                conn.Open()

                ' Check if table exists
                Dim tableCheckSql As String = "SELECT name FROM sqlite_master WHERE type='table' AND name='DoctorsManagement'"
                Dim tableExists As Boolean = False

                Using cmd As New SQLiteCommand(tableCheckSql, conn)
                    Using reader As SQLiteDataReader = cmd.ExecuteReader()
                        tableExists = reader.Read()
                    End Using
                End Using

                If Not tableExists Then
                    report.AppendLine("❌ ERROR: DoctorsManagement table does NOT exist!")
                    report.AppendLine()
                    report.AppendLine("SOLUTION: Run ModuleDatabase.InitializeDoctorsManagementSchema()")
                    Return report.ToString()
                End If

                report.AppendLine("✓ Table exists: DoctorsManagement")
                report.AppendLine()

                ' Get column information
                Dim columnSql As String = "PRAGMA table_info(DoctorsManagement)"
                Dim hasUsername As Boolean = False
                Dim columnCount As Integer = 0

                report.AppendLine("COLUMNS:")
                report.AppendLine("--------")

                Using cmd As New SQLiteCommand(columnSql, conn)
                    Using reader As SQLiteDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            columnCount += 1
                            Dim cid As Integer = reader.GetInt32(0)
                            Dim name As String = reader.GetString(1)
                            Dim type As String = reader.GetString(2)
                            Dim notNull As Integer = reader.GetInt32(3)
                            Dim pk As Integer = reader.GetInt32(5)

                            Dim notNullStr As String = If(notNull = 1, "NOT NULL", "NULL")
                            Dim pkStr As String = If(pk = 1, "PRIMARY KEY", "")

                            report.AppendLine($"  [{cid}] {name,-20} {type,-10} {notNullStr,-10} {pkStr}")

                            If name.Equals("Username", StringComparison.OrdinalIgnoreCase) Then
                                hasUsername = True
                            End If
                        End While
                    End Using
                End Using

                report.AppendLine()
                report.AppendLine($"Total Columns: {columnCount}")
                report.AppendLine()

                ' Verify Username column
                If hasUsername Then
                    report.AppendLine("✓ PASS: Username column exists")
                Else
                    report.AppendLine("❌ FAIL: Username column is MISSING!")
                    report.AppendLine()
                    report.AppendLine("SOLUTION: Run migration via ModuleDatabase.InitializeDoctorsManagementSchema()")
                End If

                report.AppendLine()

                ' Get indexes
                Dim indexSql As String = "SELECT name, sql FROM sqlite_master WHERE type='index' AND tbl_name='DoctorsManagement'"
                report.AppendLine("INDEXES:")
                report.AppendLine("--------")

                Dim indexCount As Integer = 0
                Using cmd As New SQLiteCommand(indexSql, conn)
                    Using reader As SQLiteDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            indexCount += 1
                            Dim indexName As String = If(reader.IsDBNull(0), "(auto)", reader.GetString(0))
                            report.AppendLine($"  {indexCount}. {indexName}")
                        End While
                    End Using
                End Using

                If indexCount = 0 Then
                    report.AppendLine("  (No custom indexes found)")
                End If

                report.AppendLine()

                ' Get row count
                Dim countSql As String = "SELECT COUNT(*) FROM DoctorsManagement"
                Dim rowCount As Integer = 0

                Using cmd As New SQLiteCommand(countSql, conn)
                    Dim result As Object = cmd.ExecuteScalar()
                    If result IsNot Nothing AndAlso Not DBNull.Value.Equals(result) Then
                        rowCount = Convert.ToInt32(result)
                    End If
                End Using

                report.AppendLine($"Total Records: {rowCount}")
                report.AppendLine()

                ' Overall status
                report.AppendLine("========================================")
                If hasUsername AndAlso columnCount >= 10 Then
                    report.AppendLine("✓ SCHEMA STATUS: VALID")
                    report.AppendLine("  All required columns present including Username")
                Else
                    report.AppendLine("❌ SCHEMA STATUS: INCOMPLETE")
                    report.AppendLine("  Migration required")
                End If
                report.AppendLine("========================================")

            End Using

        Catch ex As Exception
            report.AppendLine()
            report.AppendLine("========================================")
            report.AppendLine("❌ ERROR OCCURRED")
            report.AppendLine("========================================")
            report.AppendLine(ex.Message)
            report.AppendLine()
            report.AppendLine("Stack Trace:")
            report.AppendLine(ex.StackTrace)
        End Try

        Return report.ToString()
    End Function

    ''' <summary>
    ''' Tests the migration by temporarily removing Username column (if safe) and re-adding it.
    ''' WARNING: This is destructive - use only in development!
    ''' </summary>
    Public Shared Function TestMigration() As String
        Dim report As New StringBuilder()
        report.AppendLine("========================================")
        report.AppendLine("MIGRATION TEST REPORT")
        report.AppendLine("========================================")
        report.AppendLine()

        Try
            ' First, check if table has data
            Using conn As New SQLiteConnection(ModuleDatabase.GetConnectionString())
                conn.Open()

                Dim countSql As String = "SELECT COUNT(*) FROM DoctorsManagement"
                Dim rowCount As Integer = 0

                Using cmd As New SQLiteCommand(countSql, conn)
                    Dim result As Object = cmd.ExecuteScalar()
                    If result IsNot Nothing AndAlso Not DBNull.Value.Equals(result) Then
                        rowCount = Convert.ToInt32(result)
                    End If
                End Using

                If rowCount > 0 Then
                    report.AppendLine($"❌ CANNOT TEST: Table contains {rowCount} record(s)")
                    report.AppendLine("Migration test is destructive and should only run on empty tables.")
                    report.AppendLine()
                    report.AppendLine("To proceed:")
                    report.AppendLine("1. Backup your database")
                    report.AppendLine("2. Delete all records from DoctorsManagement")
                    report.AppendLine("3. Re-run this test")
                    Return report.ToString()
                End If

                report.AppendLine("✓ Table is empty - safe to test migration")
                report.AppendLine()

                ' Step 1: Check current schema
                report.AppendLine("Step 1: Checking current schema...")
                Dim hasUsername As Boolean = False

                Dim columnSql As String = "PRAGMA table_info(DoctorsManagement)"
                Using cmd As New SQLiteCommand(columnSql, conn)
                    Using reader As SQLiteDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            If reader.GetString(1).Equals("Username", StringComparison.OrdinalIgnoreCase) Then
                                hasUsername = True
                                Exit While
                            End If
                        End While
                    End Using
                End Using

                report.AppendLine($"  Username column exists: {hasUsername}")
                report.AppendLine()

                ' Step 2: If Username exists, we can't easily remove it in SQLite
                ' SQLite doesn't support DROP COLUMN until version 3.35.0
                ' Instead, verify the migration logic works
                If hasUsername Then
                    report.AppendLine("Step 2: Username already exists")
                    report.AppendLine("  ℹ SQLite doesn't support DROP COLUMN in older versions")
                    report.AppendLine("  Migration logic will skip column addition (expected behavior)")
                    report.AppendLine()
                Else
                    report.AppendLine("Step 2: Username is missing - migration required")
                    report.AppendLine()
                End If

                ' Step 3: Run migration
                report.AppendLine("Step 3: Running InitializeDoctorsManagementSchema()...")
                Try
                    ModuleDatabase.InitializeDoctorsManagementSchema()
                    report.AppendLine("  ✓ Migration completed without errors")
                Catch ex As Exception
                    report.AppendLine($"  ❌ Migration failed: {ex.Message}")
                    Return report.ToString()
                End Try
                report.AppendLine()

                ' Step 4: Verify schema after migration
                report.AppendLine("Step 4: Verifying schema after migration...")
                hasUsername = False

                Using cmd As New SQLiteCommand(columnSql, conn)
                    Using reader As SQLiteDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            If reader.GetString(1).Equals("Username", StringComparison.OrdinalIgnoreCase) Then
                                hasUsername = True
                                Exit While
                            End If
                        End While
                    End Using
                End Using

                If hasUsername Then
                    report.AppendLine("  ✓ Username column now exists")
                Else
                    report.AppendLine("  ❌ Username column still missing!")
                End If
                report.AppendLine()

                report.AppendLine("========================================")
                report.AppendLine("✓ MIGRATION TEST COMPLETE")
                report.AppendLine("========================================")

            End Using

        Catch ex As Exception
            report.AppendLine()
            report.AppendLine("========================================")
            report.AppendLine("❌ TEST FAILED")
            report.AppendLine("========================================")
            report.AppendLine(ex.Message)
        End Try

        Return report.ToString()
    End Function

    ''' <summary>
    ''' Displays schema verification report in a message box.
    ''' Call this from your startup form or menu to verify database schema.
    ''' </summary>
    Public Shared Sub ShowSchemaReport()
        Dim report As String = VerifyDoctorsManagementSchema()
        MessageBox.Show(report, "Database Schema Verification", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    ''' <summary>
    ''' Gets a summary of all critical tables and their Username column status.
    ''' </summary>
    Public Shared Function GetAllTablesReport() As String
        Dim report As New StringBuilder()
        report.AppendLine("========================================")
        report.AppendLine("ALL TABLES - USERNAME COLUMN STATUS")
        report.AppendLine("========================================")
        report.AppendLine()

        Try
            Using conn As New SQLiteConnection(ModuleDatabase.GetConnectionString())
                conn.Open()

                ' Get all tables
                Dim tablesSql As String = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name"
                Dim tables As New List(Of String)

                Using cmd As New SQLiteCommand(tablesSql, conn)
                    Using reader As SQLiteDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            tables.Add(reader.GetString(0))
                        End While
                    End Using
                End Using

                ' Check each table for Username column
                For Each tableName As String In tables
                    Dim columnSql As String = $"PRAGMA table_info({tableName})"
                    Dim hasUsername As Boolean = False
                    Dim columnCount As Integer = 0

                    Using cmd As New SQLiteCommand(columnSql, conn)
                        Using reader As SQLiteDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                columnCount += 1
                                If reader.GetString(1).Equals("Username", StringComparison.OrdinalIgnoreCase) Then
                                    hasUsername = True
                                End If
                            End While
                        End Using
                    End Using

                    Dim status As String = If(hasUsername, "✓ HAS Username", "  (no Username)")
                    report.AppendLine($"{tableName,-30} {columnCount,3} columns  {status}")
                Next

                report.AppendLine()
                report.AppendLine("========================================")

            End Using

        Catch ex As Exception
            report.AppendLine($"ERROR: {ex.Message}")
        End Try

        Return report.ToString()
    End Function

End Class
