Option Strict On
Option Explicit On

' ============================================================
' FormPatientVitals.vb
' CSC3226 - Hospital Appointment System
' Patient Vitals Recording Form
' Group: Sa'id Umar, Aisha Ladan, Maryam Rabiu
' ============================================================

Imports System
Imports System.Drawing
Imports System.Windows.Forms

Public Class FormPatientVitals
    Inherits Form

#Region "Private Fields"
    Private _patientID As String
    Private _patientName As String

    ' UI Controls
    Private lblTitle As Label
    Private lblPatientInfo As Label
    Private txtBloodPressure As TextBox
    Private nudHeartRate As NumericUpDown
    Private nudTemperature As NumericUpDown
    Private nudSpO2 As NumericUpDown
    Private nudWeight As NumericUpDown
    Private btnSave As Button
    Private btnCancel As Button
    Private grpVitals As GroupBox
#End Region

#Region "Constructor"
    ''' <summary>
    ''' Initializes the vitals form with patient context
    ''' </summary>
    Public Sub New(patientID As String, patientName As String)
        MyBase.New()

        If String.IsNullOrWhiteSpace(patientID) Then
            Throw New ArgumentException("PatientID cannot be null or empty", NameOf(patientID))
        End If

        _patientID = patientID
        _patientName = If(String.IsNullOrWhiteSpace(patientName), "Unknown Patient", patientName)

        InitializeComponent()
    End Sub
#End Region

#Region "Form Initialization"
    Private Sub InitializeComponent()
        Me.Text = "Record Patient Vitals"
        Me.Size = New Size(500, 550)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.BackColor = Color.FromArgb(240, 248, 255)

        ' Title Label
        lblTitle = New Label With {
            .Text = "Patient Vitals Recording",
            .Font = New Font("Segoe UI", 16.0!, FontStyle.Bold),
            .ForeColor = Color.FromArgb(41, 128, 185),
            .Location = New Point(20, 20),
            .AutoSize = True
        }

        ' Patient Info Label
        lblPatientInfo = New Label With {
            .Text = $"Patient: {_patientName} (ID: {_patientID})",
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Regular),
            .ForeColor = Color.FromArgb(52, 73, 94),
            .Location = New Point(20, 55),
            .AutoSize = True
        }

        ' Group Box for Vitals
        grpVitals = New GroupBox With {
            .Text = "Vital Signs",
            .Font = New Font("Segoe UI", 11.0!, FontStyle.Bold),
            .Location = New Point(20, 90),
            .Size = New Size(440, 330),
            .ForeColor = Color.FromArgb(41, 128, 185)
        }

        ' Blood Pressure
        Dim lblBP As New Label With {
            .Text = "Blood Pressure (e.g., 120/80):",
            .Font = New Font("Segoe UI", 9.5!),
            .Location = New Point(20, 40),
            .Size = New Size(200, 20)
        }
        txtBloodPressure = New TextBox With {
            .Location = New Point(240, 37),
            .Size = New Size(170, 25),
            .Font = New Font("Segoe UI", 10.0!)
        }

        ' Heart Rate
        Dim lblHR As New Label With {
            .Text = "Heart Rate (bpm):",
            .Font = New Font("Segoe UI", 9.5!),
            .Location = New Point(20, 90),
            .Size = New Size(200, 20)
        }
        nudHeartRate = New NumericUpDown With {
            .Location = New Point(240, 87),
            .Size = New Size(170, 25),
            .Font = New Font("Segoe UI", 10.0!),
            .Minimum = 30,
            .Maximum = 250,
            .Value = 72
        }

        ' Temperature
        Dim lblTemp As New Label With {
            .Text = "Temperature (°C):",
            .Font = New Font("Segoe UI", 9.5!),
            .Location = New Point(20, 140),
            .Size = New Size(200, 20)
        }
        nudTemperature = New NumericUpDown With {
            .Location = New Point(240, 137),
            .Size = New Size(170, 25),
            .Font = New Font("Segoe UI", 10.0!),
            .Minimum = 30.0D,
            .Maximum = 45.0D,
            .DecimalPlaces = 1,
            .Increment = 0.1D,
            .Value = 37.0D
        }

        ' SpO2
        Dim lblSpO2 As New Label With {
            .Text = "SpO2 (%):",
            .Font = New Font("Segoe UI", 9.5!),
            .Location = New Point(20, 190),
            .Size = New Size(200, 20)
        }
        nudSpO2 = New NumericUpDown With {
            .Location = New Point(240, 187),
            .Size = New Size(170, 25),
            .Font = New Font("Segoe UI", 10.0!),
            .Minimum = 50,
            .Maximum = 100,
            .Value = 98
        }

        ' Weight
        Dim lblWeight As New Label With {
            .Text = "Weight (kg):",
            .Font = New Font("Segoe UI", 9.5!),
            .Location = New Point(20, 240),
            .Size = New Size(200, 20)
        }
        nudWeight = New NumericUpDown With {
            .Location = New Point(240, 237),
            .Size = New Size(170, 25),
            .Font = New Font("Segoe UI", 10.0!),
            .Minimum = 1.0D,
            .Maximum = 500.0D,
            .DecimalPlaces = 1,
            .Increment = 0.1D,
            .Value = 70.0D
        }

        ' Add controls to group box
        grpVitals.Controls.AddRange(New Control() {
            lblBP, txtBloodPressure,
            lblHR, nudHeartRate,
            lblTemp, nudTemperature,
            lblSpO2, nudSpO2,
            lblWeight, nudWeight
        })

        ' Save Button
        btnSave = New Button With {
            .Text = "Save Vitals",
            .Location = New Point(130, 440),
            .Size = New Size(120, 40),
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Bold),
            .BackColor = Color.FromArgb(39, 174, 96),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        AddHandler btnSave.Click, AddressOf btnSave_Click

        ' Cancel Button
        btnCancel = New Button With {
            .Text = "Cancel",
            .Location = New Point(270, 440),
            .Size = New Size(120, 40),
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Bold),
            .BackColor = Color.FromArgb(231, 76, 60),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand,
            .DialogResult = DialogResult.Cancel
        }
        AddHandler btnCancel.Click, AddressOf btnCancel_Click

        ' Add all controls to form
        Me.Controls.AddRange(New Control() {
            lblTitle, lblPatientInfo, grpVitals, btnSave, btnCancel
        })

        Me.CancelButton = btnCancel
    End Sub
#End Region

#Region "Event Handlers"
    Private Sub btnSave_Click(sender As Object, e As EventArgs)
        Try
            ' Validate Blood Pressure format
            Dim bp As String = txtBloodPressure.Text.Trim()
            If Not String.IsNullOrWhiteSpace(bp) Then
                If Not bp.Contains("/") Then
                    MessageBox.Show("Blood Pressure must be in format: systolic/diastolic (e.g., 120/80)", _
                                    "Invalid Format", _
                                    MessageBoxButtons.OK, _
                                    MessageBoxIcon.Warning)
                    txtBloodPressure.Focus()
                    Return
                End If
            End If

            ' Save vitals to database
            Dim success As Boolean = ModuleDatabase.SavePatientVitals(
                _patientID,
                bp,
                CInt(nudHeartRate.Value),
                CDbl(nudTemperature.Value),
                CInt(nudSpO2.Value),
                CDbl(nudWeight.Value)
            )

            If success Then
                MessageBox.Show("Patient vitals recorded successfully!", _
                                "Success", _
                                MessageBoxButtons.OK, _
                                MessageBoxIcon.Information)
                Me.DialogResult = DialogResult.OK
                Me.Close()
            Else
                MessageBox.Show("Failed to save patient vitals. Please check the error log.", _
                                "Save Error", _
                                MessageBoxButtons.OK, _
                                MessageBoxIcon.Error)
            End If

        Catch ex As Exception
            MessageBox.Show("Error saving vitals: " & ex.Message, _
                            "Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Error)
            ModuleDatabase.LogError("FormPatientVitals.btnSave_Click error: " & ex.Message)
        End Try
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs)
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub
#End Region

End Class
