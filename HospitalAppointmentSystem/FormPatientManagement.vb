Option Strict On
Option Explicit On

' ============================================================
' FormPatientManagement.vb
' CSC3226 - Hospital Appointment System
' Patient Management UI with complete CRUD operations
' Group: Sa'id Umar, Aisha Ladan, Maryam Rabiu
' ============================================================

Imports System
Imports System.Data
Imports System.Windows.Forms
Imports Microsoft.VisualBasic

Public Class FormPatientManagement

#Region "Private Fields"
    Private _currentPatientID As String = String.Empty
    Private _isEditMode As Boolean = False
    Private _fullDataTable As DataTable = Nothing
#End Region

#Region "Form Load and Initialization"

    ''' <summary>
    ''' Form load event - Initialize controls and load data
    ''' </summary>
    Private Sub FormPatientManagement_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            ' Initialize gender dropdown
            cmbGender.Items.Clear()
            cmbGender.Items.Add("Male")
            cmbGender.Items.Add("Female")
            cmbGender.Items.Add("Other")
            cmbGender.SelectedIndex = 0

            ' Set default date to 18 years ago (minimum age)
            dtpDOB.Value = DateTime.Now.AddYears(-18)
            dtpDOB.MaxDate = DateTime.Now
            dtpDOB.CustomFormat = "yyyy-MM-dd"
            dtpDOB.Format = DateTimePickerFormat.Custom

            ' Load initial patient data
            LoadPatientsList()

            ' Configure DataGridView appearance
            ConfigureDataGridView()

            ' Set initial button states
            btnDelete.Enabled = False
            txtPatientID.Text = "[Auto-Generated]"

        Catch ex As Exception
            MessageBox.Show("Error initializing form: " & ex.Message, _
                            "Initialization Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Error)
            ModuleDatabase.LogError("FormPatientManagement_Load error: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Configure DataGridView visual appearance and behavior
    ''' </summary>
    Private Sub ConfigureDataGridView()
        Try
            With dgvPatients
                .SelectionMode = DataGridViewSelectionMode.FullRowSelect
                .ReadOnly = True
                .AllowUserToAddRows = False
                .AllowUserToDeleteRows = False
                .MultiSelect = False
                .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                .RowHeadersVisible = False
                .EnableHeadersVisualStyles = False

                ' Header styling
                .ColumnHeadersDefaultCellStyle.BackColor = Drawing.Color.FromArgb(41, 128, 185)
                .ColumnHeadersDefaultCellStyle.ForeColor = Drawing.Color.White
                .ColumnHeadersDefaultCellStyle.Font = New Drawing.Font("Segoe UI", 10.0!, Drawing.FontStyle.Bold)
                .ColumnHeadersHeight = 40

                ' Alternating row colors
                .AlternatingRowsDefaultCellStyle.BackColor = Drawing.Color.FromArgb(236, 240, 241)
                .DefaultCellStyle.SelectionBackColor = Drawing.Color.FromArgb(52, 152, 219)
                .DefaultCellStyle.SelectionForeColor = Drawing.Color.White
                .DefaultCellStyle.Font = New Drawing.Font("Segoe UI", 9.0!)
                .RowTemplate.Height = 35
            End With
        Catch ex As Exception
            ModuleDatabase.LogError("ConfigureDataGridView error: " & ex.Message)
        End Try
    End Sub

#End Region

#Region "Data Loading"

    ''' <summary>
    ''' Loads all patients from database into the DataGridView
    ''' Implements robust error handling and null-safety for Option Strict On
    ''' </summary>
    Public Sub LoadPatientsList()
        Try
            ' Retrieve data from database layer
            Dim dt As DataTable = ModuleDatabase.GetPatientsTable()

            If dt IsNot Nothing AndAlso dt.Rows.Count >= 0 Then
                ' Cache the full dataset for filtering
                _fullDataTable = dt.Copy()

                ' Bind to DataGridView
                dgvPatients.DataSource = dt

                ' Update record count display
                UpdateRecordCount(dt.Rows.Count)
            Else
                ' Handle empty or null result
                dgvPatients.DataSource = Nothing
                _fullDataTable = New DataTable()
                UpdateRecordCount(0)
            End If

        Catch ex As Exception
            MessageBox.Show("Error loading patient data: " & ex.Message, _
                            "Data Load Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Error)
            ModuleDatabase.LogError("LoadPatientsList error: " & ex.Message)

            ' Ensure UI remains stable
            dgvPatients.DataSource = Nothing
            UpdateRecordCount(0)
        End Try
    End Sub

    ''' <summary>
    ''' Updates the record count label
    ''' </summary>
    Private Sub UpdateRecordCount(count As Integer)
        lblRecordCount.Text = $"Total Patients: {count}"
    End Sub

#End Region

#Region "Button Click Events"

    ''' <summary>
    ''' Save button click - Create or update patient record
    ''' Enforces strict validation before database interaction
    ''' Implements intelligent UPSERT routing based on PatientID state
    ''' </summary>
    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            ' Enforce robust field validation
            If Not ValidateInputs() Then
                Return
            End If

            ' ===================================================================
            ' INTELLIGENT PATIENTID ASSIGNMENT
            ' Determines if this is a NEW patient or EDIT operation
            ' 
            ' NEW PATIENT: _currentPatientID is empty or "[Auto-Generated]"
            ' EXISTING PATIENT: _currentPatientID contains valid "PAT-YYYY-NNNN"
            ' ===================================================================
            Dim patientID As String = _currentPatientID

            ' Check if this is a new patient (not an edit)
            If String.IsNullOrWhiteSpace(patientID) OrElse 
               patientID.Equals("[Auto-Generated]", StringComparison.OrdinalIgnoreCase) Then
                ' Let SavePatient generate the ID automatically
                patientID = String.Empty
            End If

            ' Create patient object with validated data
            Dim patient As New PatientModel() With {
                .PatientID = patientID,
                .FirstName = txtFirstName.Text.Trim(),
                .LastName = txtLastName.Text.Trim(),
                .DateOfBirth = dtpDOB.Value.ToString("yyyy-MM-dd"),
                .Gender = cmbGender.Text,
                .PhoneNumber = txtPhone.Text.Trim(),
                .Email = txtEmail.Text.Trim()
            }

            ' ===================================================================
            ' Execute database save operation
            ' SavePatient now throws exceptions on error (no Boolean return)
            ' UPSERT logic inside SavePatient:
            '   - If PatientID exists in DB -> UPDATE
            '   - If PatientID doesn't exist or empty -> INSERT with auto-generated ID
            ' ===================================================================
            ModuleDatabase.SavePatient(patient)

            ' SUCCESS PATH - Only reached if no exception thrown
            Dim message As String = If(_isEditMode, "Patient updated successfully!", "Patient registered successfully!")
            MessageBox.Show(message, _
                            "Success", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Information)

            ' Refresh grid and reset form
            LoadPatientsList()
            ClearFormFields()

        Catch ex As Exception
            ' ===================================================================
            ' COMPLETE ERROR EXPOSURE - Shows exact exception details for debugging
            ' This catch block now receives the actual database exception
            ' ===================================================================
            MessageBox.Show("Failed to save patient. Error: " & ex.Message & vbCrLf & vbCrLf & _
                            "Stack Trace: " & ex.StackTrace, _
                            "Database Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Error)
            ModuleDatabase.LogError("btnSave_Click error: " & ex.Message & " | StackTrace: " & ex.StackTrace)
        End Try
    End Sub

    ''' <summary>
    ''' Clear button click - Reset form to initial state
    ''' Implements complete state cleanup
    ''' </summary>
    Private Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
        ClearFormFields()
    End Sub

    ''' <summary>
    ''' Delete button click - Remove selected patient record
    ''' Includes confirmation dialog to prevent accidental deletion
    ''' </summary>
    Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
        Try
            If String.IsNullOrEmpty(_currentPatientID) Then
                MessageBox.Show("Please select a patient to delete.", _
                                "No Selection", _
                                MessageBoxButtons.OK, _
                                MessageBoxIcon.Warning)
                Return
            End If

            ' Get patient name for confirmation message
            Dim patientName As String = txtFirstName.Text.Trim() & " " & txtLastName.Text.Trim()

            Dim result As DialogResult = MessageBox.Show( _
                $"Are you sure you want to delete patient '{patientName}'?" & vbCrLf & vbCrLf & _
                "This action cannot be undone!", _
                "Confirm Deletion", _
                MessageBoxButtons.YesNo, _
                MessageBoxIcon.Warning)

            If result = DialogResult.Yes Then
                Dim success As Boolean = ModuleDatabase.DeletePatient(_currentPatientID)

                If success Then
                    MessageBox.Show("Patient deleted successfully!", _
                                    "Success", _
                                    MessageBoxButtons.OK, _
                                    MessageBoxIcon.Information)
                    LoadPatientsList()
                    ClearFormFields()
                Else
                    MessageBox.Show("Failed to delete patient. Please check error log.", _
                                    "Delete Error", _
                                    MessageBoxButtons.OK, _
                                    MessageBoxIcon.Error)
                End If
            End If

        Catch ex As Exception
            MessageBox.Show("Error deleting patient: " & ex.Message, _
                            "Delete Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Error)
            ModuleDatabase.LogError("btnDelete_Click error: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Close button click - Close the form
    ''' </summary>
    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub

#End Region

#Region "DataGridView Events"

    ''' <summary>
    ''' DataGridView cell click - Load selected patient into form for editing
    ''' Implements explicit null handling and type conversion for Option Strict On
    ''' </summary>
    Private Sub dgvPatients_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvPatients.CellClick
        Try
            ' Ignore header row clicks (row index < 0)
            If e.RowIndex < 0 OrElse e.RowIndex >= dgvPatients.Rows.Count Then
                Return
            End If

            ' Get the selected row reference
            Dim row As DataGridViewRow = dgvPatients.Rows(e.RowIndex)

            ' Validate row is not null
            If row Is Nothing Then
                Return
            End If

            ' Extract data safely with explicit null handling
            _currentPatientID = GetCellValueSafe(row, "Patient ID")
            txtPatientID.Text = _currentPatientID
            txtFirstName.Text = GetCellValueSafe(row, "First Name")
            txtLastName.Text = GetCellValueSafe(row, "Last Name")
            txtPhone.Text = GetCellValueSafe(row, "Phone Number")
            txtEmail.Text = GetCellValueSafe(row, "Email")

            ' Handle Gender dropdown selection
            Dim genderValue As String = GetCellValueSafe(row, "Gender")
            If Not String.IsNullOrEmpty(genderValue) Then
                Dim genderIndex As Integer = cmbGender.FindStringExact(genderValue)
                If genderIndex >= 0 Then
                    cmbGender.SelectedIndex = genderIndex
                Else
                    cmbGender.SelectedIndex = 0 ' Default to first item
                End If
            Else
                cmbGender.SelectedIndex = 0
            End If

            ' Parse and set date of birth with explicit conversion
            Dim dobString As String = GetCellValueSafe(row, "Date of Birth")
            If Not String.IsNullOrEmpty(dobString) Then
                Dim dobDate As DateTime
                If DateTime.TryParse(dobString, dobDate) Then
                    dtpDOB.Value = dobDate
                Else
                    ' Fallback to default if parsing fails
                    dtpDOB.Value = DateTime.Now.AddYears(-18)
                End If
            Else
                dtpDOB.Value = DateTime.Now.AddYears(-18)
            End If

            ' Update UI state for edit mode
            _isEditMode = True
            btnSave.Text = "💾 Update"
            btnDelete.Enabled = True

        Catch ex As Exception
            MessageBox.Show("Error loading patient data: " & ex.Message, _
                            "Load Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Error)
            ModuleDatabase.LogError("dgvPatients_CellClick error: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Safely extracts cell value from DataGridView row with explicit null handling
    ''' Returns empty string if cell is null or doesn't exist (Option Strict On compliant)
    ''' </summary>
    Private Function GetCellValueSafe(row As DataGridViewRow, columnName As String) As String
        Try
            ' Verify row is not null
            If row Is Nothing Then
                Return String.Empty
            End If

            ' Check if column exists in the row's cells collection
            Dim cell As DataGridViewCell = Nothing
            Try
                cell = row.Cells(columnName)
            Catch ex As ArgumentException
                ' Column doesn't exist
                Return String.Empty
            End Try

            ' Verify cell and value are not null
            If cell IsNot Nothing AndAlso cell.Value IsNot Nothing AndAlso Not IsDBNull(cell.Value) Then
                ' Explicit conversion to string
                Dim cellValue As String = Convert.ToString(cell.Value)
                Return If(cellValue, String.Empty)
            End If

        Catch ex As Exception
            ModuleDatabase.LogError($"GetCellValueSafe error for column '{columnName}': " & ex.Message)
        End Try

        ' Return empty string as safe default
        Return String.Empty
    End Function

#End Region

#Region "Search Functionality"

    ''' <summary>
    ''' Real-time search as user types in txtSearch
    ''' Filters DataGridView rows dynamically without resetting database connection state
    ''' Implements efficient client-side filtering when possible
    ''' </summary>
    Private Sub txtSearch_TextChanged(sender As Object, e As EventArgs) Handles txtSearch.TextChanged
        Try
            Dim searchTerm As String = txtSearch.Text.Trim()

            If String.IsNullOrWhiteSpace(searchTerm) Then
                ' If search box is empty, restore full dataset from cache
                If _fullDataTable IsNot Nothing Then
                    dgvPatients.DataSource = _fullDataTable
                    UpdateRecordCount(_fullDataTable.Rows.Count)
                Else
                    ' Fallback: reload from database
                    LoadPatientsList()
                End If
            Else
                ' Perform database search with parameterized query
                Dim filteredData As DataTable = ModuleDatabase.SearchPatients(searchTerm)

                If filteredData IsNot Nothing Then
                    dgvPatients.DataSource = filteredData
                    UpdateRecordCount(filteredData.Rows.Count)
                Else
                    ' Handle null result
                    dgvPatients.DataSource = Nothing
                    UpdateRecordCount(0)
                End If
            End If

        Catch ex As Exception
            MessageBox.Show("Error searching patients: " & ex.Message, _
                            "Search Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Error)
            ModuleDatabase.LogError("txtSearch_TextChanged error: " & ex.Message)

            ' Attempt to restore stable state
            If _fullDataTable IsNot Nothing Then
                dgvPatients.DataSource = _fullDataTable
                UpdateRecordCount(_fullDataTable.Rows.Count)
            End If
        End Try
    End Sub

#End Region

#Region "Form Validation and Helper Methods"

    ''' <summary>
    ''' Validates all required input fields before save operation
    ''' Returns True only if all validation rules pass
    ''' </summary>
    Private Function ValidateInputs() As Boolean
        ' Validate First Name (Required)
        If String.IsNullOrWhiteSpace(txtFirstName.Text) Then
            MessageBox.Show("First Name is required.", _
                            "Validation Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Warning)
            txtFirstName.Focus()
            Return False
        End If

        ' Validate Last Name (Required)
        If String.IsNullOrWhiteSpace(txtLastName.Text) Then
            MessageBox.Show("Last Name is required.", _
                            "Validation Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Warning)
            txtLastName.Focus()
            Return False
        End If

        ' Validate Phone Number (Required)
        If String.IsNullOrWhiteSpace(txtPhone.Text) Then
            MessageBox.Show("Phone Number is required.", _
                            "Validation Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Warning)
            txtPhone.Focus()
            Return False
        End If

        ' Validate phone number format (minimum length check)
        If txtPhone.Text.Trim().Length < 10 Then
            MessageBox.Show("Phone Number must be at least 10 digits.", _
                            "Validation Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Warning)
            txtPhone.Focus()
            Return False
        End If

        ' Validate email format if provided (Optional field)
        If Not String.IsNullOrWhiteSpace(txtEmail.Text) Then
            If Not IsValidEmail(txtEmail.Text.Trim()) Then
                MessageBox.Show("Please enter a valid email address.", _
                                "Validation Error", _
                                MessageBoxButtons.OK, _
                                MessageBoxIcon.Warning)
                txtEmail.Focus()
                Return False
            End If
        End If

        ' Validate date of birth (must be in the past)
        If dtpDOB.Value >= DateTime.Now Then
            MessageBox.Show("Date of Birth must be in the past.", _
                            "Validation Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Warning)
            dtpDOB.Focus()
            Return False
        End If

        ' All validation rules passed
        Return True
    End Function

    ''' <summary>
    ''' Basic email validation using simple format checks
    ''' Returns True if email format appears valid
    ''' </summary>
    Private Function IsValidEmail(email As String) As Boolean
        Try
            If String.IsNullOrWhiteSpace(email) Then
                Return False
            End If

            ' Simple validation: must contain @ and . with @ appearing before the last .
            Dim atIndex As Integer = email.IndexOf("@")
            Dim lastDotIndex As Integer = email.LastIndexOf(".")

            Return atIndex > 0 AndAlso _
                   lastDotIndex > atIndex AndAlso _
                   lastDotIndex < email.Length - 1
        Catch ex As Exception
            ModuleDatabase.LogError("IsValidEmail error: " & ex.Message)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Clears all form fields and resets to initial state
    ''' Implements complete state cleanup for new record entry
    ''' </summary>
    Private Sub ClearFormFields()
        Try
            ' Reset internal state variables
            _currentPatientID = String.Empty
            _isEditMode = False

            ' Clear all text input fields
            txtPatientID.Text = "[Auto-Generated]"
            txtFirstName.Clear()
            txtLastName.Clear()
            txtPhone.Clear()
            txtEmail.Clear()

            ' Reset date picker to default (18 years ago for minimum age)
            dtpDOB.Value = DateTime.Now.AddYears(-18)

            ' Reset gender dropdown to default selection
            If cmbGender.Items.Count > 0 Then
                cmbGender.SelectedIndex = 0
            End If

            ' Clear search box
            txtSearch.Clear()

            ' Reset button states
            btnSave.Text = "💾 Save"
            btnDelete.Enabled = False

            ' Clear DataGridView selection
            If dgvPatients.Rows.Count > 0 Then
                dgvPatients.ClearSelection()
            End If

            ' Set focus to first input field
            txtFirstName.Focus()

        Catch ex As Exception
            ModuleDatabase.LogError("ClearFormFields error: " & ex.Message)

            ' Attempt minimal cleanup even if error occurs
            _currentPatientID = String.Empty
            _isEditMode = False
            txtPatientID.Text = "[Auto-Generated]"
            btnSave.Text = "💾 Save"
            btnDelete.Enabled = False
        End Try
    End Sub

#End Region

#Region "Phone Number Formatting"

    ''' <summary>
    ''' Restrict phone input to digits only (plus optional + and - characters)
    ''' Implements real-time input filtering for data quality
    ''' </summary>
    Private Sub txtPhone_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtPhone.KeyPress
        ' Allow only:
        ' - Digits (0-9)
        ' - Control keys (Backspace, Delete, etc.)
        ' - Plus sign (+) for international format
        ' - Hyphen (-) for formatting
        If Not Char.IsDigit(e.KeyChar) AndAlso _
           Not Char.IsControl(e.KeyChar) AndAlso _
           e.KeyChar <> "+"c AndAlso _
           e.KeyChar <> "-"c Then
            ' Block the character from being entered
            e.Handled = True
        End If
    End Sub

#End Region

#Region "Patient Vitals Management"

    ''' <summary>
    ''' Button click handler to open the Patient Vitals recording dialog.
    ''' Ensures a patient is selected and passes the PatientID safely to the vitals form.
    ''' </summary>
    Private Sub btnRecordVitals_Click(sender As Object, e As EventArgs) Handles btnRecordVitals.Click
        Try
            ' Validate that a patient is selected in the grid
            If dgvPatients.SelectedRows.Count = 0 Then
                MessageBox.Show("Please select a patient from the list to record vitals.", _
                                "No Patient Selected", _
                                MessageBoxButtons.OK, _
                                MessageBoxIcon.Warning)
                Return
            End If

            ' Safely extract the PatientID from the selected grid row
            Dim selectedRow As DataGridViewRow = dgvPatients.SelectedRows(0)
            Dim patientID As String = GetCellValueSafe(selectedRow, "Patient ID")

            ' Additional validation: ensure PatientID is not empty
            If String.IsNullOrWhiteSpace(patientID) Then
                MessageBox.Show("The selected patient does not have a valid Patient ID.", _
                                "Invalid Patient ID", _
                                MessageBoxButtons.OK, _
                                MessageBoxIcon.Error)
                ModuleDatabase.LogError("btnRecordVitals_Click: PatientID is null or empty")
                Return
            End If

            ' Get patient name for display confirmation
            Dim patientName As String = GetCellValueSafe(selectedRow, "First Name") & " " & _
                                         GetCellValueSafe(selectedRow, "Last Name")

            ' Open the vitals recording form
            Using frmVitals As New FormPatientVitals(patientID, patientName)
                If frmVitals.ShowDialog() = DialogResult.OK Then
                    ' Refresh can be added here if needed
                    MessageBox.Show("Vitals recorded successfully for " & patientName, _
                                    "Success", _
                                    MessageBoxButtons.OK, _
                                    MessageBoxIcon.Information)
                End If
            End Using

        Catch ex As Exception
            MessageBox.Show("Error opening vitals recording: " & ex.Message, _
                            "Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Error)
            ModuleDatabase.LogError("btnRecordVitals_Click error: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Button click handler to view vitals history for the selected patient.
    ''' Opens a read-only grid showing all historical vitals records.
    ''' </summary>
    Private Sub btnViewVitals_Click(sender As Object, e As EventArgs) Handles btnViewVitals.Click
        Try
            ' Validate selection
            If dgvPatients.SelectedRows.Count = 0 Then
                MessageBox.Show("Please select a patient from the list to view vitals history.", _
                                "No Patient Selected", _
                                MessageBoxButtons.OK, _
                                MessageBoxIcon.Warning)
                Return
            End If

            ' Extract PatientID safely
            Dim selectedRow As DataGridViewRow = dgvPatients.SelectedRows(0)
            Dim patientID As String = GetCellValueSafe(selectedRow, "Patient ID")

            If String.IsNullOrWhiteSpace(patientID) Then
                MessageBox.Show("The selected patient does not have a valid Patient ID.", _
                                "Invalid Patient ID", _
                                MessageBoxButtons.OK, _
                                MessageBoxIcon.Error)
                Return
            End If

            ' Get patient name for title
            Dim patientName As String = GetCellValueSafe(selectedRow, "First Name") & " " & _
                                         GetCellValueSafe(selectedRow, "Last Name")

            ' Open vitals history viewer
            Using frmVitalsHistory As New FormPatientVitalsHistory(patientID, patientName)
                frmVitalsHistory.ShowDialog()
            End Using

        Catch ex As Exception
            MessageBox.Show("Error opening vitals history: " & ex.Message, _
                            "Error", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Error)
            ModuleDatabase.LogError("btnViewVitals_Click error: " & ex.Message)
        End Try
    End Sub

#End Region

End Class
