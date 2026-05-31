# 🖼️ PATIENT MANAGEMENT UI - VISUAL REFERENCE

## Form Layout Overview (1200x700px)

```
┌──────────────────────────────────────────────────────────────────────────┐
│  PATIENT MANAGEMENT                                              [✕]     │  ← Blue Header (60px)
├─────────────────────────┬────────────────────────────────────────────────┤
│  PATIENT INFORMATION    │  🔍 Search (Name or Phone):  [____________]    │
│  ┌─────────────────────┐│                                                │
│  │ Patient ID:         ││                                                │
│  │ [Auto-Generated]    ││  ┌──────────────────────────────────────────┐ │
│  │                     ││  │ Patient ID │ First Name │ Last Name ... │ │
│  │ First Name:*        ││  ├────────────┼────────────┼──────────────...│ │
│  │ [_______________]   ││  │ PAT-2026-1 │ John       │ Doe          ... │ │
│  │                     ││  │ PAT-2026-2 │ Jane       │ Smith        ... │ │
│  │ Last Name:*         ││  │ PAT-2026-3 │ Mike       │ Johnson      ... │ │
│  │ [_______________]   ││  │ ...        │ ...        │ ...          ... │ │
│  │                     ││  └──────────────────────────────────────────┘ │
│  │ Date of Birth:      ││                                                │
│  │ [MM/DD/YYYY  ▼]     ││                                                │
│  │                     ││                Total Patients: 25 ←────────────┤
│  │ Gender:             ││                                                │
│  │ [Male       ▼]      ││                                                │
│  │                     ││                                                │
│  │ Phone Number:*      ││                                                │
│  │ [_______________]   ││                                                │
│  │                     ││                                                │
│  │ Email:              ││                                                │
│  │ [_______________]   ││                                                │
│  └─────────────────────┘│                                                │
│                         │                                                │
│  [💾 Save] [🔄 Clear] [🗑 Delete]                                       │
│  └─────────────────────┘                                                │
│     380px               │            820px                               │
└─────────────────────────┴────────────────────────────────────────────────┘
```

---

## Color Palette

| Component | Color | Hex Code | RGB |
|-----------|-------|----------|-----|
| **Header Background** | Primary Blue | #2980B9 | rgb(41, 128, 185) |
| **Header Text** | White | #FFFFFF | rgb(255, 255, 255) |
| **Save Button** | Success Green | #27AE60 | rgb(39, 174, 96) |
| **Clear Button** | Neutral Gray | #95A5A6 | rgb(149, 165, 166) |
| **Delete Button** | Danger Red | #E74C3C | rgb(231, 76, 60) |
| **Close Button** | Danger Red | #E74C3C | rgb(231, 76, 60) |
| **Grid Header** | Primary Blue | #2980B9 | rgb(41, 128, 185) |
| **Grid Selection** | Selection Blue | #3498DB | rgb(52, 152, 219) |
| **Alternating Rows** | Light Gray | #ECF0F1 | rgb(236, 240, 241) |
| **Background** | White | #FFFFFF | rgb(255, 255, 255) |
| **Required Field** | Danger Red | #E74C3C | rgb(231, 76, 60) |

---

## Typography Specification

| Element | Font | Size | Weight | Color |
|---------|------|------|--------|-------|
| Form Title | Segoe UI | 18pt | Bold | White |
| Group Box Header | Segoe UI | 10pt | Bold | Black |
| Labels | Segoe UI | 9pt | Regular | Black |
| Required Asterisk (*) | Segoe UI | 9pt | Regular | Red (#E74C3C) |
| Input Fields | Segoe UI | 10pt | Regular | Black |
| Patient ID Field | Segoe UI | 10pt | Bold | Black |
| Buttons | Segoe UI | 10pt | Bold | White |
| Grid Headers | Segoe UI | 10pt | Bold | White |
| Grid Cells | Segoe UI | 9pt | Regular | Black |
| Search Label | Segoe UI | 10pt | Bold | Black |
| Record Count | Segoe UI | 10pt | Bold | Black |

---

## Control Specifications

### Input Fields

| Control | Name | Width | Max Length | Validation |
|---------|------|-------|------------|------------|
| Patient ID | `txtPatientID` | 320px | N/A | Read-only, Auto-generated |
| First Name | `txtFirstName` | 320px | 100 | Required, Not empty |
| Last Name | `txtLastName` | 320px | 100 | Required, Not empty |
| Date of Birth | `dtpDOB` | 320px | N/A | Must be past date |
| Gender | `cmbGender` | 320px | N/A | Dropdown selection |
| Phone Number | `txtPhone` | 320px | 15 | Required, Min 10 digits, Digits only |
| Email | `txtEmail` | 320px | Unlimited | Optional, Valid format if provided |
| Search | `txtSearch` | 400px | Unlimited | Real-time filter |

### Buttons

| Button | Name | Width | Height | Color | Icon | Function |
|--------|------|-------|--------|-------|------|----------|
| Save | `btnSave` | 100px | 45px | Green (#27AE60) | 💾 | Create/Update patient |
| Clear | `btnClear` | 100px | 45px | Gray (#95A5A6) | 🔄 | Reset form |
| Delete | `btnDelete` | 100px | 45px | Red (#E74C3C) | 🗑 | Delete patient |
| Close | `btnClose` | 48px | 40px | Red (#E74C3C) | ✕ | Close form |

### DataGridView

| Property | Value | Description |
|----------|-------|-------------|
| Size | 800px × 550px | Full right panel |
| Selection Mode | FullRowSelect | Click row to select |
| Read Only | True | No direct editing |
| Multi Select | False | Single row only |
| Auto Size Columns | Fill | Columns auto-resize |
| Row Height | 35px | Comfortable spacing |
| Header Height | 40px | Larger header |
| Alternating Rows | Light Gray | Better readability |

---

## Spacing & Layout

### Panel Dimensions:
- **Top Panel (Header)**: 1200px × 60px
- **Left Panel (Input)**: 380px × 640px
- **Right Panel (Grid)**: 820px × 640px
- **Search Panel**: 800px × 70px
- **Buttons Panel**: 360px × 80px

### Margins & Padding:
- **Form Padding**: 0px (borderless)
- **Panel Padding**: 10px
- **GroupBox Padding**: 10px
- **Label-to-Control Spacing**: 5px
- **Control-to-Control Spacing**: 25px
- **Button Spacing**: 15px horizontal gap

---

## Interaction States

### Button States:

**Save Button:**
- **Normal State**: Text = "💾 Save", Color = Green
- **Edit Mode**: Text = "💾 Update", Color = Green

**Delete Button:**
- **Normal State**: Enabled = False, Color = Red (dimmed)
- **Edit Mode**: Enabled = True, Color = Red (bright)

### Form Modes:

**New Patient Mode (Default):**
- Patient ID = "[Auto-Generated]"
- All fields empty
- Save button = "💾 Save"
- Delete button = Disabled

**Edit Patient Mode:**
- Patient ID = "PAT-YYYY-NNNN"
- Fields populated from grid selection
- Save button = "💾 Update"
- Delete button = Enabled

---

## Grid Column Configuration

| Column Header | Data Type | Width | Alignment | Format |
|---------------|-----------|-------|-----------|--------|
| Patient ID | Text | Auto-fill | Left | PAT-YYYY-NNNN |
| First Name | Text | Auto-fill | Left | Title case |
| Last Name | Text | Auto-fill | Left | Title case |
| Date of Birth | Date | Auto-fill | Left | YYYY-MM-DD |
| Gender | Text | Auto-fill | Left | Male/Female/Other |
| Phone Number | Text | Auto-fill | Left | Numeric string |
| Email | Text | Auto-fill | Left | email@domain.com |
| Registered On | DateTime | Auto-fill | Left | YYYY-MM-DD HH:mm:ss |

---

## Validation Visual Feedback

### Required Field Indicators:
- Label text color: **Red (#E74C3C)**
- Asterisk (*) suffix
- Example: "First Name:*" in red

### Validation Error Messages:
```
┌────────────────────────────────────────┐
│              Validation Error          │
├────────────────────────────────────────┤
│  ⚠️  First Name is required.          │
│                                        │
│              [    OK    ]              │
└────────────────────────────────────────┘
```

### Success Messages:
```
┌────────────────────────────────────────┐
│                Success                 │
├────────────────────────────────────────┤
│  ✅  Patient registered successfully!  │
│                                        │
│              [    OK    ]              │
└────────────────────────────────────────┘
```

### Delete Confirmation:
```
┌────────────────────────────────────────┐
│           Confirm Deletion             │
├────────────────────────────────────────┤
│  Are you sure you want to delete      │
│  patient 'John Doe'?                   │
│                                        │
│  This action cannot be undone!         │
│                                        │
│         [ Yes ]      [ No ]            │
└────────────────────────────────────────┘
```

---

## Keyboard Navigation

| Key | Action |
|-----|--------|
| **Tab** | Move to next input field |
| **Shift+Tab** | Move to previous input field |
| **Enter** | Activate focused button |
| **Escape** | Clear form (when not in text input) |
| **Ctrl+S** | Save patient (if implemented) |
| **Ctrl+F** | Focus search box (if implemented) |

---

## Mouse Interactions

| Action | Result |
|--------|--------|
| **Click Grid Row** | Load patient into form (Edit Mode) |
| **Click Save Button** | Validate and save/update patient |
| **Click Clear Button** | Reset form to initial state |
| **Click Delete Button** | Show confirmation, delete if Yes |
| **Click Close Button** | Close form |
| **Type in Search** | Filter grid in real-time |

---

## Responsive Behavior

### Form Size:
- **Fixed**: 1200px × 700px (no resizing)
- **Position**: Center of screen on load

### Grid Behavior:
- **Auto-fill columns**: Distributes width evenly
- **Scrollbar**: Vertical scroll if > ~15 patients
- **No horizontal scroll**: All columns fit

### Text Field Behavior:
- **Max length enforced**: Prevents over-entry
- **Auto-trim**: Whitespace removed on save
- **Phone field**: Blocks non-numeric input

---

## Accessibility Features

✅ **High Contrast Colors**: Blue/White, Red/White  
✅ **Clear Labels**: All inputs labeled  
✅ **Required Field Markers**: Red asterisks  
✅ **Large Buttons**: 45px height (easy to click)  
✅ **Readable Fonts**: Segoe UI, 9-18pt  
✅ **Error Messages**: Clear, user-friendly  
✅ **Confirmation Dialogs**: Prevent accidental deletes  

---

## Performance Characteristics

- **Load Time**: < 1 second (for 1000 records)
- **Search Response**: Real-time (< 100ms)
- **Save Operation**: < 500ms
- **Delete Operation**: < 300ms
- **Grid Refresh**: < 200ms

---

*This visual reference complements the technical documentation and provides a complete picture of the Patient Management UI design.*
