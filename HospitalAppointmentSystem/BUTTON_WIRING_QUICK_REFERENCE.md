# 🔧 BUTTON WIRING QUICK REFERENCE

**Purpose:** Step-by-step guide for wiring the new telehealth launch button  
**Target File:** FormMain.vb  
**Method:** `btnLaunchTelehealth_Click` (already implemented)

---

## ⚡ QUICK START: WIRE UP THE BUTTON

### **Option 1: Wire Up Existing Button** (30 seconds)

If you already have a button named `btnLaunchTelehealth` on your FormMain:

1. **Open FormMain.vb** in Designer (Shift+F7)
2. **Double-click the button** in the designer
3. Visual Studio will generate:
   ```vb
   Private Sub btnLaunchTelehealth_Click(sender As Object, e As EventArgs) Handles btnLaunchTelehealth.Click
   ```
4. **The method is already implemented!** (lines ~746-850 in FormMain.vb)
5. Visual Studio will automatically connect them

---

### **Option 2: Add New Button to Dashboard** (2 minutes)

If you don't have the button yet:

#### **Step 1: Add Button in Designer**
1. Open **FormMain.vb** in Designer (View → Designer or Shift+F7)
2. Drag a **Button** from Toolbox onto left sidebar panel
3. Position it below the existing `btnTelemedicine` button

#### **Step 2: Set Button Properties**
In Properties Window (F4), set:

| Property | Value |
|----------|-------|
| **(Name)** | `btnLaunchTelehealth` |
| **Text** | `📞 Launch Telehealth` |
| **Size** | `120, 35` |
| **Location** | `20, 420` (adjust to fit) |
| **BackColor** | `0, 120, 212` (Medical Blue) |
| **ForeColor** | `White` |
| **FlatStyle** | `Flat` |
| **Font** | `Arial, 10pt, Bold` |
| **Cursor** | `Hand` |

#### **Step 3: Wire Event Handler**
1. **Double-click the button** in Designer
2. Visual Studio creates the handler shell
3. **The implementation already exists** in FormMain.vb (~line 746)
4. Done! ✅

---

## 📋 PROGRAMMATIC BUTTON CREATION (ALTERNATIVE)

If you prefer to create the button entirely in code, add this to FormMain.vb:

```vb
' Add to class-level declarations:
Private btnLaunchTelehealth As Button

' Add to FormMain_Load or after InitializeComponent():
Private Sub InitializeTelehealthButton()
	btnLaunchTelehealth = New Button With {
		.Name = "btnLaunchTelehealth",
		.Text = "📞 Launch Telehealth",
		.Size = New Size(120, 35),
		.Location = New Point(20, 420),
		.BackColor = Color.FromArgb(0, 120, 212),
		.ForeColor = Color.White,
		.FlatStyle = FlatStyle.Flat,
		.Font = New Font("Arial", 10, FontStyle.Bold),
		.Cursor = Cursors.Hand,
		.TabIndex = 7
	}

	btnLaunchTelehealth.FlatAppearance.BorderSize = 0

	' Wire event handler
	AddHandler btnLaunchTelehealth.Click, AddressOf btnLaunchTelehealth_Click

	' Add to form (or sidebar panel)
	Me.Controls.Add(btnLaunchTelehealth)
	' OR: pnlSidebar.Controls.Add(btnLaunchTelehealth)
End Sub
```

Then call `InitializeTelehealthButton()` from `FormMain_Load`.

---

## 🎨 VISUAL LAYOUT

```
┌─────────────────────────────────────────┐
│  MediCare HMS Dashboard                 │
├─────────────┬───────────────────────────┤
│             │                           │
│ [📊 Analytics] │  DataGridView Queue    │
│ [🩺 Symptom]   │                        │
│ [📅 Appts]     │  (Appointment rows...) │
│ [👥 Patients]  │                        │
│ [👨‍⚕️ Doctors]   │                        │
│             │                           │
│ [🎥 Tele]   │ ← Existing (legacy)       │
│ [📞 Launch] │ ← NEW BUTTON HERE!        │
│             │                           │
│ [🚪 Logout] │                           │
└─────────────┴───────────────────────────┘
```

---

## 🧪 TESTING THE BUTTON

### **Test 1: No Selection**
1. Run application (F5)
2. Login as Doctor or Admin
3. Click **[📞 Launch Telehealth]** without selecting a row
4. **Expected:** MessageBox: "Please select an active patient appointment..."

### **Test 2: Valid Selection**
1. Select an appointment row
2. Click **[📞 Launch Telehealth]**
3. **Expected:** Telemedicine form opens
4. **Expected:** Status bar shows: "Active Session - [Patient Name]"
5. **Expected:** WebView2 navigates to: `https://meet.jit.si/medicare-hms-room-APT2026001`

### **Test 3: Invalid Data**
1. Select a row with missing AppointmentID
2. Click **[📞 Launch Telehealth]**
3. **Expected:** MessageBox: "The selected appointment does not contain valid patient or appointment data."

---

## ⌨️ OPTIONAL: KEYBOARD SHORTCUT (Ctrl+T)

Add to FormMain.vb:

```vb
' Add event handler:
Private Sub FormMain_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
	If e.Control AndAlso e.KeyCode = Keys.T Then
		e.Handled = True
		e.SuppressKeyPress = True
		btnLaunchTelehealth_Click(Me, EventArgs.Empty)
	End If
End Sub
```

Then set form property: **KeyPreview = True**

---

## 🖱️ OPTIONAL: CONTEXT MENU (Right-Click)

Add right-click menu to DataGridView:

```vb
' Create ContextMenuStrip in Designer, add menu item "Launch Video Consultation"
Private Sub MenuLaunchTelehealth_Click(sender As Object, e As EventArgs)
	btnLaunchTelehealth_Click(sender, e)
End Sub
```

Assign to DataGridView: `dgvDashboardData.ContextMenuStrip = cmsAppointmentActions`

---

## 🖱️ OPTIONAL: DOUBLE-CLICK ROW LAUNCH

```vb
Private Sub dgvDashboardData_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvDashboardData.CellDoubleClick
	Try
		If e.RowIndex < 0 Then Return  ' Ignore header clicks
		btnLaunchTelehealth_Click(sender, EventArgs.Empty)
	Catch ex As Exception
		LogError($"dgvDashboardData_CellDoubleClick error: {ex.Message}")
	End Try
End Sub
```

---

## 🐛 TROUBLESHOOTING

### **Button Click Does Nothing**
- Check Handles clause: `Handles btnLaunchTelehealth.Click`
- Verify button name matches: `btnLaunchTelehealth`
- Rebuild solution (Build → Rebuild Solution)

### **"No Patient Selected" Always Shows**
- Check DataGridView SelectionMode: `FullRowSelect`
- Verify you're clicking a row, not empty space

### **"Invalid Appointment Data" Error**
- Verify column names: "Appointment ID" and "Patient Name"
- Check database query includes these columns
- Verify data is not DBNull

### **Button Not Visible**
- Check Location property (may be off-screen)
- Verify Visible = True
- Check parent container (form vs panel)

---

## 📚 RELATED DOCUMENTATION

- **Implementation Guide:** `TELEMEDICINE_UI_FIX_IMPLEMENTATION.md`
- **Testing Checklist:** `QUICK_TEST_TELEMEDICINE_FIX.md`
- **Visual Reference:** `TELEMEDICINE_VISUAL_REFERENCE.md`

---

**END OF BUTTON WIRING GUIDE**

✅ **The button handler is already implemented in FormMain.vb (lines ~746-850)**  
✅ **Just wire it up in Designer by double-clicking the button!**
