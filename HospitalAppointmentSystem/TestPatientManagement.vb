Option Strict On
Option Explicit On

' ============================================================
' TestPatientManagement.vb
' Quick launcher for testing Patient Management module
' ============================================================

Imports System.Windows.Forms

Public Module TestPatientManagement

    ''' <summary>
    ''' Standalone test launcher for Patient Management form
    ''' Call this from Program.vb Main() or add a button to FormMain
    ''' </summary>
    Public Sub LaunchPatientManagement()
        Try
            ' Initialize database if not already done
            If Not ModuleDatabase.TestConnection() Then
                ModuleDatabase.InitialiseDatabase()
            End If

            ' Create and show form
            Dim frmPatient As New FormPatientManagement()
            frmPatient.ShowDialog()

        Catch ex As Exception
            MessageBox.Show("Error launching Patient Management: " & ex.Message, _
                            "Launch Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Error)
            ModuleDatabase.LogError("LaunchPatientManagement error: " & ex.Message)
        End Try
    End Sub

End Module
