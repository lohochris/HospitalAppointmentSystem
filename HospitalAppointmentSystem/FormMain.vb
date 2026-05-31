Option Strict On
Option Explicit On

' ============================================================
' FormMain.vb
' CSC3226 - Hospital Appointment System
' Main Dashboard Form (Session & Thread Validated)
' Group: Sa'id Umar, Aisha Ladan, Maryam Rabiu
' ============================================================

Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic

Public Class FormMain
    Inherits Form

#Region "Member Variables"
    Private WithEvents btnAdminAnalytics As Button
    Private WithEvents btnSymptomChecker As Button
    Private WithEvents btnAppointments As Button
    Private WithEvents btnPatients As Button
    Private WithEvents btnDoctors As Button
    Private WithEvents btnLogout As Button
    Private lblWelcome As Label
    Private lblRole As Label
    Private pnlSidebar As Panel
    Private pnlContent As Panel
    Private lblDateTime As Label
    Private timerDateTime As Timer

    ' RBAC Dashboard Components
    Private lblHeader As Label
    Private lblDescription As Label
    Private pnlStats As FlowLayoutPanel
    Private statCard1 As Panel
    Private statCard2 As Panel
    Private statCard3 As Panel
    Private statCard4 As Panel
    Private lblStatValue1 As Label
    Private lblStatValue2 As Label
    Private lblStatValue3 As Label
    Private lblStatValue4 As Label
    Private dgvDashboardData As DataGridView

    ' Current user context
    Private _currentDoctorID As String = String.Empty
#End Region

#Region "Initialization"
    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Size = New Size(1200, 700)
        Me.Text = "MediCare Hospital Management System"
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = Color.FromArgb(240, 248, 255)
        Me.WindowState = FormWindowState.Maximized

        ' Sidebar Panel (Left Menu)
        pnlSidebar = New Panel With {
            .Dock = DockStyle.Left,
            .Width = 250,
            .BackColor = Color.FromArgb(0, 84, 126)
        }

        ' Logo / Title in Sidebar
        Dim lblAppTitle As New Label With {
            .Text = "MediCare HMS",
            .Font = New Font("Arial", 18, FontStyle.Bold),
            .ForeColor = Color.White,
            .TextAlign = ContentAlignment.MiddleCenter,
            .Dock = DockStyle.Top,
            .Height = 80,
            .BackColor = Color.FromArgb(0, 64, 96)
        }

        ' Welcome Label
        lblWelcome = New Label With {
            .Text = "Welcome, User",
            .Font = New Font("Arial", 12, FontStyle.Bold),
            .ForeColor = Color.White,
            .Location = New Point(10, 90),
            .Size = New Size(230, 30),
            .TextAlign = ContentAlignment.MiddleCenter
        }

        ' Role Label
        lblRole = New Label With {
            .Text = "Role: ",
            .Font = New Font("Arial", 10),
            .ForeColor = Color.FromArgb(200, 220, 255),
            .Location = New Point(10, 125),
            .Size = New Size(230, 25),
            .TextAlign = ContentAlignment.MiddleCenter
        }

        ' Separator Line
        Dim sepLine As New Panel With {
            .Location = New Point(10, 160),
            .Size = New Size(230, 2),
            .BackColor = Color.FromArgb(100, 150, 180)
        }

        ' Menu Buttons
        btnAdminAnalytics = CreateMenuButton("Admin Analytics", 180)
        btnSymptomChecker = CreateMenuButton("Symptom Checker", 230)
        btnAppointments = CreateMenuButton("Appointments", 280)
        btnPatients = CreateMenuButton("Patients", 330)
        btnDoctors = CreateMenuButton("Doctors Management", 380)
        btnLogout = CreateMenuButton("Logout", 530)

        ' Add controls to sidebar
        pnlSidebar.Controls.AddRange(New Control() {
            lblAppTitle, lblWelcome, lblRole, sepLine,
            btnAdminAnalytics, btnSymptomChecker, btnAppointments, btnPatients, btnDoctors, btnLogout
        })

        ' Content Panel (Right Area)
        pnlContent = New Panel With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(20),
            .BackColor = Color.FromArgb(240, 248, 255),
            .AutoScroll = True
        }

        ' Welcome Header in Content Area
        lblHeader = New Label With {
            .Text = "Hospital Appointment System Dashboard",
            .Font = New Font("Arial", 24, FontStyle.Bold),
            .ForeColor = Color.FromArgb(0, 84, 126),
            .Location = New Point(20, 20),
            .AutoSize = True
        }

        ' Date/Time Label
        lblDateTime = New Label With {
            .Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy  hh:mm tt"),
            .Font = New Font("Arial", 12),
            .ForeColor = Color.DimGray,
            .Location = New Point(20, 70),
            .AutoSize = True
        }

        ' Timer for updating date/time
        timerDateTime = New Timer With {.Interval = 1000}
        AddHandler timerDateTime.Tick, AddressOf UpdateDateTime
        timerDateTime.Start()

        ' Quick Stats Panel
        pnlStats = New FlowLayoutPanel With {
            .Location = New Point(20, 110),
            .Size = New Size(900, 120),
            .FlowDirection = FlowDirection.LeftToRight
        }

        statCard1 = CreateStatCard("Total Patients", "1,245", Color.FromArgb(0, 102, 153))
        statCard2 = CreateStatCard("Today's Appointments", "28", Color.FromArgb(0, 130, 100))
        statCard3 = CreateStatCard("Available Doctors", "12", Color.FromArgb(180, 100, 0))
        statCard4 = CreateStatCard("Emergency Cases", "3", Color.FromArgb(180, 30, 30))

        pnlStats.Controls.Add(statCard1)
        pnlStats.Controls.Add(statCard2)
        pnlStats.Controls.Add(statCard3)
        pnlStats.Controls.Add(statCard4)

        ' Store references to value labels for dynamic updates
        lblStatValue1 = DirectCast(statCard1.Controls(1), Label)
        lblStatValue2 = DirectCast(statCard2.Controls(1), Label)
        lblStatValue3 = DirectCast(statCard3.Controls(1), Label)
        lblStatValue4 = DirectCast(statCard4.Controls(1), Label)

        ' Description Panel
        lblDescription = New Label With {
            .Text = "Welcome to the MediCare Hospital Management System. Use the menu on the left to access different features." &
                    Environment.NewLine & Environment.NewLine &
                    "• Admin Analytics: View hospital statistics and charts" &
                    Environment.NewLine &
                    "• Symptom Checker: AI-powered symptom analysis" &
                    Environment.NewLine &
                    "• Appointments: Book and manage appointments" &
                    Environment.NewLine &
                    "• Patients: Manage patient records" &
                    Environment.NewLine &
                    "• Doctors Management: View and manage medical staff",
            .Font = New Font("Arial", 11),
            .ForeColor = Color.DarkGray,
            .Location = New Point(20, 250),
            .Size = New Size(900, 150),
            .BackColor = Color.White,
            .BorderStyle = BorderStyle.FixedSingle,
            .Padding = New Padding(15)
        }

        ' DataGridView for dashboard data (initially hidden, shown for Doctor role)
        dgvDashboardData = New DataGridView With {
            .Location = New Point(20, 420),
            .Size = New Size(900, 250),
            .BackgroundColor = Color.White,
            .BorderStyle = BorderStyle.Fixed3D,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .ReadOnly = True,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .Visible = False
        }

        pnlContent.Controls.AddRange(New Control() {
            lblHeader, lblDateTime, pnlStats, lblDescription, dgvDashboardData
        })

        Me.Controls.Add(pnlContent)
        Me.Controls.Add(pnlSidebar)
    End Sub

    Private Function CreateMenuButton(text As String, yPosition As Integer) As Button
        Dim btn As New Button With {
            .Text = text,
            .Location = New Point(10, yPosition),
            .Size = New Size(230, 40),
            .Font = New Font("Arial", 10, FontStyle.Bold),
            .BackColor = Color.FromArgb(0, 102, 153),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand,
            .TextAlign = ContentAlignment.MiddleLeft
        }
        btn.FlatAppearance.BorderSize = 0

        ' Explicit casting inside Lambdas to completely satisfy Option Strict On
        AddHandler btn.MouseEnter, Sub(s As Object, e As EventArgs)
                                       Dim targetBtn As Button = TryCast(s, Button)
                                       If targetBtn IsNot Nothing Then targetBtn.BackColor = Color.FromArgb(0, 120, 180)
                                   End Sub

        AddHandler btn.MouseLeave, Sub(s As Object, e As EventArgs)
                                       Dim targetBtn As Button = TryCast(s, Button)
                                       If targetBtn IsNot Nothing Then targetBtn.BackColor = Color.FromArgb(0, 102, 153)
                                   End Sub
        Return btn
    End Function

    Private Function CreateStatCard(title As String, value As String, color As Color) As Panel
        Dim pnl As New Panel With {
            .Size = New Size(210, 100),
            .BackColor = color,
            .Margin = New Padding(5)
        }

        Dim lblTitle As New Label With {
            .Text = title,
            .Font = New Font("Arial", 10, FontStyle.Bold),
            .ForeColor = Color.White,
            .Location = New Point(10, 10),
            .AutoSize = True
        }

        Dim lblValue As New Label With {
            .Text = value,
            .Font = New Font("Arial", 24, FontStyle.Bold),
            .ForeColor = Color.White,
            .Location = New Point(10, 45),
            .AutoSize = True
        }

        pnl.Controls.AddRange(New Control() {lblTitle, lblValue})
        Return pnl
    End Function
#End Region

#Region "Form Events"
    Private Sub FormMain_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            Dim currentUser As UserAccount = SessionManager.CurrentUser

            If currentUser Is Nothing Then
                MessageBox.Show("No user session found. Please login again.", "Session Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Application.Exit()
                Return
            End If

            lblWelcome.Text = "Welcome, " & currentUser.FullName
            lblRole.Text = "Role: " & currentUser.Role

            ' Set menu visibility based on role
            SetMenuVisibilityByRole(currentUser.Role)

            ' ===================================================================
            ' ROLE-SPECIFIC DASHBOARD CONFIGURATION
            ' Dynamically reconfigures dashboard UI based on logged-in user role
            ' ===================================================================
            ConfigureDashboardForRole(currentUser.Role, currentUser.Username, currentUser.UserID)

        Catch ex As Exception
            MessageBox.Show("Error loading main form: " & ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
            Application.Exit()
        End Try
    End Sub

    Private Sub UpdateDateTime(sender As Object, e As EventArgs)
        lblDateTime.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy  hh:mm:ss tt")
    End Sub

    Private Sub SetMenuVisibilityByRole(role As String)
        ' ===================================================================
        ' ROLE-BASED ACCESS CONTROL (RBAC) FOR NAVIGATION MENU
        ' ===================================================================
        ' This method configures sidebar menu visibility based on user role
        ' under strict Option Strict On standards with explicit role checking.
        ' 
        ' DOCTORS MANAGEMENT ACCESS POLICY:
        ' - ADMIN ONLY: Full CRUD access to Doctors Management module
        ' - All other roles (Doctor, Patient, Receptionist): NO ACCESS
        ' ===================================================================

        ' Default: Hide all role-specific buttons initially
        btnAdminAnalytics.Visible = False
        btnPatients.Visible = False
        btnDoctors.Visible = False

        ' ADMIN ROLE: Full administrative access
        If String.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) Then
            btnAdminAnalytics.Visible = True
            btnPatients.Visible = True
            btnDoctors.Visible = True       ' ADMIN EXCLUSIVE: Full CRUD access

        ' DOCTOR ROLE: Clinical staff access (NO Doctors Management access)
        ElseIf String.Equals(role, "Doctor", StringComparison.OrdinalIgnoreCase) Then
            ' Doctors Management button intentionally hidden for security
            ' Doctors cannot view or modify staff records

        ' PATIENT ROLE: Limited patient-centric access
        ElseIf String.Equals(role, "Patient", StringComparison.OrdinalIgnoreCase) Then
            ' Patients cannot access administrative modules

        ' RECEPTIONIST ROLE: Front-desk operations access
        ElseIf String.Equals(role, "Receptionist", StringComparison.OrdinalIgnoreCase) Then
            btnPatients.Visible = True
            ' Doctors Management intentionally hidden - Receptionists use appointment booking interface

        End If

        ' Common modules visible to all roles (no hiding)
        ' - btnSymptomChecker (health assessment tool)
        ' - btnAppointments (appointment booking/viewing)
        ' - btnLogout (session management)
    End Sub

    ''' <summary>
    ''' ROLE-SPECIFIC DASHBOARD CONFIGURATION
    ''' Dynamically reconfigures dashboard hero text, metrics cards, and data grid
    ''' based on the logged-in user's role for personalized clinical insights.
    ''' 
    ''' ADMIN ROLE: Standard hospital-wide metrics
    ''' - Total Patients, Today's Appointments, Available Doctors, Emergency Cases
    ''' - Generic welcome message and feature list
    ''' 
    ''' DOCTOR ROLE: Personalized clinical command center
    ''' - My Appointments Today, Urgent Cases, Completed Shifts, Office Location
    ''' - Physician-specific welcome and clinical workflow notifications
    ''' - DataGridView showing doctor's personal appointment queue
    ''' 
    ''' SECURITY: All database queries are parameterized
    ''' PERFORMANCE: Metrics calculated in single database call
    ''' </summary>
    ''' <param name="role">The user's role from SessionManager</param>
    ''' <param name="username">The user's username for audit logging</param>
    ''' <param name="userID">The user's UserID for doctor lookup</param>
    Private Sub ConfigureDashboardForRole(role As String, username As String, userID As Integer)
        Try
            ' ===================================================================
            ' ADMIN DASHBOARD CONFIGURATION (Default)
            ' ===================================================================
            If String.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) Then
                ' Keep standard Admin metrics
                lblHeader.Text = "Hospital Appointment System Dashboard"
                lblDateTime.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy  hh:mm tt")

                lblDescription.Text = "Welcome to the MediCare Hospital Management System. Use the menu on the left to access different features." &
                    Environment.NewLine & Environment.NewLine &
                    "• Admin Analytics: View hospital statistics and charts" &
                    Environment.NewLine &
                    "• Symptom Checker: AI-powered symptom analysis" &
                    Environment.NewLine &
                    "• Appointments: Book and manage appointments" &
                    Environment.NewLine &
                    "• Patients: Manage patient records" &
                    Environment.NewLine &
                    "• Doctors Management: View and manage medical staff"

                ' Admin sees generic metrics (already set in InitializeComponent)
                dgvDashboardData.Visible = False

                LogError($"ConfigureDashboardForRole: Admin dashboard configured for user '{username}'")

            ' ===================================================================
            ' DOCTOR DASHBOARD CONFIGURATION (Personalized Clinical Insights)
            ' ===================================================================
            ElseIf String.Equals(role, "Doctor", StringComparison.OrdinalIgnoreCase) Then
                ' Resolve DoctorID from UserID
                _currentDoctorID = GetDoctorIDByUserID(userID)

                If String.IsNullOrWhiteSpace(_currentDoctorID) Then
                    ' Doctor account not linked to DoctorsManagement table
                    LogError($"ConfigureDashboardForRole WARNING: No DoctorID found for Doctor user '{username}' (UserID={userID})")
                    MessageBox.Show("Your doctor profile is not fully configured. Please contact system administrator.", _
                                  "Profile Incomplete", MessageBoxButtons.OK, MessageBoxIcon.Warning)

                    ' Fall back to generic view
                    lblHeader.Text = "Physician Clinical Command Center"
                    lblDescription.Text = "⚠️ Your doctor profile is not yet linked. Please contact IT support to complete your account setup."
                    dgvDashboardData.Visible = False
                    Return
                End If

                ' Retrieve personalized clinical metrics
                Dim metrics As DoctorMetricsStructure = GetDoctorDashboardMetrics(_currentDoctorID)

                ' ===================================================================
                ' UPDATE HERO TEXT: Physician-specific welcome
                ' ===================================================================
                lblHeader.Text = "Physician Clinical Command Center"
                lblHeader.ForeColor = Color.FromArgb(0, 102, 153)

                lblDateTime.Text = "Active Shift Summary | " & DateTime.Now.ToLongDateString()
                lblDateTime.ForeColor = Color.FromArgb(50, 50, 150)
                lblDateTime.Font = New Font("Arial", 12, FontStyle.Bold)

                ' ===================================================================
                ' UPDATE DESCRIPTION: Clinical workflow notifications
                ' ===================================================================
                lblDescription.Text = $"Welcome, Dr. {metrics.DoctorFullName}" & Environment.NewLine &
                    $"Specialization: {metrics.Specialization} | Office: {metrics.OfficeRoom}" & Environment.NewLine & Environment.NewLine &
                    "• View and process your assigned patient appointment queue." & Environment.NewLine &
                    "• Run AI-assisted symptom assessments for local diagnoses." & Environment.NewLine &
                    "• Track real-time patient triage risk factors flags dynamically." & Environment.NewLine & Environment.NewLine &
                    "Your personalized clinical metrics are displayed below. " &
                    "Use the Appointments module to view detailed patient information."

                lblDescription.ForeColor = Color.FromArgb(40, 60, 100)
                lblDescription.BackColor = Color.FromArgb(245, 250, 255)

                ' ===================================================================
                ' UPDATE METRICS CARDS: Doctor-specific KPIs
                ' ===================================================================
                ' Card 1: My Appointments Today
                DirectCast(statCard1.Controls(0), Label).Text = "My Appointments"
                lblStatValue1.Text = metrics.MyAppointmentsToday.ToString()
                statCard1.BackColor = Color.FromArgb(0, 102, 153)

                ' Card 2: Urgent Cases
                DirectCast(statCard2.Controls(0), Label).Text = "Urgent Cases"
                lblStatValue2.Text = metrics.UrgentCasesCount.ToString()
                statCard2.BackColor = If(metrics.UrgentCasesCount > 0, Color.FromArgb(200, 50, 50), Color.FromArgb(0, 130, 100))

                ' Card 3: Completed Shifts Today
                DirectCast(statCard3.Controls(0), Label).Text = "Completed Today"
                lblStatValue3.Text = metrics.CompletedShiftsToday.ToString()
                statCard3.BackColor = Color.FromArgb(0, 130, 100)

                ' Card 4: Office Location
                DirectCast(statCard4.Controls(0), Label).Text = "Office Location"
                lblStatValue4.Text = metrics.OfficeRoom
                lblStatValue4.Font = New Font("Arial", 16, FontStyle.Bold)
                statCard4.BackColor = Color.FromArgb(100, 100, 150)

                ' ===================================================================
                ' POPULATE DATA GRID: Doctor's Personal Appointment Queue
                ' ===================================================================
                dgvDashboardData.Visible = True
                dgvDashboardData.DataSource = GetDoctorAppointments(_currentDoctorID)

                ' Style DataGridView for professional medical interface
                dgvDashboardData.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 84, 126)
                dgvDashboardData.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
                dgvDashboardData.ColumnHeadersDefaultCellStyle.Font = New Font("Arial", 10, FontStyle.Bold)
                dgvDashboardData.EnableHeadersVisualStyles = False

                ' Highlight emergency appointments
                For Each row As DataGridViewRow In dgvDashboardData.Rows
                    If row.Cells("Emergency").Value IsNot Nothing AndAlso _
                       row.Cells("Emergency").Value.ToString() = "Yes" Then
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220)
                        row.DefaultCellStyle.Font = New Font("Arial", 9, FontStyle.Bold)
                    End If
                Next

                LogError($"ConfigureDashboardForRole: Doctor dashboard configured for '{username}' (DoctorID={_currentDoctorID}) | " &
                        $"Appointments={metrics.MyAppointmentsToday} | Urgent={metrics.UrgentCasesCount} | Completed={metrics.CompletedShiftsToday}")

            ' ===================================================================
            ' PATIENT / RECEPTIONIST DASHBOARD (Future Enhancement)
            ' ===================================================================
            Else
                ' Keep default Admin-style dashboard for other roles
                dgvDashboardData.Visible = False
                LogError($"ConfigureDashboardForRole: Default dashboard configured for role '{role}', user '{username}'")
            End If

        Catch ex As Exception
            LogError($"ConfigureDashboardForRole error: {ex.Message} | Role={role} | Username={username} | UserID={userID}")
            MessageBox.Show("Error configuring dashboard: " & ex.Message, "Dashboard Error", _
                          MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub FormMain_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        ' Clean up timer resources on close
        If timerDateTime IsNot Nothing Then
            timerDateTime.Stop()
            timerDateTime.Dispose()
        End If

        ' Ensure that closing the main dashboard cleanly exits the complete application lifecycle
        If e.CloseReason = CloseReason.UserClosing Then
            Application.Exit()
        End If
    End Sub
#End Region

#Region "Menu Button Click Handlers"
    Private Sub btnAdminAnalytics_Click(sender As Object, e As EventArgs) Handles btnAdminAnalytics.Click
        Try
            Dim frmAnalytics As New FormAdminAnalytics()
            frmAnalytics.ShowDialog()
            frmAnalytics.Dispose()
        Catch ex As Exception
            MessageBox.Show("Error opening Admin Analytics: " & ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnSymptomChecker_Click(sender As Object, e As EventArgs) Handles btnSymptomChecker.Click
        Try
            ' ===================================================================
            ' SYMPTOM CHECKER MODULE LAUNCHER - DEDICATED DIAGNOSTIC TOOL
            ' ===================================================================
            ' COMPREHENSIVE SEPARATION: This handler explicitly opens the isolated
            ' FormSymptomChecker form for patient symptom assessment and diagnostic
            ' tracking, completely separate from Admin Analytics Dashboard.
            ' ===================================================================

            Using frmSymptoms As New FormSymptomChecker()
                frmSymptoms.ShowDialog()
            End Using

        Catch ex As Exception
            MessageBox.Show("Error opening Symptom Checker: " & ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
            ModuleDatabase.LogError($"btnSymptomChecker_Click error: {ex.Message}")
        End Try
    End Sub

    Private Sub btnAppointments_Click(sender As Object, e As EventArgs) Handles btnAppointments.Click
        Try
            Dim frmAppointments As New FormAppointmentBooking()
            frmAppointments.ShowDialog()
            frmAppointments.Dispose()
        Catch ex As Exception
            MessageBox.Show("Error opening Appointments: " & ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnPatients_Click(sender As Object, e As EventArgs) Handles btnPatients.Click
        Try
            ' Launch the Patient Management module
            Dim frmPatientManagement As New FormPatientManagement()
            frmPatientManagement.ShowDialog()
            frmPatientManagement.Dispose()
        Catch ex As Exception
            MessageBox.Show("Error opening Patient Management: " & ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
            ModuleDatabase.LogError("btnPatients_Click error: " & ex.Message)
        End Try
    End Sub

    Private Sub btnDoctors_Click(sender As Object, e As EventArgs) Handles btnDoctors.Click
        Try
            ' ===================================================================
            ' DOCTORS MANAGEMENT MODULE LAUNCHER - ADMIN EXCLUSIVE
            ' ===================================================================
            ' SECURITY POLICY: This module is ADMIN-ONLY for staff CRUD operations.
            ' Double-check authentication before launching to prevent unauthorized access.
            ' ===================================================================

            Dim currentUser As UserAccount = SessionManager.CurrentUser

            If currentUser Is Nothing Then
                MessageBox.Show("Session expired. Please login again.", "Authentication Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' SECURITY VALIDATION: Enforce Admin-only access
            If Not String.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase) Then
                MessageBox.Show("Access Denied." & vbCrLf & vbCrLf & _
                                "Doctors Management is restricted to Administrators only." & vbCrLf & _
                                "Please contact your system administrator for access.", _
                                "Insufficient Privileges", _
                                MessageBoxButtons.OK, _
                                MessageBoxIcon.Warning)
                ModuleDatabase.LogError($"SECURITY: Unauthorized Doctors Management access attempt by user '{currentUser.Username}' (Role: {currentUser.Role})")
                Return
            End If

            ' Launch Doctors Management form modally (Admin confirmed)
            Dim frmDoctors As New FormDoctorsManagement()
            frmDoctors.Tag = currentUser.Role  ' Pass role for internal UI state management
            frmDoctors.ShowDialog()            ' Modal launch - blocks parent form until closed
            frmDoctors.Dispose()               ' Clean resource disposal

        Catch ex As Exception
            MessageBox.Show("Error opening Doctors Management: " & ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
            ModuleDatabase.LogError("btnDoctors_Click error: " & ex.Message)
        End Try
    End Sub

    Private Sub btnLogout_Click(sender As Object, e As EventArgs) Handles btnLogout.Click
        Try
            ' Confirm explicit logout intent
            Dim result As DialogResult = MessageBox.Show("Are you sure you want to log out?", "Logout Confirmation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question)

            If result = DialogResult.Yes Then
                ' Clear execution session values cleanly
                SessionManager.Logout()

                ' Instantiate a clean login framework, showing it as a baseline framework
                Dim frmLogin As New FormLogin()
                frmLogin.Show()

                ' Unbind closing trap logic to protect application pipeline execution parameters
                RemoveHandler Me.FormClosing, AddressOf FormMain_FormClosing
                Me.Close()
                Me.Dispose()
            End If

        Catch ex As Exception
            MessageBox.Show("Error during logout: " & ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
#End Region

End Class