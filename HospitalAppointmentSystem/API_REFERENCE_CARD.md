# 🔵 ENTERPRISE DATABASE API REFERENCE CARD

## 📋 QUICK REFERENCE - Copy & Paste Ready

---

## 🔐 AUDIT LOGGING

### Write Simple Audit Entry
```vb
WriteAuditEntry("username", "Action description", "ModuleName")
```

### Write Advanced Audit Entry
```vb
LogSystemActivity(
	username:="dr_smith",
	action:="Patient record modified",
	moduleName:="FormPatientManagement",
	ipAddress:="192.168.1.100",
	severity:="WARNING",
	additionalContext:="Changed diagnosis field"
)
```

### Query Audit Logs
```vb
' All logs
Dim allLogs As DataTable = GetAuditLogs()

' Filter by date range
Dim logs As DataTable = GetAuditLogs(
	startDate:="2026-01-01",
	endDate:="2026-01-09"
)

' Filter by user
Dim userLogs As DataTable = GetAuditLogs(username:="admin")

' Filter by severity
Dim criticalLogs As DataTable = GetAuditLogs(severity:="CRITICAL")

' Filter by module
Dim moduleLogs As DataTable = GetAuditLogs(moduleName:="FormPatientManagement")
```

### Get Audit Statistics
```vb
Dim stats As Dictionary(Of String, Integer) = GetAuditStatistics()
' Returns: TotalRecords, Severity_INFO, Severity_WARNING, Severity_ERROR, Severity_CRITICAL, TodayRecords
```

---

## 🏥 CLINICAL ASSESSMENTS

### Save NEW Assessment (Auto-Generate ID)
```vb
Dim assessmentID As String = SaveClinicalAssessment(
	assessmentID:="",                      ' Empty = new
	patientID:="PAT-2026-0001",           ' Required
	systolicBP:=120,
	diastolicBP:=80,
	heartRate:=72,
	temperature:=37.0,
	triageStatus:="ROUTINE",              ' ROUTINE | URGENT | EMERGENCY | CRITICAL | DECEASED
	respiratoryRate:=16,
	painScore:=0,                          ' 0-10
	consciousnessLevel:="Alert",
	clinicalNotes:="Patient stable",
	assessedBy:="nurse_jane"
)
' Returns: ASS-2026-0001
```

### Update EXISTING Assessment
```vb
Dim assessmentID As String = SaveClinicalAssessment(
	assessmentID:="ASS-2026-0001",        ' Existing ID
	patientID:="PAT-2026-0001",
	systolicBP:=135,                       ' Updated values
	diastolicBP:=88,
	heartRate:=82,
	temperature:=37.5,
	triageStatus:="URGENT",                ' Escalated
	respiratoryRate:=18,
	painScore:=5,
	consciousnessLevel:="Alert",
	clinicalNotes:="BP elevated, monitor closely",
	assessedBy:="dr_wilson"
)
' Returns: ASS-2026-0001 (same ID)
```

### Get Patient Assessment History
```vb
Dim history As DataTable = GetClinicalAssessmentsByPatient("PAT-2026-0001")
dgvHistory.DataSource = history
```

### Get Assessments by Triage Status
```vb
' CRITICAL patients
Dim critical As DataTable = GetAssessmentsByTriageStatus("CRITICAL")

' URGENT patients
Dim urgent As DataTable = GetAssessmentsByTriageStatus("URGENT")

' EMERGENCY patients
Dim emergency As DataTable = GetAssessmentsByTriageStatus("EMERGENCY")

' ROUTINE patients
Dim routine As DataTable = GetAssessmentsByTriageStatus("ROUTINE")
```

### Delete Assessment
```vb
Dim success As Boolean = DeleteClinicalAssessment("ASS-2026-0001")
```

---

## 🛡️ ERROR HANDLING

### Try-Catch Pattern
```vb
Try
	Dim assessID As String = SaveClinicalAssessment(...)
	MessageBox.Show($"Assessment {assessID} saved!", "Success")

Catch ex As InvalidOperationException
	' User-friendly error message
	MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)

Catch ex As ArgumentOutOfRangeException
	' Validation error (e.g., pain score out of range)
	MessageBox.Show(ex.Message, "Validation Error")
End Try
```

---

## 📊 TRIAGE STATUS LEVELS

| Status | Description | Use Case |
|--------|-------------|----------|
| `ROUTINE` | Standard care | Annual checkups, non-urgent |
| `URGENT` | Priority needed | Elevated vitals, moderate pain |
| `EMERGENCY` | Immediate attention | Acute symptoms, severe pain |
| `CRITICAL` | Life-threatening | Cardiac arrest, trauma |
| `DECEASED` | Post-mortem | Documentation only |

---

## 🎨 UI BINDING EXAMPLES

### Bind Assessment History to Grid
```vb
Private Sub LoadPatientAssessments(patientID As String)
	Dim history As DataTable = GetClinicalAssessmentsByPatient(patientID)
	dgvAssessments.DataSource = history
	dgvAssessments.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells

	' Highlight by triage status
	For Each row As DataGridViewRow In dgvAssessments.Rows
		If row.Cells("Triage Status").Value IsNot Nothing Then
			Select Case row.Cells("Triage Status").Value.ToString()
				Case "CRITICAL"
					row.DefaultCellStyle.BackColor = Color.DarkRed
					row.DefaultCellStyle.ForeColor = Color.White
				Case "EMERGENCY"
					row.DefaultCellStyle.BackColor = Color.Red
				Case "URGENT"
					row.DefaultCellStyle.BackColor = Color.Orange
			End Select
		End If
	Next
End Sub
```

### Bind Triage Dashboard
```vb
Private Sub LoadTriageDashboard()
	dgvCriticalQueue.DataSource = GetAssessmentsByTriageStatus("CRITICAL")
	dgvEmergencyQueue.DataSource = GetAssessmentsByTriageStatus("EMERGENCY")
	dgvUrgentQueue.DataSource = GetAssessmentsByTriageStatus("URGENT")

	lblCriticalCount.Text = $"⚠️ {dgvCriticalQueue.Rows.Count} CRITICAL"
	lblEmergencyCount.Text = $"🚨 {dgvEmergencyQueue.Rows.Count} EMERGENCY"
	lblUrgentCount.Text = $"⏰ {dgvUrgentQueue.Rows.Count} URGENT"
End Sub
```

### Bind Audit Logs
```vb
Private Sub LoadAuditLogs()
	Dim today As String = DateTime.Now.ToString("yyyy-MM-dd")
	Dim logs As DataTable = GetAuditLogs(
		startDate:=DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd"),
		endDate:=today
	)

	dgvAuditLogs.DataSource = logs
	lblTotalLogs.Text = $"Total Logs: {logs.Rows.Count}"
End Sub
```

---

## 🔢 VALIDATION RULES

### Clinical Assessment:
- `SystolicBP` - INTEGER (optional, e.g., 120)
- `DiastolicBP` - INTEGER (optional, e.g., 80)
- `HeartRate` - INTEGER (optional, e.g., 72)
- `Temperature` - REAL (optional, e.g., 37.0)
- `TriageStatus` - TEXT (ROUTINE | URGENT | EMERGENCY | CRITICAL | DECEASED)
- `RespiratoryRate` - INTEGER (optional, e.g., 16)
- `PainScore` - INTEGER (0-10, **required**)
- `ConsciousnessLevel` - TEXT (optional, e.g., "Alert", "AVPU", "GCS 15")
- `ClinicalNotes` - TEXT (optional)
- `AssessedBy` - TEXT (optional, defaults to "SYSTEM")
- `PatientID` - TEXT (**required**, must exist in PatientsManagement)

### Audit Entry:
- `user` - TEXT (username, auto-defaults to "UNKNOWN_USER" if empty)
- `action` - TEXT (description, auto-defaults to "UNSPECIFIED_ACTION" if empty)
- `moduleName` - TEXT (form/module, auto-defaults to "UNSPECIFIED_MODULE" if empty)
- `severity` - TEXT (INFO | WARNING | ERROR | CRITICAL, defaults to INFO)

---

## 🚨 ERROR MESSAGES

| SQLite Error | User-Friendly Message |
|--------------|----------------------|
| `UNIQUE constraint failed` | `"Assessment ID 'ASS-2026-0001' already exists."` |
| `FOREIGN KEY constraint failed` | `"Patient 'PAT-2026-9999' does not exist in system."` |
| `database is locked` | `"Database is locked. Please try again."` |
| `CHECK constraint failed` | `"Invalid triage status or pain score."` |

---

## 📂 DATABASE TABLES

### ClinicalAssessments (NEW)
```
AssessmentID (PK)     - TEXT (ASS-YYYY-NNNN)
PatientID (FK)        - TEXT (references PatientsManagement)
SystolicBP            - INTEGER
DiastolicBP           - INTEGER
HeartRate             - INTEGER
Temperature           - REAL
TriageStatusFlag      - TEXT (ROUTINE/URGENT/EMERGENCY/CRITICAL/DECEASED)
RespiratoryRate       - INTEGER
PainScore             - INTEGER (0-10)
ConsciousnessLevel    - TEXT
ClinicalNotes         - TEXT
AssessedBy            - TEXT
LastUpdated           - TEXT (auto-timestamp)
```

### SystemAuditLogs (EXISTING)
```
LogID (PK)            - INTEGER (auto-increment)
Timestamp             - TEXT
ActiveUser            - TEXT
ActionPerformed       - TEXT
ModuleName            - TEXT
IPAddress             - TEXT
SessionID             - TEXT
Severity              - TEXT (INFO/WARNING/ERROR/CRITICAL)
AdditionalContext     - TEXT
MachineNameHost       - TEXT
CreatedDate           - DATETIME
```

---

## 🔗 METHOD SIGNATURES

```vb
' Audit Logging
Public Sub InitializeSystemAuditSchema()
Public Sub WriteAuditEntry(user As String, action As String, moduleName As String)
Public Sub LogSystemActivity(username As String, action As String, moduleName As String, 
	Optional ipAddress As String = "127.0.0.1", Optional severity As String = "INFO", 
	Optional additionalContext As String = "")
Public Function GetAuditLogs(Optional startDate As String = "", Optional endDate As String = "",
	Optional username As String = "", Optional moduleName As String = "", 
	Optional severity As String = "") As DataTable
Public Function GetAuditStatistics() As Dictionary(Of String, Integer)

' Clinical Assessments
Public Sub InitializeClinicalAssessmentsSchema()
Public Function SaveClinicalAssessment(assessmentID As String, patientID As String,
	systolicBP As Integer, diastolicBP As Integer, heartRate As Integer, 
	temperature As Double, triageStatus As String, respiratoryRate As Integer,
	painScore As Integer, consciousnessLevel As String, clinicalNotes As String,
	assessedBy As String) As String
Public Function GetClinicalAssessmentsByPatient(patientID As String) As DataTable
Public Function GetAssessmentsByTriageStatus(triageStatus As String) As DataTable
Public Function DeleteClinicalAssessment(assessmentID As String) As Boolean
```

---

## 💡 REAL-WORLD WORKFLOW

### Emergency Department Triage Form
```vb
Private Sub btnSaveAssessment_Click(sender As Object, e As EventArgs) Handles btnSaveAssessment.Click
	Try
		' Validate patient exists
		Dim patientID As String = txtPatientID.Text.Trim()
		If GetPatientByID(patientID) Is Nothing Then
			MessageBox.Show("Patient not found!", "Error")
			Return
		End If

		' Capture vitals from UI
		Dim systolic As Integer = Convert.ToInt32(numSystolicBP.Value)
		Dim diastolic As Integer = Convert.ToInt32(numDiastolicBP.Value)
		Dim heartRate As Integer = Convert.ToInt32(numHeartRate.Value)
		Dim temperature As Double = Convert.ToDouble(numTemperature.Value)
		Dim triageStatus As String = cboTriageStatus.SelectedItem.ToString()
		Dim respiratoryRate As Integer = Convert.ToInt32(numRespiratoryRate.Value)
		Dim painScore As Integer = Convert.ToInt32(numPainScore.Value)
		Dim consciousness As String = cboConsciousness.SelectedItem.ToString()
		Dim notes As String = txtClinicalNotes.Text.Trim()
		Dim assessedBy As String = SessionManager.CurrentUser.Username

		' Save assessment
		Dim assessmentID As String = SaveClinicalAssessment(
			assessmentID:="",
			patientID:=patientID,
			systolicBP:=systolic,
			diastolicBP:=diastolic,
			heartRate:=heartRate,
			temperature:=temperature,
			triageStatus:=triageStatus,
			respiratoryRate:=respiratoryRate,
			painScore:=painScore,
			consciousnessLevel:=consciousness,
			clinicalNotes:=notes,
			assessedBy:=assessedBy
		)

		' Log audit trail
		WriteAuditEntry(assessedBy, 
			$"Created {triageStatus} assessment {assessmentID} for patient {patientID}",
			"FormTriage")

		' Auto-escalate if CRITICAL
		If triageStatus = "CRITICAL" Then
			SendPriorityAlert($"CRITICAL patient {patientID} requires immediate attention!")
		End If

		MessageBox.Show($"Assessment {assessmentID} saved!", "Success")
		LoadTriageDashboard()

	Catch ex As InvalidOperationException
		MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
	End Try
End Sub
```

---

## ✅ CHECKLIST FOR IMPLEMENTATION

- [ ] Import `ModuleDatabase` in your form
- [ ] Add UI controls (NumericUpDown for vitals, ComboBox for triage)
- [ ] Populate ComboBox with triage levels: `ROUTINE`, `URGENT`, `EMERGENCY`, `CRITICAL`, `DECEASED`
- [ ] Validate patient exists before saving
- [ ] Call `SaveClinicalAssessment()` in button click handler
- [ ] Wrap in Try-Catch for error handling
- [ ] Log audit entry after successful save
- [ ] Refresh dashboard/grid with updated data
- [ ] Highlight CRITICAL/EMERGENCY rows in red/orange

---

## 🎯 PERFORMANCE TIPS

1. **Use indexes for queries** (already created automatically)
2. **Batch audit writes** if logging high volume (use transactions)
3. **Cache frequently accessed data** (e.g., triage queues)
4. **Limit audit log queries** (use date ranges)
5. **Paginate large result sets** (add `LIMIT` to queries)

---

## 📱 CONTACT & SUPPORT

- **Full Documentation**: `ENTERPRISE_DATABASE_EXPANSION_DOCUMENTATION.md`
- **Quick Start**: `QUICK_START_ENTERPRISE_FEATURES.md`
- **Summary**: `IMPLEMENTATION_SUMMARY.md`
- **This Card**: `API_REFERENCE_CARD.md`

---

**PRINT THIS CARD AND KEEP IT ON YOUR DESK! 📌**

Quick copy-paste reference for all enterprise database operations.

**Build Status**: ✅ SUCCESSFUL  
**Compliance**: ✅ OPTION STRICT ON  
**Production Ready**: ✅ YES  

**Happy Coding! 🚀**
