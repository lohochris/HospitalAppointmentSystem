Option Strict On
Option Explicit On

' ============================================================
' FormSymptomChecker.vb
' CSC3226 - Hospital Appointment System
' Symptom Checker - Patient Diagnostic Assessment Tool
' Group: Sa'id Umar, Aisha Ladan, Maryam Rabiu
' ============================================================

Imports System.Drawing
Imports System.Windows.Forms
Imports System.Data
Imports Microsoft.VisualBasic

Public Class FormSymptomChecker
    Inherits Form

#Region "Controls"
    Private WithEvents btnCheckSymptoms As Button
    Private WithEvents btnAIAssist As Button
    Private WithEvents btnClearForm As Button
    Private WithEvents btnClose As Button
    Private WithEvents lstSymptoms As CheckedListBox
    Private WithEvents cmbPatientLookup As ComboBox
    Private WithEvents txtAge As TextBox
    Private WithEvents txtTemperature As TextBox
    Private WithEvents txtBloodPressure As TextBox
    Private cmbGender As ComboBox
    Private txtAdditionalInfo As TextBox
    Private txtResults As TextBox
    Private lblResultTitle As Label
    Private lblRiskStatus As Label
    Private pnlRiskIndicator As Panel
#End Region

#Region "Member Variables"
    Private patientDataTable As DataTable
    Private isLoadingPatientData As Boolean = False
#End Region

#Region "Initialization"
    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        ' ===================================================================
        ' PROFESSIONAL DESKTOP UI/UX - FIXED COMPACT FRAME DESIGN
        ' Ensures all elements visible on standard 1366x768 monitors
        ' ===================================================================
        Me.Size = New Size(1020, 720)
        Me.Text = "Symptom Checker - AI-Enhanced Diagnostic Assessment"
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = Color.FromArgb(240, 248, 255)
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = True

        ' ===================================================================
        ' HEADER PANEL (FIXED AT TOP) - Compact Design
        ' ===================================================================
        Dim pnlHeader As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 60,
            .BackColor = Color.FromArgb(0, 102, 153)
        }

        Dim lblTitle As New Label With {
            .Text = "🩺 Symptom Checker & Health Assessment",
            .Font = New Font("Segoe UI", 14, FontStyle.Bold),
            .ForeColor = Color.White,
            .Location = New Point(20, 10),
            .AutoSize = True
        }

        Dim lblSubtitle As New Label With {
            .Text = "Dynamic patient lookup with AI-enhanced preliminary diagnostic assessment",
            .Font = New Font("Segoe UI", 8.5F),
            .ForeColor = Color.FromArgb(200, 230, 255),
            .Location = New Point(20, 35),
            .AutoSize = True
        }

        pnlHeader.Controls.AddRange(New Control() {lblTitle, lblSubtitle})

        ' ===================================================================
        ' BOTTOM ACTION BUTTON PANEL (FIXED AT BOTTOM)
        ' Professional fixed height ensures buttons never hidden by taskbar
        ' ===================================================================
        Dim pnlActionButtons As New Panel With {
            .Name = "pnlActionButtons",
            .Dock = DockStyle.Bottom,
            .Height = 60,
            .BackColor = Color.FromArgb(230, 240, 250),
            .Padding = New Padding(15, 10, 15, 10)
        }

        Dim pnlButtonsFlow As New FlowLayoutPanel With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = False,
            .Padding = New Padding(5, 5, 5, 5)
        }

        btnCheckSymptoms = New Button With {
            .Text = "🔍 Analyze Symptoms",
            .Size = New Size(190, 38),
            .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold),
            .BackColor = Color.FromArgb(0, 153, 76),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand,
            .Margin = New Padding(5, 0, 5, 0)
        }
        btnCheckSymptoms.FlatAppearance.BorderSize = 0

        btnAIAssist = New Button With {
            .Text = "✨ Generate AI Insights",
            .Size = New Size(195, 38),
            .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold),
            .BackColor = Color.FromArgb(0, 102, 153),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand,
            .Enabled = False,
            .Margin = New Padding(5, 0, 5, 0)
        }
        btnAIAssist.FlatAppearance.BorderSize = 0
        btnAIAssist.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 122, 183)

        btnClearForm = New Button With {
            .Text = "🔄 Clear Form",
            .Size = New Size(145, 38),
            .Font = New Font("Segoe UI", 9.5F),
            .BackColor = Color.FromArgb(108, 117, 125),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand,
            .Margin = New Padding(5, 0, 5, 0)
        }
        btnClearForm.FlatAppearance.BorderSize = 0

        btnClose = New Button With {
            .Text = "❌ Close",
            .Size = New Size(125, 38),
            .Font = New Font("Segoe UI", 9.5F),
            .BackColor = Color.Gray,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand,
            .Margin = New Padding(5, 0, 5, 0)
        }
        btnClose.FlatAppearance.BorderSize = 0

        pnlButtonsFlow.Controls.AddRange(New Control() {btnCheckSymptoms, btnAIAssist, btnClearForm, btnClose})
        pnlActionButtons.Controls.Add(pnlButtonsFlow)

        ' ===================================================================
        ' SCROLLABLE MIDDLE CONTENT AREA (FILLS REMAINING SPACE)
        ' Professional container for all input forms and results
        ' Only this section scrolls - header and buttons remain anchored
        ' ===================================================================
        Dim pnlCoreContent As New Panel With {
            .Name = "pnlCoreContent",
            .Dock = DockStyle.Fill,
            .AutoScroll = True,
            .Padding = New Padding(20, 15, 20, 15),
            .BackColor = Color.FromArgb(240, 248, 255)
        }

        ' ===================================================================
        ' PATIENT INFORMATION GROUP - COMPACT LAYOUT
        ' ===================================================================
        Dim grpPatientInfo As New GroupBox With {
            .Text = "Patient Information (Dynamic Lookup)",
            .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold),
            .Location = New Point(0, 0),
            .Size = New Size(935, 110),
            .ForeColor = Color.FromArgb(0, 102, 153)
        }

        ' DYNAMIC AUTO-COMPLETE PATIENT LOOKUP
        Dim lblPatient As New Label With {
            .Text = "Patient Name / Search:",
            .Font = New Font("Segoe UI", 9),
            .Location = New Point(15, 28),
            .Size = New Size(135, 20)
        }

        cmbPatientLookup = New ComboBox With {
            .Font = New Font("Segoe UI", 9),
            .Location = New Point(155, 26),
            .Size = New Size(280, 22),
            .DropDownStyle = ComboBoxStyle.DropDown,
            .AutoCompleteMode = AutoCompleteMode.SuggestAppend,
            .AutoCompleteSource = AutoCompleteSource.ListItems
        }

        Dim lblAge As New Label With {
            .Text = "Age:",
            .Font = New Font("Segoe UI", 9),
            .Location = New Point(455, 28),
            .Size = New Size(35, 20)
        }
        txtAge = New TextBox With {
            .Font = New Font("Segoe UI", 9),
            .Location = New Point(495, 26),
            .Size = New Size(70, 22),
            .MaxLength = 3,
            .ReadOnly = True,
            .BackColor = Color.FromArgb(245, 250, 255)
        }

        Dim lblGender As New Label With {
            .Text = "Gender:",
            .Font = New Font("Segoe UI", 9),
            .Location = New Point(585, 28),
            .Size = New Size(55, 20)
        }
        cmbGender = New ComboBox With {
            .Font = New Font("Segoe UI", 9),
            .Location = New Point(645, 26),
            .Size = New Size(120, 22),
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Enabled = False
        }
        cmbGender.Items.AddRange(New String() {"Male", "Female", "Other"})

        Dim lblTemperature As New Label With {
            .Text = "Temperature (°C):",
            .Font = New Font("Segoe UI", 9),
            .Location = New Point(15, 65),
            .Size = New Size(135, 20)
        }
        txtTemperature = New TextBox With {
            .Font = New Font("Segoe UI", 9),
            .Location = New Point(155, 63),
            .Size = New Size(110, 22)
        }

        Dim lblBloodPressure As New Label With {
            .Text = "Blood Pressure:",
            .Font = New Font("Segoe UI", 9),
            .Location = New Point(285, 65),
            .Size = New Size(100, 20)
        }
        txtBloodPressure = New TextBox With {
            .Font = New Font("Segoe UI", 9),
            .Location = New Point(390, 63),
            .Size = New Size(110, 22)
        }

        ' ===================================================================
        ' INTELLIGENT CLINICAL RISK INDICATOR
        ' Real-time vital signs analysis with dynamic visual feedback
        ' ===================================================================
        pnlRiskIndicator = New Panel With {
            .Location = New Point(520, 63),
            .Size = New Size(160, 24),
            .BackColor = Color.FromArgb(245, 245, 245),
            .BorderStyle = BorderStyle.FixedSingle
        }

        lblRiskStatus = New Label With {
            .Text = "Risk: Not Assessed",
            .Font = New Font("Segoe UI", 8.5F, FontStyle.Bold),
            .ForeColor = Color.FromArgb(100, 100, 100),
            .Location = New Point(5, 4),
            .Size = New Size(150, 16),
            .TextAlign = ContentAlignment.MiddleLeft
        }

        pnlRiskIndicator.Controls.Add(lblRiskStatus)

        grpPatientInfo.Controls.AddRange(New Control() {
            lblPatient, cmbPatientLookup, lblAge, txtAge, lblGender, cmbGender,
            lblTemperature, txtTemperature, lblBloodPressure, txtBloodPressure, pnlRiskIndicator
        })

        ' ===================================================================
        ' SYMPTOMS CHECKLIST GROUP - COMPACT & EFFICIENT
        ' ===================================================================
        Dim grpSymptoms As New GroupBox With {
            .Text = "Select Symptoms (Check all that apply)",
            .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold),
            .Location = New Point(0, 120),
            .Size = New Size(935, 180),
            .ForeColor = Color.FromArgb(0, 102, 153)
        }

        lstSymptoms = New CheckedListBox With {
            .Font = New Font("Segoe UI", 8.5F),
            .Location = New Point(15, 25),
            .Size = New Size(905, 145),
            .CheckOnClick = True,
            .MultiColumn = True,
            .ColumnWidth = 225
        }

        ' Populate common symptoms
        lstSymptoms.Items.AddRange(New String() {
            "Fever",
            "Cough",
            "Shortness of Breath",
            "Chest Pain",
            "Fatigue",
            "Headache",
            "Sore Throat",
            "Nausea/Vomiting",
            "Diarrhea",
            "Abdominal Pain",
            "Dizziness",
            "Loss of Taste/Smell",
            "Muscle Aches",
            "Joint Pain",
            "Skin Rash",
            "Difficulty Swallowing",
            "Persistent Cough",
            "Wheezing",
            "Back Pain",
            "Urinary Issues",
            "Vision Problems",
            "Hearing Problems"
        })

        grpSymptoms.Controls.Add(lstSymptoms)

        ' ===================================================================
        ' ADDITIONAL INFORMATION GROUP - COMPACT
        ' ===================================================================
        Dim grpAdditional As New GroupBox With {
            .Text = "Additional Information / Medical History",
            .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold),
            .Location = New Point(0, 310),
            .Size = New Size(935, 100),
            .ForeColor = Color.FromArgb(0, 102, 153)
        }

        txtAdditionalInfo = New TextBox With {
            .Font = New Font("Segoe UI", 9),
            .Location = New Point(15, 25),
            .Size = New Size(905, 65),
            .Multiline = True,
            .ScrollBars = ScrollBars.Vertical
        }

        grpAdditional.Controls.Add(txtAdditionalInfo)

        ' ===================================================================
        ' ASSESSMENT RESULTS AREA - OPTIMIZED FOR DESKTOP
        ' ===================================================================
        lblResultTitle = New Label With {
            .Text = "📋 Assessment Results & AI Diagnostic Analysis:",
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .ForeColor = Color.FromArgb(0, 102, 153),
            .Location = New Point(0, 420),
            .Size = New Size(935, 25)
        }

        txtResults = New TextBox With {
            .Font = New Font("Segoe UI", 9.5F),
            .Location = New Point(0, 450),
            .Size = New Size(935, 165),
            .Multiline = True,
            .ScrollBars = ScrollBars.Vertical,
            .WordWrap = True,
            .ReadOnly = True,
            .BackColor = Color.FromArgb(250, 255, 250),
            .ForeColor = Color.FromArgb(40, 40, 40),
            .BorderStyle = BorderStyle.FixedSingle,
            .Padding = New Padding(8),
            .Text = "Enter patient information and symptoms, then click 'Analyze Symptoms' to generate preliminary assessment." & vbCrLf & vbCrLf &
                    "For advanced AI-powered diagnostic insights, click the '✨ Generate AI Insights' button (requires AI agent integration)."
        }

        ' ===================================================================
        ' ADD ALL CONTROLS TO SCROLLABLE CORE CONTENT PANEL
        ' ===================================================================
        pnlCoreContent.Controls.AddRange(New Control() {
            grpPatientInfo, grpSymptoms, grpAdditional, lblResultTitle, txtResults
        })

        ' ===================================================================
        ' ADD PANELS TO FORM IN CORRECT DOCK ORDER
        ' Critical: Add in reverse visual order for proper dock stacking
        ' ===================================================================
        Me.Controls.Add(pnlCoreContent)      ' Fills remaining space
        Me.Controls.Add(pnlActionButtons)    ' Fixed bottom
        Me.Controls.Add(pnlHeader)           ' Fixed top
    End Sub
#End Region

#Region "Patient Profile Lookup Methods"
    ''' <summary>
    ''' DYNAMIC AUTO-COMPLETE LOOKUP: Loads patient profiles from database
    ''' Populates the ComboBox with patient records for quick selection
    ''' </summary>
    Private Sub LoadPatientProfiles()
        Try
            ' Query patients from database with essential demographics
            Dim sql As String = "SELECT PatientID, FirstName || ' ' || LastName AS FullName, " &
                               "CAST((julianday('now') - julianday(DateOfBirth)) / 365.25 AS INTEGER) AS Age, " &
                               "Gender FROM PatientsManagement ORDER BY LastName, FirstName"

            patientDataTable = ModuleDatabase.GetDataTable(sql)

            If patientDataTable IsNot Nothing AndAlso patientDataTable.Rows.Count > 0 Then
                cmbPatientLookup.Items.Clear()

                ' Populate ComboBox with patient names
                For Each row As DataRow In patientDataTable.Rows
                    Dim patientDisplay As String = row("FullName").ToString() & " (ID: " & row("PatientID").ToString() & ")"
                    cmbPatientLookup.Items.Add(patientDisplay)
                Next

                ModuleDatabase.LogError($"LoadPatientProfiles: Loaded {patientDataTable.Rows.Count} patient profiles")
            Else
                ModuleDatabase.LogError("LoadPatientProfiles: No patient records found in database")
            End If

        Catch ex As Exception
            MessageBox.Show($"Error loading patient profiles: {ex.Message}", "Database Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
            ModuleDatabase.LogError($"LoadPatientProfiles error: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' Extracts PatientID from the ComboBox display string
    ''' Format: "John Doe (ID: PAT-2024-0001)"
    ''' </summary>
    Private Function ExtractPatientID(displayText As String) As String
        Try
            Dim startIndex As Integer = displayText.IndexOf("(ID: ")
            If startIndex >= 0 Then
                startIndex += 5 ' Skip past "(ID: "
                Dim endIndex As Integer = displayText.IndexOf(")", startIndex)
                If endIndex > startIndex Then
                    Return displayText.Substring(startIndex, endIndex - startIndex).Trim()
                End If
            End If
        Catch ex As Exception
            ModuleDatabase.LogError($"ExtractPatientID error: {ex.Message}")
        End Try
        Return String.Empty
    End Function

    ''' <summary>
    ''' Auto-populates patient demographics when a patient is selected from the lookup
    ''' Wired to cmbPatientLookup.SelectedIndexChanged event
    ''' </summary>
    Private Sub cmbPatientLookup_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbPatientLookup.SelectedIndexChanged
        Try
            If isLoadingPatientData Then Return

            If cmbPatientLookup.SelectedIndex >= 0 AndAlso patientDataTable IsNot Nothing Then
                Dim selectedRow As DataRow = patientDataTable.Rows(cmbPatientLookup.SelectedIndex)

                ' Auto-populate age
                txtAge.Text = selectedRow("Age").ToString()
                txtAge.ReadOnly = True
                txtAge.BackColor = Color.FromArgb(245, 250, 255)

                ' Auto-populate gender
                Dim gender As String = selectedRow("Gender").ToString()
                cmbGender.Enabled = True
                Select Case gender.ToLower()
                    Case "male"
                        cmbGender.SelectedIndex = 0
                    Case "female"
                        cmbGender.SelectedIndex = 1
                    Case Else
                        cmbGender.SelectedIndex = 2
                End Select
                cmbGender.Enabled = False

                ' Load latest vitals if available
                Dim patientID As String = selectedRow("PatientID").ToString()
                LoadLatestVitals(patientID)

                ModuleDatabase.LogError($"Patient selected: {selectedRow("FullName")} (ID: {patientID})")
            End If

        Catch ex As Exception
            MessageBox.Show($"Error loading patient data: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
            ModuleDatabase.LogError($"cmbPatientLookup_SelectedIndexChanged error: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' Loads the most recent vitals for the selected patient
    ''' Auto-populates temperature and blood pressure fields
    ''' </summary>
    Private Sub LoadLatestVitals(patientID As String)
        Try
            Dim sql As String = "SELECT Temperature, BloodPressure FROM InpatientVitals " &
                               "WHERE PatientID = @patientID " &
                               "ORDER BY DateRecorded DESC LIMIT 1"

            Dim params As New Dictionary(Of String, Object) From {
                {"@patientID", patientID}
            }

            Dim vitalsTable As DataTable = ModuleDatabase.GetDataTable(sql, params)

            If vitalsTable IsNot Nothing AndAlso vitalsTable.Rows.Count > 0 Then
                Dim vitalsRow As DataRow = vitalsTable.Rows(0)

                ' Auto-populate temperature if available
                If Not IsDBNull(vitalsRow("Temperature")) Then
                    Dim temp As Double = Convert.ToDouble(vitalsRow("Temperature"))
                    txtTemperature.Text = temp.ToString("0.0")
                End If

                ' Auto-populate blood pressure if available (format: "120/80")
                If Not IsDBNull(vitalsRow("BloodPressure")) Then
                    txtBloodPressure.Text = vitalsRow("BloodPressure").ToString()
                End If

                ModuleDatabase.LogError($"LoadLatestVitals: Loaded vitals for patient {patientID}")
            End If

        Catch ex As Exception
            ModuleDatabase.LogError($"LoadLatestVitals error: {ex.Message}")
            ' Don't show error to user - just log it (vitals are optional)
        End Try
    End Sub
#End Region

#Region "Intelligent Clinical Risk Assessment"
    ''' <summary>
    ''' COMPUTE CLINICAL ALERT THRESHOLDS
    ''' Calculates preliminary patient risk status based on vital signs
    ''' Implements evidence-based clinical thresholds for temperature and blood pressure
    ''' </summary>
    ''' <param name="temp">Temperature in Celsius</param>
    ''' <param name="bp">Blood pressure string in format "systolic/diastolic" (e.g., "120/80")</param>
    ''' <returns>Risk status: "High Risk (Red)", "Moderate Risk (Amber)", or "Normal (Green)"</returns>
    Private Function CalculatePatientRiskStatus(temp As Double, bp As String) As String
        Try
            ' ===================================================================
            ' CLINICAL THRESHOLD ANALYSIS
            ' Evidence-based vital signs risk stratification
            ' ===================================================================

            ' HIGH RISK CONDITIONS (Red Alert)
            ' Temperature > 38.5°C indicates potential severe infection/sepsis
            ' Systolic BP > 160 mmHg indicates hypertensive crisis risk
            ' ===================================================================
            If temp > 38.5 Then
                Return "High Risk (Red)"
            End If

            ' Parse systolic blood pressure from "120/80" format
            If Not String.IsNullOrWhiteSpace(bp) Then
                Dim bpParts As String() = bp.Split("/"c)
                If bpParts.Length >= 1 Then
                    Dim systolic As Integer = 0
                    If Integer.TryParse(bpParts(0).Trim(), systolic) Then
                        ' High blood pressure threshold
                        If systolic > 160 Then
                            Return "High Risk (Red)"
                        End If
                    End If
                End If
            End If

            ' ===================================================================
            ' MODERATE RISK CONDITIONS (Amber Warning)
            ' Temperature 37.5-38.5°C indicates elevated temperature requiring monitoring
            ' ===================================================================
            If temp >= 37.5 AndAlso temp <= 38.5 Then
                Return "Moderate Risk (Amber)"
            End If

            ' ===================================================================
            ' NORMAL RANGE (Green - Safe)
            ' Patient vitals within acceptable clinical parameters
            ' ===================================================================
            Return "Normal (Green)"

        Catch ex As Exception
            ModuleDatabase.LogError($"CalculatePatientRiskStatus error: {ex.Message}")
            Return "Error (Unable to Assess)"
        End Try
    End Function

    ''' <summary>
    ''' DYNAMIC UI FEEDBACK LOOP
    ''' Updates the risk indicator panel in real-time as vitals are entered
    ''' Provides immediate visual feedback to medical staff
    ''' </summary>
    Private Sub UpdateRiskIndicator()
        Try
            ' Validate that we have vitals data to assess
            If String.IsNullOrWhiteSpace(txtTemperature.Text) AndAlso String.IsNullOrWhiteSpace(txtBloodPressure.Text) Then
                ' No vitals entered yet - reset to default state
                pnlRiskIndicator.BackColor = Color.FromArgb(245, 245, 245)
                lblRiskStatus.ForeColor = Color.FromArgb(100, 100, 100)
                lblRiskStatus.Text = "Risk: Not Assessed"
                Return
            End If

            ' Parse temperature (default to normal if invalid/empty)
            Dim temperature As Double = 36.5 ' Normal body temperature baseline
            If Not String.IsNullOrWhiteSpace(txtTemperature.Text) Then
                If Not Double.TryParse(txtTemperature.Text, temperature) Then
                    ' Invalid temperature format - show warning
                    pnlRiskIndicator.BackColor = Color.FromArgb(255, 250, 205)
                    lblRiskStatus.ForeColor = Color.FromArgb(150, 100, 0)
                    lblRiskStatus.Text = "Invalid Temperature"
                    Return
                End If
            End If

            ' Get blood pressure string (may be empty)
            Dim bloodPressure As String = txtBloodPressure.Text.Trim()

            ' Calculate clinical risk status
            Dim riskStatus As String = CalculatePatientRiskStatus(temperature, bloodPressure)

            ' ===================================================================
            ' APPLY DYNAMIC VISUAL FEEDBACK
            ' Color-coded indicators for rapid clinical assessment
            ' ===================================================================
            Select Case riskStatus
                Case "High Risk (Red)"
                    pnlRiskIndicator.BackColor = Color.FromArgb(220, 53, 69) ' Medical red
                    lblRiskStatus.ForeColor = Color.White
                    lblRiskStatus.Text = "⚠ HIGH RISK"

                Case "Moderate Risk (Amber)"
                    pnlRiskIndicator.BackColor = Color.FromArgb(255, 193, 7) ' Medical amber
                    lblRiskStatus.ForeColor = Color.FromArgb(50, 50, 50)
                    lblRiskStatus.Text = "⚡ MODERATE RISK"

                Case "Normal (Green)"
                    pnlRiskIndicator.BackColor = Color.FromArgb(40, 167, 69) ' Medical green
                    lblRiskStatus.ForeColor = Color.White
                    lblRiskStatus.Text = "✓ NORMAL RANGE"

                Case Else
                    pnlRiskIndicator.BackColor = Color.FromArgb(220, 220, 220)
                    lblRiskStatus.ForeColor = Color.FromArgb(80, 80, 80)
                    lblRiskStatus.Text = riskStatus
            End Select

            ' Log risk assessment for audit trail
            ModuleDatabase.LogError($"RISK_ASSESSMENT: Temp={temperature}°C, BP={bloodPressure}, Status={riskStatus}")

        Catch ex As Exception
            ModuleDatabase.LogError($"UpdateRiskIndicator error: {ex.Message}")
            pnlRiskIndicator.BackColor = Color.FromArgb(255, 235, 235)
            lblRiskStatus.ForeColor = Color.FromArgb(200, 0, 0)
            lblRiskStatus.Text = "Assessment Error"
        End Try
    End Sub

    ''' <summary>
    ''' EVENT: Temperature field changed
    ''' Triggers real-time risk assessment when temperature is entered/modified
    ''' </summary>
    Private Sub txtTemperature_TextChanged(sender As Object, e As EventArgs) Handles txtTemperature.TextChanged
        UpdateRiskIndicator()
    End Sub

    ''' <summary>
    ''' EVENT: Blood pressure field changed
    ''' Triggers real-time risk assessment when blood pressure is entered/modified
    ''' </summary>
    Private Sub txtBloodPressure_TextChanged(sender As Object, e As EventArgs) Handles txtBloodPressure.TextChanged
        UpdateRiskIndicator()
    End Sub

    ''' <summary>
    ''' EVENT: Temperature field loses focus
    ''' Additional validation checkpoint when doctor finishes entering temperature
    ''' </summary>
    Private Sub txtTemperature_Leave(sender As Object, e As EventArgs) Handles txtTemperature.Leave
        UpdateRiskIndicator()
    End Sub

    ''' <summary>
    ''' EVENT: Blood pressure field loses focus
    ''' Additional validation checkpoint when doctor finishes entering blood pressure
    ''' </summary>
    Private Sub txtBloodPressure_Leave(sender As Object, e As EventArgs) Handles txtBloodPressure.Leave
        UpdateRiskIndicator()
    End Sub
#End Region

#Region "Event Handlers"
    Private Sub FormSymptomChecker_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            ' Load patient profiles for dynamic lookup
            LoadPatientProfiles()

            ' Pre-populate patient if current user is a Patient
            If SessionManager.CurrentUser IsNot Nothing Then
                If String.Equals(SessionManager.CurrentUser.Role, "Patient", StringComparison.OrdinalIgnoreCase) Then
                    ' Search for current user in patient list
                    Dim currentUserFullName As String = SessionManager.CurrentUser.FullName
                    For i As Integer = 0 To cmbPatientLookup.Items.Count - 1
                        Dim itemText As String = cmbPatientLookup.Items(i).ToString()
                        If itemText.StartsWith(currentUserFullName, StringComparison.OrdinalIgnoreCase) Then
                            isLoadingPatientData = True
                            cmbPatientLookup.SelectedIndex = i
                            cmbPatientLookup.Enabled = False ' Lock selection for patient users
                            isLoadingPatientData = False
                            Exit For
                        End If
                    Next
                End If
            End If

        Catch ex As Exception
            ModuleDatabase.LogError($"FormSymptomChecker_Load error: {ex.Message}")
        End Try
    End Sub

    Private Sub btnCheckSymptoms_Click(sender As Object, e As EventArgs) Handles btnCheckSymptoms.Click
        Try
            ' Validate inputs
            Dim patientName As String = String.Empty

            ' Get patient name from ComboBox (typed or selected)
            If cmbPatientLookup.SelectedIndex >= 0 Then
                ' Extract name from selected item "John Doe (ID: PAT-2024-0001)"
                Dim displayText As String = cmbPatientLookup.Text
                Dim startIndex As Integer = displayText.IndexOf(" (ID:")
                If startIndex > 0 Then
                    patientName = displayText.Substring(0, startIndex).Trim()
                Else
                    patientName = displayText.Trim()
                End If
            Else
                ' User typed a new patient name
                patientName = cmbPatientLookup.Text.Trim()
            End If

            If String.IsNullOrWhiteSpace(patientName) Then
                MessageBox.Show("Please enter or select a patient name.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning)
                cmbPatientLookup.Focus()
                Return
            End If

            If lstSymptoms.CheckedItems.Count = 0 Then
                MessageBox.Show("Please select at least one symptom.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' Build symptom list
            Dim symptoms As New System.Text.StringBuilder()
            For Each item As String In lstSymptoms.CheckedItems
                symptoms.AppendLine($"  • {item}")
            Next

            ' Generate assessment result
            Dim result As New System.Text.StringBuilder()
            result.AppendLine($"PATIENT: {patientName}")
            result.AppendLine($"DATE: {DateTime.Now:yyyy-MM-dd HH:mm}")
            result.AppendLine()
            result.AppendLine("REPORTED SYMPTOMS:")
            result.Append(symptoms.ToString())
            result.AppendLine()

            ' Vitals summary
            If Not String.IsNullOrWhiteSpace(txtTemperature.Text) Then
                result.AppendLine($"Temperature: {txtTemperature.Text} °C")
            End If
            If Not String.IsNullOrWhiteSpace(txtBloodPressure.Text) Then
                result.AppendLine($"Blood Pressure: {txtBloodPressure.Text}")
            End If

            result.AppendLine()
            result.AppendLine("PRELIMINARY ASSESSMENT:")
            result.AppendLine("Based on the reported symptoms, it is recommended to consult with a medical professional.")
            result.AppendLine()
            result.AppendLine("⚠️ DISCLAIMER: This is a preliminary assessment tool only.")
            result.AppendLine("For accurate diagnosis and treatment, please schedule an appointment with a qualified physician.")

            txtResults.Text = result.ToString()
            txtResults.BackColor = Color.FromArgb(255, 255, 240)

            ' Enable AI Assist button after preliminary assessment is complete
            btnAIAssist.Enabled = True
            btnAIAssist.BackColor = Color.FromArgb(0, 102, 153)

            ' Log the assessment
            ModuleDatabase.LogError($"SYMPTOM_CHECK: Patient={patientName}, Symptoms={lstSymptoms.CheckedItems.Count}, User={SessionManager.CurrentUser?.Username}")

            MessageBox.Show("Symptom assessment completed. Please review the results below." & vbCrLf & vbCrLf &
                          "For advanced AI-powered diagnostic insights, click '✨ Generate AI Insights'.",
                          "Assessment Complete",
                MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show($"Error analyzing symptoms: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
            ModuleDatabase.LogError($"btnCheckSymptoms_Click error: {ex.Message}")
        End Try
    End Sub

    Private Sub btnClearForm_Click(sender As Object, e As EventArgs) Handles btnClearForm.Click
        Try
            ' Clear all input fields
            If cmbPatientLookup.Enabled Then
                cmbPatientLookup.SelectedIndex = -1
                cmbPatientLookup.Text = String.Empty
            End If

            ' Reset age field
            txtAge.Text = String.Empty
            txtAge.ReadOnly = False
            txtAge.BackColor = Color.White

            ' Reset gender
            cmbGender.SelectedIndex = -1
            cmbGender.Enabled = True

            ' Clear vitals
            txtTemperature.Clear()
            txtBloodPressure.Clear()
            txtAdditionalInfo.Clear()

            ' Reset clinical risk indicator
            pnlRiskIndicator.BackColor = Color.FromArgb(245, 245, 245)
            lblRiskStatus.ForeColor = Color.FromArgb(100, 100, 100)
            lblRiskStatus.Text = "Risk: Not Assessed"

            ' Reset results area
            txtResults.Text = "Enter patient information and symptoms, then click 'Analyze Symptoms' to generate preliminary assessment." & vbCrLf & vbCrLf &
                    "For advanced AI-powered diagnostic insights, click the '✨ Generate AI Insights' button (requires AI agent integration)."
            txtResults.BackColor = Color.FromArgb(250, 255, 250)

            ' Disable AI Assist button when form is cleared
            btnAIAssist.Enabled = False
            btnAIAssist.BackColor = Color.FromArgb(108, 117, 125)

            ' Uncheck all symptoms
            For i As Integer = 0 To lstSymptoms.Items.Count - 1
                lstSymptoms.SetItemChecked(i, False)
            Next

            MessageBox.Show("Form cleared successfully.", "Cleared",
                MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show($"Error clearing form: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
            ModuleDatabase.LogError($"btnClearForm_Click error: {ex.Message}")
        End Try
    End Sub

    Private Sub btnAIAssist_Click(sender As Object, e As EventArgs) Handles btnAIAssist.Click
        Try
            ' ===================================================================
            ' AI DIAGNOSTIC AGENT INTEGRATION PLACEHOLDER
            ' ===================================================================
            ' This is the future integration point for AI-powered medical analysis
            ' The AI agent will analyze patient symptoms, vitals, and medical history
            ' to provide advanced diagnostic insights and treatment recommendations
            ' ===================================================================

            ' Validate that preliminary assessment has been completed
            If String.IsNullOrWhiteSpace(cmbPatientLookup.Text) OrElse lstSymptoms.CheckedItems.Count = 0 Then
                MessageBox.Show("Please complete the preliminary symptom assessment first before generating AI insights.",
                              "Assessment Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' Get patient name for AI report
            Dim patientName As String = String.Empty
            If cmbPatientLookup.SelectedIndex >= 0 Then
                Dim displayText As String = cmbPatientLookup.Text
                Dim startIndex As Integer = displayText.IndexOf(" (ID:")
                If startIndex > 0 Then
                    patientName = displayText.Substring(0, startIndex).Trim()
                Else
                    patientName = displayText.Trim()
                End If
            Else
                patientName = cmbPatientLookup.Text.Trim()
            End If

            ' Show processing message
            Dim originalText As String = txtResults.Text
            txtResults.Text = "🤖 AI DIAGNOSTIC AGENT PROCESSING..." & vbCrLf & vbCrLf &
                            "Analyzing patient symptoms and medical data..." & vbCrLf &
                            "Consulting medical knowledge base..." & vbCrLf &
                            "Generating differential diagnosis..." & vbCrLf & vbCrLf &
                            "Please wait..."
            txtResults.BackColor = Color.FromArgb(240, 248, 255)
            Application.DoEvents()

            ' Simulate AI processing delay (remove when integrating real AI agent)
            System.Threading.Thread.Sleep(1500)

            ' ===================================================================
            ' PLACEHOLDER: AI AGENT RESPONSE
            ' Replace this section with actual AI diagnostic agent API call
            ' ===================================================================
            Dim aiInsights As New System.Text.StringBuilder()
            aiInsights.AppendLine("═══════════════════════════════════════════════════════════════")
            aiInsights.AppendLine("🤖 AI-ENHANCED DIAGNOSTIC ANALYSIS")
            aiInsights.AppendLine("═══════════════════════════════════════════════════════════════")
            aiInsights.AppendLine()
            aiInsights.AppendLine($"PATIENT: {patientName}")
            aiInsights.AppendLine($"ANALYSIS DATE: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
            aiInsights.AppendLine()
            aiInsights.AppendLine("DIFFERENTIAL DIAGNOSIS (AI-Generated):")
            aiInsights.AppendLine("  1. [AI AGENT PLACEHOLDER] - Primary diagnostic possibility based on symptom pattern")
            aiInsights.AppendLine("  2. [AI AGENT PLACEHOLDER] - Secondary diagnostic consideration")
            aiInsights.AppendLine("  3. [AI AGENT PLACEHOLDER] - Alternative diagnostic pathway")
            aiInsights.AppendLine()
            aiInsights.AppendLine("RECOMMENDED DIAGNOSTIC TESTS:")
            aiInsights.AppendLine("  • [AI AGENT PLACEHOLDER] - Laboratory tests")
            aiInsights.AppendLine("  • [AI AGENT PLACEHOLDER] - Imaging studies")
            aiInsights.AppendLine("  • [AI AGENT PLACEHOLDER] - Specialist referrals")
            aiInsights.AppendLine()
            aiInsights.AppendLine("URGENCY ASSESSMENT:")
            aiInsights.AppendLine("  [AI AGENT PLACEHOLDER] - Risk stratification and priority level")
            aiInsights.AppendLine()
            aiInsights.AppendLine("TREATMENT CONSIDERATIONS:")
            aiInsights.AppendLine("  [AI AGENT PLACEHOLDER] - Evidence-based therapeutic options")
            aiInsights.AppendLine()
            aiInsights.AppendLine("═══════════════════════════════════════════════════════════════")
            aiInsights.AppendLine()
            aiInsights.AppendLine("ORIGINAL PRELIMINARY ASSESSMENT:")
            aiInsights.AppendLine("─────────────────────────────────────────────────────────────")
            aiInsights.Append(originalText)
            aiInsights.AppendLine()
            aiInsights.AppendLine("─────────────────────────────────────────────────────────────")
            aiInsights.AppendLine()
            aiInsights.AppendLine("⚠️ AI MEDICAL DISCLAIMER:")
            aiInsights.AppendLine("This AI-generated analysis is for informational and decision-support purposes only.")
            aiInsights.AppendLine("Final diagnostic and treatment decisions must be made by qualified medical professionals.")
            aiInsights.AppendLine("AI insights should be validated through clinical examination and appropriate testing.")

            txtResults.Text = aiInsights.ToString()
            txtResults.BackColor = Color.FromArgb(240, 250, 255)

            ' Log AI analysis request
            ModuleDatabase.LogError($"AI_ASSIST: Patient={patientName}, Symptoms={lstSymptoms.CheckedItems.Count}, User={SessionManager.CurrentUser?.Username}")

            MessageBox.Show("AI diagnostic insights generated successfully." & vbCrLf & vbCrLf &
                          "⚠️ NOTE: This is a placeholder response. Integrate with your AI diagnostic agent API for real analysis.",
                          "AI Analysis Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show($"Error generating AI insights: {ex.Message}", "AI Agent Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
            ModuleDatabase.LogError($"btnAIAssist_Click error: {ex.Message}")
        End Try
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub

    Private Sub txtAge_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtAge.KeyPress
        ' Allow only numeric input for age
        If Not Char.IsDigit(e.KeyChar) AndAlso Not Char.IsControl(e.KeyChar) Then
            e.Handled = True
        End If
    End Sub
#End Region
End Class
