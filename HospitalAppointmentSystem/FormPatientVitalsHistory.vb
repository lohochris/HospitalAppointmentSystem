Option Strict On
Option Explicit On

' ============================================================
' FormPatientVitalsHistory.vb
' CSC3226 - Hospital Appointment System
' Patient Vitals History Viewer
' Group: Sa'id Umar, Aisha Ladan, Maryam Rabiu
' ============================================================

Imports System
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic

Public Class FormPatientVitalsHistory
    Inherits Form

#Region "Private Fields"
    Private _patientID As String
    Private _patientName As String

    ' UI Controls
    Private lblTitle As Label
    Private lblPatientInfo As Label
    Private lblRecordCount As Label
    Private dgvVitals As DataGridView
    Private btnClose As Button
    Private btnRefresh As Button
    Private btnDelete As Button
#End Region

#Region "Constructor"
    ''' <summary>
    ''' Initializes the vitals history form with patient context
    ''' </summary>
    Public Sub New(patientID As String, patientName As String)
        MyBase.New()

        If String.IsNullOrWhiteSpace(patientID) Then
            Throw New ArgumentException("PatientID cannot be null or empty", NameOf(patientID))
        End If

        _patientID = patientID
        _patientName = If(String.IsNullOrWhiteSpace(patientName), "Unknown Patient", patientName)

        InitializeComponent()
        LoadVitalsHistory()
    End Sub
#End Region

#Region "Form Initialization"
    Private Sub InitializeComponent()
        Me.Text = "Patient Vitals History"
        Me.Size = New Size(1000, 600)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.BackColor = Color.FromArgb(240, 248, 255)

        ' Title Label
        lblTitle = New Label With {
            .Text = "Vitals History",
            .Font = New Font("Segoe UI", 16.0!, FontStyle.Bold),
            .ForeColor = Color.FromArgb(41, 128, 185),
            .Location = New Point(20, 20),
            .AutoSize = True
        }

        ' Patient Info Label
        lblPatientInfo = New Label With {
            .Text = $"Patient: {_patientName} (ID: {_patientID})",
            .Font = New Font("Segoe UI", 11.0!, FontStyle.Regular),
            .ForeColor = Color.FromArgb(52, 73, 94),
            .Location = New Point(20, 55),
            .AutoSize = True
        }

        ' Record Count Label
        lblRecordCount = New Label With {
            .Text = "Total Records: 0",
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Italic),
            .ForeColor = Color.FromArgb(127, 140, 141),
            .Location = New Point(20, 85),
            .AutoSize = True
        }

        ' DataGridView for Vitals History
        dgvVitals = New DataGridView With {
            .Location = New Point(20, 120),
            .Size = New Size(940, 360),
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .ReadOnly = True,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .MultiSelect = False,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            .RowHeadersVisible = False,
            .EnableHeadersVisualStyles = False
        }

        ' Header styling
        With dgvVitals.ColumnHeadersDefaultCellStyle
            .BackColor = Color.FromArgb(41, 128, 185)
            .ForeColor = Color.White
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
        End With
        dgvVitals.ColumnHeadersHeight = 40

        ' Alternating row colors
        dgvVitals.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(236, 240, 241)
        dgvVitals.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219)
        dgvVitals.DefaultCellStyle.SelectionForeColor = Color.White
        dgvVitals.DefaultCellStyle.Font = New Font("Segoe UI", 9.5!)
        dgvVitals.RowTemplate.Height = 35

        ' Refresh Button
        btnRefresh = New Button With {
            .Text = "Refresh",
            .Location = New Point(700, 500),
            .Size = New Size(120, 40),
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Bold),
            .BackColor = Color.FromArgb(52, 152, 219),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        AddHandler btnRefresh.Click, AddressOf btnRefresh_Click

        ' Delete Button
        btnDelete = New Button With {
            .Text = "Delete Record",
            .Location = New Point(560, 500),
            .Size = New Size(120, 40),
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Bold),
            .BackColor = Color.FromArgb(231, 76, 60),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand,
            .Enabled = False
        }
        AddHandler btnDelete.Click, AddressOf btnDelete_Click

        ' Close Button
        btnClose = New Button With {
            .Text = "Close",
            .Location = New Point(840, 500),
            .Size = New Size(120, 40),
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Bold),
            .BackColor = Color.FromArgb(149, 165, 166),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        AddHandler btnClose.Click, AddressOf btnClose_Click

        ' Add selection changed handler for enabling delete button
        AddHandler dgvVitals.SelectionChanged, AddressOf dgvVitals_SelectionChanged

        ' Add all controls to form
        Me.Controls.AddRange(New Control() {
            lblTitle, lblPatientInfo, lblRecordCount,
            dgvVitals, btnRefresh, btnDelete, btnClose
        })
    End Sub
#End Region

#Region "Data Loading"
    Private Sub LoadVitalsHistory()
        Try
            Dim dt As DataTable = ModuleDatabase.GetPatientVitalsHistory(_patientID)

            If dt IsNot Nothing Then
                dgvVitals.DataSource = dt

                ' Update record count
                lblRecordCount.Text = $"Total Records: {dt.Rows.Count}"

                ' Hide VitalID column (internal use only)
                If dgvVitals.Columns.Contains("Vital ID") Then
                    dgvVitals.Columns("Vital ID").Visible = False
                End If

                ' Hide PatientID column (already shown in header)
                If dgvVitals.Columns.Contains("Patient ID") Then
                    dgvVitals.Columns("Patient ID").Visible = False
                End If

                ' Format Date column
                If dgvVitals.Columns.Contains("Date Recorded") Then
                    dgvVitals.Columns("Date Recorded").DefaultCellStyle.Format = "yyyy-MM-dd HH:mm"
                End If

            Else
                MessageBox.Show("Failed to load vitals history.", _
                                "Load Error", _
                                MessageBoxButtons.OK, _
                                MessageBoxIcon.Error)
            End If

        Catch ex As Exception
            MessageBox.Show("Error loading vitals history: " & ex.Message, _
                            "Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Error)
            ModuleDatabase.LogError("FormPatientVitalsHistory.LoadVitalsHistory error: " & ex.Message)
        End Try
    End Sub
#End Region

#Region "Event Handlers"
    Private Sub dgvVitals_SelectionChanged(sender As Object, e As EventArgs)
        ' Enable delete button only if a row is selected
        btnDelete.Enabled = (dgvVitals.SelectedRows.Count > 0)
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs)
        LoadVitalsHistory()
    End Sub

    Private Sub btnDelete_Click(sender As Object, e As EventArgs)
        Try
            If dgvVitals.SelectedRows.Count = 0 Then
                Return
            End If

            ' Confirm deletion
            Dim result As DialogResult = MessageBox.Show(
                "Are you sure you want to delete this vitals record?" & vbCrLf & vbCrLf & _
                "WARNING: This action cannot be undone and may affect audit trails.",
                "Confirm Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning)

            If result <> DialogResult.Yes Then
                Return
            End If

            ' Get the VitalID from the hidden column
            Dim selectedRow As DataGridViewRow = dgvVitals.SelectedRows(0)
            Dim vitalID As Integer = 0

            ' Safely retrieve VitalID
            If dgvVitals.Columns.Contains("Vital ID") Then
                Dim cellValue As Object = selectedRow.Cells("Vital ID").Value
                If cellValue IsNot Nothing AndAlso Not DBNull.Value.Equals(cellValue) Then
                    vitalID = Convert.ToInt32(cellValue)
                End If
            End If

            If vitalID <= 0 Then
                MessageBox.Show("Cannot identify the vitals record to delete.", _
                                "Delete Error", _
                                MessageBoxButtons.OK, _
                                MessageBoxIcon.Error)
                Return
            End If

            ' Delete the record
            If ModuleDatabase.DeleteVitalsRecord(vitalID) Then
                MessageBox.Show("Vitals record deleted successfully.", _
                                "Success", _
                                MessageBoxButtons.OK, _
                                MessageBoxIcon.Information)
                LoadVitalsHistory() ' Refresh grid
            Else
                MessageBox.Show("Failed to delete vitals record. Please check the error log.", _
                                "Delete Error", _
                                MessageBoxButtons.OK, _
                                MessageBoxIcon.Error)
            End If

        Catch ex As Exception
            MessageBox.Show("Error deleting vitals record: " & ex.Message, _
                            "Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Error)
            ModuleDatabase.LogError("FormPatientVitalsHistory.btnDelete_Click error: " & ex.Message)
        End Try
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs)
        Me.Close()
    End Sub
#End Region

End Class
