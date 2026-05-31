Option Strict On
Option Explicit On

' ============================================================
' FormQueueManagement.vb
' CSC3226 - Hospital Appointment System
' Live queue tracking and management
' Group: Sa'id Umar, Aisha Ladan, Maryam Rabiu
' ============================================================

Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

Public Class FormQueueManagement
    Inherits Form

#Region "Controls"
    Private WithEvents btnCallNext As Button
    Private WithEvents btnComplete As Button
    Private WithEvents btnRefresh As Button
    Private WithEvents btnCheckTicket As Button
    Private WithEvents tmrRefresh As Timer
    Private lstQueue As ListBox
    Private dgvQueue As DataGridView
    Private lblCurrentTicket As Label
    Private lblWaitingCount As Label
    Private lblNowServing As Label
    Private txtCheckTicket As TextBox
    Private lblTicketStatus As Label
#End Region

#Region "Data"
    ' Demonstrates: List(Of T) collection
    Private _queueEntries As New List(Of QueueEntry)()
    Private _currentTicket As String = ""
#End Region

#Region "Initialization"
    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Size = New Size(900, 620)
        Me.Text = "Queue Management System"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.BackColor = Color.FromArgb(240, 248, 255)

        ' Header
        Dim pnlHeader As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 90,
            .BackColor = Color.FromArgb(0, 102, 153)
        }
        Dim lblTitle As New Label With {
            .Text = "🔢 Live Queue Management",
            .Font = New Font("Arial", 16, FontStyle.Bold),
            .ForeColor = Color.White,
            .Location = New Point(20, 10),
            .AutoSize = True
        }
        lblNowServing = New Label With {
            .Text = "NOW SERVING: ---",
            .Font = New Font("Arial", 18, FontStyle.Bold),
            .ForeColor = Color.FromArgb(255, 230, 0),
            .Location = New Point(20, 45),
            .AutoSize = True
        }
        lblWaitingCount = New Label With {
            .Text = "Waiting: 0",
            .Font = New Font("Arial", 12),
            .ForeColor = Color.LightBlue,
            .Location = New Point(500, 45),
            .AutoSize = True
        }
        pnlHeader.Controls.AddRange(New Control() {lblTitle, lblNowServing, lblWaitingCount})

        ' Left panel - queue list
        Dim pnlLeft As New Panel With {
            .Width = 320,
            .Dock = DockStyle.Left,
            .Padding = New Padding(10),
            .BackColor = Color.White,
            .BorderStyle = BorderStyle.FixedSingle
        }

        Dim lblQueueTitle As New Label With {
            .Text = "Current Queue",
            .Font = New Font("Arial", 11, FontStyle.Bold),
            .ForeColor = Color.FromArgb(0, 102, 153),
            .Dock = DockStyle.Top,
            .Height = 30,
            .TextAlign = ContentAlignment.MiddleCenter
        }

        lstQueue = New ListBox With {
            .Dock = DockStyle.Fill,
            .Font = New Font("Courier New", 9),
            .DrawMode = DrawMode.OwnerDrawFixed,
            .ItemHeight = 30,
            .BorderStyle = BorderStyle.None
        }
        AddHandler lstQueue.DrawItem, AddressOf LstQueue_DrawItem

        pnlLeft.Controls.Add(lstQueue)
        pnlLeft.Controls.Add(lblQueueTitle)

        ' Right panel - details and actions
        Dim pnlRight As New Panel With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(10)
        }

        ' Action buttons
        Dim pnlActions As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 120,
            .BackColor = Color.FromArgb(240, 248, 255),
            .Padding = New Padding(10)
        }

        btnCallNext = New Button With {
            .Text = "📢  CALL NEXT PATIENT",
            .Location = New Point(10, 15),
            .Size = New Size(220, 50),
            .Font = New Font("Arial", 11, FontStyle.Bold),
            .BackColor = Color.FromArgb(0, 153, 76),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        btnCallNext.FlatAppearance.BorderSize = 0

        btnComplete = New Button With {
            .Text = "✅  Mark Complete",
            .Location = New Point(240, 15),
            .Size = New Size(150, 50),
            .Font = New Font("Arial", 10),
            .BackColor = Color.FromArgb(0, 102, 204),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        btnComplete.FlatAppearance.BorderSize = 0

        btnRefresh = New Button With {
            .Text = "🔄 Refresh",
            .Location = New Point(400, 15),
            .Size = New Size(100, 50),
            .Font = New Font("Arial", 9),
            .BackColor = Color.Gray,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }

        lblCurrentTicket = New Label With {
            .Location = New Point(10, 75),
            .Size = New Size(490, 30),
            .Font = New Font("Arial", 10),
            .ForeColor = Color.DarkGreen,
            .Text = "Click 'Call Next' to call the next waiting patient"
        }

        pnlActions.Controls.AddRange(New Control() {btnCallNext, btnComplete, btnRefresh, lblCurrentTicket})

        ' Ticket check panel
        Dim pnlTicket As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 70,
            .BackColor = Color.FromArgb(230, 245, 255),
            .Padding = New Padding(10),
            .BorderStyle = BorderStyle.FixedSingle
        }
        Dim lblCheckTitle As New Label With {.Text = "Check Ticket Position:", .Location = New Point(10, 25), .AutoSize = True, .Font = New Font("Arial", 9, FontStyle.Bold)}
        txtCheckTicket = New TextBox With {
            .Location = New Point(170, 22),
            .Size = New Size(120, 25),
            .Font = New Font("Arial", 9),
            .Text = "e.g. TKT-001",
            .ForeColor = Color.Gray
        }
        btnCheckTicket = New Button With {
            .Text = "Check",
            .Location = New Point(300, 21),
            .Size = New Size(80, 27),
            .Font = New Font("Arial", 9),
            .BackColor = Color.FromArgb(0, 102, 153),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        lblTicketStatus = New Label With {
            .Location = New Point(390, 25),
            .AutoSize = True,
            .Font = New Font("Arial", 9, FontStyle.Bold),
            .ForeColor = Color.DarkBlue
        }
        pnlTicket.Controls.AddRange(New Control() {lblCheckTitle, txtCheckTicket, btnCheckTicket, lblTicketStatus})

        ' Full queue grid
        dgvQueue = New DataGridView With {
            .Dock = DockStyle.Fill,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            .ReadOnly = True,
            .AllowUserToAddRows = False,
            .RowHeadersVisible = False,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .Font = New Font("Arial", 9),
            .BackgroundColor = Color.White,
            .BorderStyle = BorderStyle.None,
            .GridColor = Color.FromArgb(200, 220, 240)
        }
        dgvQueue.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 102, 153)
        dgvQueue.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        dgvQueue.ColumnHeadersDefaultCellStyle.Font = New Font("Arial", 9, FontStyle.Bold)

        pnlRight.Controls.Add(dgvQueue)
        pnlRight.Controls.Add(pnlTicket)
        pnlRight.Controls.Add(pnlActions)

        tmrRefresh = New Timer With {.Interval = 15000, .Enabled = True}

        Me.Controls.Add(pnlRight)
        Me.Controls.Add(pnlLeft)
        Me.Controls.Add(pnlHeader)
    End Sub
#End Region

#Region "Custom List Drawing"
    ''' <summary>
    ''' Custom drawing for queue list items - highlights emergency in red.
    ''' Demonstrates: For Each loop, conditional statement
    ''' </summary>
    Private Sub LstQueue_DrawItem(sender As Object, e As DrawItemEventArgs)
        If e.Index < 0 OrElse e.Index >= _queueEntries.Count Then Return
        Dim entry As QueueEntry = _queueEntries(e.Index)

        ' Demonstrates: Conditional statement for colour selection
        Dim bgColor As Color
        If entry.IsEmergency Then
            bgColor = Color.FromArgb(255, 200, 200) ' Red for emergency
        ElseIf entry.Status = "Called" Then
            bgColor = Color.FromArgb(200, 255, 200) ' Green for called
        ElseIf (e.State And DrawItemState.Selected) = DrawItemState.Selected Then
            bgColor = Color.FromArgb(180, 220, 255)
        Else
            bgColor = If(e.Index Mod 2 = 0, Color.White, Color.FromArgb(245, 250, 255))
        End If

        e.Graphics.FillRectangle(New SolidBrush(bgColor), e.Bounds)

        Dim emgTag As String = If(entry.IsEmergency, " 🚨 EMERG", "")
        Dim text As String = $"  #{entry.QueuePosition}  {entry.TicketNumber}  {entry.PatientName}{emgTag}"
        Dim waitText As String = $"  Wait: {entry.GetWaitDisplay()}"

        e.Graphics.DrawString(text, New Font("Courier New", 8, FontStyle.Bold),
            New SolidBrush(If(entry.IsEmergency, Color.DarkRed, Color.Black)),
            New RectangleF(CSng(e.Bounds.X), CSng(e.Bounds.Y), CSng(e.Bounds.Width), CSng(e.Bounds.Height / 2)))
        e.Graphics.DrawString(waitText, New Font("Courier New", 8),
            New SolidBrush(Color.DarkGray),
            New RectangleF(CSng(e.Bounds.X), CSng(e.Bounds.Y + e.Bounds.Height / 2), CSng(e.Bounds.Width), CSng(e.Bounds.Height / 2)))
    End Sub
#End Region

#Region "Form Load and Refresh"
    Private Sub FormQueueManagement_Load(sender As Object, e As EventArgs) Handles Me.Load
        RefreshQueue()
    End Sub

    ''' <summary>
    ''' Refreshes queue data.
    ''' Demonstrates: Exception handling, For Each loop, List(Of T)
    ''' </summary>
    Public Sub RefreshQueue()
        Try
            Dim dt As DataTable = GetQueue()
            _queueEntries.Clear()
            lstQueue.Items.Clear()

            ' Demonstrates: For Each loop populating a List(Of T)
            For Each row As DataRow In dt.Rows
                Dim entry As New QueueEntry With {
                    .QueueID = CInt(row("QueueID")),
                    .TicketNumber = row("TicketNumber").ToString(),
                    .QueuePosition = CInt(row("QueuePosition")),
                    .EstimatedWait = CInt(row("EstimatedWait")),
                    .Status = row("Status").ToString(),
                    .PatientName = row("PatientName").ToString(),
                    .DoctorName = row("DoctorName").ToString(),
                    .DepartmentName = row("DepartmentName").ToString(),
                    .IsEmergency = (CInt(row("IsEmergency")) = 1),
                    .AppointmentTime = row("AppointmentTime").ToString()
                }
                _queueEntries.Add(entry)
                lstQueue.Items.Add(entry.ToString())
            Next

            dgvQueue.DataSource = dt
            lblWaitingCount.Text = $"Waiting: {_queueEntries.Count}"

            ' Highlight emergency rows in grid
            For Each row As DataGridViewRow In dgvQueue.Rows
                If row.Cells("IsEmergency").Value IsNot Nothing AndAlso
                   CInt(row.Cells("IsEmergency").Value) = 1 Then
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220)
                    row.DefaultCellStyle.ForeColor = Color.DarkRed
                End If
            Next

        Catch ex As Exception
            MessageBox.Show("Error refreshing queue: " & ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
            LogError("RefreshQueue: " & ex.Message)
        End Try
    End Sub
#End Region

#Region "Event Handlers"
    ''' <summary>
    ''' Calls the next patient in queue.
    ''' Demonstrates: Exception handling, conditional statement
    ''' </summary>
    Private Sub btnCallNext_Click(sender As Object, e As EventArgs) Handles btnCallNext.Click
        Try
            _currentTicket = CallNextPatient()

            ' Demonstrates: Conditional statement
            If _currentTicket = "" Then
                MessageBox.Show("No patients waiting in the queue.", "Queue Empty",
                    MessageBoxButtons.OK, MessageBoxIcon.Information)
                lblNowServing.Text = "NOW SERVING: --- (Queue Empty)"
                Return
            End If

            lblNowServing.Text = $"NOW SERVING: {_currentTicket}"
            lblCurrentTicket.Text = $"Now serving: {_currentTicket}. Please mark complete when done."
            lblCurrentTicket.ForeColor = Color.DarkGreen
            RefreshQueue()

        Catch ex As Exception
            MessageBox.Show("Error calling next patient: " & ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
            LogError("btnCallNext_Click: " & ex.Message)
        End Try
    End Sub

    Private Sub btnComplete_Click(sender As Object, e As EventArgs) Handles btnComplete.Click
        If _currentTicket = "" Then
            MessageBox.Show("No patient is currently being served.", "No Current Patient",
                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        CompleteQueueItem(_currentTicket)
        lblNowServing.Text = "NOW SERVING: ---"
        lblCurrentTicket.Text = $"Ticket {_currentTicket} marked as complete."
        _currentTicket = ""
        RefreshQueue()
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        RefreshQueue()
    End Sub

    ''' <summary>
    ''' Checks a patient's position by ticket number.
    ''' Demonstrates: Conditional statement, For Each loop
    ''' </summary>
    Private Sub btnCheckTicket_Click(sender As Object, e As EventArgs) Handles btnCheckTicket.Click
        Dim ticketNum As String = txtCheckTicket.Text.Trim().ToUpper()

        If String.IsNullOrEmpty(ticketNum) Then
            lblTicketStatus.Text = "Enter a ticket number."
            Return
        End If

        ' Demonstrates: For Each loop searching a collection
        Dim found As Boolean = False
        For Each entry As QueueEntry In _queueEntries
            If entry.TicketNumber.Equals(ticketNum, StringComparison.OrdinalIgnoreCase) Then
                Dim waitDisplay As String = entry.GetWaitDisplay()
                lblTicketStatus.Text = $"Position: #{entry.QueuePosition} | Wait: {waitDisplay}"
                lblTicketStatus.ForeColor = Color.DarkGreen
                found = True
                Exit For
            End If
        Next

        If Not found Then
            lblTicketStatus.Text = "Ticket not found in today's queue."
            lblTicketStatus.ForeColor = Color.Red
        End If
    End Sub

    Private Sub tmrRefresh_Tick(sender As Object, e As EventArgs) Handles tmrRefresh.Tick
        RefreshQueue()
    End Sub
#End Region

End Class
