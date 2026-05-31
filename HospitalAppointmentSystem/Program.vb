Option Strict On
Option Explicit On

' ============================================================
' Program.vb
' CSC3226 - Hospital Appointment System
' Application entry point
' Group: Sa'id Umar, Aisha Ladan, Maryam Rabiu
' ============================================================

Imports System.Windows.Forms

Module Program
    <STAThread>
    Sub Main()
        ' Demonstrates: Exception handling at application level
        Try
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)

            ' Initialize database on startup
            ModuleDatabase.InitialiseDatabase()

            ' Test connection before proceeding
            If Not ModuleDatabase.TestConnection() Then
                MessageBox.Show(
                    "Failed to connect to the database. Please ensure HospitalDB.db is accessible.",
                    "Database Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            Application.Run(New FormLogin())
        Catch ex As Exception
            MessageBox.Show(
                "A fatal error occurred:" & Environment.NewLine & ex.Message & Environment.NewLine & Environment.NewLine &
                "Stack Trace:" & Environment.NewLine & ex.StackTrace,
                "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Module
