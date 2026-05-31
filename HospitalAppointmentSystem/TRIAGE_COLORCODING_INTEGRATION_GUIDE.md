# 🏥 PART 2: DATAGRIDVIEW TRIAGE COLOR-CODING INTEGRATION

## ✅ FORMAIN.VB ENHANCEMENTS - PRODUCTION-READY CODE

**Emergency Severity Index (ESI) Visual Triage System**  
**Option Strict On Compliant | Defensive Null Handling | Auto-Sort by Severity**

---

## 📋 IMPLEMENTATION STEPS

### Step 1: Modify GetDoctorAppointments Query (ModuleDatabase.vb)

**Location**: `ModuleDatabase.vb` - Line 3254 (GetDoctorAppointments function)

**Replace** the existing GetDoctorAppointments function with the enhanced version that joins with InpatientVitals:

```vb
''' <summary>
''' Retrieves appointments filtered by DoctorID with patient vitals for triage color-coding.
''' Returns a DataTable ready for DataGridView binding with ESI triage calculations.
''' 
''' COLUMNS RETURNED:
''' - Appointment ID
''' - Patient Name
''' - Date
''' - Time
''' - Department
''' - Status
''' - Emergency Flag
''' - SystolicBP (from vitals, nullable)
''' - DiastolicBP (from vitals, nullable)
''' - HeartRate (from vitals, nullable)
''' - SpO2 (from vitals, nullable)
''' - ESILevel (calculated via TriageEngine)
''' - TriageColor (calculated via TriageEngine)
''' 
''' SECURITY: Parameterized query prevents SQL injection.
''' PERFORMANCE: Indexed on DoctorID, PatientID, and DateRecorded.
''' </summary>
''' <param name="doctorID">The DoctorID to filter appointments</param>
''' <returns>DataTable with doctor-specific appointments including vitals for triage</returns>
Public Function GetDoctorAppointments(doctorID As String) As DataTable
	Try
		If String.IsNullOrWhiteSpace(doctorID) Then
			LogError("GetDoctorAppointments: DoctorID parameter is null or empty")
			Return New DataTable()
		End If

		' ===================================================================
		' ENHANCED QUERY WITH VITALS JOIN FOR TRIAGE COLOR-CODING
		' LEFT JOIN ensures appointments without vitals are still shown
		' Subquery gets MOST RECENT vitals for each patient
		' ===================================================================
		Dim sql As String = "
SELECT 
	a.AppointmentID AS 'Appointment ID',
	(SELECT FullName FROM Users WHERE UserID = p.UserID) AS 'Patient Name',
	a.AppointmentDate AS 'Date',
	a.AppointmentTime AS 'Time',
	d.DepartmentName AS 'Department',
	a.Status,
	CASE WHEN a.IsEmergency = 1 THEN 'Yes' ELSE 'No' END AS 'Emergency',
	a.Notes,
	v.SystolicBP,
	v.DiastolicBP,
	v.HeartRate,
	v.SpO2,
	v.DateRecorded AS 'Vitals Recorded'
FROM Appointments a
INNER JOIN Patients p ON a.PatientID = p.PatientID
LEFT JOIN Departments d ON a.DepartmentID = d.DepartmentID
LEFT JOIN (
	SELECT 
		PatientID,
		CAST(SUBSTR(BloodPressure, 1, INSTR(BloodPressure, '/') - 1) AS INTEGER) AS SystolicBP,
		CAST(SUBSTR(BloodPressure, INSTR(BloodPressure, '/') + 1) AS INTEGER) AS DiastolicBP,
		HeartRate,
		SpO2,
		DateRecorded,
		ROW_NUMBER() OVER (PARTITION BY PatientID ORDER BY DateRecorded DESC) AS rn
	FROM InpatientVitals
	WHERE BloodPressure IS NOT NULL AND BloodPressure != ''
) v ON p.PatientID = v.PatientID AND v.rn = 1
WHERE a.DoctorID = @DocID
ORDER BY a.AppointmentDate DESC, a.AppointmentTime DESC"

		Dim params As New Dictionary(Of String, Object) From {{"@DocID", doctorID}}
		Dim dt As DataTable = GetDataTable(sql, params)

		' ===================================================================
		' POST-QUERY TRIAGE CALCULATION
		' Calculate ESI level and color for each row using TriageEngine
		' ===================================================================
		dt.Columns.Add("ESILevel", GetType(Integer))
		dt.Columns.Add("TriageColor", GetType(Integer))  ' Store Color.ToArgb() for DataGridView
		dt.Columns.Add("SeverityLabel", GetType(String))

		For Each row As DataRow In dt.Rows
			' Extract vitals (nullable integers)
			Dim sbp As Integer? = If(IsDBNull(row("SystolicBP")), Nothing, CType(row("SystolicBP"), Integer?))
			Dim dbp As Integer? = If(IsDBNull(row("DiastolicBP")), Nothing, CType(row("DiastolicBP"), Integer?))
			Dim hr As Integer? = If(IsDBNull(row("HeartRate")), Nothing, CType(row("HeartRate"), Integer?))
			Dim spo2 As Integer? = If(IsDBNull(row("SpO2")), Nothing, CType(row("SpO2"), Integer?))

			' Calculate ESI using TriageEngine
			Dim triageResult As TriageEngine.TriageResult = TriageEngine.CalculateESI(sbp, dbp, hr, spo2)

			' Store triage data in row for DataGridView formatting
			row("ESILevel") = triageResult.ESILevel
			row("TriageColor") = triageResult.ColorIndicator.ToArgb()
			row("SeverityLabel") = triageResult.SeverityLabel
		Next

		' ===================================================================
		' DYNAMIC SORTING: Critical patients (ESI Level 1) bubble to top
		' ===================================================================
		Dim dv As DataView = dt.DefaultView
		dv.Sort = "ESILevel ASC, [Date] ASC, [Time] ASC"
		dt = dv.ToTable()

		LogError($"GetDoctorAppointments: Retrieved {dt.Rows.Count} appointments for DoctorID={doctorID} with triage color-coding")
		Return dt

	Catch ex As Exception
		LogError($"GetDoctorAppointments error: {ex.Message} | DoctorID={doctorID}")
		Return New DataTable()
	End Try
End Function
```

---

### Step 2: Add CellFormatting Event Handler (FormMain.vb)

**Location**: `FormMain.vb` - Inside `InitializeComponent()` method

**Add** this line when initializing dgvDashboardData (around line 196):

```vb
' Add event handler for triage color-coding
AddHandler dgvDashboardData.CellFormatting, AddressOf DgvDashboardData_CellFormatting
```

---

### Step 3: Implement CellFormatting Handler (FormMain.vb)

**Location**: `FormMain.vb` - Add new region after `ConfigureDashboardForRole` method

**Insert** this complete event handler:

```vb
#Region "Triage Color-Coding - Emergency Severity Index (ESI) Visual System"

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

			' Hide raw vitals columns (optional - keep if doctors want to see vitals directly)
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
```

---

### Step 4: Remove Legacy Emergency Highlighting (FormMain.vb)

**Location**: `FormMain.vb` - Line 484-491 (inside ConfigureDashboardForRole)

**Replace** the existing emergency highlighting loop:

```vb
' OLD CODE (REMOVE THIS):
' Highlight emergency appointments
For Each row As DataGridViewRow In dgvDashboardData.Rows
	If row.Cells("Emergency").Value IsNot Nothing AndAlso _
	   row.Cells("Emergency").Value.ToString() = "Yes" Then
		row.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220)
		row.DefaultCellStyle.Font = New Font("Arial", 9, FontStyle.Bold)
	End If
Next
```

**With**:

```vb
' NEW CODE: Triage color-coding handled by CellFormatting event
' No manual row iteration needed - event fires automatically for each visible row
' ESI Level 1 (Critical) patients automatically sorted to top via DataView.Sort in GetDoctorAppointments
```

---

## 🧪 TESTING GUIDE

### Test Scenario 1: Critical Patient (ESI Level 1)

**Setup**:
1. Open database (`HospitalAppointmentSystem.db`)
2. Insert test vitals for a patient:
```sql
INSERT INTO InpatientVitals (PatientID, BloodPressure, HeartRate, SpO2, Temperature, Weight)
VALUES ('PAT-000001', '185/125', 135, 88, 98.6, 70.5);
```
3. Create appointment for that patient with a doctor
4. Login as that doctor

**Expected Result**:
- ✅ Patient row appears at TOP of queue (auto-sorted by ESI Level)
- ✅ Row background color is **Soft Red** (255, 200, 200)
- ✅ Row text is **Bold** font
- ✅ Selection color is darker red for visibility

---

### Test Scenario 2: Urgent Patient (ESI Level 2)

**Setup**:
1. Insert test vitals:
```sql
INSERT INTO InpatientVitals (PatientID, BloodPressure, HeartRate, SpO2)
VALUES ('PAT-000002', '150/95', 110, 92);
```
2. Create appointment for that patient

**Expected Result**:
- ✅ Row background color is **Soft Amber** (255, 235, 180)
- ✅ Appears below Level 1 patients, above Level 3 patients

---

### Test Scenario 3: Stable Patient (ESI Level 3)

**Setup**:
1. Insert normal vitals:
```sql
INSERT INTO InpatientVitals (PatientID, BloodPressure, HeartRate, SpO2)
VALUES ('PAT-000003', '120/80', 75, 98);
```

**Expected Result**:
- ✅ Row background color is **Soft Yellow** (255, 255, 200)
- ✅ Appears at bottom of queue (normal priority)

---

### Test Scenario 4: Patient Without Vitals

**Setup**:
1. Create appointment WITHOUT inserting vitals record

**Expected Result**:
- ✅ Row background color is **Soft Gray** (240, 240, 240)
- ✅ No crash/exception (defensive null handling)
- ✅ Default ESI Level 3 assignment with "INCOMPLETE DATA" label

---

## 🔒 DEFENSIVE PROGRAMMING FEATURES

### 1. **Null-Safe Vitals Handling**
```vb
Dim sbp As Integer? = If(IsDBNull(row("SystolicBP")), Nothing, CType(row("SystolicBP"), Integer?))
```
- Nullable integers prevent crashes when vitals missing
- TriageEngine gracefully handles `Nothing` values

### 2. **Schema Change Protection**
```vb
If Not row.Row.Table.Columns.Contains("TriageColor") Then Return
```
- Event handler checks column existence before access
- Prevents crashes if database schema evolves

### 3. **Type-Safe Casting**
```vb
Dim dgv As DataGridView = TryCast(sender, DataGridView)
If dgv Is Nothing Then Return
```
- Explicit type conversion with null check
- Option Strict On compliant throughout

### 4. **Performance Optimization**
- `CellFormatting` event only fires for **visible cells**
- No manual row iteration needed (automatic)
- DataView.Sort happens once in database layer

### 5. **Medical Safety Fallback**
```vb
Return TriageResult.Create(
	esiLevel:=3,
	severityLabel:="INCOMPLETE DATA",
	...
)
```
- Missing vitals default to Level 3 (Moderate) to prevent ignoring patients
- Logs warnings for audit trail compliance

---

## 📊 VISUAL COLOR PALETTE

| ESI Level | Severity | RGB Color | Hex Code | Use Case |
|-----------|----------|-----------|----------|----------|
| **1** | CRITICAL | (255, 200, 200) | #FFC8C8 | SpO2 < 90%, BP crisis, severe tachycardia |
| **2** | URGENT | (255, 235, 180) | #FFEBB4 | Moderate hypoxemia, Stage 2 HTN, tachycardia |
| **3** | MODERATE | (255, 255, 200) | #FFFFC8 | Stable vitals, requires medical evaluation |
| **4** | LOW | (200, 255, 200) | #C8FFC8 | Non-urgent, minimal intervention needed |
| **5** | MINIMAL | (200, 230, 255) | #C8E6FF | Minor issues, long wait tolerable |
| **N/A** | INCOMPLETE | (240, 240, 240) | #F0F0F0 | Missing vitals data |

---

## ✅ IMPLEMENTATION CHECKLIST

- [x] TriageEngine.vb created with ESI calculation logic
- [x] GetDoctorAppointments enhanced with vitals JOIN
- [x] ESI Level and TriageColor columns added to DataTable
- [x] Auto-sort by ESI Level (Critical patients to top)
- [x] CellFormatting event handler implemented
- [x] Legacy emergency highlighting removed
- [x] Defensive null handling throughout
- [x] Option Strict On compliant
- [x] Comprehensive error logging
- [x] Testing scenarios documented

---

**Your doctor dashboard now features enterprise-grade Emergency Severity Index (ESI) visual triage with automatic critical patient prioritization! 🏥🚨**
