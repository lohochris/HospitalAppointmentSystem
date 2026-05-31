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
Imports System.Data
Imports Microsoft.VisualBasic

Public Class FormMain
    Inherits Form

#Region "Member Variables"
    Private WithEvents btnAdminAnalytics As Button
    Private WithEvents btnSymptomChecker As Button
    Private WithEvents btnAppointments As Button
    Private WithEvents btnPatients As Button
    Private WithEvents btnDoctors As Button
    Private WithEvents btnTelemedicine As Button  ' NEW: Telemedicine video consultation
    Private WithEvents btnLaunchTelehealth As Button  ' DYNAMIC: Context-aware telehealth launcher
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
        btnTelemedicine = CreateMenuButton("🎥 Telemedicine", 430)  ' NEW: Video consultation
        btnLogout = CreateMenuButton("Logout", 530)

        ' Add controls to sidebar
        pnlSidebar.Controls.AddRange(New Control() {
            lblAppTitle, lblWelcome, lblRole, sepLine,
            btnAdminAnalytics, btnSymptomChecker, btnAppointments, btnPatients, btnDoctors, btnTelemedicine, btnLogout
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

        ' ===================================================================
        ' ESI TRIAGE COLOR-CODING: Add CellFormatting event handler
        ' Automatically applies Emergency Severity Index visual triage system
        ' ===================================================================
        AddHandler dgvDashboardData.CellFormatting, AddressOf DgvDashboardData_CellFormatting

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

            ' ===================================================================
            ' DYNAMIC RUNTIME UI INJECTION: TELEHEALTH LAUNCH BUTTON
            ' Programmatically injects context-aware telehealth button into sidebar
            ' ===================================================================
            InjectTelehealthButton()

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

#Region "Dynamic Runtime UI Injection"
    ''' <summary>
    ''' ENTERPRISE RUNTIME UI INJECTION ROUTINE: LAUNCH TELEHEALTH BUTTON
    ''' 
    ''' FUNCTIONALITY:
    ''' - Programmatically injects "Launch Telehealth" button into sidebar at runtime
    ''' - Prevents duplicate button creation with defensive existence checks
    ''' - Matches styling and positioning of existing Cancel button (red theme)
    ''' - Calculates dynamic coordinates with 6-pixel spacing from reference button
    ''' - Wires Click event to btnLaunchTelehealth_Click handler
    ''' 
    ''' POSITIONING LOGIC:
    ''' - Locates existing btnTelemedicine button as anchor reference
    ''' - Calculates Y coordinate: btnTelemedicine.Bottom + 10 (spacing)
    ''' - X coordinate: Matches sidebar button standard (20px from left)
    ''' - Ensures button appears below existing Telemedicine button
    ''' 
    ''' STYLING SPECIFICATIONS:
    ''' - BackColor: DarkCyan (medical teal theme)
    ''' - ForeColor: White (high contrast for accessibility)
    ''' - FlatStyle: Flat (modern UI consistency)
    ''' - Font: Arial, 10pt, Bold (matches sidebar buttons)
    ''' - Size: 210 x 40 (standard sidebar button dimensions)
    ''' 
    ''' ERROR HANDLING:
    ''' - Try/Catch block captures initialization failures
    ''' - Logs errors to audit trail via ModuleDatabase.LogError
    ''' - Non-fatal: Application continues if button creation fails
    ''' 
    ''' CALLED FROM:
    ''' - FormMain_Load (after ConfigureDashboardForRole)
    ''' </summary>
    Private Sub InjectTelehealthButton()
        Try
            ' ═══════════════════════════════════════════════════════════════
            ' STEP 1: DUPLICATE PREVENTION CHECK
            ' Verify button doesn't already exist to prevent rendering conflicts
            ' ═══════════════════════════════════════════════════════════════
            If pnlSidebar.Controls.ContainsKey("btnLaunchTelehealth") Then
                ModuleDatabase.LogError("InjectTelehealthButton: Button already exists, skipping injection")
                Return
            End If

            ' ═══════════════════════════════════════════════════════════════
            ' STEP 2: INSTANTIATE BUTTON WITH MEDICAL-GRADE STYLING
            ' Create new button matching enterprise design system standards
            ' ═══════════════════════════════════════════════════════════════
            btnLaunchTelehealth = New Button With {
                .Name = "btnLaunchTelehealth",
                .Text = "📞 Launch Telehealth",
                .Size = New Size(210, 40),
                .BackColor = Color.DarkCyan,  ' Medical teal theme
                .ForeColor = Color.White,
                .FlatStyle = FlatStyle.Flat,
                .Font = New Font("Arial", 10, FontStyle.Bold),
                .Cursor = Cursors.Hand,
                .TextAlign = ContentAlignment.MiddleLeft,
                .Padding = New Padding(15, 0, 0, 0),
                .TabIndex = 8,
                .UseVisualStyleBackColor = False
            }

            ' Modern flat appearance with hover effects
            btnLaunchTelehealth.FlatAppearance.BorderSize = 0
            btnLaunchTelehealth.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 130, 130)  ' Lighter cyan on hover

            ' ═══════════════════════════════════════════════════════════════
            ' STEP 3: DYNAMIC COORDINATE CALCULATION
            ' Position button relative to existing Telemedicine button
            ' ═══════════════════════════════════════════════════════════════
            If btnTelemedicine IsNot Nothing Then
                ' Calculate position: 10 pixels below Telemedicine button
                Dim calculatedY As Integer = btnTelemedicine.Bottom + 10
                btnLaunchTelehealth.Location = New Point(20, calculatedY)
            Else
                ' Fallback: Use fixed position if reference button not found
                btnLaunchTelehealth.Location = New Point(20, 480)
                ModuleDatabase.LogError("InjectTelehealthButton: Reference button not found, using fallback position")
            End If

            ' ═══════════════════════════════════════════════════════════════
            ' STEP 4: EVENT HANDLER WIRING
            ' Link Click event to existing btnLaunchTelehealth_Click handler
            ' ═══════════════════════════════════════════════════════════════
            AddHandler btnLaunchTelehealth.Click, AddressOf btnLaunchTelehealth_Click

            ' ═══════════════════════════════════════════════════════════════
            ' STEP 5: SIDEBAR INJECTION & Z-ORDER MANAGEMENT
            ' Add button to sidebar panel and bring to front for visibility
            ' ═══════════════════════════════════════════════════════════════
            pnlSidebar.Controls.Add(btnLaunchTelehealth)
            btnLaunchTelehealth.BringToFront()

            ModuleDatabase.LogError("InjectTelehealthButton: SUCCESS - Launch Telehealth button injected at runtime")

        Catch ex As Exception
            ModuleDatabase.LogError($"InjectTelehealthButton: CRITICAL FAILURE - {ex.Message}")
            ' Non-fatal error: Application continues without button
            MessageBox.Show(
                "Failed to initialize Launch Telehealth button. Contact IT support if this persists." & Environment.NewLine & Environment.NewLine &
                "Technical Details: " & ex.Message,
                "UI Initialization Warning",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )
        End Try
    End Sub
#End Region

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
                ' POPULATE DATA GRID: Doctor's Personal Appointment Queue with ESI Triage
                ' ===================================================================
                dgvDashboardData.Visible = True
                dgvDashboardData.DataSource = GetDoctorAppointments(_currentDoctorID)

                ' Style DataGridView for professional medical interface
                dgvDashboardData.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 84, 126)
                dgvDashboardData.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
                dgvDashboardData.ColumnHeadersDefaultCellStyle.Font = New Font("Arial", 10, FontStyle.Bold)
                dgvDashboardData.EnableHeadersVisualStyles = False

                ' ===================================================================
                ' ESI TRIAGE COLOR-CODING
                ' CellFormatting event handler automatically applies color-coding
                ' Critical patients (Level 1) auto-sorted to top via GetDoctorAppointments
                ' Legacy emergency highlighting replaced by ESI visual triage system
                ' ===================================================================

                LogError($"ConfigureDashboardForRole: Doctor dashboard configured for '{username}' (DoctorID={_currentDoctorID}) | " &
                        $"Appointments={metrics.MyAppointmentsToday} | Urgent={metrics.UrgentCasesCount} | Completed={metrics.CompletedShiftsToday} | ESI Triage Color-Coding ENABLED")

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

    Private Sub btnTelemedicine_Click(sender As Object, e As EventArgs) Handles btnTelemedicine.Click
        Try
            ' ===================================================================
            ' TELEMEDICINE MODULE LAUNCHER - VIDEO CONSULTATION PORTAL
            ' ===================================================================
            ' FEATURES:
            ' - WebView2-powered secure video conferencing
            ' - HTTPS-only medical portal navigation (HIPAA compliance)
            ' - Dynamic video room generation with appointment tracking
            ' - Support for Daily.co, Jitsi Meet, and Zoom platforms
            ' - Context-aware: Passes appointment data from selected row
            ' ===================================================================

            LogError("btnTelemedicine_Click: Launching telemedicine portal")

            ' ===================================================================
            ' ATTEMPT TO EXTRACT APPOINTMENT CONTEXT FROM SELECTED ROW
            ' If doctor has appointment selected, pass context to telemedicine form
            ' Otherwise, launch portal in standalone mode (manual room entry)
            ' ===================================================================
            Dim selectedAppointmentID As String = String.Empty
            Dim selectedPatientName As String = String.Empty

            ' Check if DataGridView is visible and has a selected row
            If dgvDashboardData.Visible AndAlso dgvDashboardData.SelectedRows.Count > 0 Then
                Try
                    ' ===================================================================
                    ' EXTRACT APPOINTMENT DATA FROM SELECTED ROW
                    ' Defensive programming: Check for column existence and DBNull values
                    ' ===================================================================
                    Dim selectedRow As DataGridViewRow = dgvDashboardData.SelectedRows(0)

                    ' Extract Appointment ID (column name: "Appointment ID")
                    If dgvDashboardData.Columns.Contains("Appointment ID") AndAlso selectedRow.Cells("Appointment ID").Value IsNot Nothing Then
                        Dim appointmentIDValue As Object = selectedRow.Cells("Appointment ID").Value
                        If Not IsDBNull(appointmentIDValue) Then
                            selectedAppointmentID = appointmentIDValue.ToString().Trim()
                        End If
                    End If

                    ' Extract Patient Name (column name: "Patient Name")
                    If dgvDashboardData.Columns.Contains("Patient Name") AndAlso selectedRow.Cells("Patient Name").Value IsNot Nothing Then
                        Dim patientNameValue As Object = selectedRow.Cells("Patient Name").Value
                        If Not IsDBNull(patientNameValue) Then
                            selectedPatientName = patientNameValue.ToString().Trim()
                        End If
                    End If

                    ' Validate extracted data
                    If Not String.IsNullOrWhiteSpace(selectedAppointmentID) AndAlso Not String.IsNullOrWhiteSpace(selectedPatientName) Then
                        LogError($"btnTelemedicine_Click: Appointment context extracted | AppointmentID={selectedAppointmentID} | PatientName={selectedPatientName}")
                    Else
                        LogError("btnTelemedicine_Click: Selected row does not contain valid appointment data, launching in standalone mode")
                    End If

                Catch extractEx As Exception
                    ' Non-fatal error: If extraction fails, continue with standalone launch
                    LogError($"btnTelemedicine_Click: Error extracting appointment context (non-fatal): {extractEx.Message}")
                End Try
            Else
                LogError("btnTelemedicine_Click: No appointment selected, launching telemedicine portal in standalone mode")
            End If

            ' ===================================================================
            ' LAUNCH TELEMEDICINE FORM
            ' ===================================================================
            Dim frmTelemedicine As New FormTelemedicine()

            ' ===================================================================
            ' CONTEXT-AWARE LAUNCH: Pass appointment data if available
            ' ===================================================================
            If Not String.IsNullOrWhiteSpace(selectedAppointmentID) AndAlso Not String.IsNullOrWhiteSpace(selectedPatientName) Then
                ' Show confirmation dialog before launching consultation
                Dim confirmResult As DialogResult = MessageBox.Show(
                    $"Launch video consultation for:" & Environment.NewLine & Environment.NewLine &
                    $"Patient: {selectedPatientName}" & Environment.NewLine &
                    $"Appointment ID: {selectedAppointmentID}" & Environment.NewLine & Environment.NewLine &
                    "Continue?",
                    "Confirm Consultation Launch",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                )

                If confirmResult = DialogResult.Yes Then
                    ' Pass appointment context to telemedicine form BEFORE showing it
                    frmTelemedicine.ActiveAppointmentID = selectedAppointmentID
                    frmTelemedicine.ActivePatientName = selectedPatientName

                    ' Show form (non-modal to allow dashboard interaction)
                    frmTelemedicine.Show()

                    ' Load active consultation context (auto-navigates to room)
                    frmTelemedicine.LoadActiveConsultation(selectedAppointmentID, selectedPatientName)

                    LogError($"btnTelemedicine_Click: Telemedicine portal launched with appointment context | AppointmentID={selectedAppointmentID}")
                Else
                    LogError("btnTelemedicine_Click: User cancelled consultation launch")
                    frmTelemedicine.Dispose()
                    Return
                End If
            Else
                ' Standalone launch (no appointment context) - show modal dialog
                frmTelemedicine.ShowDialog()
                frmTelemedicine.Dispose()
                LogError("btnTelemedicine_Click: Telemedicine portal launched in standalone mode (no appointment context)")
            End If

        Catch ex As Exception
            MessageBox.Show("Error opening Telemedicine portal: " & ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
            ModuleDatabase.LogError($"btnTelemedicine_Click error: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' DASHBOARD ENTRY POINT: LAUNCH TELEHEALTH CONSULTATION FROM SELECTED APPOINTMENT
    ''' 
    ''' WORKFLOW:
    ''' 1. Validates that an appointment row is selected in the DataGridView
    ''' 2. Extracts AppointmentID and PatientName with defensive DBNull checks
    ''' 3. Instantiates FormTelemedicine and passes context data
    ''' 4. Calls .Show() to bring telemedicine portal to front (non-modal)
    ''' 5. Auto-navigates to secure room: https://meet.jit.si/medicare-hms-room-{appointmentID}
    ''' 
    ''' VALIDATION:
    ''' - Shows friendly error if no row is selected
    ''' - Validates column existence and DBNull values before access
    ''' - Provides detailed audit logging
    ''' 
    ''' USAGE:
    ''' - Wire to button: Handles btnLaunchTelehealth.Click
    ''' - Or call directly from context menu / double-click handler
    ''' </summary>
    Private Sub btnLaunchTelehealth_Click(sender As Object, e As EventArgs)
        ' ===================================================================
        ' DEFENSIVE VALIDATION: ENSURE ROW IS SELECTED
        ' ===================================================================
        If dgvDashboardData.CurrentRow Is Nothing OrElse dgvDashboardData.CurrentRow.Index < 0 Then
            MessageBox.Show(
                "Please select an active patient appointment from the grid before launching the telemedicine suite.",
                "No Appointment Selected",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )
            LogError("btnLaunchTelehealth_Click: REJECTED - No appointment row selected")
            Return
        End If

        Try
            LogError("btnLaunchTelehealth_Click: Telehealth launch requested from dashboard")

            ' ===================================================================
            ' EXTRACT APPOINTMENT CONTEXT WITH DEFENSIVE TYPING
            ''' Safely extract values from the selected DataGridView row
            ' ===================================================================
            Dim selectedRow As DataGridViewRow = dgvDashboardData.CurrentRow

            ' Check for DBNull defensively to prevent parsing crashes
            If selectedRow.Cells("Appointment ID").Value Is DBNull.Value OrElse 
               selectedRow.Cells("Patient Name").Value Is DBNull.Value Then
                MessageBox.Show(
                    "The selected record contains incomplete clinical identifiers.",
                    "Data Integrity Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                )
                LogError("btnLaunchTelehealth_Click: REJECTED - DBNull values in required fields")
                Return
            End If

            ' Extract appointment data with type-safe conversion
            Dim targetApptID As String = Convert.ToString(selectedRow.Cells("Appointment ID").Value)
            Dim targetPatientName As String = Convert.ToString(selectedRow.Cells("Patient Name").Value)

            ' Validate extracted data is not empty
            If String.IsNullOrWhiteSpace(targetApptID) OrElse String.IsNullOrWhiteSpace(targetPatientName) Then
                MessageBox.Show(
                    "The selected appointment does not contain valid patient or appointment data." & Environment.NewLine & Environment.NewLine &
                    "Please ensure the appointment record is complete before launching telehealth.",
                    "Invalid Appointment Data",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                )
                LogError($"btnLaunchTelehealth_Click: REJECTED - Empty data | AppointmentID={targetApptID} | PatientName={targetPatientName}")
                Return
            End If

            LogError($"btnLaunchTelehealth_Click: Appointment data validated | AppointmentID={targetApptID} | PatientName={targetPatientName}")

            ' ===================================================================
            ' INSTANTIATE TELEMEDICINE FORM AND PASS CONTEXT
            ' ===================================================================
            Dim telehealthPortal As New FormTelemedicine()
            telehealthPortal.ActiveAppointmentID = targetApptID
            telehealthPortal.ActivePatientName = targetPatientName

            ' ===================================================================
            ' SHOW FORM (NON-MODAL) AND AUTO-NAVIGATE TO SECURE ROOM
            ' Show the form first to trigger its handle creation
            ' ===================================================================
            telehealthPortal.Show()

            ' Route the clinical context and initialize the browser environment
            telehealthPortal.LoadActiveConsultation(targetApptID, targetPatientName)

            LogError($"btnLaunchTelehealth_Click: SUCCESS - Telehealth portal launched | AppointmentID={targetApptID} | PatientName={targetPatientName}")

        Catch ex As Exception
            MessageBox.Show(
                "Critical failure launching telemedicine runtime environment: " & ex.Message,
                "System Failure",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )
            ModuleDatabase.LogError($"btnLaunchTelehealth_Click error: {ex.Message}")
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

#Region "ESI Triage Color-Coding - Emergency Severity Index Visual System"

    ''' <summary>
    ''' DATAGRIDVIEW CELL FORMATTING EVENT HANDLER
    ''' Applies Emergency Severity Index (ESI) color-coding to appointment queue rows
    ''' 
    ''' WORKFLOW:
    ''' 1. Extracts ESI level and TriageColor from bound DataRow
    ''' 2. Applies soft medical safety colors to entire row (BackColor, SelectionBackColor)
    ''' 3. Enhances text legibility with high-contrast foreground colors
    ''' 4. Critical (Level 1) patients automatically sorted to top of queue
    ''' 
    ''' COLOR PALETTE (Soft Medical Safety Colors):
    ''' - Level 1 (CRITICAL): Soft Red (255, 200, 200) - Immediate life-threatening
    ''' - Level 2 (URGENT): Soft Amber (255, 235, 180) - High-risk deterioration
    ''' - Level 3 (MODERATE): Soft Yellow (255, 255, 200) - Stable, requires evaluation
    ''' - Level 4-5 (LOW/MINIMAL): Soft Green/Blue - Non-urgent conditions
    ''' 
    ''' DEFENSIVE PROGRAMMING:
    ''' - Null-safe: Handles missing TriageColor column gracefully
    ''' - Type-safe: Explicit conversions with DBNull checks
    ''' - Performance: Only formats visible cells (CellFormatting event)
    ''' </summary>
    Private Sub DgvDashboardData_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs)
        Try
            ' Cast sender to DataGridView for type safety
            Dim dgv As DataGridView = TryCast(sender, DataGridView)
            If dgv Is Nothing OrElse e.RowIndex < 0 Then Return

            ' ===================================================================
            ' DEFENSIVE NULL CHECK: Ensure row is bound to data
            ' ===================================================================
            If dgv.Rows(e.RowIndex).DataBoundItem Is Nothing Then Return

            ' ===================================================================
            ' EXTRACT TRIAGE COLOR FROM BOUND DATAROW
            ' TriageColor column stores Color.ToArgb() integer value
            ' ===================================================================
            Dim row As DataRowView = TryCast(dgv.Rows(e.RowIndex).DataBoundItem, DataRowView)
            If row Is Nothing Then Return

            ' Check if TriageColor column exists (defensive against schema changes)
            If Not row.Row.Table.Columns.Contains("TriageColor") Then Return

            ' Extract TriageColor value (nullable to handle DBNull)
            Dim triageColorArgb As Object = row.Row("TriageColor")
            If IsDBNull(triageColorArgb) Then Return

            ' Convert ARGB integer back to Color structure
            Dim triageColor As Color = Color.FromArgb(CInt(triageColorArgb))

            ' ===================================================================
            ' APPLY ROW-LEVEL COLOR FORMATTING
            ' BackColor: Base row color for unselected state
            ' SelectionBackColor: Darker shade for selected state (maintains visibility)
            ' ForeColor: High-contrast text color for legibility
            ' ===================================================================
            dgv.Rows(e.RowIndex).DefaultCellStyle.BackColor = triageColor
            dgv.Rows(e.RowIndex).DefaultCellStyle.SelectionBackColor = DarkenColor(triageColor, 0.15F)
            dgv.Rows(e.RowIndex).DefaultCellStyle.ForeColor = Color.Black
            dgv.Rows(e.RowIndex).DefaultCellStyle.SelectionForeColor = Color.Black

            ' ===================================================================
            ' OPTIONAL: BOLD FONT FOR CRITICAL PATIENTS (ESI LEVEL 1)
            ' Draws additional visual attention to life-threatening cases
            ' ===================================================================
            If row.Row.Table.Columns.Contains("ESILevel") Then
                Dim esiLevel As Object = row.Row("ESILevel")
                If Not IsDBNull(esiLevel) AndAlso CInt(esiLevel) = 1 Then
                    dgv.Rows(e.RowIndex).DefaultCellStyle.Font = New Font(dgv.Font, FontStyle.Bold)
                End If
            End If

            ' ===================================================================
            ' HIDE INTERNAL TRIAGE COLUMNS FROM USER VIEW
            ' ESILevel, TriageColor, SeverityLabel are used for formatting only
            ' ===================================================================
            If dgv.Columns.Contains("ESILevel") Then dgv.Columns("ESILevel").Visible = False
            If dgv.Columns.Contains("TriageColor") Then dgv.Columns("TriageColor").Visible = False
            If dgv.Columns.Contains("SeverityLabel") Then dgv.Columns("SeverityLabel").Visible = False
            If dgv.Columns.Contains("PatientIDInternal") Then dgv.Columns("PatientIDInternal").Visible = False

            ' Hide raw vitals columns (optional - doctors can see values directly if needed)
            If dgv.Columns.Contains("SystolicBP") Then dgv.Columns("SystolicBP").Visible = False
            If dgv.Columns.Contains("DiastolicBP") Then dgv.Columns("DiastolicBP").Visible = False
            If dgv.Columns.Contains("HeartRate") Then dgv.Columns("HeartRate").Visible = False
            If dgv.Columns.Contains("SpO2") Then dgv.Columns("SpO2").Visible = False
            If dgv.Columns.Contains("Vitals Recorded") Then dgv.Columns("Vitals Recorded").Visible = False

        Catch ex As Exception
            ' ===================================================================
            ' CATASTROPHIC ERROR HANDLING
            ' If formatting fails, log error but DO NOT crash application
            ' Medical systems must remain operational even with UI formatting issues
            ' ===================================================================
            LogError($"DgvDashboardData_CellFormatting error: {ex.Message} | RowIndex={e.RowIndex} | ColumnIndex={e.ColumnIndex}")
        End Try
    End Sub

    ''' <summary>
    ''' UTILITY FUNCTION: Darkens a color by specified percentage
    ''' Used to create Selection background color (slightly darker than base color)
    ''' </summary>
    ''' <param name="color">Base color to darken</param>
    ''' <param name="factor">Darkening factor (0.0 to 1.0, where 0.15 = 15% darker)</param>
    ''' <returns>Darkened color for selection highlighting</returns>
    Private Function DarkenColor(color As Color, factor As Single) As Color
        Dim r As Integer = CInt(Math.Max(0, color.R * (1 - factor)))
        Dim g As Integer = CInt(Math.Max(0, color.G * (1 - factor)))
        Dim b As Integer = CInt(Math.Max(0, color.B * (1 - factor)))
        Return Color.FromArgb(r, g, b)
    End Function

#End Region

End Class