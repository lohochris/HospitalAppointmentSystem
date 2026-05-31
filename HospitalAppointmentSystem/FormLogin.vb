Option Strict On
Option Explicit On

' ============================================================
' FormLogin.vb
' CSC3226 - Hospital Appointment System
' Login form - entry point for all users (Database Integrated)
' Group: Sa'id Umar, Aisha Ladan, Maryam Rabiu
' ============================================================

Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic

Public Class FormLogin
    Inherits Form

#Region "Constants"
    Private Const APP_VERSION As String = "1.0.0"
#End Region

#Region "Designer Controls"
    Private WithEvents btnLogin As Button
    Private WithEvents btnExit As Button
    Private txtUsername As TextBox
    Private txtPassword As TextBox
    Private lblTitle As Label
    Private lblSubtitle As Label
    Private lblUsername As Label
    Private lblPassword As Label
    Private pnlMain As Panel
    Private pnlHeader As Panel
    Private lblVersion As Label
    Private WithEvents chkShowPassword As CheckBox
    Private WithEvents lnkDemoCredentials As LinkLabel
#End Region

#Region "Initialization"
    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Size = New Size(480, 560)
        Me.Text = "Hospital Appointment System - Login"
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.BackColor = Color.FromArgb(240, 248, 255)

        ' Header panel
        pnlHeader = New Panel With {
            .Dock = DockStyle.Top,
            .Height = 140,
            .BackColor = Color.FromArgb(0, 102, 153)
        }

        ' Hospital icon
        Dim lblCross As New Label With {
            .Text = "H",
            .Font = New Font("Arial", 32, FontStyle.Bold),
            .ForeColor = Color.White,
            .AutoSize = True,
            .Location = New Point(210, 15),
            .TextAlign = ContentAlignment.MiddleCenter
        }

        lblTitle = New Label With {
            .Text = "MediCare Hospital",
            .Font = New Font("Arial", 18, FontStyle.Bold),
            .ForeColor = Color.White,
            .AutoSize = True,
            .Location = New Point(110, 70)
        }

        lblSubtitle = New Label With {
            .Text = "Appointment Management System",
            .Font = New Font("Arial", 10),
            .ForeColor = Color.FromArgb(180, 220, 255),
            .AutoSize = True,
            .Location = New Point(117, 105)
        }

        pnlHeader.Controls.AddRange(New Control() {lblCross, lblTitle, lblSubtitle})

        ' Main panel
        pnlMain = New Panel With {
            .Location = New Point(40, 160),
            .Size = New Size(400, 330),
            .BackColor = Color.White,
            .BorderStyle = BorderStyle.FixedSingle
        }

        Dim lblLoginTitle As New Label With {
            .Text = "Sign In",
            .Font = New Font("Arial", 16, FontStyle.Bold),
            .ForeColor = Color.FromArgb(0, 102, 153),
            .Location = New Point(165, 20),
            .AutoSize = True
        }

        lblUsername = New Label With {
            .Text = "Username:",
            .Font = New Font("Arial", 10),
            .Location = New Point(30, 65),
            .AutoSize = True
        }

        txtUsername = New TextBox With {
            .Location = New Point(30, 85),
            .Size = New Size(340, 30),
            .Font = New Font("Arial", 11),
            .BorderStyle = BorderStyle.FixedSingle
        }

        lblPassword = New Label With {
            .Text = "Password:",
            .Font = New Font("Arial", 10),
            .Location = New Point(30, 125),
            .AutoSize = True
        }

        txtPassword = New TextBox With {
            .Location = New Point(30, 145),
            .Size = New Size(340, 30),
            .Font = New Font("Arial", 11),
            .PasswordChar = "*"c,
            .BorderStyle = BorderStyle.FixedSingle
        }

        chkShowPassword = New CheckBox With {
            .Text = "Show Password",
            .Location = New Point(30, 185),
            .AutoSize = True,
            .Font = New Font("Arial", 9)
        }

        btnLogin = New Button With {
            .Text = "LOGIN",
            .Location = New Point(30, 215),
            .Size = New Size(340, 45),
            .Font = New Font("Arial", 12, FontStyle.Bold),
            .BackColor = Color.FromArgb(0, 153, 76),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        btnLogin.FlatAppearance.BorderSize = 0

        lnkDemoCredentials = New LinkLabel With {
            .Text = "View Database Credentials",
            .Location = New Point(125, 270),
            .AutoSize = True,
            .Font = New Font("Arial", 9)
        }

        pnlMain.Controls.AddRange(New Control() {
            lblLoginTitle, lblUsername, txtUsername,
            lblPassword, txtPassword, chkShowPassword,
            btnLogin, lnkDemoCredentials
        })

        btnExit = New Button With {
            .Text = "Exit",
            .Location = New Point(200, 500),
            .Size = New Size(80, 30),
            .Font = New Font("Arial", 9),
            .FlatStyle = FlatStyle.Flat,
            .BackColor = Color.FromArgb(200, 200, 200),
            .Cursor = Cursors.Hand
        }

        lblVersion = New Label With {
            .Text = String.Format("Version {0} | CSC3226 Group Project", APP_VERSION),
            .Location = New Point(112, 510),
            .AutoSize = True,
            .Font = New Font("Arial", 7),
            .ForeColor = Color.Gray
        }

        Me.Controls.AddRange(New Control() {pnlHeader, pnlMain, btnExit, lblVersion})
        Me.AcceptButton = btnLogin
    End Sub
#End Region

#Region "Event Handlers"

    Private Sub FormLogin_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            txtUsername.Focus()
        Catch ex As Exception
            MessageBox.Show("Error loading login form: " & ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        Try
            Dim username As String = txtUsername.Text.Trim()
            Dim password As String = txtPassword.Text

            ' Input validation
            If String.IsNullOrWhiteSpace(username) Then
                MessageBox.Show("Please enter your username.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtUsername.Focus()
                Return
            End If

            If String.IsNullOrWhiteSpace(password) Then
                MessageBox.Show("Please enter your password.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtPassword.Focus()
                Return
            End If

            ' Authenticate against the real Access database using ModuleDatabase
            Dim userAccount As UserAccount = ModuleDatabase.AuthenticateUser(username, password)

            If userAccount Is Nothing Then
                MessageBox.Show("Invalid username or password. Please try again.", "Login Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error)
                txtPassword.Clear()
                txtPassword.Focus()
                Return
            End If

            ' Set global session using SessionManager
            SessionManager.CurrentUser = userAccount

            ' Open main dashboard
            Dim mainDashboard As New FormMain()
            mainDashboard.Show()
            Me.Hide()

        Catch ex As Exception
            MessageBox.Show("An error occurred during login: " & ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnExit_Click(sender As Object, e As EventArgs) Handles btnExit.Click
        Dim result As DialogResult = MessageBox.Show("Are you sure you want to exit?", "Confirm Exit",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        If result = DialogResult.Yes Then
            Application.Exit()
        End If
    End Sub

    Private Sub chkShowPassword_CheckedChanged(sender As Object, e As EventArgs) Handles chkShowPassword.CheckedChanged
        If chkShowPassword.Checked Then
            txtPassword.PasswordChar = ControlChars.NullChar
        End If
    End Sub

    Private Sub lnkDemoCredentials_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles lnkDemoCredentials.LinkClicked
        Dim creds As String = "DATABASE USER CREDENTIALS:" & Environment.NewLine &
            Environment.NewLine &
            "Admin:" & ControlChars.Tab & ControlChars.Tab & "admin / Admin@123" & Environment.NewLine &
            "Doctor:" & ControlChars.Tab & ControlChars.Tab & "dr_james / Doctor@123" & Environment.NewLine &
            "Receptionist:" & ControlChars.Tab & "receptionist / Recep@123" & Environment.NewLine &
            "Patient:" & ControlChars.Tab & ControlChars.Tab & "patient1 / Patient@123"

        MessageBox.Show(creds, "Database Reference Credentials",
            MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

#End Region

End Class