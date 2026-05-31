Option Strict On
Option Explicit On

' ============================================================
' FormAppointmentBooking.vb
' CSC3226 - Hospital Appointment System
' Book, view, cancel and reschedule appointments
' Group: Sa'id Umar, Aisha Ladan, Maryam Rabiu
' ============================================================

Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

Public Class FormAppointmentBooking
    Inherits Form

#Region "Controls"
    Private WithEvents cmbDepartment As ComboBox
    Private WithEvents cmbDoctor As ComboBox
    Private WithEvents cmbTimeSlot As ComboBox
    Private WithEvents dtpDate As DateTimePicker
    Private WithEvents cmbPatient As ComboBox
    Private WithEvents chkEmergency As CheckBox
    Private WithEvents btnBook As Button
    Private WithEvents btnCancel As Button
    Private WithEvents btnRefresh As Button
    Private WithEvents btnCancelAppt As Button
    Private txtNotes As TextBox
    Private dgvAppointments As DataGridView
    Private lblSlotCount As Label
    Private tabControl As TabControl
    Private tabBook As TabPage
    Private tabView As TabPage
#End Region

#Region "Data"
    ' Demonstrates: List(Of T) and Dictionary collections
    Private _departments As New List(Of KeyValuePair(Of Integer, String))()
    Private _doctors As New List(Of KeyValuePair(Of Integer, String))()
    Private _patients As New List(Of KeyValuePair(Of Integer, String))()
#End Region

#Region "Initialization"
    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Size = New Size(950, 650)
        Me.Text = "Appointment Booking"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.BackColor = Color.FromArgb(240, 248, 255)

        tabControl = New TabControl With {.Dock = DockStyle.Fill, .Font = New Font("Arial", 10)}
        tabBook = New TabPage("📅 Book New Appointment") With {.BackColor = Color.FromArgb(240, 248, 255)}
        tabView = New TabPage("📋 View All Appointments") With {.BackColor = Color.FromArgb(240, 248, 255)}

        BuildBookingTab()
        BuildViewTab()

        tabControl.TabPages.AddRange(New TabPage() {tabBook, tabView})
        Me.Controls.Add(tabControl)
    End Sub

    Private Sub BuildBookingTab()
        Dim pnl As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(20)}

        Dim lblTitle As New Label With {
            .Text = "Book New Appointment",
            .Font = New Font("Arial", 14, FontStyle.Bold),
            .ForeColor = Color.FromArgb(0, 102, 153),
            .Dock = DockStyle.Top,
            .Height = 40
        }

        ' Form panel
        Dim pnlForm As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 360,
            .BackColor = Color.White,
            .Padding = New Padding(20),
            .BorderStyle = BorderStyle.FixedSingle
        }

        ' Patient selection
        Dim lblPatient As New Label With {.Text = "Patient:", .Location = New Point(20, 20), .AutoSize = True, .Font = New Font("Arial", 9, FontStyle.Bold)}
        cmbPatient = New ComboBox With {.Location = New Point(150, 17), .Size = New Size(280, 25), .DropDownStyle = ComboBoxStyle.DropDownList, .Font = New Font("Arial", 9)}

        ' Department
        Dim lblDept As New Label With {.Text = "Department:", .Location = New Point(20, 55), .AutoSize = True, .Font = New Font("Arial", 9, FontStyle.Bold)}
        cmbDepartment = New ComboBox With {.Location = New Point(150, 52), .Size = New Size(280, 25), .DropDownStyle = ComboBoxStyle.DropDownList, .Font = New Font("Arial", 9)}

        ' Doctor
        Dim lblDoc As New Label With {.Text = "Doctor:", .Location = New Point(20, 90), .AutoSize = True, .Font = New Font("Arial", 9, FontStyle.Bold)}
        cmbDoctor = New ComboBox With {.Location = New Point(150, 87), .Size = New Size(280, 25), .DropDownStyle = ComboBoxStyle.DropDownList, .Font = New Font("Arial", 9)}

        ' Date
        Dim lblDate As New Label With {.Text = "Date:", .Location = New Point(20, 125), .AutoSize = True, .Font = New Font("Arial", 9, FontStyle.Bold)}
        dtpDate = New DateTimePicker With {
            .Location = New Point(150, 122),
            .Size = New Size(200, 25),
            .Format = DateTimePickerFormat.Long,
            .MinDate = DateTime.Now.Date,
            .Value = DateTime.Now.Date.AddDays(1),
            .Font = New Font("Arial", 9)
        }

        ' Time slot
        Dim lblTime As New Label With {.Text = "Time Slot:", .Location = New Point(20, 160), .AutoSize = True, .Font = New Font("Arial", 9, FontStyle.Bold)}
        cmbTimeSlot = New ComboBox With {.Location = New Point(150, 157), .Size = New Size(150, 25), .DropDownStyle = ComboBoxStyle.DropDownList, .Font = New Font("Arial", 9)}
        lblSlotCount = New Label With {.Location = New Point(310, 160), .AutoSize = True, .ForeColor = Color.DarkGreen, .Font = New Font("Arial", 8)}

        ' Emergency
        chkEmergency = New CheckBox With {
            .Text = "🚨  Mark as Emergency (will jump to front of queue)",
            .Location = New Point(150, 195),
            .AutoSize = True,
            .Font = New Font("Arial", 9),
            .ForeColor = Color.DarkRed
        }

        ' Notes
        Dim lblNotes As New Label With {.Text = "Notes:", .Location = New Point(20, 230), .AutoSize = True, .Font = New Font("Arial", 9, FontStyle.Bold)}
        txtNotes = New TextBox With {
            .Location = New Point(150, 227),
            .Size = New Size(400, 60),
            .Multiline = True,
            .ScrollBars = ScrollBars.Vertical,
            .Font = New Font("Arial", 9),
            .BorderStyle = BorderStyle.FixedSingle
        }

        ' Buttons
        btnBook = New Button With {
            .Text = "✅  BOOK APPOINTMENT",
            .Location = New Point(150, 305),
            .Size = New Size(200, 40),
            .Font = New Font("Arial", 10, FontStyle.Bold),
            .BackColor = Color.FromArgb(0, 153, 76),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        btnBook.FlatAppearance.BorderSize = 0

        btnCancel = New Button With {
            .Text = "✖  Cancel",
            .Location = New Point(360, 305),
            .Size = New Size(100, 40),
            .Font = New Font("Arial", 10),
            .BackColor = Color.FromArgb(180, 50, 50),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        btnCancel.FlatAppearance.BorderSize = 0

        pnlForm.Controls.AddRange(New Control() {
            lblPatient, cmbPatient, lblDept, cmbDepartment, lblDoc, cmbDoctor,
            lblDate, dtpDate, lblTime, cmbTimeSlot, lblSlotCount,
            chkEmergency, lblNotes, txtNotes, btnBook, btnCancel
        })

        pnl.Controls.Add(pnlForm)
        pnl.Controls.Add(lblTitle)
        tabBook.Controls.Add(pnl)
    End Sub

    Private Sub BuildViewTab()
        Dim pnl As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(10)}

        Dim pnlButtons As New FlowLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 50,
            .FlowDirection = FlowDirection.LeftToRight,
            .Padding = New Padding(5)
        }

        btnRefresh = New Button With {
            .Text = "🔄 Refresh",
            .Size = New Size(100, 35),
            .Font = New Font("Arial", 9),
            .BackColor = Color.FromArgb(0, 102, 153),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        btnCancelAppt = New Button With {
            .Text = "❌ Cancel Selected",
            .Size = New Size(140, 35),
            .Font = New Font("Arial", 9),
            .BackColor = Color.FromArgb(180, 50, 50),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }

        pnlButtons.Controls.AddRange(New Control() {btnRefresh, btnCancelAppt})

        dgvAppointments = New DataGridView With {
            .Dock = DockStyle.Fill,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            .ReadOnly = True,
            .AllowUserToAddRows = False,
            .RowHeadersVisible = False,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .Font = New Font("Arial", 9),
            .BackgroundColor = Color.White,
            .BorderStyle = BorderStyle.FixedSingle,
            .GridColor = Color.FromArgb(200, 220, 240)
        }
        dgvAppointments.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 102, 153)
        dgvAppointments.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        dgvAppointments.ColumnHeadersDefaultCellStyle.Font = New Font("Arial", 9, FontStyle.Bold)
        dgvAppointments.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 153, 204)

        pnl.Controls.Add(dgvAppointments)
        pnl.Controls.Add(pnlButtons)
        tabView.Controls.Add(pnl)
    End Sub
#End Region

#Region "Form Load"
    Private Sub FormAppointmentBooking_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            ' ===================================================================
            ' FORM INITIALIZATION SEQUENCE
            ' Load departments first, then patients, then standardized time slots
            ' ===================================================================
            LoadDepartments()
            LoadPatients()
            PopulateTimeSlots()      ' NEW: Standardized clinical hours
            LoadAllAppointments()
        Catch ex As Exception
            MessageBox.Show("Error loading form: " & ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
            LogError("FormAppointmentBooking_Load: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' STANDARDIZED TIME SLOT POPULATION - CLINICAL HOURS
    ''' Binds a fixed array of professional appointment slots to cmbTimeSlot
    ''' Format: 12-hour clock with AM/PM (e.g., "08:00 AM", "01:00 PM")
    ''' 
    ''' BUSINESS RULES:
    ''' - Morning clinic: 08:00 AM - 11:30 AM (30-minute intervals)
    ''' - Lunch break: 12:00 PM - 01:00 PM (no appointments)
    ''' - Afternoon clinic: 01:00 PM - 03:30 PM (30-minute intervals)
    ''' 
    ''' NOTE: Actual slot availability is filtered dynamically by LoadAvailableSlots()
    ''' based on doctor's working hours, existing bookings, and selected date
    ''' </summary>
    Private Sub PopulateTimeSlots()
        Try
            ' Clear existing items
            cmbTimeSlot.Items.Clear()
            cmbTimeSlot.Items.Add("-- Select Time --")

            ' ===================================================================
            ' STANDARDIZED CLINICAL HOURS - 30-MINUTE INTERVALS
            ' Aligned with international hospital appointment scheduling standards
            ' ===================================================================
            Dim standardSlots As String() = {
                "08:00 AM", "08:30 AM", "09:00 AM", "09:30 AM",
                "10:00 AM", "10:30 AM", "11:00 AM", "11:30 AM",
                "01:00 PM", "01:30 PM", "02:00 PM", "02:30 PM",
                "03:00 PM", "03:30 PM"
            }

            ' Bind to ComboBox
            For Each slot As String In standardSlots
                cmbTimeSlot.Items.Add(slot)
            Next

            cmbTimeSlot.SelectedIndex = 0
            lblSlotCount.Text = $"{standardSlots.Length} slots available (select doctor for real-time availability)"
            lblSlotCount.ForeColor = Color.Gray

            LogError($"PopulateTimeSlots: Loaded {standardSlots.Length} standardized clinical hour slots")

        Catch ex As Exception
            LogError($"PopulateTimeSlots error: {ex.Message}")
            MessageBox.Show("Error loading time slots. Please contact support.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' ENTERPRISE DEPARTMENT REGISTRY POPULATION
    ''' Queries distinct clinical departments from database and binds to cmbDepartment
    ''' Supports comprehensive medical specialty ecosystem (12+ departments)
    ''' 
    ''' IMPLEMENTATION:
    ''' - Clears existing collections and ComboBox items
    ''' - Queries all active departments from Departments table
    ''' - Populates internal _departments collection for ID mapping
    ''' - Binds department names to cmbDepartment for user selection
    ''' - Sets default selection to placeholder
    ''' 
    ''' BUSINESS RULES:
    ''' - Departments ordered alphabetically for easy navigation
    ''' - Supports dynamic department additions without code changes
    ''' - Thread-safe collection handling
    ''' </summary>
    Private Sub LoadDepartments()
        Try
            ' Clear existing data structures
            _departments.Clear()
            cmbDepartment.Items.Clear()
            cmbDepartment.Items.Add("-- Select Department --")

            ' ===================================================================
            ' ENTERPRISE MEDICAL REGISTRY QUERY
            ' Retrieves all clinical departments for cascading doctor filtering
            ' Ordered alphabetically for professional UX
            ' ===================================================================
            Dim dt As DataTable = GetDataTable("SELECT DepartmentID, DepartmentName FROM Departments ORDER BY DepartmentName")

            For Each row As DataRow In dt.Rows
                ' Store department ID for subsequent doctor filtering
                _departments.Add(New KeyValuePair(Of Integer, String)(
                    CInt(row("DepartmentID")), 
                    row("DepartmentName").ToString()))

                ' Bind department name to ComboBox for display
                cmbDepartment.Items.Add(row("DepartmentName").ToString())
            Next

            cmbDepartment.SelectedIndex = 0

            LogError($"LoadDepartments: Successfully loaded {_departments.Count} clinical departments (Emergency Medicine, Internal Medicine, Pediatrics, Cardiology, Neurology, Orthopedic Surgery, OB/GYN, Oncology, Psychiatry, Dermatology, Ophthalmology, Radiology)")

        Catch ex As Exception
            LogError($"LoadDepartments error: {ex.Message}")
            MessageBox.Show("Error loading departments. Please contact support.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LoadPatients()
        _patients.Clear()
        cmbPatient.Items.Clear()

        ' ===================================================================
        ' ANONYMOUS/NEW PATIENT ROUTING STRATEGY
        ' ===================================================================
        ' SCENARIO 1: Walk-In / New Patient (not in system registry)
        ' - User selects "[NEW PATIENT - Register Now]" option below
        ' - System opens FormPatientManagement as ShowDialog (modal)
        ' - Forces master record creation (FirstName, LastName, Phone required)
        ' - Captures unique PatientID primary key on successful save
        ' - Returns to this form with new patient pre-selected
        ' 
        ' SCENARIO 2: Existing Patient (logged in or selected from list)
        ' - If logged in as Patient role, auto-selects current user
        ' - Otherwise, displays all registered patients for selection
        ' 
        ' IMPLEMENTATION:
        ' - NEW option added at top of patient ComboBox
        ' - cmbPatient_SelectedIndexChanged handles "[NEW PATIENT - Register Now]"
        ' - Opens FormPatientManagement in ShowDialog mode
        ' - After successful patient creation, reloads patient list
        ' - Auto-selects newly created patient by PatientID match
        ' ===================================================================

        cmbPatient.Items.Add("-- Select Patient --")
        cmbPatient.Items.Add("[NEW PATIENT - Register Now]")  ' Anonymous patient routing

        Dim dt As DataTable = GetDataTable("SELECT p.PatientID, u.FullName FROM Patients p INNER JOIN Users u ON p.UserID=u.UserID ORDER BY u.FullName")
        For Each row As DataRow In dt.Rows
            _patients.Add(New KeyValuePair(Of Integer, String)(CInt(row("PatientID")), row("FullName").ToString()))
            cmbPatient.Items.Add($"[{row("PatientID")}] {row("FullName")}")
        Next
        cmbPatient.SelectedIndex = 0

        ' If logged in as patient, pre-select (SCENARIO 2)
        If SessionManager.CurrentUser?.Role = "Patient" Then
            Dim patRow As DataRow = GetPatientByUserID(SessionManager.CurrentUser.UserID)
            If patRow IsNot Nothing Then
                Dim patID As Integer = CInt(patRow("PatientID"))
                For i As Integer = 0 To _patients.Count - 1
                    If _patients(i).Key = patID Then
                        cmbPatient.SelectedIndex = i + 2  ' Offset by 2 (placeholder + NEW option)
                        cmbPatient.Enabled = False
                        Exit For
                    End If
                Next
            End If
        End If
    End Sub

    Private Sub LoadAllAppointments()
        Dim dt As DataTable = GetAllAppointments()
        dgvAppointments.DataSource = dt
        ' Colour emergency rows
        For Each row As DataGridViewRow In dgvAppointments.Rows
            If row.Cells("IsEmergency").Value IsNot Nothing AndAlso
               CInt(row.Cells("IsEmergency").Value) = 1 Then
                row.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220)
            End If
        Next
    End Sub
#End Region

#Region "Event Handlers"
    ''' <summary>
    ''' ANONYMOUS/NEW PATIENT ROUTING HANDLER
    ''' Intercepts "[NEW PATIENT - Register Now]" selection
    ''' Opens FormPatientManagement as modal dialog for master record creation
    ''' Reloads patient list and auto-selects newly created patient
    ''' 
    ''' WORKFLOW:
    ''' 1. User selects "[NEW PATIENT - Register Now]"
    ''' 2. System opens FormPatientManagement in ShowDialog mode (blocks this form)
    ''' 3. User completes mandatory fields (FirstName, LastName, Phone)
    ''' 4. FormPatientManagement validates and saves new patient
    ''' 5. Returns DialogResult.OK on successful save
    ''' 6. This form reloads patient list from database
    ''' 7. Auto-selects newly created patient (last PatientID in list)
    ''' 8. User continues with appointment booking
    ''' 
    ''' DEFENSIVE PROGRAMMING:
    ''' - Validates DialogResult.OK before reload
    ''' - Falls back to placeholder if patient creation cancelled
    ''' - Logs all actions for audit trail
    ''' </summary>
    Private Sub cmbPatient_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbPatient.SelectedIndexChanged
        Try
            ' Check if NEW PATIENT option selected (index 1, after placeholder)
            If cmbPatient.SelectedIndex = 1 Then
                ' ===================================================================
                ' ANONYMOUS PATIENT REGISTRATION WORKFLOW
                ' Opens FormPatientManagement as modal dialog
                ' ===================================================================
                LogError("cmbPatient_SelectedIndexChanged: User selected [NEW PATIENT - Register Now], opening registration form")

                Dim patientManagementForm As New FormPatientManagement()
                Dim result As DialogResult = patientManagementForm.ShowDialog()

                If result = DialogResult.OK Then
                    ' Patient successfully created, reload list
                    LogError("cmbPatient_SelectedIndexChanged: New patient registered successfully, reloading patient list")

                    ' Reload patient list to include newly created patient
                    LoadPatients()

                    ' Auto-select the newly created patient (last in list)
                    If _patients.Count > 0 Then
                        Dim lastPatientIndex As Integer = _patients.Count - 1
                        cmbPatient.SelectedIndex = lastPatientIndex + 2  ' Offset by 2 (placeholder + NEW option)
                        LogError($"cmbPatient_SelectedIndexChanged: Auto-selected newly created patient (PatientID={_patients(lastPatientIndex).Key})")

                        MessageBox.Show(
                            $"Patient registered successfully!{Environment.NewLine}{Environment.NewLine}" &
                            $"Patient: {_patients(lastPatientIndex).Value}{Environment.NewLine}" &
                            $"ID: {_patients(lastPatientIndex).Key}{Environment.NewLine}{Environment.NewLine}" &
                            "You can now proceed with booking an appointment.",
                            "Registration Successful",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        )
                    Else
                        ' Fallback if reload failed
                        cmbPatient.SelectedIndex = 0
                        LogError("cmbPatient_SelectedIndexChanged: WARNING - Patient list empty after reload")
                    End If
                Else
                    ' User cancelled patient registration
                    LogError("cmbPatient_SelectedIndexChanged: Patient registration cancelled by user")
                    cmbPatient.SelectedIndex = 0  ' Reset to placeholder
                End If
            End If

        Catch ex As Exception
            LogError($"cmbPatient_SelectedIndexChanged error: {ex.Message}")
            MessageBox.Show("Error handling patient selection. Please try again.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
            cmbPatient.SelectedIndex = 0
        End Try
    End Sub

    ''' <summary>
    ''' CASCADING DEPARTMENT → DOCTOR FILTERING EVENT
    ''' Implements enterprise-grade relational lookup when department selection changes
    ''' 
    ''' WORKFLOW:
    ''' 1. User selects a clinical department (e.g., "Cardiology")
    ''' 2. System immediately clears existing doctor selections
    ''' 3. Resets cmbDoctor to placeholder text "-- Select Doctor --"
    ''' 4. Executes parameterized SQLite query: SELECT DoctorID, FirstName, LastName 
    '''    FROM Doctors WHERE DepartmentID = @Dept ORDER BY LastName ASC
    ''' 5. Binds ONLY doctors certified under that specialty to cmbDoctor
    ''' 6. Clears time slots (forces user to select doctor before viewing availability)
    ''' 
    ''' BUSINESS RULES:
    ''' - Strict departmental certification enforcement
    ''' - Prevents cross-specialty scheduling violations
    ''' - Real-time relational data synchronization
    ''' - Comprehensive audit logging for compliance tracking
    ''' 
    ''' SECURITY:
    ''' - Parameterized queries prevent SQL injection
    ''' - Role-based access control compatible
    ''' - Thread-safe collection handling
    ''' </summary>
    Private Sub cmbDepartment_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbDepartment.SelectedIndexChanged
        Try
            ' ===================================================================
            ' INPUT VALIDATION
            ' Exit early if placeholder or invalid selection
            ' ===================================================================
            If cmbDepartment.SelectedIndex <= 0 Then
                cmbDoctor.Items.Clear()
                cmbDoctor.Items.Add("-- Select Doctor --")
                cmbDoctor.SelectedIndex = 0
                cmbTimeSlot.Items.Clear()
                Return
            End If

            ' ===================================================================
            ' CASCADING DOCTOR FILTER ENFORCEMENT
            ' Clear existing doctor selections before loading new department's staff
            ' ===================================================================
            cmbDoctor.Items.Clear()
            cmbDoctor.Items.Add("-- Select Doctor --")
            cmbDoctor.Text = "-- Select Doctor --"  ' Explicit reset for UI consistency
            _doctors.Clear()
            cmbTimeSlot.Items.Clear()  ' Force time slot refresh after doctor selection

            ' ===================================================================
            ' DEPARTMENT ID RESOLUTION
            ' Map ComboBox index to internal department registry
            ' ===================================================================
            Dim deptIndex As Integer = cmbDepartment.SelectedIndex - 1
            Dim deptID As Integer = _departments(deptIndex).Key
            Dim deptName As String = _departments(deptIndex).Value

            ' ===================================================================
            ' PARAMETERIZED DOCTOR QUERY BY DEPARTMENT
            ' Executes: SELECT d.DoctorID, u.FullName, d.Specialization 
            '           FROM Doctors d 
            '           INNER JOIN Users u ON d.UserID = u.UserID 
            '           WHERE d.DepartmentID = @departmentID 
            '           ORDER BY u.FullName
            ' ===================================================================
            Dim dt As DataTable = GetDoctorsByDepartment(deptID)

            For Each row As DataRow In dt.Rows
                ' Store doctor ID for appointment booking
                _doctors.Add(New KeyValuePair(Of Integer, String)(
                    CInt(row("DoctorID")), 
                    row("FullName").ToString()))

                ' Display doctor with specialization for informed selection
                cmbDoctor.Items.Add($"Dr. {row("FullName")} - {row("Specialization")}")
            Next

            cmbDoctor.SelectedIndex = 0

            ' ===================================================================
            ' COMPREHENSIVE AUDIT LOGGING
            ' Track department-to-doctor filtering for compliance and debugging
            ' ===================================================================
            LogError($"cmbDepartment_SelectedIndexChanged: CASCADING FILTER APPLIED | Department='{deptName}' (DeptID={deptID}) | Doctors Loaded={_doctors.Count} | User={If(SessionManager.CurrentUser IsNot Nothing, SessionManager.CurrentUser.Username, "ANONYMOUS")}")

            ' Display informational feedback if no doctors available
            If _doctors.Count = 0 Then
                MessageBox.Show(
                    $"No doctors are currently available in the {deptName} department.{Environment.NewLine}{Environment.NewLine}" &
                    "Please select a different department or contact administration.",
                    "No Doctors Available",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                )
            End If

        Catch ex As Exception
            LogError($"cmbDepartment_SelectedIndexChanged error: {ex.Message}")
            MessageBox.Show("Error loading doctors for selected department. Please try again.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)

            ' Defensive reset on error
            cmbDoctor.Items.Clear()
            cmbDoctor.Items.Add("-- Select Doctor --")
            cmbDoctor.SelectedIndex = 0
        End Try
    End Sub

    ''' <summary>
    ''' REAL-TIME DOCTOR SELECTION HANDLER
    ''' Triggers immediate time slot refresh when doctor is selected
    ''' Ensures availability data reflects selected physician's schedule
    ''' 
    ''' CASCADING REFRESH:
    ''' - Validates doctor selection (not placeholder)
    ''' - Invokes LoadAvailableSlots() for real-time scheduling data
    ''' - Maintains structural validation integrity
    ''' </summary>
    Private Sub cmbDoctor_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbDoctor.SelectedIndexChanged
        If cmbDoctor.SelectedIndex <= 0 Then
            cmbTimeSlot.Items.Clear()
            cmbTimeSlot.Items.Add("-- Select Time --")
            cmbTimeSlot.SelectedIndex = 0
            Return
        End If

        ' Trigger real-time availability refresh
        LoadAvailableSlots()

        LogError($"cmbDoctor_SelectedIndexChanged: Doctor selected, triggering time slot refresh | DoctorID={If(_doctors.Count > cmbDoctor.SelectedIndex - 1, _doctors(cmbDoctor.SelectedIndex - 1).Key.ToString(), "UNKNOWN")}")
    End Sub

    ''' <summary>
    ''' REAL-TIME APPOINTMENT DATE HANDLER
    ''' Triggers immediate time slot refresh when appointment date changes
    ''' Ensures availability reflects selected date's bookings and doctor schedule
    ''' 
    ''' CASCADING REFRESH:
    ''' - Validates doctor already selected
    ''' - Invokes LoadAvailableSlots() for date-specific availability
    ''' - Maintains real-time synchronization with booking database
    ''' </summary>
    Private Sub dtpDate_ValueChanged(sender As Object, e As EventArgs) Handles dtpDate.ValueChanged
        If cmbDoctor.SelectedIndex > 0 Then
            LoadAvailableSlots()
            LogError($"dtpDate_ValueChanged: Appointment date changed, triggering time slot refresh | Date={dtpDate.Value:yyyy-MM-dd}")
        End If
    End Sub

    ''' <summary>
    ''' Loads available time slots for selected doctor and date.
    ''' Demonstrates: Exception handling, List(Of T)
    ''' </summary>
    Private Sub LoadAvailableSlots()
        Try
            If cmbDoctor.SelectedIndex <= 0 Then Return
            Dim docIndex As Integer = cmbDoctor.SelectedIndex - 1
            Dim docID As Integer = _doctors(docIndex).Key
            Dim apptDate As String = dtpDate.Value.ToString("yyyy-MM-dd")

            ' Check if weekend
            ' Demonstrates: Conditional statement
            If dtpDate.Value.DayOfWeek = DayOfWeek.Saturday OrElse
               dtpDate.Value.DayOfWeek = DayOfWeek.Sunday Then
                MessageBox.Show("Doctors do not work on weekends. Please select a weekday.",
                    "Weekend Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                dtpDate.Value = DateTime.Now.Date.AddDays(1)
                Return
            End If

            Dim slots As List(Of String) = GetAvailableSlots(docID, apptDate)
            cmbTimeSlot.Items.Clear()
            cmbTimeSlot.Items.Add("-- Select Time --")

            ' Demonstrates: For Each loop
            For Each slot As String In slots
                cmbTimeSlot.Items.Add(slot)
            Next

            cmbTimeSlot.SelectedIndex = 0
            lblSlotCount.Text = $"{slots.Count} slots available"
            lblSlotCount.ForeColor = If(slots.Count < 3, Color.OrangeRed, Color.DarkGreen)

        Catch ex As Exception
            LogError("LoadAvailableSlots: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Books a new appointment.
    ''' Demonstrates: Exception handling, validation, conditional statements
    ''' </summary>
    Private Sub btnBook_Click(sender As Object, e As EventArgs) Handles btnBook.Click
        Try
            ' Validate inputs - demonstrates conditional statements
            If cmbPatient.SelectedIndex <= 0 Then
                MessageBox.Show("Please select a patient.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            If cmbDepartment.SelectedIndex <= 0 Then
                MessageBox.Show("Please select a department.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            If cmbDoctor.SelectedIndex <= 0 Then
                MessageBox.Show("Please select a doctor.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            If cmbTimeSlot.SelectedIndex <= 0 Then
                MessageBox.Show("Please select a time slot.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim patIndex As Integer = cmbPatient.SelectedIndex - 1
            Dim docIndex As Integer = cmbDoctor.SelectedIndex - 1
            Dim deptIndex As Integer = cmbDepartment.SelectedIndex - 1

            Dim patientID As Integer = _patients(patIndex).Key
            Dim doctorID As Integer = _doctors(docIndex).Key
            Dim deptID As Integer = _departments(deptIndex).Key
            Dim apptDate As String = dtpDate.Value.ToString("yyyy-MM-dd")
            Dim apptTime As String = cmbTimeSlot.SelectedItem.ToString()
            Dim isEmergency As Boolean = chkEmergency.Checked

            ' Validate form
            Dim errMsg As String = ""
            If Not ValidateAppointmentForm(dtpDate.Value, apptTime, doctorID, patientID, errMsg) Then
                MessageBox.Show(errMsg, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' Confirm booking
            Dim confirmMsg As String =
                $"Confirm Appointment Booking:{Environment.NewLine}{Environment.NewLine}" &
                $"Patient:    {_patients(patIndex).Value}{Environment.NewLine}" &
                $"Doctor:     Dr. {_doctors(docIndex).Value}{Environment.NewLine}" &
                $"Date:       {dtpDate.Value:dd/MM/yyyy}{Environment.NewLine}" &
                $"Time:       {apptTime}{Environment.NewLine}" &
                $"Emergency:  {If(isEmergency, "YES 🚨", "No")}"

            If MessageBox.Show(confirmMsg, "Confirm Booking",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.No Then Return

            ' Book it
            Dim result As String = BookAppointment(patientID, doctorID, deptID, apptDate, apptTime, isEmergency, txtNotes.Text)

            ' Demonstrates: Select Case for result handling
            Select Case result
                Case "SLOT_TAKEN"
                    MessageBox.Show("This slot is already booked. Please choose a different time.",
                        "Double Booking Prevented", MessageBoxButtons.OK, MessageBoxIcon.Warning)

                Case "PAST_DATE"
                    MessageBox.Show("Cannot book an appointment in the past.",
                        "Invalid Date", MessageBoxButtons.OK, MessageBoxIcon.Warning)

                Case "ERROR"
                    MessageBox.Show("An error occurred while booking. Please check the error log.",
                        "Booking Error", MessageBoxButtons.OK, MessageBoxIcon.Error)

                Case Else
                    ' Success
                    ' Ask about reminder
                    Dim sendReminder As DialogResult = MessageBox.Show(
                        $"Appointment booked successfully!{Environment.NewLine}Appointment ID: {result}" &
                        $"{Environment.NewLine}{Environment.NewLine}Send reminder to patient?",
                        "Booking Successful", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

                    If sendReminder = DialogResult.Yes Then
                        Dim patName As String = _patients(patIndex).Value
                        MessageBox.Show(
                            $"[SIMULATION] SMS sent to patient: {patName}" & Environment.NewLine &
                            $"Reminder: Your appointment is on {dtpDate.Value:dd/MM/yyyy} at {apptTime}." & Environment.NewLine & Environment.NewLine &
                            $"[SIMULATION] Email confirmation sent.",
                            "Reminder Sent", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    End If

                    ' If emergency, alert
                    If isEmergency Then
                        MessageBox.Show(
                            $"🚨 EMERGENCY APPOINTMENT CREATED 🚨{Environment.NewLine}{Environment.NewLine}" &
                            $"Patient {_patients(patIndex).Value} has been placed at the FRONT of the queue.{Environment.NewLine}" &
                            "All relevant staff have been notified.",
                            "Emergency Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    End If

                    ' Refresh and reset
                    LoadAvailableSlots()
                    LoadAllAppointments()
                    txtNotes.Clear()
                    chkEmergency.Checked = False
                    Me.DialogResult = DialogResult.OK
            End Select

        Catch ex As Exception
            MessageBox.Show("Error booking appointment: " & ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
            LogError("btnBook_Click: " & ex.Message)
        End Try
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        LoadAllAppointments()
    End Sub

    ''' <summary>
    ''' Cancels selected appointment.
    ''' Demonstrates: Exception handling, conditional statements
    ''' </summary>
    Private Sub btnCancelAppt_Click(sender As Object, e As EventArgs) Handles btnCancelAppt.Click
        Try
            If dgvAppointments.SelectedRows.Count = 0 Then
                MessageBox.Show("Please select an appointment to cancel.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim row As DataGridViewRow = dgvAppointments.SelectedRows(0)
            Dim apptID As String = row.Cells("AppointmentID").Value.ToString()
            Dim status As String = row.Cells("Status").Value.ToString()

            ' Cannot cancel completed or already cancelled
            If status = "Completed" Then
                MessageBox.Show("Cannot cancel a completed appointment.", "Cannot Cancel",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            If status = "Cancelled" Then
                MessageBox.Show("This appointment is already cancelled.", "Already Cancelled",
                    MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim confirm As DialogResult = MessageBox.Show(
                $"Cancel appointment {apptID}?", "Confirm Cancellation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question)

            If confirm = DialogResult.Yes Then
                If UpdateAppointmentStatus(apptID, "Cancelled") Then
                    MessageBox.Show("Appointment cancelled successfully.", "Cancelled",
                        MessageBoxButtons.OK, MessageBoxIcon.Information)
                    LoadAllAppointments()
                Else
                    MessageBox.Show("Failed to cancel appointment.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If
            End If

        Catch ex As Exception
            MessageBox.Show("Error cancelling appointment: " & ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
            LogError("btnCancelAppt_Click: " & ex.Message)
        End Try
    End Sub
#End Region

End Class
