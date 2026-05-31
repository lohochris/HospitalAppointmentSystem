# MediCare Hospital Appointment System
## CSC3226 - Visual Basic .NET Programming
### Group Project

**Group Members:**
- Sa'id Umar
- Aisha Ladan
- Maryam Rabiu

---

## HOW TO RUN (Step-by-Step)

### Prerequisites
- Visual Studio 2019 or 2022 (Community edition is free)
- .NET Framework 4.7.2 (included with VS)
- Windows 10 or later

### Step 1 – Open the Project
1. Extract the project ZIP to any folder (e.g. `C:\Projects\`)
2. Double-click `HospitalAppointmentSystem.sln`
3. Visual Studio will open the solution

### Step 2 – Install the SQLite NuGet Package
1. In Visual Studio, go to **Tools → NuGet Package Manager → Manage NuGet Packages for Solution**
2. Click the **Browse** tab and search for: `System.Data.SQLite`
3. Select **System.Data.SQLite** by the SQLite Development Team
4. Click **Install** → Accept the licence
5. Wait for installation to complete

**Alternative (Package Manager Console):**
```
Tools → NuGet Package Manager → Package Manager Console
PM> Install-Package System.Data.SQLite
```

### Step 3 – Build the Project
1. Press **Ctrl+Shift+B** or go to **Build → Build Solution**
2. Confirm there are **0 errors** in the Error List (warnings are OK)

### Step 4 – Run the Application
1. Press **F5** or click the green **▶ Start** button
2. The Login form will appear
3. The SQLite database (`HospitalDB.db`) is created automatically on first run

---

## DEMO LOGIN CREDENTIALS

| Role         | Username        | Password     |
|--------------|-----------------|--------------|
| Admin        | `admin`         | `Admin@123`  |
| Doctor       | `dr_james`      | `Doctor@123` |
| Doctor       | `dr_fatima`     | `Doctor@123` |
| Receptionist | `receptionist`  | `Recep@123`  |
| Patient      | `patient1`      | `Patient@123`|
| Patient      | `patient2`      | `Patient@123`|

> Click **"View Demo Credentials"** on the login screen for a quick reminder.

---

## PROJECT STRUCTURE

```
HospitalAppointmentSystem/
├── HospitalAppointmentSystem.sln          ← Open this in Visual Studio
└── HospitalAppointmentSystem/
    ├── Program.vb                         ← Application entry point
    ├── ModuleDatabase.vb                  ← All SQLite DB operations (CRUD)
    ├── ModuleValidation.vb                ← Shared validation functions
    ├── ClassModels.vb                     ← Patient, Doctor, Appointment, Queue classes
    ├── FormLogin.vb                       ← Login screen
    ├── FormMain.vb                        ← Main dashboard (role-based)
    ├── FormAppointmentBooking.vb          ← Book/view/cancel appointments
    ├── FormQueueManagement.vb             ← Live queue tracking
    ├── FormOtherForms.vb                  ← Patient portal, Medical records, Doctor schedule
    ├── FormAnalyticsAndSymptoms.vb        ← Admin analytics + Symptom checker
    ├── DatabaseSchema.sql                 ← SQL schema + sample data (reference)
    ├── HospitalAppointmentSystem.vbproj   ← Project file
    └── packages.config                    ← NuGet dependencies
```

---

## FEATURE OVERVIEW

### 🔐 Login System
- Multi-role authentication (Admin, Doctor, Receptionist, Patient)
- Role-based menu and feature access
- Session management via `CurrentUser` global object

### 📅 Appointment Booking
- Department → Doctor → Date → Time slot selection
- Automatic slot availability (30-minute slots, 09:00–17:00, no lunch 13:00–14:00)
- Prevents double-booking (slot conflict detection)
- Emergency priority booking (jumps to front of queue)
- SMS/Email reminder simulation

### 🔢 Queue Management
- Live queue display with custom owner-drawn list
- Emergency appointments highlighted in red
- Call Next Patient / Mark Complete workflow
- Check queue position by ticket number
- Auto-refresh every 15 seconds

### 👤 Patient Portal
- New patient registration with full validation
- View personal appointment history
- View own medical records (read-only for patients)

### 📋 Medical Records
- Doctors add diagnosis, prescription, test results
- Timestamp-stamped entries
- Patients can only view their own records

### 📊 Admin Analytics
- Stat cards: total patients, appointments, emergencies, queue
- Bar chart: Appointments per doctor
- Line chart: Monthly appointment trend
- Export all data to CSV file

### 🩺 Symptom Checker
- Select symptoms from a checklist
- Expert-system logic suggests department and urgency level
- Option to directly open booking form with recommendation

### 🗓️ Doctor Schedule
- View weekly appointment schedule per doctor
- Filter by doctor
- Highlights next 7 days

---

## ASSIGNMENT REQUIREMENTS MAP

| Requirement                        | Location                              |
|------------------------------------|---------------------------------------|
| Variables & Constants              | `ModuleDatabase.vb` – `#Region "Constants"` |
| Conditional statements (If/ElseIf) | All forms – login validation, booking, queue |
| Select Case                        | `FormMain.vb` (roles), `ClassModels.vb` (status), `FormSymptomChecker.vb` |
| For Each loop                      | `ModuleDatabase.vb` – `GetAvailableSlots`, `InsertSampleData` |
| Do While loop                      | `ModuleDatabase.vb` – `GetAvailableSlots` slot generation |
| Functions returning values         | `ModuleValidation.vb` – `CalculateAge`, `SuggestDepartment` |
| Sub procedures                     | `FormMain.vb` – `RefreshDashboard`, `CheckTomorrowReminders` |
| List(Of T) collection              | `FormAppointmentBooking.vb` – `_departments`, `_patients` |
| Dictionary(Of K,V) collection      | `ModuleDatabase.vb` – `GetAnalyticsSummary` |
| Try-Catch-Finally                  | Every form load and button click |
| File handling (write)              | `ModuleDatabase.vb` – `LogError`, `ExportAnalyticsToCSV` |
| SQLite database                    | `ModuleDatabase.vb` – all DB operations |
| Full CRUD                          | Patients, Appointments, Queue, Medical Records |
| Input validation                   | `ModuleValidation.vb` – email, phone, password, dates |
| MessageBox types                   | Error, Warning, Information, Question used throughout |
| Double-booking prevention          | `ModuleDatabase.vb` – `IsSlotTaken()` |
| Multi-role dashboards              | `FormMain.vb` – `ApplyRolePermissions()` |
| Queue management                   | `FormQueueManagement.vb` |
| Emergency priority                 | `ModuleDatabase.vb` – `AddToQueue()` |
| Analytics charts                   | `FormAdminAnalytics.vb` |
| Symptom checker (AI)               | `FormSymptomChecker.vb` + `ModuleValidation.vb` |
| SMS/Email simulation               | `FormAppointmentBooking.vb` – post-booking dialog |
| Reminder check on startup          | `FormMain.vb` – `CheckTomorrowReminders()` |

---

## ERROR HANDLING DEMONSTRATION

The following edge cases are handled (try them!):

1. **Past date booking** → Error: "Appointment date cannot be in the past"
2. **Double booking** → Error: "This slot is already booked"
3. **Weekend booking** → Warning: "Doctors do not work on weekends"
4. **Invalid email format** → Validation error on patient registration
5. **Invalid phone number** → Must be 11-digit Nigerian format (e.g. 08012345678)
6. **Weak password** → Must have 8+ chars, 1 uppercase, 1 digit
7. **Empty required fields** → Highlighted with descriptive message
8. **Role access control** → Patient cannot access Admin menu
9. **Cancel completed appointment** → Warning: "Cannot cancel a completed appointment"
10. **All errors logged** → Check `error_log.txt` in the `bin\Debug\` folder

---

## DATABASE TABLES

| Table             | Purpose                              |
|-------------------|--------------------------------------|
| Users             | Login accounts for all roles         |
| Departments       | Hospital departments                 |
| Doctors           | Doctor profiles and schedules        |
| Patients          | Patient demographic records          |
| Appointments      | All bookings                         |
| Queue             | Daily patient queue                  |
| MedicalRecords    | Diagnosis, prescriptions, test results |
| EmergencyRequests | Emergency request tracking           |

The database file (`HospitalDB.db`) is auto-created in `bin\Debug\` on first run.

---

## TROUBLESHOOTING

| Problem | Solution |
|---------|----------|
| Build error: `System.Data.SQLite not found` | Install NuGet package (Step 2 above) |
| Build error: `System.Windows.Forms.DataVisualization not found` | Right-click References → Add Reference → Assemblies → search "Chart" |
| Login form appears blank / crash | Check error_log.txt in bin\Debug\ |
| Database not created | Ensure bin\Debug\ folder is writable |
| Charts not showing | Add reference: `System.Windows.Forms.DataVisualization` |

---

*CSC3226 Group Project | MediCare Hospital Appointment System*
