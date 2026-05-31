# PATIENT VITALS TRACKING - COMPLETE IMPLEMENTATION GUIDE
**Hospital Appointment System - CSC3226**
**Group: Sa'id Umar, Aisha Ladan, Maryam Rabiu**

---

## 📋 OVERVIEW

This document provides the complete implementation of **Patient Vitals Tracking** with SQLite backend, full referential integrity, and professional WinForms UI integration under strict `Option Strict On` compliance.

---

## 🗄️ DATABASE SCHEMA

### InpatientVitals Table

```sql
CREATE TABLE IF NOT EXISTS InpatientVitals (
	VitalID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	PatientID TEXT NOT NULL,
	BloodPressure TEXT,
	HeartRate INTEGER,
	Temperature REAL,
	SpO2 INTEGER,
	Weight REAL,
	DateRecorded TEXT NOT NULL DEFAULT (datetime('now')),
	FOREIGN KEY (PatientID) REFERENCES PatientsManagement(PatientID) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_vitals_patient ON InpatientVitals(PatientID);
CREATE INDEX IF NOT EXISTS idx_vitals_date ON InpatientVitals(DateRecorded DESC);
```

### Key Features:
✅ **Foreign Key Constraint**: Enforces referential integrity with CASCADE delete  
✅ **Indexed**: Optimized queries on PatientID and DateRecorded  
✅ **Auto-Initialization**: Created automatically during `InitialiseDatabase()`

---

## 🔧 DATA ACCESS LAYER (ModuleDatabase.vb)

### Public Functions Added

#### 1. `InitializePatientVitalsSchema()`
```vb
' Automatically called during database initialization
' Creates the InpatientVitals table with referential integrity
' Enables SQLite foreign key enforcement per connection
```

#### 2. `SavePatientVitals(...)`
```vb
Public Function SavePatientVitals(
	patientID As String, 
	bloodPressure As String,
	heartRate As Integer, 
	temperature As Double,
	spO2 As Integer, 
	weight As Double
) As Boolean
```

**Features**:
- Thread-safe transaction handling
- Comprehensive input validation (business rules)
- Verifies patient exists before saving
- Returns `True` on success, `False` on failure
- Logs all errors to `error_log.txt`

**Validation Rules**:
- Heart Rate: 0-300 bpm
- Temperature: 30-45 °C
- SpO2: 0-100%
- Weight: 0-500 kg
- Blood Pressure: Optional, free text format (e.g., "120/80")

#### 3. `GetPatientVitalsHistory(patientID As String) As DataTable`
```vb
' Returns complete vitals history for a specific patient
' Sorted by most recent first (DateRecorded DESC)
' Column names are UI-friendly with proper units
```

**Returned Columns**:
- Vital ID (hidden in UI)
- Patient ID (hidden in UI)
- Blood Pressure
- Heart Rate (bpm)
- Temperature (°C)
- SpO2 (%)
- Weight (kg)
- Date Recorded (formatted: `yyyy-MM-dd HH:mm`)

#### 4. `GetLatestPatientVitals(patientID As String) As DataRow`
```vb
' Returns the most recent vitals record for a patient
' Useful for dashboard "current vitals" display
' Returns Nothing if no records exist
```

#### 5. `DeleteVitalsRecord(vitalID As Integer) As Boolean`
```vb
' Deletes a specific vitals record by VitalID
' Use with caution - typically vitals should not be deleted for audit trails
' Returns True if successful
```

#### 6. `GetPatientVitalsCount(patientID As String) As Integer`
```vb
' Returns the total count of vitals records for a patient
' Useful for statistics and validation
```

---

## 🖥️ USER INTERFACE COMPONENTS

### 1. FormPatientVitals.vb
**Purpose**: Modal dialog for recording new patient vitals

**Constructor**:
```vb
Public Sub New(patientID As String, patientName As String)
```

**UI Controls**:
- **Blood Pressure**: TextBox (free format, e.g., "120/80")
- **Heart Rate**: NumericUpDown (30-250 bpm, default: 72)
- **Temperature**: NumericUpDown (30-45°C, decimal: 0.1, default: 37.0)
- **SpO2**: NumericUpDown (50-100%, default: 98)
- **Weight**: NumericUpDown (1-500kg, decimal: 0.1, default: 70.0)

**Validation**:
- Blood Pressure must contain `/` separator if entered
- All numeric ranges enforced at UI level
- Database-level business rules validated on save

**Usage from FormPatientManagement**:
```vb
Using frmVitals As New FormPatientVitals(patientID, patientName)
	If frmVitals.ShowDialog() = DialogResult.OK Then
		' Vitals saved successfully
	End If
End Using
```

---

### 2. FormPatientVitalsHistory.vb
**Purpose**: View complete vitals history for a patient

**Constructor**:
```vb
Public Sub New(patientID As String, patientName As String)
```

**Features**:
- **DataGridView**: Displays all vitals records
- **Refresh Button**: Reloads data from database
- **Delete Button**: Removes selected vitals record (with confirmation)
- **Record Count**: Shows total vitals entries for the patient

**UI Enhancements**:
- Alternating row colors for readability
- Professional column styling
- Hidden internal columns (VitalID, PatientID)
- Date formatting: `yyyy-MM-dd HH:mm`

---

### 3. Integration with FormPatientManagement.vb

#### New Buttons Added:
1. **📋 Record Vitals** (btnRecordVitals)
   - Opens `FormPatientVitals` dialog
   - Passes selected PatientID and name
   - Validates patient selection before opening

2. **📊 View Vitals History** (btnViewVitals)
   - Opens `FormPatientVitalsHistory` dialog
   - Displays complete vitals timeline
   - Allows deletion of records

#### Event Handlers:

```vb
Private Sub btnRecordVitals_Click(sender As Object, e As EventArgs) Handles btnRecordVitals.Click
	' Validates selection
	' Safely extracts PatientID from DataGridView
	' Handles DBNull values explicitly
	' Opens modal vitals recording form
End Sub

Private Sub btnViewVitals_Click(sender As Object, e As EventArgs) Handles btnViewVitals.Click
	' Validates selection
	' Extracts PatientID safely
	' Opens vitals history viewer
End Sub
```

#### Safe PatientID Extraction Pattern:

```vb
' Validate selection
If dgvPatients.SelectedRows.Count = 0 Then
	MessageBox.Show("Please select a patient...")
	Return
End If

' Extract PatientID safely using existing GetCellValueSafe helper
Dim selectedRow As DataGridViewRow = dgvPatients.SelectedRows(0)
Dim patientID As String = GetCellValueSafe(selectedRow, "Patient ID")

' Validate PatientID
If String.IsNullOrWhiteSpace(patientID) Then
	MessageBox.Show("Invalid Patient ID...")
	Return
End If

' Get patient name for display
Dim patientName As String = GetCellValueSafe(selectedRow, "First Name") & " " & _
							 GetCellValueSafe(selectedRow, "Last Name")
```

---

## 📐 DESIGNER LAYOUT (FormPatientManagement.Designer.vb)

### New Panel: panelVitals

```vb
Me.panelVitals = New System.Windows.Forms.Panel()
Me.btnRecordVitals = New System.Windows.Forms.Button()
Me.btnViewVitals = New System.Windows.Forms.Button()

' Panel Configuration
Me.panelVitals.Dock = System.Windows.Forms.DockStyle.Bottom
Me.panelVitals.Location = New System.Drawing.Point(10, 560)
Me.panelVitals.Size = New System.Drawing.Size(800, 70)
```

### Button Styling:

**Record Vitals Button**:
- BackColor: Green (`39, 174, 96`)
- Icon: 📋
- Location: Left side of panel

**View Vitals Button**:
- BackColor: Blue (`52, 152, 219`)
- Icon: 📊
- Location: Right of Record button

### Layout Adjustment:
```vb
' DataGridView adjusted to accommodate vitals panel
Me.dgvPatients.Size = New System.Drawing.Size(800, 480)  ' Was 550
```

---

## 🚀 USAGE WORKFLOW

### Recording Patient Vitals

1. **Navigate** to Patient Management form (click "Patients" button on main dashboard)
2. **Select** a patient from the DataGridView
3. **Click** "📋 Record Vitals" button
4. **Enter** vital signs in the modal dialog:
   - Blood Pressure (optional): e.g., "120/80"
   - Heart Rate: e.g., 72 bpm
   - Temperature: e.g., 37.0 °C
   - SpO2: e.g., 98%
   - Weight: e.g., 70.0 kg
5. **Click** "Save Vitals"
6. **Confirmation** message appears on success

### Viewing Vitals History

1. **Select** a patient from the DataGridView
2. **Click** "📊 View Vitals History" button
3. **Review** complete vitals timeline
4. **Optionally** delete records (with confirmation)
5. **Refresh** to reload latest data

---

## 🛡️ ERROR HANDLING & LOGGING

### All Errors Logged To: `error_log.txt`

**Log Format**:
```
2025-05-13 14:32:15 - ERROR: SavePatientVitals error: Invalid heart rate value: 350
```

### Try-Catch Blocks:
- Database operations
- UI event handlers
- Data validation
- Form initialization

### User-Friendly Messages:
- "Please select a patient from the list to record vitals."
- "The selected patient does not have a valid Patient ID."
- "Blood Pressure must be in format: systolic/diastolic (e.g., 120/80)"
- "Failed to save patient vitals. Please check the error log."

---

## 🧪 TESTING CHECKLIST

### Database Layer
- ✅ Schema created successfully
- ✅ Foreign key constraint enforced
- ✅ Cascade delete works (delete patient → delete vitals)
- ✅ Validation rules enforced
- ✅ Transaction rollback on error

### UI Integration
- ✅ Buttons visible in Patient Management form
- ✅ Modal dialogs open correctly
- ✅ PatientID passed safely between forms
- ✅ DBNull values handled
- ✅ No crashes on empty selection

### Data Entry
- ✅ Numeric ranges enforced
- ✅ Blood pressure format validated
- ✅ Save operation successful
- ✅ Error messages display correctly

### History Viewer
- ✅ DataGridView populated
- ✅ Column formatting correct
- ✅ Delete operation works
- ✅ Refresh updates grid

---

## 📊 SAMPLE DATA

After recording vitals for **PAT-2025-0001 (John Doe)**:

| Vital ID | Blood Pressure | Heart Rate | Temperature | SpO2 | Weight | Date Recorded       |
|----------|----------------|------------|-------------|------|--------|---------------------|
| 1        | 120/80         | 72         | 37.0        | 98   | 70.5   | 2025-05-13 14:30:00 |
| 2        | 118/78         | 68         | 36.8        | 99   | 70.3   | 2025-05-14 09:15:00 |

---

## 🎯 KEY SUCCESS FACTORS

✅ **Strict Typing**: All code compiles under `Option Strict On`  
✅ **Referential Integrity**: Foreign key CASCADE ensures data consistency  
✅ **Thread Safety**: Transactions protect concurrent access  
✅ **Error Logging**: All exceptions captured in `error_log.txt`  
✅ **Input Validation**: Business rules enforced at both UI and DAL  
✅ **Professional UI**: Clean layout with intuitive workflow  
✅ **Safe Data Access**: DBNull and null checks prevent crashes  
✅ **Audit Trail**: DateRecorded automatically set on insert  

---

## 🔗 RELATED FILES

### Modified:
- `ModuleDatabase.vb` (Patient Vitals DAL region added)
- `FormPatientManagement.vb` (Event handlers added)
- `FormPatientManagement.Designer.vb` (Vitals panel and buttons added)

### Created:
- `FormPatientVitals.vb` (Recording dialog)
- `FormPatientVitalsHistory.vb` (History viewer)

---

## 📝 CONCLUSION

The Patient Vitals Tracking feature is now **fully functional** and integrated into the Hospital Appointment System. It follows professional VB.NET WinForms architecture with strict typing, comprehensive error handling, and clean separation of concerns.

**Database**: SQLite with foreign key constraints  
**DAL**: Thread-safe, parameterized queries  
**UI**: Modal dialogs with validation  
**Integration**: Seamless launch from Patient Management  

**Build Status**: ✅ Compiles with 0 errors  
**Runtime Tested**: ✅ Ready for production use

---

*Generated: 2025-05-13*  
*Version: 1.0.0*
