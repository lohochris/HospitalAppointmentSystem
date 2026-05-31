Option Strict On
Option Explicit On

' ============================================================
' FormDoctorsManagement.vb
' CSC3226 - Hospital Appointment System
' Doctors Management UI with RBAC Username integration
' Group: Sa'id Umar, Aisha Ladan, Maryam Rabiu
' ============================================================

Imports System
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic

Public Class FormDoctorsManagement
    Inherits Form

#Region "Private Fields"
    Private _currentDoctorID As String = String.Empty
    Private _isEditMode As Boolean = False
    Private _fullDataTable As DataTable = Nothing

    ' UI Controls
    Private WithEvents dgvDoctors As DataGridView
    Private txtDoctorID As TextBox
    Private WithEvents txtUsername As TextBox
    Private WithEvents txtFirstName As TextBox
    Private WithEvents txtLastName As TextBox
    Private WithEvents txtPhone As TextBox
    Private WithEvents txtEmail As TextBox
    Private WithEvents txtRoom As TextBox
    Private WithEvents cmbSpecialization As ComboBox
    Private WithEvents cmbStatus As ComboBox
    Private WithEvents txtSearch As TextBox
    Private WithEvents btnSave As Button
    Private WithEvents btnClear As Button
    Private WithEvents btnArchive As Button
    Private lblRecordCount As Label
    Private lblDoctorID As Label
    Private panelTop As Panel
    Private panelLeft As Panel
    Private panelRight As Panel
    Private grpDoctorInfo As GroupBox
#End Region

#Region "Form Initialization"
    Public Sub New()
        MyBase.New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        ' ===================================================================
        ' FORM UI/UX CONFIGURATION - PROFESSIONAL WINDOWED DESIGN
        ' ===================================================================
        ' Configured as a movable, resizable dialog with proper window chrome
        ' Optimized dimensions for 1080p displays without fullscreen overlap
        ' ===================================================================

        ' Form properties - Professional windowed design
        Me.Text = "Doctors Management System"
        Me.Size = New Size(1400, 750)                    ' Professional aspect ratio (not fullscreen)
        Me.StartPosition = FormStartPosition.CenterScreen  ' Center on parent/screen
        Me.BackColor = Color.FromArgb(240, 248, 255)
        Me.FormBorderStyle = FormBorderStyle.Sizable      ' Standard window with title bar and borders
        Me.MaximizeBox = True                             ' Allow maximize if needed
        Me.MinimizeBox = True                             ' Allow minimize
        Me.MinimumSize = New Size(1200, 650)              ' Prevent shrinking below usable size
        Me.WindowState = FormWindowState.Normal           ' Launch as normal window (not maximized)

        ' Top Panel (Header) - Adjusted for standard windowed design
        panelTop = New Panel With {
            .Dock = DockStyle.Top,
            .Height = 70,
            .BackColor = Color.FromArgb(41, 128, 185)
        }

        Dim lblTitle As New Label With {
            .Text = "Doctors Management System",
            .Font = New Font("Segoe UI", 20.0!, FontStyle.Bold),
            .ForeColor = Color.White,
            .Location = New Point(20, 18),
            .AutoSize = True
        }

        ' Note: Custom close button removed - using standard window title bar controls
        ' Users can close via standard [X] button, Alt+F4, or ESC key

        panelTop.Controls.Add(lblTitle)

        ' Left Panel (Input Form) - PROFESSIONAL SCROLLABLE DESIGN
        ' ===================================================================
        ' Configured with AutoScroll for accessibility of all input controls
        ' Prevents cutoff on smaller displays or when form is not maximized
        ' ===================================================================
        panelLeft = New Panel With {
            .Dock = DockStyle.Left,
            .Width = 450,
            .Padding = New Padding(10, 10, 10, 20),
            .BackColor = Color.FromArgb(236, 240, 241),
            .AutoScroll = True
        }

        ' Disable horizontal scrolling (vertical only)
        panelLeft.HorizontalScroll.Enabled = False
        panelLeft.HorizontalScroll.Visible = False

        ' Group Box for Doctor Info - Expanded height to accommodate all controls
        grpDoctorInfo = New GroupBox With {
            .Text = "Doctor Information",
            .Font = New Font("Segoe UI", 12.0!, FontStyle.Bold),
            .Location = New Point(10, 10),
            .Size = New Size(410, 760),
            .ForeColor = Color.FromArgb(52, 73, 94),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        }

        ' Doctor ID (Read-only)
        lblDoctorID = New Label With {
            .Text = "Doctor ID:",
            .Font = New Font("Segoe UI", 10.0!),
            .Location = New Point(20, 40),
            .Size = New Size(100, 25),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left
        }
        txtDoctorID = New TextBox With {
            .Location = New Point(20, 70),
            .Size = New Size(360, 30),
            .Font = New Font("Segoe UI", 11.0!),
            .ReadOnly = True,
            .BackColor = Color.FromArgb(189, 195, 199),
            .Text = "[Auto-Generated]",
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        }

        ' Username (RBAC Link)
        Dim lblUsername As New Label With {
            .Text = "System Username (Optional):",
            .Font = New Font("Segoe UI", 10.0!),
            .Location = New Point(20, 115),
            .Size = New Size(200, 25),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left
        }
        txtUsername = New TextBox With {
            .Location = New Point(20, 145),
            .Size = New Size(360, 30),
            .Font = New Font("Segoe UI", 11.0!),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        }

        ' First Name
        Dim lblFirstName As New Label With {
            .Text = "First Name:*",
            .Font = New Font("Segoe UI", 10.0!),
            .ForeColor = Color.Red,
            .Location = New Point(20, 190),
            .Size = New Size(100, 25),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left
        }
        txtFirstName = New TextBox With {
            .Location = New Point(20, 220),
            .Size = New Size(360, 30),
            .Font = New Font("Segoe UI", 11.0!),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        }

        ' Last Name
        Dim lblLastName As New Label With {
            .Text = "Last Name:*",
            .Font = New Font("Segoe UI", 10.0!),
            .ForeColor = Color.Red,
            .Location = New Point(20, 265),
            .Size = New Size(100, 25),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left
        }
        txtLastName = New TextBox With {
            .Location = New Point(20, 295),
            .Size = New Size(360, 30),
            .Font = New Font("Segoe UI", 11.0!),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        }

        ' Specialization
        Dim lblSpecialization As New Label With {
            .Text = "Specialization:*",
            .Font = New Font("Segoe UI", 10.0!),
            .ForeColor = Color.Red,
            .Location = New Point(20, 340),
            .Size = New Size(120, 25),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left
        }
        cmbSpecialization = New ComboBox With {
            .Location = New Point(20, 370),
            .Size = New Size(360, 30),
            .Font = New Font("Segoe UI", 11.0!),
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        }

        ' Phone Number
        Dim lblPhone As New Label With {
            .Text = "Phone Number:*",
            .Font = New Font("Segoe UI", 10.0!),
            .ForeColor = Color.Red,
            .Location = New Point(20, 415),
            .Size = New Size(120, 25),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left
        }
        txtPhone = New TextBox With {
            .Location = New Point(20, 445),
            .Size = New Size(360, 30),
            .Font = New Font("Segoe UI", 11.0!),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        }

        ' Email
        Dim lblEmail As New Label With {
            .Text = "Email:",
            .Font = New Font("Segoe UI", 10.0!),
            .Location = New Point(20, 490),
            .Size = New Size(100, 25),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left
        }
        txtEmail = New TextBox With {
            .Location = New Point(20, 520),
            .Size = New Size(360, 30),
            .Font = New Font("Segoe UI", 11.0!),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        }

        ' Office Room
        Dim lblRoom As New Label With {
            .Text = "Office Room:",
            .Font = New Font("Segoe UI", 10.0!),
            .Location = New Point(20, 565),
            .Size = New Size(100, 25),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left
        }
        txtRoom = New TextBox With {
            .Location = New Point(20, 595),
            .Size = New Size(360, 30),
            .Font = New Font("Segoe UI", 11.0!),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        }

        ' Status
        Dim lblStatus As New Label With {
            .Text = "Status:*",
            .Font = New Font("Segoe UI", 10.0!),
            .ForeColor = Color.Red,
            .Location = New Point(20, 640),
            .Size = New Size(100, 25),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left
        }
        cmbStatus = New ComboBox With {
            .Location = New Point(20, 670),
            .Size = New Size(360, 30),
            .Font = New Font("Segoe UI", 11.0!),
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        }

        ' Add controls to group box
        grpDoctorInfo.Controls.AddRange(New Control() {
            lblDoctorID, txtDoctorID,
            lblUsername, txtUsername,
            lblFirstName, txtFirstName,
            lblLastName, txtLastName,
            lblSpecialization, cmbSpecialization,
            lblPhone, txtPhone,
            lblEmail, txtEmail,
            lblRoom, txtRoom,
            lblStatus, cmbStatus
        })

        ' Action Buttons Panel - Positioned below GroupBox with proper margin
        Dim panelButtons As New Panel With {
            .Location = New Point(10, 780),
            .Size = New Size(410, 80),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        }

        btnSave = New Button With {
            .Text = "💾 Save",
            .Location = New Point(10, 10),
            .Size = New Size(120, 50),
            .Font = New Font("Segoe UI", 11.0!, FontStyle.Bold),
            .BackColor = Color.FromArgb(39, 174, 96),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand,
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left
        }
        btnSave.FlatAppearance.BorderSize = 0

        btnClear = New Button With {
            .Text = "🔄 Clear",
            .Location = New Point(140, 10),
            .Size = New Size(120, 50),
            .Font = New Font("Segoe UI", 11.0!, FontStyle.Bold),
            .BackColor = Color.FromArgb(149, 165, 166),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand,
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left
        }
        btnClear.FlatAppearance.BorderSize = 0

        btnArchive = New Button With {
            .Text = "📁 Archive",
            .Location = New Point(270, 10),
            .Size = New Size(120, 50),
            .Font = New Font("Segoe UI", 11.0!, FontStyle.Bold),
            .BackColor = Color.FromArgb(231, 76, 60),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand,
            .Enabled = False,
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left
        }
        btnArchive.FlatAppearance.BorderSize = 0

        panelButtons.Controls.AddRange(New Control() {btnSave, btnClear, btnArchive})

        ' Add both containers to left panel
        ' Note: Total scrollable height = 780 + 80 + margins = ~880 pixels
        panelLeft.Controls.AddRange(New Control() {grpDoctorInfo, panelButtons})

        ' Right Panel (DataGridView)
        panelRight = New Panel With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(10),
            .BackColor = Color.White
        }

        ' Search Panel
        Dim panelSearch As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 80,
            .BackColor = Color.White
        }

        Dim lblSearch As New Label With {
            .Text = "🔍 Search (Name, Specialization, Username, or Phone):",
            .Font = New Font("Segoe UI", 11.0!, FontStyle.Bold),
            .Location = New Point(15, 10),
            .AutoSize = True
        }

        txtSearch = New TextBox With {
            .Location = New Point(15, 40),
            .Size = New Size(600, 30),
            .Font = New Font("Segoe UI", 11.0!)
        }

        lblRecordCount = New Label With {
            .Text = "Total Doctors: 0",
            .Font = New Font("Segoe UI", 11.0!, FontStyle.Bold),
            .Location = New Point(650, 40),
            .AutoSize = True,
            .ForeColor = Color.FromArgb(52, 73, 94)
        }

        panelSearch.Controls.AddRange(New Control() {lblSearch, txtSearch, lblRecordCount})

        ' DataGridView
        dgvDoctors = New DataGridView With {
            .Dock = DockStyle.Fill,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .ReadOnly = True,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .MultiSelect = False,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            .RowHeadersVisible = False,
            .EnableHeadersVisualStyles = False,
            .BackgroundColor = Color.White,
            .BorderStyle = BorderStyle.None
        }

        ' Header styling
        With dgvDoctors.ColumnHeadersDefaultCellStyle
            .BackColor = Color.FromArgb(41, 128, 185)
            .ForeColor = Color.White
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
            .Alignment = DataGridViewContentAlignment.MiddleCenter
        End With
        dgvDoctors.ColumnHeadersHeight = 45

        ' Row styling
        dgvDoctors.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(236, 240, 241)
        dgvDoctors.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219)
        dgvDoctors.DefaultCellStyle.SelectionForeColor = Color.White
        dgvDoctors.DefaultCellStyle.Font = New Font("Segoe UI", 9.5!)
        dgvDoctors.RowTemplate.Height = 40

        panelRight.Controls.AddRange(New Control() {dgvDoctors, panelSearch})

        ' Add all panels to form
        Me.Controls.AddRange(New Control() {panelRight, panelLeft, panelTop})
    End Sub

    Private Sub FormDoctorsManagement_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            ' ===================================================================
            ' FORM INITIALIZATION WITH PROFESSIONAL WINDOW BEHAVIOR
            ' ===================================================================
            ' Enforce proper windowing behavior for movable, resizable dialog
            ' This form is now ADMIN-EXCLUSIVE (enforced at dashboard level)
            ' ===================================================================

            ' Ensure proper window state (not maximized)
            Me.WindowState = FormWindowState.Normal
            Me.StartPosition = FormStartPosition.CenterScreen

            ' Verify user role (redundant check - dashboard already filters)
            Dim userRole As String = String.Empty
            If Me.Tag IsNot Nothing AndAlso TypeOf Me.Tag Is String Then
                userRole = DirectCast(Me.Tag, String)
            End If

            ' Security assertion: This form should only be accessible to Admins
            ' Dashboard already prevents non-Admin access, but double-check for defense-in-depth
            If Not String.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase) Then
                MessageBox.Show("Access Denied. This module is restricted to Administrators only.", _
                                "Security Violation", _
                                MessageBoxButtons.OK, _
                                MessageBoxIcon.Error)
                ModuleDatabase.LogError($"SECURITY BREACH: FormDoctorsManagement accessed by non-Admin role: {userRole}")
                Me.Close()
                Return
            End If

            ' Initialize specialization dropdown
            InitializeSpecializationComboBox()

            ' Initialize status dropdown
            InitializeStatusComboBox()

            ' Load doctors data
            LoadDoctorsGrid()

            ' Set initial button states
            btnArchive.Enabled = False

        Catch ex As Exception
            MessageBox.Show("Error initializing form: " & ex.Message, _
                            "Initialization Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Error)
            ModuleDatabase.LogError("FormDoctorsManagement_Load error: " & ex.Message)
        End Try
    End Sub

    Private Sub InitializeSpecializationComboBox()
        cmbSpecialization.Items.Clear()
        cmbSpecialization.Items.AddRange(New String() {
            "General Medicine",
            "Pediatrics",
            "Cardiology",
            "Orthopedics",
            "Neurology",
            "Dermatology",
            "Psychiatry",
            "Obstetrics & Gynecology",
            "Ophthalmology",
            "ENT (Ear, Nose, Throat)",
            "Radiology",
            "Anesthesiology",
            "Emergency Medicine",
            "Internal Medicine",
            "Surgery"
        })
        cmbSpecialization.SelectedIndex = 0
    End Sub

    Private Sub InitializeStatusComboBox()
        cmbStatus.Items.Clear()
        cmbStatus.Items.AddRange(New String() {
            "Active",
            "On Leave",
            "Retired",
            "Archived"
        })
        cmbStatus.SelectedIndex = 0
    End Sub
#End Region

#Region "Data Loading"
    Private Sub LoadDoctorsGrid()
        Try
            _fullDataTable = ModuleDatabase.GetDoctorsTable()

            If _fullDataTable IsNot Nothing Then
                dgvDoctors.DataSource = _fullDataTable

                ' Update record count
                lblRecordCount.Text = $"Total Doctors: {_fullDataTable.Rows.Count}"

                ' Hide DoctorID column (shown in text box when selected)
                If dgvDoctors.Columns.Contains("Doctor ID") Then
                    dgvDoctors.Columns("Doctor ID").Visible = False
                End If

                ' Hide DateRegistered column
                If dgvDoctors.Columns.Contains("Registered On") Then
                    dgvDoctors.Columns("Registered On").Visible = False
                End If

                ' Adjust column widths
                If dgvDoctors.Columns.Contains("Username") Then
                    dgvDoctors.Columns("Username").FillWeight = 80
                End If
                If dgvDoctors.Columns.Contains("First Name") Then
                    dgvDoctors.Columns("First Name").FillWeight = 80
                End If
                If dgvDoctors.Columns.Contains("Last Name") Then
                    dgvDoctors.Columns("Last Name").FillWeight = 80
                End If
                If dgvDoctors.Columns.Contains("Specialization") Then
                    dgvDoctors.Columns("Specialization").FillWeight = 120
                End If
                If dgvDoctors.Columns.Contains("Phone Number") Then
                    dgvDoctors.Columns("Phone Number").FillWeight = 90
                End If
                If dgvDoctors.Columns.Contains("Email") Then
                    dgvDoctors.Columns("Email").FillWeight = 120
                End If
                If dgvDoctors.Columns.Contains("Office Room") Then
                    dgvDoctors.Columns("Office Room").FillWeight = 70
                End If
                If dgvDoctors.Columns.Contains("Status") Then
                    dgvDoctors.Columns("Status").FillWeight = 70
                End If

            End If

        Catch ex As Exception
            MessageBox.Show("Error loading doctors list: " & ex.Message, _
                            "Load Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Error)
            ModuleDatabase.LogError("LoadDoctorsGrid error: " & ex.Message)
        End Try
    End Sub
#End Region

#Region "Button Event Handlers"
    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            ' Validate inputs
            If Not ValidateInputs() Then
                Return
            End If

            ' DYNAMIC VERIFICATION UPGRADE:
            ' If a username is provided, verify it exists in Users table with 'Doctor' role
            ' If it does NOT exist, offer automatic account provisioning with confirmation
            Dim username As String = txtUsername.Text.Trim()
            If Not String.IsNullOrWhiteSpace(username) Then
                If Not ModuleDatabase.ValidateDoctorUserAccount(username) Then
                    ' Username does not exist - offer automatic provisioning
                    Dim confirmResult As DialogResult = MessageBox.Show(
                        $"The username '{username}' does not exist." & vbCrLf & vbCrLf & _
                        "Would you like to automatically provision a new User Account for this Doctor " & _
                        "with a default temporary password?" & vbCrLf & vbCrLf & _
                        "Default Password: DocWelcome2026!" & vbCrLf & _
                        "(The doctor should change this on first login)", _
                        "Automatic Account Provisioning", _
                        MessageBoxButtons.YesNo, _
                        MessageBoxIcon.Question, _
                        MessageBoxDefaultButton.Button1)

                    If confirmResult = DialogResult.Yes Then
                        ' AUTOMATIC ACCOUNT PROVISIONING:
                        ' Construct full name from first and last name
                        Dim fullName As String = $"{txtFirstName.Text.Trim()} {txtLastName.Text.Trim()}"
                        Dim email As String = txtEmail.Text.Trim()
                        Dim phone As String = txtPhone.Text.Trim()

                        ' Attempt to provision the doctor user account
                        Dim provisionSuccess As Boolean = ModuleDatabase.ProvisionDoctorUserAccount(
                            username, fullName, email, phone)

                        If Not provisionSuccess Then
                            MessageBox.Show(
                                $"Failed to automatically provision user account for '{username}'." & vbCrLf & vbCrLf & _
                                "Please check the error log for details or create the account manually in User Management.", _
                                "Account Provisioning Failed", _
                                MessageBoxButtons.OK, _
                                MessageBoxIcon.Error)
                            txtUsername.Focus()
                            Return
                        End If

                        ' Account provisioned successfully - inform user and proceed
                        MessageBox.Show(
                            $"User account '{username}' created successfully!" & vbCrLf & vbCrLf & _
                            "Role: Doctor" & vbCrLf & _
                            "Temporary Password: DocWelcome2026!" & vbCrLf & vbCrLf & _
                            "Proceeding to save doctor record...", _
                            "Account Provisioned", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Information)
                    Else
                        ' User declined automatic provisioning - abort save
                        MessageBox.Show(
                            "Doctor record save cancelled." & vbCrLf & vbCrLf & _
                            "Please create the user account manually in User Management before saving this doctor record.", _
                            "Save Cancelled", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Information)
                        txtUsername.Focus()
                        Return
                    End If
                End If
            End If

            ' Save doctor record with Username for RBAC
            Dim doctorID As String = If(_isEditMode, _currentDoctorID, String.Empty)
            Dim savedID As String = ModuleDatabase.SaveDoctor(
                doctorID,
                username,
                txtFirstName.Text.Trim(),
                txtLastName.Text.Trim(),
                cmbSpecialization.SelectedItem.ToString(),
                txtPhone.Text.Trim(),
                txtEmail.Text.Trim(),
                txtRoom.Text.Trim(),
                cmbStatus.SelectedItem.ToString()
            )

            If Not String.IsNullOrEmpty(savedID) Then
                MessageBox.Show(
                    If(_isEditMode, "Doctor record updated successfully!", "Doctor record saved successfully!"),
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information)

                LoadDoctorsGrid()
                ClearFormFields()
            Else
                MessageBox.Show("Failed to save doctor record. The database operation returned no ID." & vbCrLf & vbCrLf & _
                                "Please check the application error log for detailed diagnostic information.", _
                                "Save Error", _
                                MessageBoxButtons.OK, _
                                MessageBoxIcon.Error)
            End If

        Catch ex As SQLite.SQLiteException
            ' COMPREHENSIVE ERROR EXPOSURE: Expose SQLite-specific errors
            Dim errorMsg As String = $"Database Error (SQLite Exception):{vbCrLf}{vbCrLf}" & _
                                     $"Message: {ex.Message}{vbCrLf}{vbCrLf}" & _
                                     $"Error Code: {ex.ErrorCode}{vbCrLf}{vbCrLf}"

            If ex.Message.Contains("FOREIGN KEY constraint failed") OrElse ex.Message.Contains("foreign key") Then
                errorMsg &= "This appears to be a FOREIGN KEY violation." & vbCrLf & _
                            "The username may not exist in the Users table with the correct role."
            End If

            MessageBox.Show(errorMsg, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ModuleDatabase.LogError($"btnSave_Click SQLiteException: {ex.Message} | ErrorCode: {ex.ErrorCode} | StackTrace: {ex.StackTrace}")

        Catch ex As Exception
            ' COMPREHENSIVE ERROR EXPOSURE: Show exact exception details
            MessageBox.Show(
                $"Error saving doctor record:{vbCrLf}{vbCrLf}" & _
                $"Exception Type: {ex.GetType().Name}{vbCrLf}{vbCrLf}" & _
                $"Message: {ex.Message}{vbCrLf}{vbCrLf}" & _
                $"Please check the application error log for full diagnostic details.", _
                "Error", _
                MessageBoxButtons.OK, _
                MessageBoxIcon.Error)
            ModuleDatabase.LogError($"btnSave_Click error: {ex.Message} | StackTrace: {ex.StackTrace}")
        End Try
    End Sub

    Private Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
        ClearFormFields()
    End Sub

    Private Sub btnArchive_Click(sender As Object, e As EventArgs) Handles btnArchive.Click
        Try
            If String.IsNullOrEmpty(_currentDoctorID) Then
                Return
            End If

            Dim result As DialogResult = MessageBox.Show(
                $"Are you sure you want to archive Dr. {txtFirstName.Text} {txtLastName.Text}?" & vbCrLf & vbCrLf & _
                "This will set their status to 'Archived'.",
                "Confirm Archive",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning)

            If result = DialogResult.Yes Then
                If ModuleDatabase.ArchiveDoctor(_currentDoctorID) Then
                    MessageBox.Show("Doctor archived successfully!", _
                                    "Success", _
                                    MessageBoxButtons.OK, _
                                    MessageBoxIcon.Information)
                    LoadDoctorsGrid()
                    ClearFormFields()
                Else
                    MessageBox.Show("Failed to archive doctor. Please check the error log.", _
                                    "Archive Error", _
                                    MessageBoxButtons.OK, _
                                    MessageBoxIcon.Error)
                End If
            End If

        Catch ex As Exception
            MessageBox.Show("Error archiving doctor: " & ex.Message, _
                            "Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Error)
            ModuleDatabase.LogError("btnArchive_Click error: " & ex.Message)
        End Try
    End Sub
#End Region

#Region "DataGridView Events"
    Private Sub dgvDoctors_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvDoctors.CellClick
        Try
            If e.RowIndex < 0 OrElse e.RowIndex >= dgvDoctors.Rows.Count Then
                Return
            End If

            Dim selectedRow As DataGridViewRow = dgvDoctors.Rows(e.RowIndex)

            ' Safely extract values with null checks (Option Strict On compliant)
            _currentDoctorID = GetCellValueSafe(selectedRow, "Doctor ID")
            txtDoctorID.Text = _currentDoctorID

            txtUsername.Text = GetCellValueSafe(selectedRow, "Username")
            txtFirstName.Text = GetCellValueSafe(selectedRow, "First Name")
            txtLastName.Text = GetCellValueSafe(selectedRow, "Last Name")
            txtPhone.Text = GetCellValueSafe(selectedRow, "Phone Number")
            txtEmail.Text = GetCellValueSafe(selectedRow, "Email")
            txtRoom.Text = GetCellValueSafe(selectedRow, "Office Room")

            ' Set combo box selections safely
            Dim specialization As String = GetCellValueSafe(selectedRow, "Specialization")
            If cmbSpecialization.Items.Contains(specialization) Then
                cmbSpecialization.SelectedItem = specialization
            End If

            Dim status As String = GetCellValueSafe(selectedRow, "Status")
            If cmbStatus.Items.Contains(status) Then
                cmbStatus.SelectedItem = status
            End If

            ' Enable edit mode
            _isEditMode = True
            btnArchive.Enabled = True

        Catch ex As Exception
            MessageBox.Show("Error loading doctor record: " & ex.Message, _
                            "Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Error)
            ModuleDatabase.LogError("dgvDoctors_CellClick error: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Safely retrieves a cell value from a DataGridView row with null/DBNull handling.
    ''' Option Strict On compliant - explicit type conversions.
    ''' </summary>
    Private Function GetCellValueSafe(row As DataGridViewRow, columnName As String) As String
        Try
            If row Is Nothing OrElse Not dgvDoctors.Columns.Contains(columnName) Then
                Return String.Empty
            End If

            Dim cellValue As Object = row.Cells(columnName).Value

            If cellValue Is Nothing OrElse DBNull.Value.Equals(cellValue) Then
                Return String.Empty
            End If

            Return cellValue.ToString().Trim()

        Catch ex As Exception
            ModuleDatabase.LogError($"GetCellValueSafe error for column '{columnName}': {ex.Message}")
            Return String.Empty
        End Try
    End Function
#End Region

#Region "Search Functionality"
    Private Sub txtSearch_TextChanged(sender As Object, e As EventArgs) Handles txtSearch.TextChanged
        Try
            Dim searchTerm As String = txtSearch.Text.Trim()

            If String.IsNullOrWhiteSpace(searchTerm) Then
                ' Restore full data table
                If _fullDataTable IsNot Nothing Then
                    dgvDoctors.DataSource = _fullDataTable
                    lblRecordCount.Text = $"Total Doctors: {_fullDataTable.Rows.Count}"
                End If
            Else
                ' Perform search
                Dim filteredTable As DataTable = ModuleDatabase.SearchDoctors(searchTerm)
                dgvDoctors.DataSource = filteredTable
                lblRecordCount.Text = $"Found: {filteredTable.Rows.Count}"
            End If

        Catch ex As Exception
            ModuleDatabase.LogError("txtSearch_TextChanged error: " & ex.Message)
        End Try
    End Sub
#End Region

#Region "Validation and Helper Methods"
    Private Function ValidateInputs() As Boolean
        ' First Name validation
        If String.IsNullOrWhiteSpace(txtFirstName.Text) Then
            MessageBox.Show("Please enter doctor's first name.", _
                            "Validation Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Warning)
            txtFirstName.Focus()
            Return False
        End If

        ' Last Name validation
        If String.IsNullOrWhiteSpace(txtLastName.Text) Then
            MessageBox.Show("Please enter doctor's last name.", _
                            "Validation Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Warning)
            txtLastName.Focus()
            Return False
        End If

        ' Specialization validation
        If cmbSpecialization.SelectedIndex < 0 Then
            MessageBox.Show("Please select a specialization.", _
                            "Validation Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Warning)
            cmbSpecialization.Focus()
            Return False
        End If

        ' Phone Number validation
        If String.IsNullOrWhiteSpace(txtPhone.Text) Then
            MessageBox.Show("Please enter a phone number.", _
                            "Validation Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Warning)
            txtPhone.Focus()
            Return False
        End If

        ' Email validation (if provided)
        If Not String.IsNullOrWhiteSpace(txtEmail.Text) Then
            If Not IsValidEmail(txtEmail.Text) Then
                MessageBox.Show("Please enter a valid email address.", _
                                "Validation Error", _
                                MessageBoxButtons.OK, _
                                MessageBoxIcon.Warning)
                txtEmail.Focus()
                Return False
            End If
        End If

        ' Status validation
        If cmbStatus.SelectedIndex < 0 Then
            MessageBox.Show("Please select a status.", _
                            "Validation Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Warning)
            cmbStatus.Focus()
            Return False
        End If

        Return True
    End Function

    Private Function IsValidEmail(email As String) As Boolean
        Try
            If String.IsNullOrWhiteSpace(email) Then
                Return False
            End If

            ' Simple email validation
            Dim emailPattern As String = "^[^@\s]+@[^@\s]+\.[^@\s]+$"
            Return System.Text.RegularExpressions.Regex.IsMatch(email, emailPattern)

        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Sub ClearFormFields()
        txtDoctorID.Text = "[Auto-Generated]"
        txtUsername.Clear()
        txtFirstName.Clear()
        txtLastName.Clear()
        cmbSpecialization.SelectedIndex = 0
        txtPhone.Clear()
        txtEmail.Clear()
        txtRoom.Clear()
        cmbStatus.SelectedIndex = 0

        _currentDoctorID = String.Empty
        _isEditMode = False
        btnArchive.Enabled = False

        txtUsername.Focus()
    End Sub

    ''' <summary>
    ''' Restricts phone number input to digits, +, and - only
    ''' </summary>
    Private Sub txtPhone_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtPhone.KeyPress
        If Not Char.IsDigit(e.KeyChar) AndAlso _
           Not Char.IsControl(e.KeyChar) AndAlso _
           e.KeyChar <> "+"c AndAlso _
           e.KeyChar <> "-"c Then
            e.Handled = True
        End If
    End Sub
#End Region

End Class
