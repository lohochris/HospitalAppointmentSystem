Option Strict On
Option Explicit On

' ============================================================
' FormAdminAnalytics.vb
' CSC3226 - Hospital Appointment System
' Admin Analytics Dashboard with charts and statistics
' Group: Sa'id Umar, Aisha Ladan, Maryam Rabiu
' ============================================================
Imports System.Data
Imports System.Data.SqlClient
Imports System.Drawing
Imports System.Windows.Forms
Imports System.Windows.Forms.DataVisualization.Charting
Imports System.IO

Public Class FormAdminAnalytics
    Inherits Form

#Region "Controls"
    Private WithEvents btnExportCSV As Button
    Private WithEvents btnRefresh As Button
    Private WithEvents btnClose As Button
    Private lblTotalPatients As Label
    Private lblTotalAppts As Label
    Private lblTodayAppts As Label
    Private lblEmergency As Label
    Private lblCompleted As Label
    Private lblWaiting As Label
    Private chartDoctorAppts As Chart
    Private chartMonthly As Chart
    Private dgvSummary As DataGridView
#End Region

#Region "Initialization"
    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Size = New Size(1100, 700)
        Me.Text = "Admin Analytics Dashboard"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.BackColor = Color.FromArgb(240, 248, 255)

        ' Header
        Dim pnlHeader As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 60,
            .BackColor = Color.FromArgb(0, 84, 126)
        }
        Dim lblTitle As New Label With {
            .Text = "Admin Analytics Dashboard",
            .Font = New Font("Arial", 16, FontStyle.Bold),
            .ForeColor = Color.White,
            .Location = New Point(20, 15),
            .AutoSize = True
        }
        pnlHeader.Controls.Add(lblTitle)

        ' Button toolbar
        Dim pnlToolbar As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 50,
            .BackColor = Color.FromArgb(230, 245, 255),
            .Padding = New Padding(10)
        }
        btnRefresh = New Button With {
            .Text = "Refresh",
            .Location = New Point(10, 10),
            .Size = New Size(110, 32),
            .Font = New Font("Arial", 9),
            .BackColor = Color.FromArgb(0, 102, 153),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        btnExportCSV = New Button With {
            .Text = "Export CSV",
            .Location = New Point(130, 10),
            .Size = New Size(120, 32),
            .Font = New Font("Arial", 9),
            .BackColor = Color.FromArgb(0, 153, 76),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        btnClose = New Button With {
            .Text = "Close",
            .Location = New Point(260, 10),
            .Size = New Size(80, 32),
            .Font = New Font("Arial", 9),
            .BackColor = Color.Gray,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        pnlToolbar.Controls.AddRange(New Control() {btnRefresh, btnExportCSV, btnClose})

        ' Stats cards panel
        Dim pnlStats As New FlowLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 115,
            .Padding = New Padding(10, 8, 10, 8),
            .BackColor = Color.FromArgb(240, 248, 255),
            .FlowDirection = FlowDirection.LeftToRight
        }

        ' Demonstrates: Function to create stat cards (Sub procedure)
        Dim CreateCard As Func(Of String, String, Color, Label) = Function(title As String, value As String, color As Color) As Label
                                                                      Dim pnl As New Panel With {
                                                                          .Size = New Size(155, 90),
                                                                          .BackColor = color,
                                                                          .Margin = New Padding(5)
                                                                      }
                                                                      Dim lTitle As New Label With {
                                                                          .Text = title,
                                                                          .Font = New Font("Arial", 8, FontStyle.Bold),
                                                                          .ForeColor = Color.White,
                                                                          .Location = New Point(8, 8),
                                                                          .AutoSize = True
                                                                      }
                                                                      Dim lValue As New Label With {
                                                                          .Text = value,
                                                                          .Font = New Font("Arial", 22, FontStyle.Bold),
                                                                          .ForeColor = Color.White,
                                                                          .Location = New Point(8, 30),
                                                                          .AutoSize = True,
                                                                          .Name = "valLabel"
                                                                      }
                                                                      pnl.Controls.AddRange(New Control() {lTitle, lValue})
                                                                      pnlStats.Controls.Add(pnl)
                                                                      Return lValue
                                                                  End Function

        lblTotalPatients = CreateCard("Total Patients", "0", Color.FromArgb(0, 102, 153))
        lblTotalAppts = CreateCard("Total Appointments", "0", Color.FromArgb(0, 130, 100))
        lblTodayAppts = CreateCard("Today's Appointments", "0", Color.FromArgb(180, 100, 0))
        lblEmergency = CreateCard("Emergencies Today", "0", Color.FromArgb(180, 30, 30))
        lblCompleted = CreateCard("Completed Today", "0", Color.FromArgb(0, 100, 180))
        lblWaiting = CreateCard("Waiting in Queue", "0", Color.FromArgb(100, 50, 150))

        ' Charts and grid
        Dim pnlCharts As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(10)}

        ' Chart: Appointments per doctor
        chartDoctorAppts = New Chart With {
            .Location = New Point(10, 10),
            .Size = New Size(490, 280),
            .BackColor = Color.White
        }
        chartDoctorAppts.ChartAreas.Add(New ChartArea("Main") With {
            .BackColor = Color.White,
            .AxisX = New Axis With {.MajorGrid = New Grid With {.Enabled = False}},
            .AxisY = New Axis With {.Title = "Appointments", .TitleFont = New Font("Arial", 8)}
        })
        chartDoctorAppts.Titles.Add(New Title("Appointments per Doctor") With {
            .Font = New Font("Arial", 10, FontStyle.Bold),
            .ForeColor = Color.FromArgb(0, 102, 153)
        })
        chartDoctorAppts.Series.Add(New Series("Appointments") With {
            .ChartType = SeriesChartType.Bar,
            .Color = Color.FromArgb(0, 102, 153),
            .IsValueShownAsLabel = True,
            .Font = New Font("Arial", 8)
        })

        ' Chart: Monthly trend
        chartMonthly = New Chart With {
            .Location = New Point(510, 10),
            .Size = New Size(490, 280),
            .BackColor = Color.White
        }
        chartMonthly.ChartAreas.Add(New ChartArea("Main2") With {
            .BackColor = Color.White,
            .AxisX = New Axis With {.MajorGrid = New Grid With {.Enabled = False}},
            .AxisY = New Axis With {.Title = "Appointments", .TitleFont = New Font("Arial", 8)}
        })
        chartMonthly.Titles.Add(New Title("Monthly Appointment Trend") With {
            .Font = New Font("Arial", 10, FontStyle.Bold),
            .ForeColor = Color.FromArgb(0, 130, 100)
        })
        chartMonthly.Series.Add(New Series("Monthly") With {
            .ChartType = SeriesChartType.Line,
            .Color = Color.FromArgb(0, 153, 76),
            .BorderWidth = 3,
            .MarkerStyle = MarkerStyle.Circle,
            .MarkerSize = 8,
            .IsValueShownAsLabel = True,
            .Font = New Font("Arial", 8)
        })

        ' Summary grid
        dgvSummary = New DataGridView With {
            .Location = New Point(10, 300),
            .Size = New Size(990, 200),
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            .ReadOnly = True,
            .AllowUserToAddRows = False,
            .RowHeadersVisible = False,
            .Font = New Font("Arial", 9),
            .BackgroundColor = Color.White,
            .BorderStyle = BorderStyle.FixedSingle
        }
        dgvSummary.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 102, 153)
        dgvSummary.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        dgvSummary.ColumnHeadersDefaultCellStyle.Font = New Font("Arial", 9, FontStyle.Bold)

        pnlCharts.Controls.AddRange(New Control() {chartDoctorAppts, chartMonthly, dgvSummary})

        Me.Controls.Add(pnlCharts)
        Me.Controls.Add(pnlStats)
        Me.Controls.Add(pnlToolbar)
        Me.Controls.Add(pnlHeader)
    End Sub
#End Region

#Region "Form Load and Refresh"
    Private Sub FormAdminAnalytics_Load(sender As Object, e As EventArgs) Handles Me.Load
        ' Demonstrates: Exception handling on load
        Try
            If SessionManager.CurrentUser Is Nothing OrElse SessionManager.CurrentUser.Role <> "Admin" Then
                MessageBox.Show("Access denied. Admin role required.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Me.Close()
                Return
            End If
            LoadAnalytics()
        Catch ex As Exception
            MessageBox.Show("Error loading analytics: " & ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
            LogError("FormAdminAnalytics_Load: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Loads all analytics data.
    ''' Demonstrates: Exception handling, Dictionary, For Each loop
    ''' </summary>
    Private Sub LoadAnalytics()
        Try
            ' Load stat cards - uses Dictionary(Of String, Integer)
            Dim stats As Dictionary(Of String, Integer) = GetAnalyticsSummary()
            lblTotalPatients.Text = stats("TotalPatients").ToString()
            lblTotalAppts.Text = stats("TotalAppointments").ToString()
            lblTodayAppts.Text = stats("TodayAppointments").ToString()
            lblEmergency.Text = stats("EmergencyToday").ToString()
            lblCompleted.Text = stats("CompletedToday").ToString()
            lblWaiting.Text = stats("WaitingQueue").ToString()

            ' Load doctor appointments chart
            chartDoctorAppts.Series("Appointments").Points.Clear()
            Dim dtDoc As DataTable = GetAppointmentsByDoctor()
            ' Demonstrates: For Each loop populating chart
            For Each row As DataRow In dtDoc.Rows
                chartDoctorAppts.Series("Appointments").Points.AddXY(
                    row("DoctorName").ToString().Replace("Dr. ", ""),
                    CInt(row("TotalAppointments")))
            Next

            ' Load monthly trend chart
            chartMonthly.Series("Monthly").Points.Clear()
            Dim dtMonth As DataTable = GetMonthlyTrend()
            For Each row As DataRow In dtMonth.Rows
                chartMonthly.Series("Monthly").Points.AddXY(
                    row("Month").ToString(),
                    CInt(row("Count")))
            Next

            ' Load summary grid - all appointments
            dgvSummary.DataSource = GetTodayAppointments()

            ' Calculate emergency percentage
            Dim total As Integer = stats("TodayAppointments")
            Dim emergency As Integer = stats("EmergencyToday")
            Dim pct As Double = If(total > 0, Math.Round(CDbl(emergency) / CDbl(total) * 100, 1), 0)

            Me.Text = String.Format("Admin Analytics Dashboard | Emergency Rate Today: {0}% | {1:dd/MM/yyyy HH:mm}", pct, DateTime.Now)

        Catch ex As Exception
            MessageBox.Show("Error loading analytics data: " & ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
            LogError("LoadAnalytics: " & ex.Message)
        End Try
    End Sub
#End Region

#Region "Button Events"
    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        LoadAnalytics()
        MessageBox.Show("Analytics refreshed successfully.", "Refreshed",
            MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnExportCSV_Click(sender As Object, e As EventArgs) Handles btnExportCSV.Click
        ' Demonstrates: File handling - export to CSV
        Try
            Dim sfd As New SaveFileDialog With {
                .Filter = "CSV Files (*.csv)|*.csv",
                .FileName = "HospitalAnalytics_" & DateTime.Now.ToString("yyyyMMdd_HHmm") & ".csv"
            }
            If sfd.ShowDialog() = DialogResult.OK Then
                If ExportAnalyticsToCSV(sfd.FileName) Then
                    MessageBox.Show("Analytics exported successfully to:" & Environment.NewLine & sfd.FileName,
                        "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Else
                    MessageBox.Show("Export failed. Check error_log.txt for details.", "Export Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If
            End If
        Catch ex As Exception
            MessageBox.Show("Export error: " & ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
            LogError("btnExportCSV_Click: " & ex.Message)
        End Try
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
#End Region

#Region "Helper Methods (Placeholder implementations)"
    Private Function GetAnalyticsSummary() As Dictionary(Of String, Integer)
        ' TODO: Implement actual database queries
        Dim result As New Dictionary(Of String, Integer)()
        result.Add("TotalPatients", 150)
        result.Add("TotalAppointments", 320)
        result.Add("TodayAppointments", 25)
        result.Add("EmergencyToday", 3)
        result.Add("CompletedToday", 18)
        result.Add("WaitingQueue", 7)
        Return result
    End Function

    Private Function GetAppointmentsByDoctor() As DataTable
        Dim dt As New DataTable()
        dt.Columns.Add("DoctorName", GetType(String))
        dt.Columns.Add("TotalAppointments", GetType(Integer))
        ' TODO: Populate from database
        dt.Rows.Add("Dr. Smith", 45)
        dt.Rows.Add("Dr. Jones", 38)
        dt.Rows.Add("Dr. Williams", 52)
        Return dt
    End Function

    Private Function GetMonthlyTrend() As DataTable
        Dim dt As New DataTable()
        dt.Columns.Add("Month", GetType(String))
        dt.Columns.Add("Count", GetType(Integer))
        dt.Rows.Add("Jan", 85)
        dt.Rows.Add("Feb", 92)
        dt.Rows.Add("Mar", 78)
        Return dt
    End Function

    Private Function GetTodayAppointments() As DataTable
        Dim dt As New DataTable()
        dt.Columns.Add("PatientName", GetType(String))
        dt.Columns.Add("DoctorName", GetType(String))
        dt.Columns.Add("Time", GetType(String))
        dt.Columns.Add("Status", GetType(String))
        dt.Rows.Add("John Doe", "Dr. Smith", "09:00 AM", "Completed")
        dt.Rows.Add("Jane Roe", "Dr. Jones", "10:30 AM", "Waiting")
        Return dt
    End Function

    Private Function ExportAnalyticsToCSV(filePath As String) As Boolean
        Try
            ' TODO: Implement actual export logic
            Return True
        Catch ex As Exception
            LogError("ExportAnalyticsToCSV: " & ex.Message)
            Return False
        End Try
    End Function

    Private Sub LogError(message As String)
        ' TODO: Implement logging to file
        System.Diagnostics.Debug.WriteLine(message)
    End Sub
#End Region

End Class