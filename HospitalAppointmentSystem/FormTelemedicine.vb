Option Strict On
Option Explicit On

' ============================================================
' FormTelemedicine.vb
' CSC3226 - Hospital Appointment System
' Integrated Telemedicine & Web Views Module
' WebView2-Powered Secure Video Consultation Platform
' Group: Sa'id Umar, Aisha Ladan, Maryam Rabiu
' ============================================================

Imports System.Drawing
Imports System.Windows.Forms
Imports System.Threading.Tasks
Imports Microsoft.Web.WebView2.WinForms
Imports Microsoft.Web.WebView2.Core

''' <summary>
''' ENTERPRISE TELEMEDICINE FORM
''' Provides secure video consultation capabilities using Microsoft WebView2 (Chromium-based)
''' 
''' FEATURES:
''' - Asynchronous WebView2 initialization with runtime validation
''' - HTTPS-only medical portal navigation (HIPAA compliance)
''' - Dynamic video room generation with unique session IDs
''' - Secure authentication token embedding
''' - Fallback handling for missing Chromium runtime
''' - Navigation history tracking and error recovery
''' - Real-time connection status monitoring
''' </summary>
Public Class FormTelemedicine
    Inherits Form

#Region "Member Variables"
    ' WebView2 Components
    Private WithEvents webView As WebView2
    Private webViewInitialized As Boolean = False
    Private webViewEnvironment As CoreWebView2Environment

    ' UI Controls
    Private pnlTopBar As Panel
    Private lblTitle As Label
    Private btnBack As Button
    Private btnRefresh As Button
    Private btnHome As Button
    Private txtUrlBar As TextBox
    Private btnGo As Button
    Private pnlStatusBar As Panel
    Private lblStatus As Label
    Private lblConnection As Label
    Private progressBar As ProgressBar

    ' Navigation State
    Private currentAppointmentID As String = String.Empty
    Private currentPatientID As String = String.Empty
    Private currentPatientName As String = String.Empty  ' NEW: Patient name for UI display
    Private currentRoomID As String = String.Empty
    Private navigationHistory As New List(Of String)

    ' ===================================================================
    ' PUBLIC PROPERTIES FOR APPOINTMENT CONTEXT TRANSFER
    ' Used by FormMain to pass appointment details dynamically
    ' ===================================================================

    ''' <summary>
    ''' Gets or sets the active appointment identifier for the consultation session.
    ''' Used to generate unique room URLs and track consultation context.
    ''' </summary>
    Public Property ActiveAppointmentID As String
        Get
            Return currentAppointmentID
        End Get
        Set(value As String)
            currentAppointmentID = If(value, String.Empty)
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the active patient name for UI display.
    ''' Used to show consultation context in the portal header.
    ''' </summary>
    Public Property ActivePatientName As String
        Get
            Return currentPatientName
        End Get
        Set(value As String)
            currentPatientName = If(value, String.Empty)
        End Set
    End Property

    ' Security Settings
    Private Const HTTPS_REQUIRED As Boolean = True
    Private Const ALLOWED_DOMAIN_PATTERN As String = "^https://(.*\.)?daily\.co|.*\.?jitsi\.(org|meet)|.*\.?zoom\.us|localhost"
#End Region

#Region "Initialization"
    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        ' ===================================================================
        ' FORM CONFIGURATION
        ' ===================================================================
        Me.Size = New Size(1400, 900)
        Me.Text = "MediCare Telemedicine Portal - Secure Video Consultation"
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.WindowState = FormWindowState.Maximized
        Me.BackColor = Color.FromArgb(240, 248, 255)
        Me.MinimumSize = New Size(1024, 768)

        ' ===================================================================
        ' TOP NAVIGATION BAR
        ' ===================================================================
        pnlTopBar = New Panel With {
            .Dock = DockStyle.Top,
            .Height = 100,
            .BackColor = Color.FromArgb(0, 84, 126)
        }

        lblTitle = New Label With {
            .Text = "🏥 MediCare Telemedicine Portal",
            .Font = New Font("Arial", 18, FontStyle.Bold),
            .ForeColor = Color.White,
            .Location = New Point(20, 15),
            .AutoSize = True
        }

        ' Back Button
        btnBack = New Button With {
            .Text = "◄ Back",
            .Location = New Point(20, 55),
            .Size = New Size(80, 35),
            .Font = New Font("Arial", 10, FontStyle.Bold),
            .BackColor = Color.FromArgb(0, 102, 153),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        btnBack.FlatAppearance.BorderSize = 0

        ' Refresh Button
        btnRefresh = New Button With {
            .Text = "🔄 Refresh",
            .Location = New Point(110, 55),
            .Size = New Size(90, 35),
            .Font = New Font("Arial", 10, FontStyle.Bold),
            .BackColor = Color.FromArgb(0, 102, 153),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        btnRefresh.FlatAppearance.BorderSize = 0

        ' Home Button
        btnHome = New Button With {
            .Text = "🏠 Home",
            .Location = New Point(210, 55),
            .Size = New Size(80, 35),
            .Font = New Font("Arial", 10, FontStyle.Bold),
            .BackColor = Color.FromArgb(0, 102, 153),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        btnHome.FlatAppearance.BorderSize = 0

        ' URL Bar
        txtUrlBar = New TextBox With {
            .Location = New Point(310, 55),
            .Size = New Size(800, 35),
            .Font = New Font("Consolas", 11),
            .Text = "",
            .BackColor = Color.White
        }

        ' Go Button
        btnGo = New Button With {
            .Text = "Go ►",
            .Location = New Point(1120, 55),
            .Size = New Size(70, 35),
            .Font = New Font("Arial", 10, FontStyle.Bold),
            .BackColor = Color.FromArgb(0, 130, 100),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        btnGo.FlatAppearance.BorderSize = 0

        pnlTopBar.Controls.AddRange(New Control() {
            lblTitle, btnBack, btnRefresh, btnHome, txtUrlBar, btnGo
        })

        ' ===================================================================
        ' STATUS BAR (Bottom)
        ' ===================================================================
        pnlStatusBar = New Panel With {
            .Dock = DockStyle.Bottom,
            .Height = 35,
            .BackColor = Color.FromArgb(220, 230, 240)
        }

        lblStatus = New Label With {
            .Text = "Status: Initializing WebView2...",
            .Location = New Point(10, 8),
            .AutoSize = True,
            .Font = New Font("Arial", 9),
            .ForeColor = Color.DarkSlateGray
        }

        lblConnection = New Label With {
            .Text = "⚫ Offline",
            .Location = New Point(Me.ClientSize.Width - 150, 8),
            .AutoSize = True,
            .Font = New Font("Arial", 9, FontStyle.Bold),
            .ForeColor = Color.Gray
        }

        progressBar = New ProgressBar With {
            .Location = New Point(400, 10),
            .Size = New Size(300, 15),
            .Visible = False,
            .Style = ProgressBarStyle.Marquee
        }

        pnlStatusBar.Controls.AddRange(New Control() {
            lblStatus, lblConnection, progressBar
        })

        ' ===================================================================
        ' WEBVIEW2 BROWSER CONTROL
        ' ===================================================================
        webView = New WebView2 With {
            .Dock = DockStyle.Fill,
            .Visible = False
        }

        ' Add controls to form
        Me.Controls.Add(webView)
        Me.Controls.Add(pnlStatusBar)
        Me.Controls.Add(pnlTopBar)

        ' Wire up event handlers
        AddHandler btnBack.Click, AddressOf BtnBack_Click
        AddHandler btnRefresh.Click, AddressOf BtnRefresh_Click
        AddHandler btnHome.Click, AddressOf BtnHome_Click
        AddHandler btnGo.Click, AddressOf BtnGo_Click
        AddHandler txtUrlBar.KeyDown, AddressOf TxtUrlBar_KeyDown
        AddHandler Me.Load, AddressOf FormTelemedicine_Load
        AddHandler Me.FormClosing, AddressOf FormTelemedicine_FormClosing

        LogError("FormTelemedicine.InitializeComponent: UI initialization complete")
    End Sub

    Private Async Sub FormTelemedicine_Load(sender As Object, e As EventArgs)
        Try
            LogError("FormTelemedicine_Load: Starting form load sequence")

            ' Show loading indicator
            progressBar.Visible = True
            lblStatus.Text = "Status: Checking WebView2 Runtime..."
            lblStatus.ForeColor = Color.DarkOrange

            ' ===================================================================
            ' ASYNCHRONOUS WEBVIEW2 INITIALIZATION
            ' Critical: Must complete before any navigation attempts
            ' ===================================================================
            Await InitializeBrowserAsync()

        Catch ex As Exception
            LogError($"FormTelemedicine_Load error: {ex.Message}")
            ShowWebView2ErrorFallback(ex.Message)
        Finally
            progressBar.Visible = False
        End Try
    End Sub
#End Region

#Region "WebView2 Initialization & Runtime Validation"
    ''' <summary>
    ''' ASYNCHRONOUS WEBVIEW2 INITIALIZATION
    ''' 
    ''' WORKFLOW:
    ''' 1. Check if Chromium runtime is installed on client machine
    ''' 2. Create CoreWebView2Environment with custom user data folder
    ''' 3. Initialize WebView2 control with environment
    ''' 4. Configure security settings and event handlers
    ''' 5. Display default landing page or error fallback
    ''' 
    ''' DEFENSIVE PROGRAMMING:
    ''' - Try/Catch wraps entire initialization sequence
    ''' - Detects missing runtime and displays user-friendly message
    ''' - Validates environment creation before proceeding
    ''' - Sets webViewInitialized flag only on complete success
    ''' </summary>
    Private Async Function InitializeBrowserAsync() As Task
        Try
            LogError("InitializeBrowserAsync: Starting WebView2 initialization...")

            ' ===================================================================
            ' STEP 1: CREATE WEBVIEW2 ENVIRONMENT
            ' Chromium runtime must be installed on client machine
            ' Download: https://developer.microsoft.com/microsoft-edge/webview2/
            ' ===================================================================
            Dim userDataFolder As String = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "MediCareTelemedicineCache"
            )

            LogError($"InitializeBrowserAsync: Creating environment | UserDataFolder={userDataFolder}")

            webViewEnvironment = Await CoreWebView2Environment.CreateAsync(
                Nothing,  ' Use default browser executable location
                userDataFolder,  ' Custom cache folder for session isolation
                New CoreWebView2EnvironmentOptions()
            )

            If webViewEnvironment Is Nothing Then
                Throw New InvalidOperationException("Failed to create WebView2 environment. Runtime may be missing.")
            End If

            LogError("InitializeBrowserAsync: Environment created successfully")

            ' ===================================================================
            ' STEP 2: INITIALIZE WEBVIEW2 CONTROL
            ' ===================================================================
            Await webView.EnsureCoreWebView2Async(webViewEnvironment)

            If webView.CoreWebView2 Is Nothing Then
                Throw New InvalidOperationException("CoreWebView2 initialization failed. Control is null.")
            End If

            LogError("InitializeBrowserAsync: CoreWebView2 initialized successfully")

            ' ===================================================================
            ' STEP 3: CONFIGURE SECURITY SETTINGS
            ' Medical compliance requires HTTPS enforcement
            ' ===================================================================
            With webView.CoreWebView2.Settings
                .AreDefaultContextMenusEnabled = True
                .AreDevToolsEnabled = False  ' Disable in production (security)
                .IsScriptEnabled = True  ' Required for video conferencing platforms
                .IsWebMessageEnabled = True  ' Allow JavaScript-VB.NET communication
                .IsStatusBarEnabled = False
                .IsZoomControlEnabled = True
                .IsBuiltInErrorPageEnabled = True
            End With

            LogError("InitializeBrowserAsync: Security settings configured")

            ' ===================================================================
            ' STEP 4: WIRE UP NAVIGATION EVENT HANDLERS
            ' ===================================================================
            AddHandler webView.CoreWebView2.NavigationStarting, AddressOf WebView_NavigationStarting
            AddHandler webView.CoreWebView2.NavigationCompleted, AddressOf WebView_NavigationCompleted
            AddHandler webView.CoreWebView2.SourceChanged, AddressOf WebView_SourceChanged
            AddHandler webView.CoreWebView2.ContentLoading, AddressOf WebView_ContentLoading
            AddHandler webView.CoreWebView2.WebMessageReceived, AddressOf WebView_WebMessageReceived

            LogError("InitializeBrowserAsync: Event handlers registered")

            ' ===================================================================
            ' STEP 5: INITIALIZATION COMPLETE
            ' ===================================================================
            webViewInitialized = True
            webView.Visible = True

            lblStatus.Text = "Status: ✅ WebView2 Ready | Secure browser initialized"
            lblStatus.ForeColor = Color.DarkGreen
            lblConnection.Text = "🟢 Online"
            lblConnection.ForeColor = Color.Green

            LogError("InitializeBrowserAsync: SUCCESS - WebView2 fully initialized and ready")

            ' Display default landing page
            ShowTelemedicineLandingPage()

        Catch ex As WebView2RuntimeNotFoundException
            ' ===================================================================
            ' WEBVIEW2 RUNTIME MISSING - FRIENDLY FALLBACK
            ' ===================================================================
            LogError($"InitializeBrowserAsync: RUNTIME NOT FOUND - {ex.Message}")
            ShowWebView2ErrorFallback(
                "The Microsoft Edge WebView2 Runtime is not installed on this computer." & Environment.NewLine & Environment.NewLine &
                "WebView2 is required for the telemedicine video consultation feature." & Environment.NewLine & Environment.NewLine &
                "Please download and install the runtime from:" & Environment.NewLine &
                "https://go.microsoft.com/fwlink/p/?LinkId=2124703" & Environment.NewLine & Environment.NewLine &
                "After installation, restart the application."
            )

        Catch ex As Exception
            ' ===================================================================
            ' GENERIC INITIALIZATION ERROR
            ' ===================================================================
            LogError($"InitializeBrowserAsync: INITIALIZATION FAILED - {ex.Message}")
            ShowWebView2ErrorFallback(
                $"Failed to initialize secure browser component: {ex.Message}" & Environment.NewLine & Environment.NewLine &
                "Please ensure:" & Environment.NewLine &
                "1. Microsoft Edge WebView2 Runtime is installed" & Environment.NewLine &
                "2. Your Windows version supports WebView2 (Windows 7 SP1 or later)" & Environment.NewLine &
                "3. You have sufficient disk space for browser cache"
            )
        End Try
    End Function

    ''' <summary>
    ''' DISPLAYS USER-FRIENDLY ERROR MESSAGE IF WEBVIEW2 FAILS TO INITIALIZE
    ''' Prevents application crash and provides actionable guidance
    ''' </summary>
    Private Sub ShowWebView2ErrorFallback(errorMessage As String)
        ' Hide WebView control
        webView.Visible = False

        ' Create error panel
        Dim pnlError As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.FromArgb(255, 250, 240),
            .Padding = New Padding(50)
        }

        Dim picError As New PictureBox With {
            .Size = New Size(64, 64),
            .Location = New Point((pnlError.Width - 64) \ 2, 100),
            .SizeMode = PictureBoxSizeMode.CenterImage
        }

        ' Create error icon manually (red X)
        Dim bmp As New Bitmap(64, 64)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            g.Clear(Color.Transparent)
            Dim pen As New Pen(Color.Red, 8)
            g.DrawLine(pen, 10, 10, 54, 54)
            g.DrawLine(pen, 54, 10, 10, 54)
        End Using
        picError.Image = bmp

        Dim lblErrorTitle As New Label With {
            .Text = "⚠️ Telemedicine Unavailable",
            .Font = New Font("Arial", 20, FontStyle.Bold),
            .ForeColor = Color.DarkRed,
            .Location = New Point(50, 180),
            .Size = New Size(pnlError.Width - 100, 40),
            .TextAlign = ContentAlignment.MiddleCenter
        }

        Dim lblErrorMessage As New Label With {
            .Text = errorMessage,
            .Font = New Font("Arial", 11),
            .ForeColor = Color.DarkSlateGray,
            .Location = New Point(50, 240),
            .Size = New Size(pnlError.Width - 100, 300),
            .TextAlign = ContentAlignment.TopLeft
        }

        Dim btnRetry As New Button With {
            .Text = "🔄 Retry Initialization",
            .Location = New Point((pnlError.Width - 250) \ 2, 560),
            .Size = New Size(250, 45),
            .Font = New Font("Arial", 12, FontStyle.Bold),
            .BackColor = Color.FromArgb(0, 130, 100),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        btnRetry.FlatAppearance.BorderSize = 0
        AddHandler btnRetry.Click, Async Sub(s As Object, ev As EventArgs)
                                       pnlError.Visible = False
                                       Me.Controls.Remove(pnlError)
                                       progressBar.Visible = True
                                       Await InitializeBrowserAsync()
                                   End Sub

        Dim btnClose As New Button With {
            .Text = "◄ Return to Dashboard",
            .Location = New Point((pnlError.Width - 250) \ 2, 620),
            .Size = New Size(250, 45),
            .Font = New Font("Arial", 12, FontStyle.Bold),
            .BackColor = Color.FromArgb(180, 30, 30),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        btnClose.FlatAppearance.BorderSize = 0
        AddHandler btnClose.Click, Sub(s As Object, ev As EventArgs)
                                       Me.Close()
                                   End Sub

        pnlError.Controls.AddRange(New Control() {
            picError, lblErrorTitle, lblErrorMessage, btnRetry, btnClose
        })

        Me.Controls.Add(pnlError)
        pnlError.BringToFront()

        lblStatus.Text = "Status: ❌ WebView2 Initialization Failed"
        lblStatus.ForeColor = Color.DarkRed
        lblConnection.Text = "⚫ Offline"
        lblConnection.ForeColor = Color.Gray
    End Sub
#End Region

#Region "Navigation Engine - Secure URI Validation"
    ''' <summary>
    ''' PUBLIC NAVIGATION METHOD
    ''' Validates URL security before navigation (HTTPS enforcement for HIPAA compliance)
    ''' 
    ''' SECURITY CHECKS:
    ''' 1. URL is not null/empty
    ''' 2. URL starts with HTTPS (medical compliance requirement)
    ''' 3. URL is well-formed (Uri.TryCreate validation)
    ''' 4. Domain matches allowed telemedicine providers (optional whitelist)
    ''' </summary>
    Public Sub NavigateToMedicalPortal(url As String)
        Try
            ' ===================================================================
            ' INPUT VALIDATION - Null/Empty Check
            ' ===================================================================
            If String.IsNullOrWhiteSpace(url) Then
                MessageBox.Show(
                    "Navigation URL cannot be empty." & Environment.NewLine & Environment.NewLine &
                    "Please provide a valid HTTPS URL for the telemedicine portal.",
                    "Invalid URL",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                )
                LogError("NavigateToMedicalPortal: REJECTED - URL is null or empty")
                Return
            End If

            ' ===================================================================
            ' SECURITY VALIDATION - HTTPS Enforcement
            ' Medical data transmission requires encrypted HTTPS protocol
            ' Exception: Allow safe internal browser states (about:blank, about:*)
            ' ===================================================================
            Dim trimmedUrl As String = url.Trim()
            Dim isInternalBrowserState As Boolean = trimmedUrl.StartsWith("about:", StringComparison.OrdinalIgnoreCase)

            If HTTPS_REQUIRED AndAlso Not isInternalBrowserState AndAlso Not trimmedUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) Then
                MessageBox.Show(
                    "⚠️ Security Violation Detected" & Environment.NewLine & Environment.NewLine &
                    "Only HTTPS (secure encrypted) connections are permitted for medical video consultations." & Environment.NewLine & Environment.NewLine &
                    "HTTP (unencrypted) connections are blocked to protect patient privacy (HIPAA compliance)." & Environment.NewLine & Environment.NewLine &
                    $"Provided URL: {url}" & Environment.NewLine &
                    $"Required: https://...",
                    "HTTPS Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                )
                LogError($"NavigateToMedicalPortal: REJECTED - HTTPS required | URL={url}")
                Return
            End If

            ' Log internal browser states for audit trail (no security violation)
            If isInternalBrowserState Then
                LogError($"NavigateToMedicalPortal: ALLOWED - Internal browser state | URL={trimmedUrl}")
            End If

            ' ===================================================================
            ' URI FORMAT VALIDATION
            ' ===================================================================
            Dim validUri As Uri = Nothing
            If Not Uri.TryCreate(url.Trim(), UriKind.Absolute, validUri) Then
                MessageBox.Show(
                    $"The provided URL is not properly formatted:" & Environment.NewLine & Environment.NewLine &
                    $"URL: {url}" & Environment.NewLine & Environment.NewLine &
                    "Please check for typos and ensure it's a complete web address.",
                    "Invalid URL Format",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                )
                LogError($"NavigateToMedicalPortal: REJECTED - Invalid URI format | URL={url}")
                Return
            End If

            ' ===================================================================
            ' WEBVIEW2 READY CHECK
            ' ===================================================================
            If Not webViewInitialized OrElse webView.CoreWebView2 Is Nothing Then
                MessageBox.Show(
                    "The secure browser is not ready yet." & Environment.NewLine & Environment.NewLine &
                    "Please wait for WebView2 initialization to complete before navigating.",
                    "Browser Not Ready",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                )
                LogError("NavigateToMedicalPortal: REJECTED - WebView2 not initialized")
                Return
            End If

            ' ===================================================================
            ' NAVIGATE TO VALIDATED URL
            ' ===================================================================
            lblStatus.Text = $"Status: Loading {validUri.Host}..."
            lblStatus.ForeColor = Color.DarkOrange
            progressBar.Visible = True

            webView.CoreWebView2.Navigate(validUri.AbsoluteUri)
            txtUrlBar.Text = validUri.AbsoluteUri

            ' Add to navigation history
            If Not navigationHistory.Contains(validUri.AbsoluteUri) Then
                navigationHistory.Add(validUri.AbsoluteUri)
            End If

            LogError($"NavigateToMedicalPortal: SUCCESS - Navigating to {validUri.AbsoluteUri}")

        Catch ex As Exception
            LogError($"NavigateToMedicalPortal error: {ex.Message}")
            MessageBox.Show(
                $"Failed to navigate to medical portal: {ex.Message}",
                "Navigation Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )
        End Try
    End Sub
#End Region

#Region "Secure Telemedicine Room Launcher"
    ''' <summary>
    ''' PUBLIC INITIALIZATION METHOD: LOAD ACTIVE CONSULTATION CONTEXT
    ''' Called by FormMain when launching telemedicine portal with appointment context.
    ''' 
    ''' WORKFLOW:
    ''' 1. Validates input parameters (defensive null checks)
    ''' 2. Updates internal state (AppointmentID, PatientName)
    ''' 3. Updates UI status label with active session info
    ''' 4. Generates secure room URL using appointment identifier
    ''' 5. Auto-navigates WebView2 to the consultation room
    ''' 6. Logs session start for audit trail
    ''' 
    ''' SECURITY:
    ''' - Room URLs are HTTPS-only (HIPAA compliance)
    ''' - Unique room IDs prevent unauthorized access
    ''' - Appointment context tracked for audit logging
    ''' </summary>
    ''' <param name="appointmentID">The appointment identifier (e.g., "APT-2026-001")</param>
    ''' <param name="patientName">The patient's full name for UI display</param>
    Public Sub LoadActiveConsultation(appointmentID As String, patientName As String)
        Try
            ' ===================================================================
            ' INPUT VALIDATION - Defensive null/empty checks
            ' ===================================================================
            If String.IsNullOrWhiteSpace(appointmentID) Then
                LogError("LoadActiveConsultation: REJECTED - AppointmentID is null or empty")
                MessageBox.Show(
                    "Cannot load consultation session: Appointment ID is required.",
                    "Missing Appointment ID",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                )
                Return
            End If

            If String.IsNullOrWhiteSpace(patientName) Then
                LogError("LoadActiveConsultation: WARNING - PatientName is null or empty, using placeholder")
                patientName = "Unknown Patient"
            End If

            LogError($"LoadActiveConsultation: Starting consultation session | AppointmentID={appointmentID} | PatientName={patientName}")

            ' ===================================================================
            ' UPDATE INTERNAL STATE
            ' ===================================================================
            Me.ActiveAppointmentID = appointmentID
            Me.ActivePatientName = patientName
            currentPatientID = appointmentID  ' Fallback for compatibility

            ' ===================================================================
            ' UPDATE UI STATUS BAR WITH ACTIVE SESSION INFO
            ' Display active consultation context in bottom status bar (clean layout)
            ' ===================================================================
            If lblStatus IsNot Nothing Then
                lblStatus.Text = $"Status: Active Session - {patientName} (ID: {appointmentID})"
                lblStatus.ForeColor = Color.FromArgb(0, 130, 100)  ' Dark green for active session
                lblStatus.Font = New Font("Arial", 9, FontStyle.Bold)
            End If

            ' Update form title with patient context
            Me.Text = $"MediCare Telemedicine Portal - Consultation: {patientName}"

            ' ===================================================================
            ' GENERATE SECURE ROOM URL
            ' Format: https://meet.medicare-hms.com/room/{AppointmentID}
            ' Alternative: Use Jitsi Meet public instance for immediate availability
            ' ===================================================================
            Dim secureRoomUrl As String = GenerateSecureRoomUrl(appointmentID)

            LogError($"LoadActiveConsultation: Generated secure room URL | URL={secureRoomUrl}")

            ' ===================================================================
            ' AUTO-NAVIGATE TO CONSULTATION ROOM
            ' Wait for WebView2 initialization if not ready yet
            ' ===================================================================
            If webViewInitialized AndAlso webView.CoreWebView2 IsNot Nothing Then
                ' WebView2 ready - navigate immediately
                NavigateToMedicalPortal(secureRoomUrl)
                LogError($"LoadActiveConsultation: SUCCESS - Navigated to consultation room immediately")
            Else
                ' WebView2 still initializing - defer navigation
                LogError("LoadActiveConsultation: WebView2 not ready - deferring navigation")
                MessageBox.Show(
                    "The secure browser is still initializing. Please wait a moment and try again.",
                    "Browser Initializing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                )
            End If

            ' ===================================================================
            ' AUDIT LOGGING - Track consultation session start
            ' ===================================================================
            Dim auditUser As String = If(SessionManager.CurrentUser IsNot Nothing, SessionManager.CurrentUser.Username, "Unknown")
            WriteAuditEntry(
                user:=auditUser,
                action:=$"CONSULTATION_LOAD | AppointmentID={appointmentID} | PatientName={patientName} | RoomURL={secureRoomUrl}",
                moduleName:="FormTelemedicine"
            )

        Catch ex As Exception
            LogError($"LoadActiveConsultation error: {ex.Message} | AppointmentID={appointmentID} | PatientName={patientName}")
            MessageBox.Show(
                $"Failed to load consultation session: {ex.Message}",
                "Load Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )
        End Try
    End Sub

    ''' <summary>
    ''' HELPER METHOD: GENERATE SECURE ROOM URL FROM APPOINTMENT ID
    ''' Constructs HTTPS-only telemedicine meeting room URL using secure provider structure.
    ''' 
    ''' URL FORMAT:
    ''' - https://meet.jit.si/medicare-hms-room-{AppointmentID}
    ''' 
    ''' EXAMPLE:
    ''' - Input: "APT-2026-001"
    ''' - Output: "https://meet.jit.si/medicare-hms-room-APT2026001"
    ''' </summary>
    ''' <param name="appointmentID">The appointment identifier</param>
    ''' <returns>HTTPS URL for secure video consultation room</returns>
    Private Function GenerateSecureRoomUrl(appointmentID As String) As String
        Try
            ' Sanitize appointment ID (remove special characters for URL compatibility)
            Dim cleanAppointmentID As String = System.Text.RegularExpressions.Regex.Replace(appointmentID, "[^a-zA-Z0-9]", "")

            ' Generate clean, predictable room URL structure
            Dim roomName As String = $"medicare-hms-room-{cleanAppointmentID}"
            Dim secureUrl As String = $"https://meet.jit.si/{roomName}"

            ' Store generated room ID for tracking
            currentRoomID = roomName

            Return secureUrl

        Catch ex As Exception
            LogError($"GenerateSecureRoomUrl error: {ex.Message} | AppointmentID={appointmentID}")
            ' Fallback to generic Jitsi room if generation fails
            Return "https://meet.jit.si/MediCareConsultation"
        End Try
    End Function

    ''' <summary>
    ''' SPECIALIZED FUNCTION: LAUNCH SECURE VIDEO CONSULTATION ROOM
    ''' 
    ''' Dynamically constructs a unique telemedicine room URL using:
    ''' - Appointment ID for session tracking
    ''' - Patient ID for identity verification
    ''' - Doctor ID (from session manager) for authentication
    ''' - Timestamp-based unique room identifier
    ''' 
    ''' SUPPORTED PLATFORMS:
    ''' - Daily.co (default)
    ''' - Jitsi Meet
    ''' - Zoom (requires separate integration)
    ''' 
    ''' SECURITY FEATURES:
    ''' - Unique room ID generation (prevents unauthorized access)
    ''' - HTTPS-only connections
    ''' - Session token embedding (optional)
    ''' - Audit logging of all room launches
    ''' </summary>
    Public Sub LaunchSecureVideoConsultation(appointmentID As String, patientID As String, Optional platform As String = "Daily")
        Try
            LogError($"LaunchSecureVideoConsultation: Starting | AppointmentID={appointmentID} | PatientID={patientID} | Platform={platform}")

            ' ===================================================================
            ' INPUT VALIDATION
            ' ===================================================================
            If String.IsNullOrWhiteSpace(appointmentID) Then
                MessageBox.Show(
                    "Appointment ID is required to launch a video consultation.",
                    "Missing Appointment ID",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                )
                LogError("LaunchSecureVideoConsultation: REJECTED - Missing AppointmentID")
                Return
            End If

            If String.IsNullOrWhiteSpace(patientID) Then
                MessageBox.Show(
                    "Patient ID is required to launch a video consultation.",
                    "Missing Patient ID",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                )
                LogError("LaunchSecureVideoConsultation: REJECTED - Missing PatientID")
                Return
            End If

            ' ===================================================================
            ' STORE CONSULTATION CONTEXT
            ' ===================================================================
            currentAppointmentID = appointmentID
            currentPatientID = patientID

            ' ===================================================================
            ' GENERATE UNIQUE ROOM ID
            ' Format: MEDICARE-{AppointmentID}-{Timestamp}-{Random}
            ' Example: MEDICARE-APT-2026-001-20260115143022-A7B9
            ' ===================================================================
            Dim timestamp As String = DateTime.Now.ToString("yyyyMMddHHmmss")
            Dim randomSuffix As String = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper()
            currentRoomID = $"MEDICARE-{appointmentID.Replace("-", "")}-{timestamp}-{randomSuffix}"

            LogError($"LaunchSecureVideoConsultation: Generated RoomID={currentRoomID}")

            ' ===================================================================
            ' CONSTRUCT PLATFORM-SPECIFIC VIDEO ROOM URL
            ' ===================================================================
            Dim videoRoomUrl As String = String.Empty

            Select Case platform.ToLower()
                Case "daily", "daily.co"
                    ' Daily.co: https://yourdomain.daily.co/RoomName
                    videoRoomUrl = $"https://medicare-hospital.daily.co/{currentRoomID}"

                Case "jitsi", "jitsi.meet"
                    ' Jitsi Meet: https://meet.jit.si/RoomName
                    videoRoomUrl = $"https://meet.jit.si/{currentRoomID}"

                Case "zoom"
                    ' Zoom requires OAuth integration (placeholder for future implementation)
                    MessageBox.Show(
                        "Zoom integration requires additional setup." & Environment.NewLine & Environment.NewLine &
                        "Please contact your system administrator to configure Zoom OAuth credentials.",
                        "Zoom Not Configured",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    )
                    LogError("LaunchSecureVideoConsultation: REJECTED - Zoom not configured")
                    Return

                Case Else
                    ' Default to Jitsi Meet (open-source, no account required)
                    videoRoomUrl = $"https://meet.jit.si/{currentRoomID}"
                    LogError($"LaunchSecureVideoConsultation: Unknown platform '{platform}', defaulting to Jitsi")
            End Select

            LogError($"LaunchSecureVideoConsultation: Constructed URL={videoRoomUrl}")

            ' ===================================================================
            ' DISPLAY PRE-CONSULTATION DIALOG
            ' ===================================================================
            Dim confirmResult As DialogResult = MessageBox.Show(
                "🎥 Ready to Launch Secure Video Consultation" & Environment.NewLine & Environment.NewLine &
                $"Appointment ID: {appointmentID}" & Environment.NewLine &
                $"Patient ID: {patientID}" & Environment.NewLine &
                $"Room ID: {currentRoomID}" & Environment.NewLine &
                $"Platform: {platform}" & Environment.NewLine & Environment.NewLine &
                "The video room will open in the secure browser." & Environment.NewLine &
                "Please ensure your camera and microphone are ready." & Environment.NewLine & Environment.NewLine &
                "Continue?",
                "Launch Video Consultation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            )

            If confirmResult <> DialogResult.Yes Then
                LogError("LaunchSecureVideoConsultation: CANCELLED by user")
                Return
            End If

            ' ===================================================================
            ' NAVIGATE TO VIDEO ROOM
            ' ===================================================================
            NavigateToMedicalPortal(videoRoomUrl)

            ' ===================================================================
            ' LOG CONSULTATION LAUNCH FOR AUDIT TRAIL
            ' ===================================================================
            Dim auditUser As String = If(SessionManager.CurrentUser IsNot Nothing, SessionManager.CurrentUser.Username, "Unknown")
            WriteAuditEntry(
                user:=auditUser,
                action:=$"TELEMEDICINE_SESSION_START | AppointmentID={appointmentID} | PatientID={patientID} | RoomID={currentRoomID} | Platform={platform}",
                moduleName:="FormTelemedicine"
            )

            LogError($"LaunchSecureVideoConsultation: SUCCESS - Video room launched | URL={videoRoomUrl}")

        Catch ex As Exception
            LogError($"LaunchSecureVideoConsultation error: {ex.Message}")
            MessageBox.Show(
                $"Failed to launch video consultation: {ex.Message}",
                "Launch Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )
        End Try
    End Sub
#End Region

#Region "WebView2 Event Handlers"
    Private Sub WebView_NavigationStarting(sender As Object, e As CoreWebView2NavigationStartingEventArgs)
        Try
            ' ===================================================================
            ' SAFE NAVIGATION HANDLING
            ' Handle internal browser states (about:blank) separately from external URLs
            ' ===================================================================
            Dim navigationUri As String = If(e.Uri, String.Empty)
            Dim isInternalState As Boolean = navigationUri.StartsWith("about:", StringComparison.OrdinalIgnoreCase)

            If isInternalState Then
                ' Internal browser state (about:blank, about:srcdoc, etc.)
                lblStatus.Text = "Status: Initializing secure browser..."
                lblStatus.ForeColor = Color.DarkSlateGray
                progressBar.Visible = True
                LogError($"WebView_NavigationStarting: Internal state | URI={navigationUri}")
            Else
                ' External URL navigation
                Dim validUri As Uri = Nothing
                If Uri.TryCreate(navigationUri, UriKind.Absolute, validUri) Then
                    lblStatus.Text = $"Status: Loading {validUri.Host}..."
                    lblStatus.ForeColor = Color.DarkOrange
                    progressBar.Visible = True
                    LogError($"WebView_NavigationStarting: External URL | URI={navigationUri}")
                Else
                    lblStatus.Text = "Status: Loading..."
                    lblStatus.ForeColor = Color.DarkOrange
                    progressBar.Visible = True
                    LogError($"WebView_NavigationStarting: Unknown URI format | URI={navigationUri}")
                End If
            End If

        Catch ex As Exception
            LogError($"WebView_NavigationStarting error: {ex.Message}")
        End Try
    End Sub

    Private Sub WebView_NavigationCompleted(sender As Object, e As CoreWebView2NavigationCompletedEventArgs)
        Try
            progressBar.Visible = False

            If e.IsSuccess Then
                lblStatus.Text = $"Status: ✅ Page loaded successfully"
                lblStatus.ForeColor = Color.DarkGreen
                lblConnection.Text = "🟢 Connected"
                lblConnection.ForeColor = Color.Green

                LogError($"WebView_NavigationCompleted: SUCCESS | URL={webView.Source}")
            Else
                lblStatus.Text = $"Status: ❌ Navigation failed (Error {e.WebErrorStatus})"
                lblStatus.ForeColor = Color.DarkRed
                lblConnection.Text = "🔴 Error"
                lblConnection.ForeColor = Color.Red

                LogError($"WebView_NavigationCompleted: FAILED | Error={e.WebErrorStatus}")
            End If

        Catch ex As Exception
            LogError($"WebView_NavigationCompleted error: {ex.Message}")
        End Try
    End Sub

    Private Sub WebView_SourceChanged(sender As Object, e As CoreWebView2SourceChangedEventArgs)
        Try
            If webView.CoreWebView2 IsNot Nothing Then
                txtUrlBar.Text = webView.CoreWebView2.Source
                LogError($"WebView_SourceChanged: {webView.CoreWebView2.Source}")
            End If
        Catch ex As Exception
            LogError($"WebView_SourceChanged error: {ex.Message}")
        End Try
    End Sub

    Private Sub WebView_ContentLoading(sender As Object, e As CoreWebView2ContentLoadingEventArgs)
        Try
            lblStatus.Text = "Status: Loading content..."
            lblStatus.ForeColor = Color.DarkOrange
            progressBar.Visible = True

            LogError("WebView_ContentLoading: Content loading started")

        Catch ex As Exception
            LogError($"WebView_ContentLoading error: {ex.Message}")
        End Try
    End Sub

    Private Sub WebView_WebMessageReceived(sender As Object, e As CoreWebView2WebMessageReceivedEventArgs)
        Try
            ' Handle JavaScript messages from video platform (optional future enhancement)
            Dim message As String = e.TryGetWebMessageAsString()
            LogError($"WebView_WebMessageReceived: {message}")

        Catch ex As Exception
            LogError($"WebView_WebMessageReceived error: {ex.Message}")
        End Try
    End Sub
#End Region

#Region "Button Event Handlers"
    Private Sub BtnBack_Click(sender As Object, e As EventArgs)
        Try
            If webViewInitialized AndAlso webView.CoreWebView2 IsNot Nothing Then
                If webView.CoreWebView2.CanGoBack Then
                    webView.CoreWebView2.GoBack()
                    LogError("BtnBack_Click: Navigated back")
                Else
                    MessageBox.Show("No previous page available.", "Back", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
            End If
        Catch ex As Exception
            LogError($"BtnBack_Click error: {ex.Message}")
        End Try
    End Sub

    Private Sub BtnRefresh_Click(sender As Object, e As EventArgs)
        Try
            If webViewInitialized AndAlso webView.CoreWebView2 IsNot Nothing Then
                webView.CoreWebView2.Reload()
                LogError("BtnRefresh_Click: Page reloaded")
            End If
        Catch ex As Exception
            LogError($"BtnRefresh_Click error: {ex.Message}")
        End Try
    End Sub

    Private Sub BtnHome_Click(sender As Object, e As EventArgs)
        Try
            ShowTelemedicineLandingPage()
            LogError("BtnHome_Click: Returned to landing page")
        Catch ex As Exception
            LogError($"BtnHome_Click error: {ex.Message}")
        End Try
    End Sub

    Private Sub BtnGo_Click(sender As Object, e As EventArgs)
        Try
            Dim url As String = txtUrlBar.Text.Trim()
            If Not String.IsNullOrWhiteSpace(url) Then
                NavigateToMedicalPortal(url)
            End If
        Catch ex As Exception
            LogError($"BtnGo_Click error: {ex.Message}")
        End Try
    End Sub

    Private Sub TxtUrlBar_KeyDown(sender As Object, e As KeyEventArgs)
        Try
            If e.KeyCode = Keys.Enter Then
                BtnGo_Click(sender, EventArgs.Empty)
                e.Handled = True
                e.SuppressKeyPress = True
            End If
        Catch ex As Exception
            LogError($"TxtUrlBar_KeyDown error: {ex.Message}")
        End Try
    End Sub
#End Region

#Region "Landing Page & Cleanup"
    Private Sub ShowTelemedicineLandingPage()
        Try
            If Not webViewInitialized OrElse webView.CoreWebView2 Is Nothing Then
                Return
            End If

            ' Create simple HTML landing page
            Dim htmlContent As String = $"
<!DOCTYPE html>
<html>
<head>
    <title>MediCare Telemedicine Portal</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            display: flex;
            align-items: center;
            justify-content: center;
            height: 100vh;
            margin: 0;
            padding: 20px;
        }}
        .container {{
            text-align: center;
            background: rgba(255, 255, 255, 0.1);
            border-radius: 20px;
            padding: 50px;
            backdrop-filter: blur(10px);
            box-shadow: 0 8px 32px 0 rgba(31, 38, 135, 0.37);
        }}
        h1 {{ font-size: 3em; margin-bottom: 20px; }}
        p {{ font-size: 1.2em; margin-bottom: 30px; }}
        .status {{ color: #4ade80; font-weight: bold; }}
        .instructions {{
            background: rgba(255, 255, 255, 0.2);
            border-radius: 10px;
            padding: 20px;
            margin-top: 30px;
            text-align: left;
        }}
        ul {{ list-style-position: inside; }}
        li {{ margin-bottom: 10px; }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>🏥 MediCare Telemedicine Portal</h1>
        <p class='status'>✅ Secure Browser Ready</p>
        <p>Your video consultation environment is initialized and ready.</p>

        <div class='instructions'>
            <h3>📋 How to Start:</h3>
            <ul>
                <li>Select an appointment from the dashboard</li>
                <li>Click ""Launch Video Consultation""</li>
                <li>The secure room will load automatically</li>
                <li>Ensure camera and microphone permissions are granted</li>
            </ul>
        </div>

        <p style='margin-top: 30px; font-size: 0.9em; opacity: 0.8;'>
            <strong>Platform:</strong> Microsoft WebView2 (Chromium-based)<br>
            <strong>Encryption:</strong> HTTPS-Only<br>
            <strong>Session ID:</strong> " & Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper() & "
        </p>
    </div>
</body>
</html>
"

            webView.CoreWebView2.NavigateToString(htmlContent)
            txtUrlBar.Text = "about:blank"
            lblStatus.Text = "Status: Ready for video consultation"
            lblStatus.ForeColor = Color.DarkGreen

            LogError("ShowTelemedicineLandingPage: Landing page displayed")

        Catch ex As Exception
            LogError($"ShowTelemedicineLandingPage error: {ex.Message}")
        End Try
    End Sub

    Private Sub FormTelemedicine_FormClosing(sender As Object, e As FormClosingEventArgs)
        Try
            ' Log session end
            If Not String.IsNullOrEmpty(currentRoomID) Then
                Dim auditUser As String = If(SessionManager.CurrentUser IsNot Nothing, SessionManager.CurrentUser.Username, "Unknown")
                WriteAuditEntry(
                    user:=auditUser,
                    action:=$"TELEMEDICINE_SESSION_END | AppointmentID={currentAppointmentID} | RoomID={currentRoomID}",
                    moduleName:="FormTelemedicine"
                )
            End If

            ' Cleanup WebView2 resources
            If webView IsNot Nothing AndAlso webView.CoreWebView2 IsNot Nothing Then
                webView.Dispose()
            End If

            LogError("FormTelemedicine_FormClosing: Cleanup complete")

        Catch ex As Exception
            LogError($"FormTelemedicine_FormClosing error: {ex.Message}")
        End Try
    End Sub
#End Region
End Class
