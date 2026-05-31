Option Strict On
Option Explicit On

' ============================================================
' FormPatientPortal.vb
' CSC3226 - Hospital Appointment System
' Patient registration and portal
' Group: Sa'id Umar, Aisha Ladan, Maryam Rabiu
' ============================================================

Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

Public Class FormPatientPortal
    Inherits Form

    Private _registrationMode As Boolean
    Private WithEvents btnSave As Button
    Private WithEvents btnClose As Button
    Private txtFullName As TextBox
    Private txtUsername As TextBox
    Private txtPassword As TextBox
    Private txtEmail As TextBox
    Private txtPhone As TextBox
    Private dtpDOB As DateTimePicker
    Private cmbGender As ComboBox
    Private txtAddress As TextBox
    Private cmbBloodGroup As ComboBox
    Private txtEmergencyContact As TextBox
    Private txtEmergencyPhone As TextBox
    Private dgvAppointments As DataGridView

    Public Sub New(registrationMode As Boolean)
        _registrationMode = registrationMode
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Size = New Size(700, 620)
        Me.Text = If(_registrationMode, "Register New Patient", "Patient Portal")
        Me.StartPosition = FormStartPosition.CenterParent
        Me.BackColor = Color.FromArgb(240, 248, 255)

        Dim tabCtrl As New TabControl With {.Dock = DockStyle.Fill, .Font = New Font("Arial", 10)}
        Dim tabReg As New TabPage("👤 Registration") With {.BackColor = Color.FromArgb(240, 248, 255)}
        Dim tabHistory As New TabPage("📋 Appointment History") With {.BackColor = Color.FromArgb(240, 248, 255)}

        BuildRegistrationTab(tabReg)
        BuildHistoryTab(tabHistory)

        tabCtrl.TabPages.AddRange(New TabPage() {tabReg, tabHistory})
        Me.Controls.Add(tabCtrl)
    End Sub

    Private Sub BuildRegistrationTab(tab As TabPage)
        Dim pnl As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(20), .AutoScroll = True}

        Dim lblTitle As New Label With {
            .Text = "Patient Registration Form",
            .Font = New Font("Arial", 14, FontStyle.Bold),
            .ForeColor = Color.FromArgb(0, 102, 153),
            .Dock = DockStyle.Top,
            .Height = 40
        }

        Dim formPanel As New TableLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 460,
            .ColumnCount = 2,
            .RowCount = 12,
            .Padding = New Padding(10),
            .BackColor = Color.White,
            .CellBorderStyle = TableLayoutPanelCellBorderStyle.None
        }
        formPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 30))
        formPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 70))

        Dim AddRow As Action(Of String, Control) = Sub(label As String, ctrl As Control)
            Dim lbl As New Label With {
                .Text = label,
                .Font = New Font("Arial", 9, FontStyle.Bold),
                .Anchor = AnchorStyles.Left Or AnchorStyles.Top,
                .AutoSize = True,
                .Margin = New Padding(5, 8, 5, 2)
            }
            ctrl.Margin = New Padding(0, 5, 10, 2)
            ctrl.Dock = DockStyle.Fill
            formPanel.Controls.Add(lbl)
            formPanel.Controls.Add(ctrl)
        End Sub

        txtFullName = New TextBox With {.Font = New Font("Arial", 9)}
        txtUsername = New TextBox With {.Font = New Font("Arial", 9)}
        txtPassword = New TextBox With {.Font = New Font("Arial", 9), .PasswordChar = "●"c}
        txtEmail = New TextBox With {.Font = New Font("Arial", 9)}
        txtPhone = New TextBox With {.Font = New Font("Arial", 9)}
        dtpDOB = New DateTimePicker With {.Format = DateTimePickerFormat.Short, .MaxDate = DateTime.Now, .Font = New Font("Arial", 9)}
        cmbGender = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Font = New Font("Arial", 9)}
        cmbGender.Items.AddRange(New String() {"Male", "Female", "Other"})
        cmbGender.SelectedIndex = 0
        txtAddress = New TextBox With {.Font = New Font("Arial", 9)}
        cmbBloodGroup = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Font = New Font("Arial", 9)}
        cmbBloodGroup.Items.AddRange(New String() {"A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"})
        cmbBloodGroup.SelectedIndex = 0
        txtEmergencyContact = New TextBox With {.Font = New Font("Arial", 9)}
        txtEmergencyPhone = New TextBox With {.Font = New Font("Arial", 9)}

        AddRow("Full Name *", txtFullName)
        AddRow("Username *", txtUsername)
        AddRow("Password *", txtPassword)
        AddRow("Email *", txtEmail)
        AddRow("Phone *", txtPhone)
        AddRow("Date of Birth", dtpDOB)
        AddRow("Gender", cmbGender)
        AddRow("Address", txtAddress)
        AddRow("Blood Group", cmbBloodGroup)
        AddRow("Emergency Contact", txtEmergencyContact)
        AddRow("Emergency Phone", txtEmergencyPhone)

        Dim pnlBtns As New FlowLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 55,
            .FlowDirection = FlowDirection.LeftToRight,
            .Padding = New Padding(10, 5, 0, 0)
        }
        btnSave = New Button With {
            .Text = "✅ Register Patient",
            .Size = New Size(160, 38),
            .Font = New Font("Arial", 10, FontStyle.Bold),
            .BackColor = Color.FromArgb(0, 153, 76),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        btnSave.FlatAppearance.BorderSize = 0
        btnClose = New Button With {
            .Text = "Cancel",
            .Size = New Size(90, 38),
            .Font = New Font("Arial", 10),
            .BackColor = Color.Gray,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        btnClose.FlatAppearance.BorderSize = 0
        pnlBtns.Controls.AddRange(New Control() {btnSave, btnClose})

        pnl.Controls.Add(pnlBtns)
        pnl.Controls.Add(formPanel)
        pnl.Controls.Add(lblTitle)
        tab.Controls.Add(pnl)
    End Sub

    Private Sub BuildHistoryTab(tab As TabPage)
        Dim pnl As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(10)}
        dgvAppointments = New DataGridView With {
            .Dock = DockStyle.Fill,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            .ReadOnly = True,
            .AllowUserToAddRows = False,
            .RowHeadersVisible = False,
            .Font = New Font("Arial", 9),
            .BackgroundColor = Color.White
        }
        dgvAppointments.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 102, 153)
        dgvAppointments.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        pnl.Controls.Add(dgvAppointments)
        tab.Controls.Add(pnl)
    End Sub

    Private Sub FormPatientPortal_Load(sender As Object, e As EventArgs) Handles Me.Load
        If SessionManager.CurrentUser?.Role = "Patient" Then
            Dim row As DataRow = GetPatientByUserID(SessionManager.CurrentUser.UserID)
            If row IsNot Nothing Then
                Dim patID As Integer = CInt(row("PatientID"))
                dgvAppointments.DataSource = GetAppointmentsByPatient(patID)
            End If
        End If
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            Dim errMsg As String = ""
            If Not ValidatePatientForm(txtFullName.Text, txtUsername.Text, txtPassword.Text,
                                        txtEmail.Text, txtPhone.Text, errMsg) Then
                MessageBox.Show(errMsg, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            If RegisterPatient(txtFullName.Text.Trim(), txtUsername.Text.Trim(), txtPassword.Text,
                               txtEmail.Text.Trim(), txtPhone.Text.Trim(),
                               dtpDOB.Value.ToString("yyyy-MM-dd"), cmbGender.SelectedItem.ToString(),
                               txtAddress.Text.Trim(), cmbBloodGroup.SelectedItem.ToString(),
                               txtEmergencyContact.Text.Trim(), txtEmergencyPhone.Text.Trim()) Then

                MessageBox.Show($"Patient '{txtFullName.Text}' registered successfully!" &
                    Environment.NewLine & $"Username: {txtUsername.Text}",
                    "Registration Successful", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Me.DialogResult = DialogResult.OK
                Me.Close()
            Else
                MessageBox.Show("Registration failed. Username may already exist.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Catch ex As Exception
            MessageBox.Show("Error during registration: " & ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
            LogError("FormPatientPortal btnSave: " & ex.Message)
        End Try
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class

' ============================================================
' FormMedicalRecords.vb
' ============================================================

Public Class FormMedicalRecords
    Inherits Form

    Private WithEvents btnSave As Button
    Private WithEvents btnClose As Button
    Private WithEvents btnRefresh As Button
    Private WithEvents cmbPatient As ComboBox
    Private txtDiagnosis As TextBox
    Private txtPrescription As TextBox
    Private txtTestResults As TextBox
    Private txtNotes As TextBox
    Private dgvRecords As DataGridView
    Private _patients As New List(Of KeyValuePair(Of Integer, String))()

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Size = New Size(900, 650)
        Me.Text = "Medical Records"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.BackColor = Color.FromArgb(240, 248, 255)

        Dim tabCtrl As New TabControl With {.Dock = DockStyle.Fill, .Font = New Font("Arial", 10)}
        Dim tabAdd As New TabPage("➕ Add Record") With {.BackColor = Color.FromArgb(240, 248, 255)}
        Dim tabView As New TabPage("📋 View Records") With {.BackColor = Color.FromArgb(240, 248, 255)}

        ' Add record tab
        Dim pnlAdd As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(20)}
        Dim lblTitle As New Label With {.Text = "Add Medical Record", .Font = New Font("Arial", 14, FontStyle.Bold), .ForeColor = Color.FromArgb(0, 102, 153), .Dock = DockStyle.Top, .Height = 40}

        Dim pnlForm As New Panel With {.Dock = DockStyle.Top, .Height = 360, .BackColor = Color.White, .Padding = New Padding(15), .BorderStyle = BorderStyle.FixedSingle}

        Dim lbPatient As New Label With {.Text = "Patient:", .Location = New Point(15, 18), .AutoSize = True, .Font = New Font("Arial", 9, FontStyle.Bold)}
        cmbPatient = New ComboBox With {.Location = New Point(150, 15), .Size = New Size(300, 25), .DropDownStyle = ComboBoxStyle.DropDownList, .Font = New Font("Arial", 9)}

        Dim lbDiag As New Label With {.Text = "Diagnosis:", .Location = New Point(15, 55), .AutoSize = True, .Font = New Font("Arial", 9, FontStyle.Bold)}
        txtDiagnosis = New TextBox With {.Location = New Point(150, 52), .Size = New Size(500, 55), .Multiline = True, .Font = New Font("Arial", 9)}

        Dim lbPres As New Label With {.Text = "Prescription:", .Location = New Point(15, 120), .AutoSize = True, .Font = New Font("Arial", 9, FontStyle.Bold)}
        txtPrescription = New TextBox With {.Location = New Point(150, 117), .Size = New Size(500, 55), .Multiline = True, .Font = New Font("Arial", 9)}

        Dim lbTest As New Label With {.Text = "Test Results:", .Location = New Point(15, 185), .AutoSize = True, .Font = New Font("Arial", 9, FontStyle.Bold)}
        txtTestResults = New TextBox With {.Location = New Point(150, 182), .Size = New Size(500, 55), .Multiline = True, .Font = New Font("Arial", 9)}

        Dim lbNotes As New Label With {.Text = "Notes:", .Location = New Point(15, 250), .AutoSize = True, .Font = New Font("Arial", 9, FontStyle.Bold)}
        txtNotes = New TextBox With {.Location = New Point(150, 247), .Size = New Size(500, 40), .Multiline = True, .Font = New Font("Arial", 9)}

        btnSave = New Button With {.Text = "💾 Save Record", .Location = New Point(150, 305), .Size = New Size(150, 38), .Font = New Font("Arial", 10, FontStyle.Bold), .BackColor = Color.FromArgb(0, 153, 76), .ForeColor = Color.White, .FlatStyle = FlatStyle.Flat, .Cursor = Cursors.Hand}
        btnSave.FlatAppearance.BorderSize = 0
        btnClose = New Button With {.Text = "Close", .Location = New Point(310, 305), .Size = New Size(90, 38), .Font = New Font("Arial", 10), .BackColor = Color.Gray, .ForeColor = Color.White, .FlatStyle = FlatStyle.Flat, .Cursor = Cursors.Hand}
        btnClose.FlatAppearance.BorderSize = 0

        pnlForm.Controls.AddRange(New Control() {lbPatient, cmbPatient, lbDiag, txtDiagnosis, lbPres, txtPrescription, lbTest, txtTestResults, lbNotes, txtNotes, btnSave, btnClose})
        pnlAdd.Controls.Add(pnlForm)
        pnlAdd.Controls.Add(lblTitle)
        tabAdd.Controls.Add(pnlAdd)

        ' View tab
        Dim pnlView As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(10)}
        btnRefresh = New Button With {.Text = "🔄 Refresh", .Dock = DockStyle.Top, .Height = 35, .Font = New Font("Arial", 9), .BackColor = Color.FromArgb(0, 102, 153), .ForeColor = Color.White, .FlatStyle = FlatStyle.Flat}
        dgvRecords = New DataGridView With {.Dock = DockStyle.Fill, .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, .ReadOnly = True, .AllowUserToAddRows = False, .RowHeadersVisible = False, .Font = New Font("Arial", 9), .BackgroundColor = Color.White}
        dgvRecords.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 102, 153)
        dgvRecords.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        pnlView.Controls.Add(dgvRecords)
        pnlView.Controls.Add(btnRefresh)
        tabView.Controls.Add(pnlView)

        tabCtrl.TabPages.AddRange(New TabPage() {tabAdd, tabView})
        Me.Controls.Add(tabCtrl)
    End Sub

    Private Sub FormMedicalRecords_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            _patients.Clear()
            cmbPatient.Items.Clear()
            cmbPatient.Items.Add("-- Select Patient --")
            Dim dt As DataTable = GetDataTable("SELECT p.PatientID, u.FullName FROM Patients p INNER JOIN Users u ON p.UserID=u.UserID ORDER BY u.FullName")
            For Each row As DataRow In dt.Rows
                _patients.Add(New KeyValuePair(Of Integer, String)(CInt(row("PatientID")), row("FullName").ToString()))
                cmbPatient.Items.Add($"[{row("PatientID")}] {row("FullName")}")
            Next
            cmbPatient.SelectedIndex = 0

            If SessionManager.CurrentUser?.Role = "Patient" Then
                Dim patRow As DataRow = GetPatientByUserID(SessionManager.CurrentUser.UserID)
                If patRow IsNot Nothing Then
                    Dim pid As Integer = CInt(patRow("PatientID"))
                    dgvRecords.DataSource = GetMedicalRecordsByPatient(pid)
                End If
                cmbPatient.Enabled = False
                btnSave.Enabled = False
            End If
        Catch ex As Exception
            LogError("FormMedicalRecords_Load: " & ex.Message)
        End Try
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            If cmbPatient.SelectedIndex <= 0 Then
                MessageBox.Show("Please select a patient.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
            End If
            If String.IsNullOrWhiteSpace(txtDiagnosis.Text) Then
                MessageBox.Show("Diagnosis is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
            End If

            Dim patIndex As Integer = cmbPatient.SelectedIndex - 1
            Dim patID As Integer = _patients(patIndex).Key

            ' Find doctor ID from logged in user
            Dim docDT As DataTable = GetDataTable($"SELECT DoctorID FROM Doctors WHERE UserID={SessionManager.CurrentUser?.UserID}")
            Dim docID As Integer = If(docDT.Rows.Count > 0, CInt(docDT.Rows(0)("DoctorID")), 1)

            If AddMedicalRecord(patID, docID, "", txtDiagnosis.Text, txtPrescription.Text, txtTestResults.Text, txtNotes.Text) Then
                MessageBox.Show("Medical record saved successfully.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information)
                txtDiagnosis.Clear() : txtPrescription.Clear() : txtTestResults.Clear() : txtNotes.Clear()
                dgvRecords.DataSource = GetMedicalRecordsByPatient(patID)
            Else
                MessageBox.Show("Failed to save record.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            LogError("FormMedicalRecords btnSave: " & ex.Message)
        End Try
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        If cmbPatient.SelectedIndex > 0 Then
            Dim pid As Integer = _patients(cmbPatient.SelectedIndex - 1).Key
            dgvRecords.DataSource = GetMedicalRecordsByPatient(pid)
        Else
            ' Show all records if admin
            If SessionManager.CurrentUser?.Role = "Admin" Then
                dgvRecords.DataSource = GetDataTable("SELECT mr.RecordID, mr.RecordDate, pu.FullName AS PatientName, du.FullName AS DoctorName, mr.Diagnosis, mr.Prescription, mr.TestResults FROM MedicalRecords mr INNER JOIN Patients p ON mr.PatientID=p.PatientID INNER JOIN Users pu ON p.UserID=pu.UserID INNER JOIN Doctors d ON mr.DoctorID=d.DoctorID INNER JOIN Users du ON d.UserID=du.UserID ORDER BY mr.RecordDate DESC")
            End If
        End If
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class

' ============================================================
' FormDoctorSchedule.vb
' ============================================================

Public Class FormDoctorSchedule
    Inherits Form

    Private dgvSchedule As DataGridView
    Private WithEvents cmbDoctor As ComboBox
    Private WithEvents btnRefresh As Button
    Private WithEvents btnClose As Button
    Private _doctors As New List(Of KeyValuePair(Of Integer, String))()

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Size = New Size(900, 600)
        Me.Text = "Doctor Schedules"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.BackColor = Color.FromArgb(240, 248, 255)

        Dim pnlTop As New Panel With {.Dock = DockStyle.Top, .Height = 60, .BackColor = Color.FromArgb(0, 102, 153), .Padding = New Padding(10)}
        Dim lblTitle As New Label With {.Text = "Doctor Weekly Schedules", .Font = New Font("Arial", 14, FontStyle.Bold), .ForeColor = Color.White, .Dock = DockStyle.Left, .AutoSize = True}
        pnlTop.Controls.Add(lblTitle)

        Dim pnlFilter As New Panel With {.Dock = DockStyle.Top, .Height = 50, .Padding = New Padding(10)}
        Dim lbl As New Label With {.Text = "Select Doctor:", .Location = New Point(10, 15), .AutoSize = True, .Font = New Font("Arial", 10)}
        cmbDoctor = New ComboBox With {.Location = New Point(120, 12), .Size = New Size(280, 25), .DropDownStyle = ComboBoxStyle.DropDownList, .Font = New Font("Arial", 9)}
        btnRefresh = New Button With {.Text = "View Schedule", .Location = New Point(410, 11), .Size = New Size(130, 28), .Font = New Font("Arial", 9), .BackColor = Color.FromArgb(0, 102, 153), .ForeColor = Color.White, .FlatStyle = FlatStyle.Flat, .Cursor = Cursors.Hand}
        btnClose = New Button With {.Text = "Close", .Location = New Point(550, 11), .Size = New Size(80, 28), .Font = New Font("Arial", 9), .FlatStyle = FlatStyle.Flat, .Cursor = Cursors.Hand}
        pnlFilter.Controls.AddRange(New Control() {lbl, cmbDoctor, btnRefresh, btnClose})

        dgvSchedule = New DataGridView With {
            .Dock = DockStyle.Fill,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            .ReadOnly = True,
            .AllowUserToAddRows = False,
            .RowHeadersVisible = False,
            .Font = New Font("Arial", 9),
            .BackgroundColor = Color.White,
            .BorderStyle = BorderStyle.None
        }
        dgvSchedule.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 102, 153)
        dgvSchedule.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        dgvSchedule.ColumnHeadersDefaultCellStyle.Font = New Font("Arial", 9, FontStyle.Bold)

        Me.Controls.Add(dgvSchedule)
        Me.Controls.Add(pnlFilter)
        Me.Controls.Add(pnlTop)
    End Sub

    Private Sub FormDoctorSchedule_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            _doctors.Clear()
            cmbDoctor.Items.Clear()
            cmbDoctor.Items.Add("-- All Doctors --")
            Dim dt As DataTable = GetAllDoctors()
            For Each row As DataRow In dt.Rows
                _doctors.Add(New KeyValuePair(Of Integer, String)(CInt(row("DoctorID")), row("FullName").ToString()))
                cmbDoctor.Items.Add($"Dr. {row("FullName")} ({row("DepartmentName")})")
            Next
            cmbDoctor.SelectedIndex = 0
            dgvSchedule.DataSource = GetAllDoctors()
        Catch ex As Exception
            LogError("FormDoctorSchedule_Load: " & ex.Message)
        End Try
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        If cmbDoctor.SelectedIndex <= 0 Then
            dgvSchedule.DataSource = GetAllDoctors()
        Else
            Dim docID As Integer = _doctors(cmbDoctor.SelectedIndex - 1).Key
            ' Show this doctor's appointments for the next 7 days
            Dim startDate As String = DateTime.Now.ToString("yyyy-MM-dd")
            Dim endDate As String = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd")
            Dim sql As String = $"SELECT a.AppointmentDate, a.AppointmentTime, pu.FullName AS PatientName, a.Status, a.IsEmergency FROM Appointments a INNER JOIN Patients p ON a.PatientID=p.PatientID INNER JOIN Users pu ON p.UserID=pu.UserID WHERE a.DoctorID={docID} AND a.AppointmentDate BETWEEN '{startDate}' AND '{endDate}' AND a.Status != 'Cancelled' ORDER BY a.AppointmentDate, a.AppointmentTime"
            dgvSchedule.DataSource = GetDataTable(sql)
        End If
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
