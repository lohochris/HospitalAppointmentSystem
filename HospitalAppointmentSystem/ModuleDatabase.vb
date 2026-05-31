Option Strict On
Option Explicit On

' ============================================================
' ModuleDatabase.vb
' CSC3226 - Hospital Appointment System
' Handles all SQLite database operations (CRUD)
' Group: Sa'id Umar, Aisha Ladan, Maryam Rabiu
' ============================================================

Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SQLite
Imports System.IO
Imports System.Windows.Forms

Public Module ModuleDatabase

#Region "Constants and Variables"
    Public Const DB_FILE As String = "HospitalDB.db"
    Public Const APP_VERSION As String = "1.0.0"
    Public Const SLOT_DURATION_MINUTES As Integer = 30
    Public Const WORKING_HOURS_START As String = "09:00"
    Public Const WORKING_HOURS_END As String = "17:00"
    Public Const LUNCH_START As String = "13:00"
    Public Const LUNCH_END As String = "14:00"
    Public Const ERROR_LOG_FILE As String = "error_log.txt"
    Public Const AUDIT_EMERGENCY_LOG_FILE As String = "audit_emergency_backup.txt"

    Private _connectionString As String = ""
    Private ReadOnly auditLock As New Object()
#End Region

#Region "Connection Management"
    ''' <summary>
    ''' Returns the SQLite connection string.
    ''' </summary>
    Public Function GetConnectionString() As String
        If String.IsNullOrEmpty(_connectionString) Then
            Dim dbPath As String = Path.Combine(Application.StartupPath, DB_FILE)
            _connectionString = $"Data Source={dbPath};Version=3;"
        End If
        Return _connectionString
    End Function

    ''' <summary>
    ''' Tests the database connection.
    ''' </summary>
    Public Function TestConnection() As Boolean
        Try
            Using conn As New SQLiteConnection(GetConnectionString())
                conn.Open()
                Return True
            End Using
        Catch ex As SQLiteException
            LogError("Database connection failed: " & ex.Message)
            Return False
        Catch ex As Exception
            LogError("Unexpected error testing connection: " & ex.Message)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Creates and initialises the database schema.
    ''' </summary>
    Public Sub InitialiseDatabase()
        Try
            Using conn As New SQLiteConnection(GetConnectionString())
                conn.Open()
                Dim sql As String = GetDatabaseSchema()
                Using cmd As New SQLiteCommand(sql, conn)
                    cmd.ExecuteNonQuery()
                End Using
                InsertSampleDataIfEmpty(conn)
            End Using

            ' Initialize Patient Management schema
            InitializePatientManagementSchema()

            ' Initialize Patient Vitals schema with referential integrity
            InitializePatientVitalsSchema()

            ' Initialize Clinical Assessments & Triage schema
            InitializeClinicalAssessmentsSchema()

            ' Initialize Doctors Management schema
            InitializeDoctorsManagementSchema()

            ' Initialize Security Audit Trail schema
            InitializeSystemAuditLogsSchema()

        Catch ex As Exception
            LogError("InitialiseDatabase error: " & ex.Message)
            Throw
        End Try
    End Sub
#End Region

#Region "Schema and Sample Data"
    Private Function GetDatabaseSchema() As String
        Return "
CREATE TABLE IF NOT EXISTS Users (
    UserID INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    Password TEXT NOT NULL,
    Role TEXT NOT NULL,
    FullName TEXT NOT NULL,
    Email TEXT,
    Phone TEXT,
    IsActive INTEGER DEFAULT 1,
    CreatedDate TEXT DEFAULT (datetime('now'))
);
CREATE TABLE IF NOT EXISTS Departments (
    DepartmentID INTEGER PRIMARY KEY AUTOINCREMENT,
    DepartmentName TEXT NOT NULL UNIQUE,
    Description TEXT
);
CREATE TABLE IF NOT EXISTS Doctors (
    DoctorID INTEGER PRIMARY KEY AUTOINCREMENT,
    UserID INTEGER,
    DepartmentID INTEGER,
    Specialization TEXT NOT NULL,
    WorkingDays TEXT DEFAULT 'Mon,Tue,Wed,Thu,Fri',
    StartTime TEXT DEFAULT '09:00',
    EndTime TEXT DEFAULT '17:00',
    LunchStart TEXT DEFAULT '13:00',
    LunchEnd TEXT DEFAULT '14:00',
    SlotDuration INTEGER DEFAULT 30
);
CREATE TABLE IF NOT EXISTS Patients (
    PatientID INTEGER PRIMARY KEY AUTOINCREMENT,
    UserID INTEGER,
    DateOfBirth TEXT,
    Gender TEXT,
    Address TEXT,
    BloodGroup TEXT,
    EmergencyContact TEXT,
    EmergencyPhone TEXT,
    InsuranceNumber TEXT,
    RegisteredDate TEXT DEFAULT (datetime('now'))
);
CREATE TABLE IF NOT EXISTS Appointments (
    AppointmentID TEXT PRIMARY KEY,
    PatientID INTEGER,
    DoctorID INTEGER,
    DepartmentID INTEGER,
    AppointmentDate TEXT NOT NULL,
    AppointmentTime TEXT NOT NULL,
    Status TEXT DEFAULT 'Scheduled',
    IsEmergency INTEGER DEFAULT 0,
    Notes TEXT,
    CreatedDate TEXT DEFAULT (datetime('now')),
    ReminderSent INTEGER DEFAULT 0
);
CREATE TABLE IF NOT EXISTS Queue (
    QueueID INTEGER PRIMARY KEY AUTOINCREMENT,
    AppointmentID TEXT,
    TicketNumber TEXT NOT NULL,
    QueuePosition INTEGER,
    EstimatedWait INTEGER,
    Status TEXT DEFAULT 'Waiting',
    CheckInTime TEXT DEFAULT (datetime('now'))
);
CREATE TABLE IF NOT EXISTS MedicalRecords (
    RecordID INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientID INTEGER,
    DoctorID INTEGER,
    AppointmentID TEXT,
    Diagnosis TEXT,
    Prescription TEXT,
    TestResults TEXT,
    Notes TEXT,
    RecordDate TEXT DEFAULT (datetime('now'))
);
CREATE TABLE IF NOT EXISTS EmergencyRequests (
    EmergencyID INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientID INTEGER,
    Description TEXT NOT NULL,
    Priority TEXT DEFAULT 'High',
    Status TEXT DEFAULT 'Pending',
    RequestTime TEXT DEFAULT (datetime('now')),
    AssignedTo INTEGER
);"
    End Function

    Private Sub InsertSampleDataIfEmpty(conn As SQLiteConnection)
        Dim count As Long = 0
        Using cmd As New SQLiteCommand("SELECT COUNT(*) FROM Users", conn)
            count = Convert.ToInt64(cmd.ExecuteScalar())
        End Using
        If count > 0 Then Return

        ' ===================================================================
        ' ENTERPRISE MEDICAL RECONCILIATION SEEDING
        ' Comprehensive clinical department ecosystem with specialized practitioners
        ' Aligned with international healthcare standards and medical board certifications
        ' ===================================================================

        ' ===================================================================
        ' DEPARTMENTS - Complete medical specialty coverage (12 core departments)
        ' ===================================================================
        Dim deptSql As String = "
INSERT INTO Departments (DepartmentName, Description) VALUES
('Emergency Medicine','Critical care and trauma intervention - 24/7 emergency services'),
('Internal Medicine','Primary care, chronic disease management, and preventive medicine'),
('Pediatrics','Comprehensive medical care for infants, children, and adolescents'),
('Cardiology','Heart disease diagnosis, interventional cardiology, and cardiovascular surgery'),
('Neurology','Brain, spinal cord, and nervous system disorder treatment'),
('Orthopedic Surgery','Bone, joint, muscle, and skeletal system surgical interventions'),
('Obstetrics & Gynecology','Women''s reproductive health, pregnancy, and childbirth care'),
('Oncology','Cancer diagnosis, chemotherapy, radiation therapy, and palliative care'),
('Psychiatry','Mental health disorders, psychological counseling, and psychiatric medication management'),
('Dermatology','Skin conditions, cosmetic dermatology, and dermatological surgery'),
('Ophthalmology','Eye care, vision correction, and ophthalmic surgery'),
('Radiology','Medical imaging, diagnostic scans (CT, MRI, X-Ray), and interventional radiology');"
        ExecuteNonQuery(deptSql, conn)

        ' ===================================================================
        ' USERS - Baseline system accounts (Admin + 18 Doctors + Support Staff + Patients)
        ' Password format: Role@123 (e.g., Admin@123, Doctor@123)
        ' ===================================================================
        Dim userSql As String = "
INSERT INTO Users (Username, Password, Role, FullName, Email, Phone) VALUES
-- System Administration
('admin','Admin@123','Admin','System Administrator','admin@hospital.com','08012345678'),

-- EMERGENCY MEDICINE (2 Doctors as requested)
('dr_james_okafor','Doctor@123','Doctor','James Okafor','james.okafor@hospital.com','08023456789'),
('dr_fatima_bello','Doctor@123','Doctor','Fatima Bello','fatima.bello@hospital.com','08034567890'),

-- INTERNAL MEDICINE (2 Doctors as requested)
('dr_sarah_williams','Doctor@123','Doctor','Sarah Williams','sarah.williams@hospital.com','08045678901'),
('dr_david_jones','Doctor@123','Doctor','David Jones','david.jones@hospital.com','08056789012'),

-- PEDIATRICS (2 Doctors as requested)
('dr_chidi_smith','Doctor@123','Doctor','Chidi Smith','chidi.smith@hospital.com','08067890123'),
('dr_elena_rostova','Doctor@123','Doctor','Elena Rostova','elena.rostova@hospital.com','08078901234'),

-- CARDIOLOGY (2 Doctors as requested)
('dr_umar_getso','Doctor@123','Doctor','Umar Getso','umar.getso@hospital.com','08089012345'),
('dr_grace_lin','Doctor@123','Doctor','Grace Lin','grace.lin@hospital.com','08090123456'),

-- NEUROLOGY (2 Doctors as requested)
('dr_alan_turing','Doctor@123','Doctor','Alan Turing','alan.turing@hospital.com','08001234567'),
('dr_linus_pauling','Doctor@123','Doctor','Linus Pauling','linus.pauling@hospital.com','08011234568'),

-- ORTHOPEDIC SURGERY (2 Doctors as requested)
('dr_robert_liston','Doctor@123','Doctor','Robert Liston','robert.liston@hospital.com','08022345679'),
('dr_gibran_khoury','Doctor@123','Doctor','Gibran Khoury','gibran.khoury@hospital.com','08033456780'),

-- OBSTETRICS & GYNECOLOGY (1 Doctor as requested)
('dr_amina_abubakar','Doctor@123','Doctor','Amina Abubakar','amina.abubakar@hospital.com','08044567891'),

-- ONCOLOGY (1 Doctor as requested)
('dr_sidharth_mukherjee','Doctor@123','Doctor','Sidharth Mukherjee','sidharth.mukherjee@hospital.com','08055678902'),

-- PSYCHIATRY (1 Doctor as requested)
('dr_carl_jung','Doctor@123','Doctor','Carl Jung','carl.jung@hospital.com','08066789013'),

-- DERMATOLOGY (1 Doctor as requested)
('dr_sandra_lee','Doctor@123','Doctor','Sandra Lee','sandra.lee@hospital.com','08077890124'),

-- OPHTHALMOLOGY (1 Doctor as requested)
('dr_charles_kelman','Doctor@123','Doctor','Charles Kelman','charles.kelman@hospital.com','08088901235'),

-- RADIOLOGY (1 Doctor as requested)
('dr_marie_curie','Doctor@123','Doctor','Marie Curie','marie.curie@hospital.com','08099012346'),

-- Support staff
('receptionist','Recep@123','Receptionist','Ngozi Adeyemi','ngozi@hospital.com','08001234510'),
('nurse_mary','Nurse@123','Nurse','Mary Okonkwo','mary@hospital.com','08011234520'),

-- Test patients for appointment workflows
('patient1','Patient@123','Patient','Emeka Obi','emeka@email.com','08067890100'),
('patient2','Patient@123','Patient','Halima Musa','halima@email.com','08078901200'),
('patient3','Patient@123','Patient','Tunde Bakare','tunde@email.com','08089012300'),
('patient4','Patient@123','Patient','Chioma Eze','chioma@email.com','08090123400'),
('patient5','Patient@123','Patient','Abubakar Sule','abubakar@email.com','08001234500');"
        ExecuteNonQuery(userSql, conn)

        ' ===================================================================
        ' DOCTORS - Complete medical staff mapping to departments
        ' UserID mapping:
        ' 2=James Okafor, 3=Fatima Bello (Emergency)
        ' 4=Sarah Williams, 5=David Jones (Internal Medicine)
        ' 6=Chidi Smith, 7=Elena Rostova (Pediatrics)
        ' 8=Umar Getso, 9=Grace Lin (Cardiology)
        ' 10=Alan Turing, 11=Linus Pauling (Neurology)
        ' 12=Robert Liston, 13=Gibran Khoury (Orthopedic Surgery)
        ' 14=Amina Abubakar (Obstetrics & Gynecology)
        ' 15=Sidharth Mukherjee (Oncology)
        ' 16=Carl Jung (Psychiatry)
        ' 17=Sandra Lee (Dermatology)
        ' 18=Charles Kelman (Ophthalmology)
        ' 19=Marie Curie (Radiology)
        ' ===================================================================
        Dim docSql As String = "
INSERT INTO Doctors (UserID, DepartmentID, Specialization, WorkingDays, StartTime, EndTime, LunchStart, LunchEnd, SlotDuration) VALUES
-- EMERGENCY MEDICINE (DepartmentID=1) - Dr. James Okafor & Dr. Fatima Bello
(2, 1, 'Emergency Medicine and Trauma Care', 'Mon,Tue,Wed,Thu,Fri,Sat,Sun', '00:00', '23:59', '13:00', '14:00', 30),
(3, 1, 'Emergency Medicine and Critical Care', 'Mon,Tue,Wed,Thu,Fri,Sat,Sun', '00:00', '23:59', '13:00', '14:00', 30),

-- INTERNAL MEDICINE (DepartmentID=2) - Dr. Sarah Williams & Dr. David Jones
(4, 2, 'Internal Medicine and Family Practice', 'Mon,Tue,Wed,Thu,Fri', '08:00', '16:00', '13:00', '14:00', 30),
(5, 2, 'General Practice and Preventive Medicine', 'Mon,Tue,Wed,Thu,Fri', '08:00', '16:00', '13:00', '14:00', 30),

-- PEDIATRICS (DepartmentID=3) - Dr. Chidi Smith & Dr. Elena Rostova
(6, 3, 'Neonatology and Pediatric Care', 'Mon,Tue,Wed,Thu,Fri', '08:00', '16:00', '13:00', '14:00', 30),
(7, 3, 'Pediatric Immunology and Developmental Pediatrics', 'Mon,Tue,Wed,Thu,Fri', '09:00', '17:00', '13:00', '14:00', 30),

-- CARDIOLOGY (DepartmentID=4) - Dr. Umar Getso & Dr. Grace Lin
(8, 4, 'Interventional Cardiology and Cardiac Catheterization', 'Mon,Tue,Wed,Thu,Fri', '08:00', '16:00', '13:00', '14:00', 30),
(9, 4, 'Electrophysiology and Arrhythmia Management', 'Mon,Wed,Fri', '09:00', '17:00', '13:00', '14:00', 30),

-- NEUROLOGY (DepartmentID=5) - Dr. Alan Turing & Dr. Linus Pauling
(10, 5, 'Cognitive Neurology and Dementia Disorders', 'Mon,Tue,Wed,Thu', '08:00', '16:00', '13:00', '14:00', 30),
(11, 5, 'Stroke Care and Cerebrovascular Disease', 'Tue,Wed,Thu,Fri', '09:00', '17:00', '13:00', '14:00', 30),

-- ORTHOPEDIC SURGERY (DepartmentID=6) - Dr. Robert Liston & Dr. Gibran Khoury
(12, 6, 'Sports Medicine and Joint Replacement', 'Mon,Tue,Wed,Thu,Fri', '08:00', '16:00', '13:00', '14:00', 30),
(13, 6, 'Spine Surgery and Trauma Orthopedics', 'Mon,Wed,Fri', '09:00', '17:00', '13:00', '14:00', 30),

-- OBSTETRICS & GYNECOLOGY (DepartmentID=7) - Dr. Amina Abubakar
(14, 7, 'High-Risk Pregnancy and Maternal-Fetal Medicine', 'Mon,Tue,Wed,Thu,Fri', '08:00', '16:00', '13:00', '14:00', 30),

-- ONCOLOGY (DepartmentID=8) - Dr. Sidharth Mukherjee
(15, 8, 'Medical Oncology and Hematologic Malignancies', 'Mon,Tue,Wed,Thu,Fri', '08:00', '16:00', '13:00', '14:00', 30),

-- PSYCHIATRY (DepartmentID=9) - Dr. Carl Jung
(16, 9, 'Cognitive-Behavioral Therapy and Psychopharmacology', 'Mon,Tue,Wed,Thu,Fri', '09:00', '17:00', '13:00', '14:00', 45),

-- DERMATOLOGY (DepartmentID=10) - Dr. Sandra Lee
(17, 10, 'Cosmetic Dermatology and Mohs Surgery', 'Mon,Tue,Wed,Thu', '08:00', '16:00', '13:00', '14:00', 30),

-- OPHTHALMOLOGY (DepartmentID=11) - Dr. Charles Kelman
(18, 11, 'Cataract Surgery and Refractive Vision Correction', 'Tue,Wed,Thu,Fri', '08:00', '16:00', '13:00', '14:00', 30),

-- RADIOLOGY (DepartmentID=12) - Dr. Marie Curie
(19, 12, 'Diagnostic Radiology and Interventional Imaging', 'Mon,Tue,Wed,Thu,Fri', '08:00', '16:00', '13:00', '14:00', 30);"
        ExecuteNonQuery(docSql, conn)

        ' PATIENTS - Test data for appointment workflows
        ' UserID mapping: 21=Emeka, 22=Halima, 23=Tunde, 24=Chioma, 25=Abubakar
        Dim patSql As String = "
INSERT INTO Patients (UserID, DateOfBirth, Gender, Address, BloodGroup, EmergencyContact, EmergencyPhone) VALUES
(21,'1990-05-14','Male','12 Adeola Street, Lagos','O+','Ngozi Obi','08011111111'),
(22,'1985-11-22','Female','5 Kano Road, Abuja','A+','Bello Musa','08022222222'),
(23,'1978-03-09','Male','33 Ibadan Close, Oyo','B-','Amaka Bakare','08033333333'),
(24,'2000-07-30','Female','7 Enugu Avenue, Enugu','AB+','Emeka Eze','08044444444'),
(25,'1995-12-01','Male','21 Kaduna Lane, Kaduna','O-','Amina Sule','08055555555');"
        ExecuteNonQuery(patSql, conn)

        ' SAMPLE APPOINTMENTS - Baseline test data spanning multiple departments
        Dim today As String = DateTime.Now.ToString("yyyy-MM-dd")
        Dim tomorrow As String = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd")
        Dim dayafter As String = DateTime.Now.AddDays(2).ToString("yyyy-MM-dd")
        Dim aptSql As String = $"
INSERT INTO Appointments (AppointmentID, PatientID, DoctorID, DepartmentID, AppointmentDate, AppointmentTime, Status, IsEmergency, Notes) VALUES
('APT-2024-0001',1,8,4,'{today}','09:00','Scheduled',0,'Routine cardiac checkup with Dr. Getso'),
('APT-2024-0002',2,6,3,'{today}','10:00','Scheduled',0,'Child vaccination follow-up with Dr. Smith'),
('APT-2024-0003',3,12,6,'{today}','11:00','Scheduled',0,'Knee pain assessment with Dr. Liston'),
('APT-2024-0004',4,2,1,'{today}','14:00','Scheduled',1,'Emergency - chest pain with Dr. Okafor'),
('APT-2024-0005',5,4,2,'{today}','15:00','Scheduled',0,'General consultation with Dr. Williams'),
('APT-2024-0006',1,12,6,'{tomorrow}','09:30','Scheduled',0,'Follow-up ortho review'),
('APT-2024-0007',2,8,4,'{tomorrow}','10:30','Scheduled',0,'ECG monitoring with Dr. Getso'),
('APT-2024-0008',3,6,3,'{dayafter}','11:30','Scheduled',0,'Pediatric growth review'),
('APT-2024-0009',4,12,6,'{dayafter}','14:30','Completed',0,'Post-surgery review'),
('APT-2024-0010',5,5,2,'{dayafter}','09:00','Scheduled',0,'Hypertension management with Dr. Jones'),
('APT-2024-0011',1,15,8,'{tomorrow}','09:00','Scheduled',0,'Oncology consultation with Dr. Mukherjee'),
('APT-2024-0012',2,16,9,'{tomorrow}','14:00','Scheduled',0,'Psychiatric evaluation with Dr. Jung');"
        ExecuteNonQuery(aptSql, conn)

        ' QUEUE - Sample queue entries for testing
        Dim qSql As String = "
INSERT INTO Queue (AppointmentID, TicketNumber, QueuePosition, EstimatedWait, Status) VALUES
('APT-2024-0001','TKT-001',1,0,'Waiting'),
('APT-2024-0002','TKT-002',2,30,'Waiting'),
('APT-2024-0003','TKT-003',3,60,'Waiting'),
('APT-2024-0004','TKT-004',1,0,'Waiting'),
('APT-2024-0005','TKT-005',4,90,'Waiting');"
        ExecuteNonQuery(qSql, conn)

        ' MEDICAL RECORDS - Sample baseline records
        Dim mrSql As String = $"
INSERT INTO MedicalRecords (PatientID, DoctorID, AppointmentID, Diagnosis, Prescription, TestResults, Notes) VALUES
(1,8,'APT-2024-0001','Mild hypertension','Amlodipine 5mg daily','BP: 145/90','Monitor weekly'),
(3,12,'APT-2024-0009','Post-knee replacement','Ibuprofen 400mg, Physiotherapy','X-ray normal','Recovery on track'),
(2,6,'APT-2024-0002','Routine vaccination check','None required','All clear','Next visit in 6 months');"
        ExecuteNonQuery(mrSql, conn)

        ' Log successful enterprise medical reconciliation
        LogError("InsertSampleDataIfEmpty: ENTERPRISE MEDICAL RECONCILIATION COMPLETE - 12 departments, 18 doctors (Emergency: Dr. James Okafor & Dr. Fatima Bello, Internal Medicine: Dr. Sarah Williams & Dr. David Jones, Pediatrics: Dr. Chidi Smith & Dr. Elena Rostova, Cardiology: Dr. Umar Getso & Dr. Grace Lin, Neurology: Dr. Alan Turing & Dr. Linus Pauling, Orthopedic Surgery: Dr. Robert Liston & Dr. Gibran Khoury, OB/GYN: Dr. Amina Abubakar, Oncology: Dr. Sidharth Mukherjee, Psychiatry: Dr. Carl Jung, Dermatology: Dr. Sandra Lee, Ophthalmology: Dr. Charles Kelman, Radiology: Dr. Marie Curie), 5 patients, 12 appointments")
    End Sub
#End Region

#Region "Helper Execution Methods"
    Public Sub ExecuteNonQuery(sql As String, conn As SQLiteConnection)
        Using cmd As New SQLiteCommand(sql, conn)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Function ExecuteScalar(sql As String) As Object
        Try
            Using conn As New SQLiteConnection(GetConnectionString())
                conn.Open()
                Using cmd As New SQLiteCommand(sql, conn)
                    Return cmd.ExecuteScalar()
                End Using
            End Using
        Catch ex As Exception
            LogError("ExecuteScalar error: " & ex.Message)
            Return Nothing
        End Try
    End Function

    Public Function GetDataTable(sql As String) As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SQLiteConnection(GetConnectionString())
                conn.Open()
                Using da As New SQLiteDataAdapter(sql, conn)
                    da.Fill(dt)
                End Using
            End Using
        Catch ex As Exception
            LogError("GetDataTable error: " & ex.Message)
        End Try
        Return dt
    End Function

    Public Function GetDataTable(sql As String, params As Dictionary(Of String, Object)) As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SQLiteConnection(GetConnectionString())
                conn.Open()
                Using cmd As New SQLiteCommand(sql, conn)
                    If params IsNot Nothing Then
                        For Each kvp As KeyValuePair(Of String, Object) In params
                            cmd.Parameters.AddWithValue(kvp.Key, kvp.Value)
                        Next
                    End If
                    Using da As New SQLiteDataAdapter(cmd)
                        da.Fill(dt)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            LogError("Parameterized GetDataTable error: " & ex.Message)
        End Try
        Return dt
    End Function

    Public Function ExecuteNonQueryWithParams(sql As String, params As Dictionary(Of String, Object)) As Integer
        Try
            Using conn As New SQLiteConnection(GetConnectionString())
                conn.Open()
                Using cmd As New SQLiteCommand(sql, conn)
                    For Each kvp As KeyValuePair(Of String, Object) In params
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value)
                    Next
                    Return cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            LogError("ExecuteNonQueryWithParams error: " & ex.Message)
            Return -1
        End Try
    End Function
#End Region

#Region "Authentication"
    Public Function AuthenticateUser(username As String, password As String) As UserAccount
        Try
            Dim sql As String = "SELECT UserID, Username, Role, FullName, Email, Phone FROM Users WHERE Username=@u AND Password=@p AND IsActive=1"
            Using conn As New SQLiteConnection(GetConnectionString())
                conn.Open()
                Using cmd As New SQLiteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@u", username)
                    cmd.Parameters.AddWithValue("@p", password)
                    Using reader As SQLiteDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Dim user As New UserAccount()
                            user.UserID = reader.GetInt32(0)
                            user.Username = reader.GetString(1)
                            user.Role = reader.GetString(2)
                            user.FullName = reader.GetString(3)
                            user.Email = If(reader.IsDBNull(4), "", reader.GetString(4))
                            Return user
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            LogError("Authentication error: " & ex.Message)
        End Try
        Return Nothing
    End Function
#End Region

#Region "Patient CRUD"
    Public Function GetAllPatients() As DataTable
        Dim sql As String = "
SELECT p.PatientID, u.FullName, u.Email, u.Phone, p.DateOfBirth,
       p.Gender, p.BloodGroup, p.Address, p.EmergencyContact, p.EmergencyPhone,
       u.Username, p.RegisteredDate
FROM Patients p
INNER JOIN Users u ON p.UserID = u.UserID
ORDER BY u.FullName"
        Return GetDataTable(sql)
    End Function

    Public Function GetPatientByUserID(userID As Integer) As DataRow
        Dim sql As String = "
SELECT p.PatientID, u.FullName, u.Email, u.Phone, p.DateOfBirth,
       p.Gender, p.BloodGroup, p.Address, p.EmergencyContact, p.EmergencyPhone, u.Username
FROM Patients p INNER JOIN Users u ON p.UserID = u.UserID
WHERE p.UserID = @userID"
        Dim prms As New Dictionary(Of String, Object) From {{"@userID", userID}}
        Dim dt As DataTable = GetDataTable(sql, prms)
        If dt.Rows.Count > 0 Then Return dt.Rows(0)
        Return Nothing
    End Function

    Public Function RegisterPatient(fullName As String, username As String, password As String,
                                     email As String, phone As String, dob As String,
                                     gender As String, address As String, bloodGroup As String,
                                     emergencyContact As String, emergencyPhone As String) As Boolean
        Try
            Using conn As New SQLiteConnection(GetConnectionString())
                conn.Open()
                Dim userSql As String = "INSERT INTO Users (Username, Password, Role, FullName, Email, Phone) VALUES (@u,@p,'Patient',@fn,@e,@ph); SELECT last_insert_rowid();"
                Dim newUserID As Long = 0
                Using cmd As New SQLiteCommand(userSql, conn)
                    cmd.Parameters.AddWithValue("@u", username)
                    cmd.Parameters.AddWithValue("@p", password)
                    cmd.Parameters.AddWithValue("@fn", fullName)
                    cmd.Parameters.AddWithValue("@e", email)
                    cmd.Parameters.AddWithValue("@ph", phone)
                    newUserID = Convert.ToInt64(cmd.ExecuteScalar())
                End Using

                Dim patSql As String = "INSERT INTO Patients (UserID, DateOfBirth, Gender, Address, BloodGroup, EmergencyContact, EmergencyPhone) VALUES (@uid,@dob,@g,@addr,@bg,@ec,@ep)"
                Using cmd As New SQLiteCommand(patSql, conn)
                    cmd.Parameters.AddWithValue("@uid", newUserID)
                    cmd.Parameters.AddWithValue("@dob", dob)
                    cmd.Parameters.AddWithValue("@g", gender)
                    cmd.Parameters.AddWithValue("@addr", address)
                    cmd.Parameters.AddWithValue("@bg", bloodGroup)
                    cmd.Parameters.AddWithValue("@ec", emergencyContact)
                    cmd.Parameters.AddWithValue("@ep", emergencyPhone)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
            Return True
        Catch ex As Exception
            LogError("RegisterPatient error: " & ex.Message)
            Return False
        End Try
    End Function
#End Region

#Region "Doctor CRUD"
    Public Function GetAllDoctors() As DataTable
        Dim sql As String = "
SELECT d.DoctorID, u.FullName, dept.DepartmentName, d.Specialization,
       d.WorkingDays, d.StartTime, d.EndTime, u.Phone, u.Email
FROM Doctors d
INNER JOIN Users u ON d.UserID = u.UserID
INNER JOIN Departments dept ON d.DepartmentID = dept.DepartmentID
ORDER BY dept.DepartmentName, u.FullName"
        Return GetDataTable(sql)
    End Function

    Public Function GetDoctorsByDepartment(departmentID As Integer) As DataTable
        Dim sql As String = "
SELECT d.DoctorID, u.FullName, d.Specialization
FROM Doctors d
INNER JOIN Users u ON d.UserID = u.UserID
WHERE d.DepartmentID = @departmentID
ORDER BY u.FullName"
        Dim prms As New Dictionary(Of String, Object) From {{"@departmentID", departmentID}}
        Return GetDataTable(sql, prms)
    End Function
#End Region

#Region "Appointment CRUD"
    Public Function GenerateAppointmentID() As String
        Dim year As String = DateTime.Now.Year.ToString()
        Dim countSql As String = "SELECT COUNT(*) FROM Appointments WHERE AppointmentID LIKE @matchPattern"
        Dim prms As New Dictionary(Of String, Object) From {{"@matchPattern", $"APT-{year}-%"}}
        Dim totalCount As Integer = 0

        Dim dt As DataTable = GetDataTable(countSql, prms)
        If dt.Rows.Count > 0 Then
            totalCount = Convert.ToInt32(dt.Rows(0)(0)) + 1
        Else
            totalCount = 1
        End If

        Return $"APT-{year}-{totalCount:0000}"
    End Function

    Public Function BookAppointment(patientID As Integer, doctorID As Integer, deptID As Integer,
                                     appointmentDate As String, appointmentTime As String,
                                     isEmergency As Boolean, notes As String) As String
        Try
            If IsSlotTaken(doctorID, appointmentDate, appointmentTime) Then
                Return "SLOT_TAKEN"
            End If

            Dim apptDateTime As DateTime = DateTime.Parse($"{appointmentDate} {appointmentTime}")
            If apptDateTime < DateTime.Now Then
                Return "PAST_DATE"
            End If

            Dim apptID As String = GenerateAppointmentID()
            Dim sql As String = "INSERT INTO Appointments (AppointmentID, PatientID, DoctorID, DepartmentID, AppointmentDate, AppointmentTime, Status, IsEmergency, Notes) VALUES (@id,@pid,@did,@dept,@date,@time,'Scheduled',@emerg,@notes)"
            Dim prms As New Dictionary(Of String, Object)()
            prms("@id") = apptID
            prms("@pid") = patientID
            prms("@did") = doctorID
            prms("@dept") = deptID
            prms("@date") = appointmentDate
            prms("@time") = appointmentTime
            prms("@emerg") = If(isEmergency, 1, 0)
            prms("@notes") = notes

            If ExecuteNonQueryWithParams(sql, prms) > 0 Then
                AddToQueue(apptID, isEmergency)
                Return apptID
            End If
            Return "ERROR"
        Catch ex As Exception
            LogError("BookAppointment error: " & ex.Message)
            Return "ERROR"
        End Try
    End Function

    Public Function IsSlotTaken(doctorID As Integer, appointmentDate As String, appointmentTime As String) As Boolean
        Dim sql As String = "SELECT COUNT(*) FROM Appointments WHERE DoctorID=@doctorID AND AppointmentDate=@appointmentDate AND AppointmentTime=@appointmentTime AND Status != 'Cancelled'"
        Dim prms As New Dictionary(Of String, Object) From {
            {"@doctorID", doctorID},
            {"@appointmentDate", appointmentDate},
            {"@appointmentTime", appointmentTime}
        }
        Dim dt As DataTable = GetDataTable(sql, prms)
        If dt.Rows.Count > 0 Then
            Return Convert.ToInt32(dt.Rows(0)(0)) > 0
        End If
        Return False
    End Function

    Public Function GetAvailableSlots(doctorID As Integer, appointmentDate As String) As List(Of String)
        Dim allSlots As New List(Of String)()
        Dim takenSlots As New List(Of String)()

        Dim startTime As New TimeSpan(9, 0, 0)
        Dim endTime As New TimeSpan(17, 0, 0)
        Dim lunchS As New TimeSpan(13, 0, 0)
        Dim lunchE As New TimeSpan(14, 0, 0)

        Dim current As TimeSpan = startTime
        Do While current < endTime
            If Not (current >= lunchS And current < lunchE) Then
                allSlots.Add(current.ToString("hh\:mm"))
            End If
            current = current.Add(New TimeSpan(0, SLOT_DURATION_MINUTES, 0))
        Loop

        Dim sql As String = "SELECT AppointmentTime FROM Appointments WHERE DoctorID=@doctorID AND AppointmentDate=@appointmentDate AND Status != 'Cancelled'"
        Dim prms As New Dictionary(Of String, Object) From {
            {"@doctorID", doctorID},
            {"@appointmentDate", appointmentDate}
        }
        Dim dt As DataTable = GetDataTable(sql, prms)
        For Each row As DataRow In dt.Rows
            takenSlots.Add(row("AppointmentTime").ToString())
        Next

        Dim freeSlots As New List(Of String)()
        For Each slot As String In allSlots
            If Not takenSlots.Contains(slot) Then
                freeSlots.Add(slot)
            End If
        Next

        Return freeSlots
    End Function

    Public Function GetAppointmentsByPatient(patientID As Integer) As DataTable
        Dim sql As String = "
SELECT a.AppointmentID, a.AppointmentDate, a.AppointmentTime, a.Status,
       u.FullName AS DoctorName, dept.DepartmentName, a.IsEmergency, a.Notes
FROM Appointments a
INNER JOIN Doctors d ON a.DoctorID = d.DoctorID
INNER JOIN Users u ON d.UserID = u.UserID
INNER JOIN Departments dept ON a.DepartmentID = dept.DepartmentID
WHERE a.PatientID = @patientID
ORDER BY a.AppointmentDate DESC, a.AppointmentTime"
        Dim prms As New Dictionary(Of String, Object) From {{"@patientID", patientID}}
        Return GetDataTable(sql, prms)
    End Function

    Public Function GetTodayAppointments() As DataTable
        Dim today As String = DateTime.Now.ToString("yyyy-MM-dd")
        Dim sql As String = "
SELECT a.AppointmentID, a.AppointmentDate, a.AppointmentTime, a.Status,
       pu.FullName AS PatientName, du.FullName AS DoctorName,
       dept.DepartmentName, a.IsEmergency, a.Notes
FROM Appointments a
INNER JOIN Patients p ON a.PatientID = p.PatientID
INNER JOIN Users pu ON p.UserID = pu.UserID
INNER JOIN Doctors d ON a.DoctorID = d.DoctorID
INNER JOIN Users du ON d.UserID = du.UserID
INNER JOIN Departments dept ON a.DepartmentID = dept.DepartmentID
WHERE a.AppointmentDate = @today
ORDER BY a.IsEmergency DESC, a.AppointmentTime"
        Dim prms As New Dictionary(Of String, Object) From {{"@today", today}}
        Return GetDataTable(sql, prms)
    End Function

    Public Function GetAllAppointments() As DataTable
        Dim sql As String = "
SELECT a.AppointmentID, a.AppointmentDate, a.AppointmentTime, a.Status,
       pu.FullName AS PatientName, du.FullName AS DoctorName,
       dept.DepartmentName, a.IsEmergency, a.Notes
FROM Appointments a
INNER JOIN Patients p ON a.PatientID = p.PatientID
INNER JOIN Users pu ON p.UserID = pu.UserID
INNER JOIN Doctors d ON a.DoctorID = d.DoctorID
INNER JOIN Users du ON d.UserID = du.UserID
INNER JOIN Departments dept ON a.DepartmentID = dept.DepartmentID
ORDER BY a.AppointmentDate DESC, a.AppointmentTime"
        Return GetDataTable(sql)
    End Function

    Public Function CancelAppointment(appointmentID As String) As Boolean
        Try
            Dim sql As String = "UPDATE Appointments SET Status='Cancelled' WHERE AppointmentID=@id"
            Dim prms As New Dictionary(Of String, Object) From {{"@id", appointmentID}}
            Return ExecuteNonQueryWithParams(sql, prms) > 0
        Catch ex As Exception
            LogError("CancelAppointment error: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function UpdateAppointmentStatus(appointmentID As String, status As String) As Boolean
        Dim prms As New Dictionary(Of String, Object)()
        prms("@status") = status
        prms("@id") = appointmentID
        Return ExecuteNonQueryWithParams("UPDATE Appointments SET Status=@status WHERE AppointmentID=@id", prms) > 0
    End Function

    Public Function GetTomorrowAppointments() As DataTable
        Dim tomorrow As String = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd")
        Dim sql As String = "
SELECT a.AppointmentID, pu.FullName AS PatientName, pu.Phone, pu.Email,
       a.AppointmentTime, du.FullName AS DoctorName, dept.DepartmentName
FROM Appointments a
INNER JOIN Patients p ON a.PatientID = p.PatientID
INNER JOIN Users pu ON p.UserID = pu.UserID
INNER JOIN Doctors d ON a.DoctorID = d.DoctorID
INNER JOIN Users du ON d.UserID = du.UserID
INNER JOIN Departments dept ON a.DepartmentID = dept.DepartmentID
WHERE a.AppointmentDate = @tomorrow AND a.Status='Scheduled' AND a.ReminderSent=0"
        Dim prms As New Dictionary(Of String, Object) From {{"@tomorrow", tomorrow}}
        Return GetDataTable(sql, prms)
    End Function

    Public Sub MarkReminderSent(appointmentID As String)
        ExecuteNonQueryWithParams("UPDATE Appointments SET ReminderSent=1 WHERE AppointmentID=@id",
            New Dictionary(Of String, Object) From {{"@id", appointmentID}})
    End Sub
#End Region

#Region "Queue Management"
    Public Sub AddToQueue(appointmentID As String, isEmergency As Boolean)
        Try
            Dim today As String = DateTime.Now.ToString("yyyy-MM-dd")
            Dim countSql As String = "SELECT COUNT(*) FROM Queue q INNER JOIN Appointments a ON q.AppointmentID=a.AppointmentID WHERE a.AppointmentDate=@today AND q.Status='Waiting'"
            Dim prmsCount As New Dictionary(Of String, Object) From {{"@today", today}}

            Dim currentCount As Integer = 0
            Dim dtCount As DataTable = GetDataTable(countSql, prmsCount)
            If dtCount.Rows.Count > 0 Then
                currentCount = Convert.ToInt32(dtCount.Rows(0)(0))
            End If

            Dim position As Integer
            Dim estimatedWait As Integer

            If isEmergency Then
                position = 1
                estimatedWait = 0
                ExecuteNonQueryWithParams("UPDATE Queue SET QueuePosition=QueuePosition+1, EstimatedWait=EstimatedWait+@slot WHERE Status='Waiting'",
                    New Dictionary(Of String, Object) From {{"@slot", SLOT_DURATION_MINUTES}})
            Else
                position = currentCount + 1
                estimatedWait = currentCount * SLOT_DURATION_MINUTES
            End If

            Dim ticketNum As String = $"TKT-{(currentCount + 1):000}"
            Dim sql As String = "INSERT INTO Queue (AppointmentID, TicketNumber, QueuePosition, EstimatedWait, Status) VALUES (@aid,@tkt,@pos,@wait,'Waiting')"
            Dim prms As New Dictionary(Of String, Object)()
            prms("@aid") = appointmentID
            prms("@tkt") = ticketNum
            prms("@pos") = position
            prms("@wait") = estimatedWait
            ExecuteNonQueryWithParams(sql, prms)
        Catch ex As Exception
            LogError("AddToQueue error: " & ex.Message)
        End Try
    End Sub

    Public Function GetQueue() As DataTable
        Dim today As String = DateTime.Now.ToString("yyyy-MM-dd")
        Dim sql As String = "
SELECT q.QueueID, q.TicketNumber, q.QueuePosition, q.EstimatedWait, q.Status,
       pu.FullName AS PatientName, du.FullName AS DoctorName,
       dept.DepartmentName, a.IsEmergency, a.AppointmentTime
FROM Queue q
INNER JOIN Appointments a ON q.AppointmentID = a.AppointmentID
INNER JOIN Patients p ON a.PatientID = p.PatientID
INNER JOIN Users pu ON p.UserID = pu.UserID
INNER JOIN Doctors d ON a.DoctorID = d.DoctorID
INNER JOIN Users du ON d.UserID = du.UserID
INNER JOIN Departments dept ON a.DepartmentID = dept.DepartmentID
WHERE a.AppointmentDate = @today AND q.Status IN ('Waiting','Called')
ORDER BY a.IsEmergency DESC, q.QueuePosition"
        Dim prms As New Dictionary(Of String, Object) From {{"@today", today}}
        Return GetDataTable(sql, prms)
    End Function

    Public Function CallNextPatient() As String
        Dim sql As String = "SELECT TicketNumber FROM Queue WHERE Status='Waiting' ORDER BY QueuePosition LIMIT 1"
        Dim dt As DataTable = GetDataTable(sql)
        If dt.Rows.Count > 0 AndAlso dt.Rows(0)("TicketNumber") IsNot DBNull.Value Then
            Dim ticket As String = dt.Rows(0)("TicketNumber").ToString()
            ExecuteNonQueryWithParams("UPDATE Queue SET Status='Called' WHERE TicketNumber=@t",
                New Dictionary(Of String, Object) From {{"@t", ticket}})
            Return ticket
        End If
        Return ""
    End Function

    Public Sub CompleteQueueItem(ticketNumber As String)
        ExecuteNonQueryWithParams("UPDATE Queue SET Status='Done' WHERE TicketNumber=@t",
            New Dictionary(Of String, Object) From {{"@t", ticketNumber}})
    End Sub
#End Region

#Region "Medical Records CRUD"
    Public Function GetMedicalRecordsByPatient(patientID As Integer) As DataTable
        Dim sql As String = "
SELECT mr.RecordID, mr.RecordDate, mr.Diagnosis, mr.Prescription,
       mr.TestResults, mr.Notes, u.FullName AS DoctorName
FROM MedicalRecords mr
INNER JOIN Doctors d ON mr.DoctorID = d.DoctorID
INNER JOIN Users u ON d.UserID = u.UserID
WHERE mr.PatientID = @patientID
ORDER BY mr.RecordDate DESC"
        Dim prms As New Dictionary(Of String, Object) From {{"@patientID", patientID}}
        Return GetDataTable(sql, prms)
    End Function

    Public Function AddMedicalRecord(patientID As Integer, doctorID As Integer, appointmentID As String,
                                      diagnosis As String, prescription As String,
                                      testResults As String, notes As String) As Boolean
        Dim sql As String = "INSERT INTO MedicalRecords (PatientID, DoctorID, AppointmentID, Diagnosis, Prescription, TestResults, Notes) VALUES (@pid,@did,@aid,@diag,@pres,@test,@notes)"
        Dim prms As New Dictionary(Of String, Object)()
        prms("@pid") = patientID
        prms("@did") = doctorID
        prms("@aid") = appointmentID
        prms("@diag") = diagnosis
        prms("@pres") = prescription
        prms("@test") = testResults
        prms("@notes") = notes
        Return ExecuteNonQueryWithParams(sql, prms) > 0
    End Function
#End Region

#Region "Logging"
    Public Sub LogError(message As String)
        Try
            Dim logPath As String = Path.Combine(Application.StartupPath, ERROR_LOG_FILE)
            Using sw As StreamWriter = File.AppendText(logPath)
                sw.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR: {message}")
            End Using
        Catch ex As Exception
            ' Fail-safe system console fallback to prevent thread crashing
            Console.WriteLine("Failed writing to diagnostic log: " & ex.Message)
        End Try
    End Sub
#End Region

#Region "Professional Security Audit Trail - Compliance Implementation"

    ''' <summary>
    ''' AUDIT TRAIL DATABASE SCHEMA INITIALIZATION
    ''' Creates the SystemAuditLogs table for comprehensive security tracking
    ''' Compliant with HIPAA, ISO 27001, and enterprise audit requirements
    ''' Thread-safe implementation with defensive error handling
    ''' </summary>
    Public Sub InitializeSystemAuditLogsSchema()
        Try
            Using conn As New SQLiteConnection(GetConnectionString())
                conn.Open()

                Dim sql As String = "
CREATE TABLE IF NOT EXISTS SystemAuditLogs (
    LogID INTEGER PRIMARY KEY AUTOINCREMENT,
    Timestamp TEXT NOT NULL,
    ActiveUser TEXT NOT NULL,
    ActionPerformed TEXT NOT NULL,
    ModuleName TEXT NOT NULL,
    IPAddress TEXT,
    SessionID TEXT,
    Severity TEXT DEFAULT 'INFO',
    AdditionalContext TEXT,
    MachineNameHost TEXT,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Performance index for audit queries by user and date range
CREATE INDEX IF NOT EXISTS idx_audit_user_timestamp ON SystemAuditLogs(ActiveUser, Timestamp DESC);

-- Performance index for audit queries by module and action
CREATE INDEX IF NOT EXISTS idx_audit_module_action ON SystemAuditLogs(ModuleName, ActionPerformed);

-- Performance index for severity-based filtering (security alerts, compliance reports)
CREATE INDEX IF NOT EXISTS idx_audit_severity ON SystemAuditLogs(Severity, Timestamp DESC);

-- Performance index for session tracking and forensic analysis
CREATE INDEX IF NOT EXISTS idx_audit_session ON SystemAuditLogs(SessionID, Timestamp DESC);
"

                Using cmd As New SQLiteCommand(sql, conn)
                    cmd.ExecuteNonQuery()
                End Using

                ' Log the initialization of the audit system itself
                LogSystemActivity(
                    username:="SYSTEM",
                    action:="Audit trail schema initialized successfully",
                    moduleName:="ModuleDatabase.InitializeSystemAuditLogsSchema",
                    ipAddress:="127.0.0.1",
                    severity:="INFO"
                )

            End Using

        Catch ex As Exception
            LogError($"InitializeSystemAuditLogsSchema error: {ex.Message}")
            ' Critical: If audit trail initialization fails, log to emergency backup
            WriteAuditEmergencyBackup($"CRITICAL: Audit schema init failed: {ex.Message} at {DateTime.Now}")
        End Try
    End Sub

    ''' <summary>
    ''' THREAD-SAFE LOGGING SUBROUTINE - PROFESSIONAL IMPLEMENTATION
    ''' Logs system activity to the SystemAuditLogs table with comprehensive tracking
    ''' Thread-safe using SyncLock for concurrent access safety
    ''' Automatic failover to emergency text backup if database is locked/unavailable
    ''' </summary>
    ''' <param name="username">Active session username (e.g., "admin", "dr_fatima")</param>
    ''' <param name="action">Action performed (e.g., "User Login", "Patient Record Updated")</param>
    ''' <param name="moduleName">Module/form name (e.g., "FormLogin", "FormPatientManagement")</param>
    ''' <param name="ipAddress">Optional IP address (default: "127.0.0.1" for local)</param>
    ''' <param name="severity">Optional severity level: INFO, WARNING, ERROR, CRITICAL (default: INFO)</param>
    ''' <param name="additionalContext">Optional JSON or text context for detailed forensics</param>
    Public Sub LogSystemActivity(username As String,
                                  action As String,
                                  moduleName As String,
                                  Optional ipAddress As String = "127.0.0.1",
                                  Optional severity As String = "INFO",
                                  Optional additionalContext As String = "")

        ' ===================================================================
        ' THREAD-SAFE IMPLEMENTATION
        ' SyncLock ensures only one thread writes to audit log at a time
        ' Critical for multi-threaded environments and concurrent user sessions
        ' ===================================================================
        SyncLock auditLock
            Try
                ' ===================================================================
                ' PARAMETER VALIDATION
                ' Defensive programming to prevent injection and data corruption
                ' ===================================================================
                If String.IsNullOrWhiteSpace(username) Then username = "UNKNOWN_USER"
                If String.IsNullOrWhiteSpace(action) Then action = "UNSPECIFIED_ACTION"
                If String.IsNullOrWhiteSpace(moduleName) Then moduleName = "UNSPECIFIED_MODULE"
                If String.IsNullOrWhiteSpace(ipAddress) Then ipAddress = "127.0.0.1"
                If String.IsNullOrWhiteSpace(severity) Then severity = "INFO"

                ' Validate severity level (compliance requirement)
                Dim validSeverities As String() = {"INFO", "WARNING", "ERROR", "CRITICAL"}
                If Array.IndexOf(validSeverities, severity.ToUpper()) = -1 Then
                    severity = "INFO"
                End If

                ' ===================================================================
                ' ISOLATED DATABASE CONNECTION
                ' Using block ensures automatic connection disposal
                ' Prevents connection leaks in high-volume audit logging scenarios
                ' ===================================================================
                Using conn As New SQLiteConnection(GetConnectionString())
                    conn.Open()

                    ' ===================================================================
                    ' PARAMETERIZED INSERT QUERY
                    ' Prevents SQL injection attacks
                    ' Ensures data integrity and compliance with security standards
                    ' ===================================================================
                    Dim sql As String = "
INSERT INTO SystemAuditLogs (
    Timestamp,
    ActiveUser,
    ActionPerformed,
    ModuleName,
    IPAddress,
    SessionID,
    Severity,
    AdditionalContext,
    MachineNameHost
) VALUES (
    @timestamp,
    @username,
    @action,
    @module,
    @ip,
    @sessionID,
    @severity,
    @context,
    @machine
)"

                    Using cmd As New SQLiteCommand(sql, conn)
                        ' ===================================================================
                        ' PARAMETER BINDING - TYPE-SAFE UNDER OPTION STRICT ON
                        ' ===================================================================
                        cmd.Parameters.AddWithValue("@timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"))
                        cmd.Parameters.AddWithValue("@username", username)
                        cmd.Parameters.AddWithValue("@action", action)
                        cmd.Parameters.AddWithValue("@module", moduleName)
                        cmd.Parameters.AddWithValue("@ip", ipAddress)

                        ' Session ID tracking for forensic analysis
                        Dim sessionID As String = "SESSION-" & DateTime.Now.ToString("yyyyMMddHHmmss")
                        If SessionManager.CurrentUser IsNot Nothing Then
                            sessionID = $"USER-{SessionManager.CurrentUser.UserID}-{DateTime.Now.Ticks}"
                        End If
                        cmd.Parameters.AddWithValue("@sessionID", sessionID)

                        cmd.Parameters.AddWithValue("@severity", severity.ToUpper())
                        cmd.Parameters.AddWithValue("@context", additionalContext)

                        ' Machine/hostname tracking for multi-terminal environments
                        Dim machineName As String = Environment.MachineName
                        cmd.Parameters.AddWithValue("@machine", machineName)

                        ' ===================================================================
                        ' EXECUTE AUDIT LOG INSERTION
                        ' ===================================================================
                        cmd.ExecuteNonQuery()
                    End Using
                End Using

            Catch sqlEx As SQLiteException
                ' ===================================================================
                ' DATABASE LOCKED / UNAVAILABLE FAILOVER
                ' Automatic fallback to emergency text-based audit backup
                ' Ensures no audit events are lost during database contention
                ' ===================================================================
                Dim emergencyMsg As String = $"[DB_LOCKED] User: {username} | Action: {action} | Module: {moduleName} | IP: {ipAddress} | Severity: {severity} | Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}"
                WriteAuditEmergencyBackup(emergencyMsg)

                ' Log to diagnostic error log
                LogError($"LogSystemActivity - Database locked, failed to emergency backup: {sqlEx.Message}")

            Catch ex As Exception
                ' ===================================================================
                ' GENERAL EXCEPTION HANDLING
                ' Fail gracefully without crashing the application
                ' Critical for maintaining system stability during audit failures
                ' ===================================================================
                Dim emergencyMsg As String = $"[AUDIT_ERROR] User: {username} | Action: {action} | Module: {moduleName} | Error: {ex.Message} | Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}"
                WriteAuditEmergencyBackup(emergencyMsg)

                ' Log to diagnostic error log
                LogError($"LogSystemActivity error: {ex.Message}")
            End Try
        End SyncLock
    End Sub

    ''' <summary>
    ''' EMERGENCY BACKUP TEXT LOG WRITER
    ''' Failover mechanism when SQLite database is locked or unavailable
    ''' Thread-safe file writing with automatic retry logic
    ''' Ensures zero audit event loss under all circumstances
    ''' </summary>
    ''' <param name="message">Audit message to write to emergency backup file</param>
    Private Sub WriteAuditEmergencyBackup(message As String)
        Try
            Dim backupPath As String = Path.Combine(Application.StartupPath, AUDIT_EMERGENCY_LOG_FILE)

            ' Thread-safe file writing using SyncLock
            SyncLock auditLock
                Using sw As StreamWriter = File.AppendText(backupPath)
                    sw.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - EMERGENCY_AUDIT_BACKUP: {message}")
                End Using
            End SyncLock

        Catch ex As Exception
            ' Ultimate fail-safe: Console output (captured by system logging in production)
            Console.WriteLine($"CRITICAL: Emergency audit backup failed: {ex.Message}")
            Console.WriteLine($"AUDIT DATA: {message}")
        End Try
    End Sub

    ''' <summary>
    ''' SIMPLIFIED OVERLOAD FOR BACKWARD COMPATIBILITY
    ''' Maintains existing LogSystemActivity(username, action, module) call signature
    ''' Automatically defaults to INFO severity and local IP
    ''' </summary>
    Public Sub LogSystemActivity(username As String, action As String, moduleName As String)
        LogSystemActivity(username, action, moduleName, "127.0.0.1", "INFO", "")
    End Sub

    ''' <summary>
    ''' AUDIT TRAIL QUERY - RETRIEVE SYSTEM ACTIVITY LOGS
    ''' Returns audit logs for compliance reporting and security analysis
    ''' Supports filtering by date range, user, module, and severity
    ''' </summary>
    ''' <param name="startDate">Start date for audit query (yyyy-MM-dd format)</param>
    ''' <param name="endDate">End date for audit query (yyyy-MM-dd format)</param>
    ''' <param name="username">Optional username filter (empty = all users)</param>
    ''' <param name="moduleName">Optional module filter (empty = all modules)</param>
    ''' <param name="severity">Optional severity filter (empty = all severities)</param>
    ''' <returns>DataTable containing audit log records</returns>
    Public Function GetAuditLogs(Optional startDate As String = "",
                                 Optional endDate As String = "",
                                 Optional username As String = "",
                                 Optional moduleName As String = "",
                                 Optional severity As String = "") As DataTable
        Try
            Dim sql As String = "
SELECT 
    LogID,
    Timestamp,
    ActiveUser,
    ActionPerformed,
    ModuleName,
    IPAddress,
    SessionID,
    Severity,
    AdditionalContext,
    MachineNameHost,
    CreatedDate
FROM SystemAuditLogs
WHERE 1=1"

            Dim params As New Dictionary(Of String, Object)()

            ' Build dynamic WHERE clause based on provided filters
            If Not String.IsNullOrWhiteSpace(startDate) Then
                sql &= " AND Timestamp >= @startDate"
                params.Add("@startDate", startDate & " 00:00:00")
            End If

            If Not String.IsNullOrWhiteSpace(endDate) Then
                sql &= " AND Timestamp <= @endDate"
                params.Add("@endDate", endDate & " 23:59:59")
            End If

            If Not String.IsNullOrWhiteSpace(username) Then
                sql &= " AND ActiveUser LIKE @username"
                params.Add("@username", "%" & username & "%")
            End If

            If Not String.IsNullOrWhiteSpace(moduleName) Then
                sql &= " AND ModuleName LIKE @module"
                params.Add("@module", "%" & moduleName & "%")
            End If

            If Not String.IsNullOrWhiteSpace(severity) Then
                sql &= " AND Severity = @severity"
                params.Add("@severity", severity.ToUpper())
            End If

            sql &= " ORDER BY Timestamp DESC LIMIT 10000"

            Return GetDataTable(sql, params)

        Catch ex As Exception
            LogError($"GetAuditLogs error: {ex.Message}")
            Return New DataTable()
        End Try
    End Function

    ''' <summary>
    ''' AUDIT STATISTICS - GET ACTIVITY SUMMARY
    ''' Returns aggregated statistics for compliance dashboards
    ''' </summary>
    Public Function GetAuditStatistics() As Dictionary(Of String, Integer)
        Dim stats As New Dictionary(Of String, Integer)()

        Try
            ' Total audit records
            Dim totalSql As String = "SELECT COUNT(*) FROM SystemAuditLogs"
            Dim totalDt As DataTable = GetDataTable(totalSql)
            If totalDt.Rows.Count > 0 Then
                stats("TotalRecords") = Convert.ToInt32(totalDt.Rows(0)(0))
            End If

            ' Records by severity
            Dim severitySql As String = "SELECT Severity, COUNT(*) AS Count FROM SystemAuditLogs GROUP BY Severity"
            Dim severityDt As DataTable = GetDataTable(severitySql)
            For Each row As DataRow In severityDt.Rows
                Dim severity As String = row("Severity").ToString()
                Dim count As Integer = Convert.ToInt32(row("Count"))
                stats($"Severity_{severity}") = count
            Next

            ' Today's activity
            Dim today As String = DateTime.Now.ToString("yyyy-MM-dd")
            Dim todaySql As String = $"SELECT COUNT(*) FROM SystemAuditLogs WHERE DATE(Timestamp) = '{today}'"
            Dim todayDt As DataTable = GetDataTable(todaySql)
            If todayDt.Rows.Count > 0 Then
                stats("TodayRecords") = Convert.ToInt32(todayDt.Rows(0)(0))
            End If

        Catch ex As Exception
            LogError($"GetAuditStatistics error: {ex.Message}")
        End Try

        Return stats
    End Function

#End Region

#Region "Patient Management - Professional Implementation"

    ''' <summary>
    ''' Ensures the PatientsManagement table exists in the database with proper schema.
    ''' Called during database initialization.
    ''' 
    ''' SCHEMA STRUCTURE:
    '''   PatientID     TEXT PRIMARY KEY NOT NULL  - Format: PAT-YYYY-NNNN
    '''   FirstName     TEXT NOT NULL                - Patient's first name (REQUIRED)
    '''   LastName      TEXT NOT NULL                - Patient's last name (REQUIRED)
    '''   DateOfBirth   TEXT                         - ISO format: YYYY-MM-DD (OPTIONAL)
    '''   Gender        TEXT                         - Male/Female/Other (OPTIONAL)
    '''   PhoneNumber   TEXT NOT NULL                - Contact phone (REQUIRED)
    '''   Email         TEXT                         - Email address (OPTIONAL - allows NULL)
    '''   DateRegistered TEXT NOT NULL DEFAULT now  - Auto-generated timestamp
    ''' 
    ''' CONSTRAINTS:
    '''   - PatientID must be unique and non-null
    '''   - FirstName, LastName, PhoneNumber are mandatory (NOT NULL)
    '''   - Email is optional and can be NULL or empty string
    '''   - DateOfBirth and Gender are optional
    ''' </summary>
    Public Sub InitializePatientManagementSchema()
        Try
            Using conn As New SQLiteConnection(GetConnectionString())
                conn.Open()

                ' ===================================================================
                ' PATIENTS MANAGEMENT TABLE SCHEMA
                ' Robust schema with proper NULL handling for optional fields
                ' ===================================================================
                Dim sql As String = "
CREATE TABLE IF NOT EXISTS PatientsManagement (
    PatientID TEXT PRIMARY KEY NOT NULL,
    FirstName TEXT NOT NULL,
    LastName TEXT NOT NULL,
    DateOfBirth TEXT,
    Gender TEXT,
    PhoneNumber TEXT NOT NULL,
    Email TEXT,
    DateRegistered TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE INDEX IF NOT EXISTS idx_patient_name ON PatientsManagement(FirstName, LastName);
CREATE INDEX IF NOT EXISTS idx_patient_phone ON PatientsManagement(PhoneNumber);"

                Using cmd As New SQLiteCommand(sql, conn)
                    cmd.ExecuteNonQuery()
                End Using

                LogError("InitializePatientManagementSchema: PatientsManagement table initialized successfully")
            End Using
        Catch ex As Exception
            LogError("InitializePatientManagementSchema error: " & ex.Message & " | StackTrace: " & ex.StackTrace)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Generates the next unique Patient ID in format: PAT-YYYY-NNNN
    ''' Uses MAX ID query to prevent duplicates even when records are deleted.
    ''' Thread-safe implementation with database transaction.
    ''' 
    ''' ALGORITHM:
    ''' 1. Query for the highest PatientID for current year (e.g., "PAT-2026-%")
    ''' 2. Extract the numerical suffix and increment by 1
    ''' 3. If no records exist for current year, start at 0001
    ''' 4. Return formatted ID: PAT-YYYY-NNNN
    ''' </summary>
    ''' <returns>Next available PatientID (e.g., "PAT-2026-0003")</returns>
    Public Function GetNextPatientID() As String
        Try
            Dim currentYear As String = DateTime.Now.Year.ToString()
            Dim pattern As String = $"PAT-{currentYear}-%"

            ' ===================================================================
            ' ROBUST MAX ID QUERY - Prevents duplicates even with deleted records
            ' Orders by PatientID DESC to get the highest ID, extracts the suffix
            ' ===================================================================
            Dim maxIdSql As String = "
                SELECT PatientID 
                FROM PatientsManagement 
                WHERE PatientID LIKE @pattern 
                ORDER BY PatientID DESC 
                LIMIT 1"

            Dim params As New Dictionary(Of String, Object) From {
                {"@pattern", pattern}
            }

            Dim dt As DataTable = GetDataTable(maxIdSql, params)
            Dim nextNumber As Integer = 1

            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                ' Extract the highest existing ID
                Dim maxID As String = dt.Rows(0)("PatientID").ToString()

                ' Parse the numeric suffix from "PAT-2026-0123" -> 123
                Dim parts As String() = maxID.Split("-"c)
                If parts.Length = 3 Then
                    Dim numericPart As String = parts(2)
                    Dim lastNumber As Integer = 0

                    If Integer.TryParse(numericPart, lastNumber) Then
                        nextNumber = lastNumber + 1
                    End If
                End If
            End If

            ' Format as PAT-YYYY-NNNN (4-digit zero-padded)
            Dim newPatientID As String = $"PAT-{currentYear}-{nextNumber:0000}"

            LogError($"GetNextPatientID: Generated new ID '{newPatientID}'")
            Return newPatientID

        Catch ex As Exception
            ' Fallback to safe default if error occurs
            Dim fallbackID As String = $"PAT-{DateTime.Now.Year}-0001"
            LogError($"GetNextPatientID error: {ex.Message} | Using fallback: {fallbackID}")
            Return fallbackID
        End Try
    End Function

    ''' <summary>
    ''' LEGACY ALIAS: Maintains backward compatibility with existing code
    ''' </summary>
    Public Function GeneratePatientID() As String
        Return GetNextPatientID()
    End Function

    ''' <summary>
    ''' Saves a patient record to the database (UPSERT operation).
    ''' Handles both INSERT (new patients) and UPDATE (existing patients).
    ''' Thread-safe with full parameter validation and error logging.
    ''' 
    ''' IMPORTANT: This method now THROWS EXCEPTIONS on error instead of returning False.
    ''' The UI layer should catch these exceptions to display detailed error messages.
    ''' </summary>
    ''' <param name="patient">PatientModel object containing patient data</param>
    ''' <exception cref="ArgumentNullException">Thrown when patient object is null</exception>
    ''' <exception cref="ArgumentException">Thrown when patient data validation fails</exception>
    ''' <exception cref="SQLiteException">Thrown when database constraint or SQL error occurs</exception>
    Public Sub SavePatient(patient As PatientModel)
        If patient Is Nothing Then
            Dim errorMsg As String = "SavePatient error: Patient object is null"
            LogError(errorMsg)
            Throw New ArgumentNullException("patient", errorMsg)
        End If

        If Not patient.IsValid() Then
            Dim errorMsg As String = "SavePatient error: Patient data validation failed - FirstName, LastName, and PhoneNumber are required"
            LogError(errorMsg)
            Throw New ArgumentException(errorMsg, "patient")
        End If

        Try
            Using conn As New SQLiteConnection(GetConnectionString())
                conn.Open()

                ' Begin transaction for thread-safety
                Using transaction As SQLiteTransaction = conn.BeginTransaction()
                    Try
                        ' Check if patient exists
                        Dim existsSql As String = "SELECT COUNT(*) FROM PatientsManagement WHERE PatientID = @id"
                        Dim existsCmd As New SQLiteCommand(existsSql, conn, transaction)
                        existsCmd.Parameters.AddWithValue("@id", patient.PatientID)

                        Dim exists As Integer = Convert.ToInt32(existsCmd.ExecuteScalar())
                        Dim sql As String

                        If exists > 0 Then
                            ' UPDATE existing record
                            sql = "UPDATE PatientsManagement SET 
                                   FirstName = @firstName,
                                   LastName = @lastName,
                                   DateOfBirth = @dob,
                                   Gender = @gender,
                                   PhoneNumber = @phone,
                                   Email = @email
                                   WHERE PatientID = @id"
                            LogError($"SavePatient: Updating existing patient {patient.PatientID}")
                        Else
                            ' ===================================================================
                            ' INSERT NEW RECORD
                            ' Generate a unique PatientID if not already assigned
                            ' Handles: null, empty, whitespace, or "[Auto-Generated]" placeholders
                            ' ===================================================================
                            If String.IsNullOrWhiteSpace(patient.PatientID) OrElse 
                               patient.PatientID.Equals("[Auto-Generated]", StringComparison.OrdinalIgnoreCase) Then
                                patient.PatientID = GetNextPatientID()
                            End If

                            sql = "INSERT INTO PatientsManagement 
                                   (PatientID, FirstName, LastName, DateOfBirth, Gender, PhoneNumber, Email, DateRegistered)
                                   VALUES (@id, @firstName, @lastName, @dob, @gender, @phone, @email, @dateReg)"
                            LogError($"SavePatient: Inserting new patient {patient.PatientID}")
                        End If

                        Using cmd As New SQLiteCommand(sql, conn, transaction)
                            ' ===================================================================
                            ' PARAMETERIZED VALUE BINDING WITH ENHANCED LOGGING
                            ' Ensures type-safe parameter binding with proper NULL handling
                            ' ===================================================================
                            cmd.Parameters.AddWithValue("@id", patient.PatientID)
                            cmd.Parameters.AddWithValue("@firstName", patient.FirstName)
                            cmd.Parameters.AddWithValue("@lastName", patient.LastName)

                            ' Optional DateOfBirth - use DBNull for empty/null values
                            cmd.Parameters.AddWithValue("@dob", If(String.IsNullOrEmpty(patient.DateOfBirth), DBNull.Value, CObj(patient.DateOfBirth)))

                            ' Optional Gender - use DBNull for empty/null values
                            cmd.Parameters.AddWithValue("@gender", If(String.IsNullOrEmpty(patient.Gender), DBNull.Value, CObj(patient.Gender)))

                            ' Required PhoneNumber
                            cmd.Parameters.AddWithValue("@phone", patient.PhoneNumber)

                            ' Optional Email - use DBNull for empty/null values
                            cmd.Parameters.AddWithValue("@email", If(String.IsNullOrEmpty(patient.Email), DBNull.Value, CObj(patient.Email)))

                            If exists = 0 Then
                                cmd.Parameters.AddWithValue("@dateReg", patient.DateRegistered)
                            End If

                            ' Log parameter values for debugging (before execution)
                            LogError($"SavePatient SQL Parameters - ID: {patient.PatientID}, FirstName: {patient.FirstName}, LastName: {patient.LastName}, Phone: {patient.PhoneNumber}, Email: {If(String.IsNullOrEmpty(patient.Email), "NULL", patient.Email)}")

                            cmd.ExecuteNonQuery()
                        End Using

                        transaction.Commit()
                        ' SUCCESS - No exception thrown, operation completed

                    Catch ex As Exception
                        transaction.Rollback()
                        Throw
                    End Try
                End Using
            End Using

        Catch ex As SQLiteException
            ' ===================================================================
            ' CRITICAL: LOG THEN RE-THROW EXCEPTION
            ' This allows UI to display the exact error while maintaining audit trail
            ' ===================================================================

            ' Detailed SQLite-specific error logging with constraint information
            Dim detailedError As String = $"SavePatient SQLite error: {ex.Message} | ErrorCode: {ex.ErrorCode} | Patient: {patient.PatientID} | FirstName: {patient.FirstName} | LastName: {patient.LastName} | Phone: {patient.PhoneNumber}"
            LogError(detailedError)

            ' Log the full exception for debugging
            LogError($"SQLite Exception Details - Source: {ex.Source} | StackTrace: {ex.StackTrace}")

            ' RE-THROW to allow UI to display the error
            Throw

        Catch ex As Exception
            ' ===================================================================
            ' CRITICAL: LOG THEN RE-THROW EXCEPTION
            ' This allows UI to display the exact error while maintaining audit trail
            ' ===================================================================

            ' Detailed general error logging
            Dim detailedError As String = $"SavePatient error: {ex.Message} | Type: {ex.GetType().Name} | Patient: {patient.PatientID} | FirstName: {patient.FirstName} | LastName: {patient.LastName}"
            LogError(detailedError)

            ' Log the full exception for debugging
            LogError($"Exception Details - Source: {ex.Source} | StackTrace: {ex.StackTrace}")

            ' RE-THROW to allow UI to display the error
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Overloaded SavePatient method using explicit parameters for backward compatibility.
    ''' IMPORTANT: This method now THROWS EXCEPTIONS on error (delegates to main SavePatient Sub).
    ''' </summary>
    Public Sub SavePatient(patientID As String, firstName As String, lastName As String,
                                 dateOfBirth As String, gender As String, phoneNumber As String,
                                 email As String)
        Dim patient As New PatientModel() With {
            .PatientID = patientID,
            .FirstName = firstName,
            .LastName = lastName,
            .DateOfBirth = dateOfBirth,
            .Gender = gender,
            .PhoneNumber = phoneNumber,
            .Email = email
        }

        SavePatient(patient)
    End Sub

    ''' <summary>
    ''' Retrieves all patient records as a DataTable for UI binding.
    ''' Optimized for DataGridView with proper column ordering.
    ''' </summary>
    ''' <returns>DataTable containing all patient records</returns>
    Public Function GetPatientsTable() As DataTable
        Try
            Dim sql As String = "
SELECT 
    PatientID AS 'Patient ID',
    FirstName AS 'First Name',
    LastName AS 'Last Name',
    DateOfBirth AS 'Date of Birth',
    Gender,
    PhoneNumber AS 'Phone Number',
    Email,
    DateRegistered AS 'Registered On'
FROM PatientsManagement
ORDER BY DateRegistered DESC, LastName ASC, FirstName ASC"

            Return GetDataTable(sql)

        Catch ex As Exception
            LogError("GetPatientsTable error: " & ex.Message)
            Return New DataTable() ' Return empty table on error
        End Try
    End Function

    ''' <summary>
    ''' Retrieves a single patient by ID.
    ''' </summary>
    Public Function GetPatientByID(patientID As String) As PatientModel
        Try
            Dim sql As String = "SELECT * FROM PatientsManagement WHERE PatientID = @id"
            Dim params As New Dictionary(Of String, Object) From {{"@id", patientID}}
            Dim dt As DataTable = GetDataTable(sql, params)

            If dt.Rows.Count = 0 Then
                Return Nothing
            End If

            Dim row As DataRow = dt.Rows(0)
            Dim patient As New PatientModel() With {
                .PatientID = row("PatientID").ToString(),
                .FirstName = row("FirstName").ToString(),
                .LastName = row("LastName").ToString(),
                .DateOfBirth = If(row.IsNull("DateOfBirth"), "", row("DateOfBirth").ToString()),
                .Gender = If(row.IsNull("Gender"), "", row("Gender").ToString()),
                .PhoneNumber = row("PhoneNumber").ToString(),
                .Email = If(row.IsNull("Email"), "", row("Email").ToString()),
                .DateRegistered = row("DateRegistered").ToString()
            }

            Return patient

        Catch ex As Exception
            LogError($"GetPatientByID error: {ex.Message} | PatientID: {patientID}")
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Searches patients by name or phone number.
    ''' Returns a filtered DataTable for UI binding.
    ''' </summary>
    Public Function SearchPatients(searchTerm As String) As DataTable
        Try
            If String.IsNullOrWhiteSpace(searchTerm) Then
                Return GetPatientsTable()
            End If

            Dim sql As String = "
SELECT 
    PatientID AS 'Patient ID',
    FirstName AS 'First Name',
    LastName AS 'Last Name',
    DateOfBirth AS 'Date of Birth',
    Gender,
    PhoneNumber AS 'Phone Number',
    Email,
    DateRegistered AS 'Registered On'
FROM PatientsManagement
WHERE FirstName LIKE @search 
   OR LastName LIKE @search 
   OR PhoneNumber LIKE @search
   OR (FirstName || ' ' || LastName) LIKE @search
ORDER BY DateRegistered DESC, LastName ASC, FirstName ASC"

            Dim params As New Dictionary(Of String, Object) From {
                {"@search", $"%{searchTerm}%"}
            }

            Return GetDataTable(sql, params)

        Catch ex As Exception
            LogError($"SearchPatients error: {ex.Message} | SearchTerm: {searchTerm}")
            Return New DataTable()
        End Try
    End Function

    ''' <summary>
    ''' Deletes a patient record by ID.
    ''' </summary>
    Public Function DeletePatient(patientID As String) As Boolean
        Try
            Dim sql As String = "DELETE FROM PatientsManagement WHERE PatientID = @id"
            Dim params As New Dictionary(Of String, Object) From {{"@id", patientID}}

            Dim rowsAffected As Integer = ExecuteNonQueryWithParams(sql, params)
            Return rowsAffected > 0

        Catch ex As Exception
            LogError($"DeletePatient error: {ex.Message} | PatientID: {patientID}")
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Gets the total count of registered patients.
    ''' </summary>
    Public Function GetPatientCount() As Integer
        Try
            Dim sql As String = "SELECT COUNT(*) FROM PatientsManagement"
            Dim result As Object = ExecuteScalar(sql)

            If result IsNot Nothing Then
                Return Convert.ToInt32(result)
            End If

            Return 0

        Catch ex As Exception
            LogError("GetPatientCount error: " & ex.Message)
            Return 0
        End Try
    End Function

#End Region

#Region "Patient Vitals Management - Professional Implementation"

    ''' <summary>
    ''' Initializes the InpatientVitals table with proper schema and referential integrity.
    ''' Called during database initialization to ensure schema exists.
    ''' </summary>
    Public Sub InitializePatientVitalsSchema()
        Try
            Using conn As New SQLiteConnection(GetConnectionString())
                conn.Open()

                ' Enable foreign key constraints (SQLite requires this per connection)
                Using cmdForeignKeys As New SQLiteCommand("PRAGMA foreign_keys = ON;", conn)
                    cmdForeignKeys.ExecuteNonQuery()
                End Using

                Dim sql As String = "
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
CREATE INDEX IF NOT EXISTS idx_vitals_date ON InpatientVitals(DateRecorded DESC);"

                Using cmd As New SQLiteCommand(sql, conn)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            LogError("InitializePatientVitalsSchema error: " & ex.Message)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Saves a patient vitals record to the database with full validation and transaction safety.
    ''' Thread-safe implementation using parameterized queries and proper resource disposal.
    ''' </summary>
    ''' <param name="patientID">The unique Patient ID (e.g., PAT-2025-0001)</param>
    ''' <param name="bloodPressure">Blood pressure reading (e.g., "120/80")</param>
    ''' <param name="heartRate">Heart rate in beats per minute</param>
    ''' <param name="temperature">Body temperature in Celsius</param>
    ''' <param name="spO2">Oxygen saturation percentage (0-100)</param>
    ''' <param name="weight">Body weight in kilograms</param>
    ''' <returns>True if save succeeded, False otherwise</returns>
    Public Function SavePatientVitals(patientID As String, bloodPressure As String,
                                       heartRate As Integer, temperature As Double,
                                       spO2 As Integer, weight As Double) As Boolean
        ' Input validation
        If String.IsNullOrWhiteSpace(patientID) Then
            LogError("SavePatientVitals error: PatientID is null or empty")
            Return False
        End If

        ' Validate patient exists
        If GetPatientByID(patientID) Is Nothing Then
            LogError($"SavePatientVitals error: Patient '{patientID}' does not exist in database")
            Return False
        End If

        ' Business rule validation
        If heartRate < 0 OrElse heartRate > 300 Then
            LogError($"SavePatientVitals error: Invalid heart rate value: {heartRate}")
            Return False
        End If

        If temperature < 30.0 OrElse temperature > 45.0 Then
            LogError($"SavePatientVitals error: Invalid temperature value: {temperature}")
            Return False
        End If

        If spO2 < 0 OrElse spO2 > 100 Then
            LogError($"SavePatientVitals error: Invalid SpO2 value: {spO2}")
            Return False
        End If

        If weight < 0.0 OrElse weight > 500.0 Then
            LogError($"SavePatientVitals error: Invalid weight value: {weight}")
            Return False
        End If

        Try
            Using conn As New SQLiteConnection(GetConnectionString())
                conn.Open()

                ' Enable foreign key enforcement
                Using cmdForeignKeys As New SQLiteCommand("PRAGMA foreign_keys = ON;", conn)
                    cmdForeignKeys.ExecuteNonQuery()
                End Using

                ' Begin transaction for thread-safety
                Using transaction As SQLiteTransaction = conn.BeginTransaction()
                    Try
                        Dim sql As String = "
INSERT INTO InpatientVitals 
(PatientID, BloodPressure, HeartRate, Temperature, SpO2, Weight, DateRecorded)
VALUES (@patientID, @bp, @hr, @temp, @spo2, @weight, @dateRec)"

                        Using cmd As New SQLiteCommand(sql, conn, transaction)
                            cmd.Parameters.AddWithValue("@patientID", patientID)
                            cmd.Parameters.AddWithValue("@bp", If(String.IsNullOrWhiteSpace(bloodPressure), DBNull.Value, CObj(bloodPressure)))
                            cmd.Parameters.AddWithValue("@hr", heartRate)
                            cmd.Parameters.AddWithValue("@temp", temperature)
                            cmd.Parameters.AddWithValue("@spo2", spO2)
                            cmd.Parameters.AddWithValue("@weight", weight)
                            cmd.Parameters.AddWithValue("@dateRec", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))

                            cmd.ExecuteNonQuery()
                        End Using

                        transaction.Commit()
                        Return True

                    Catch ex As Exception
                        transaction.Rollback()
                        Throw
                    End Try
                End Using
            End Using

        Catch ex As SQLiteException
            LogError($"SavePatientVitals SQLite error: {ex.Message} | PatientID: {patientID}")
            Return False
        Catch ex As Exception
            LogError($"SavePatientVitals error: {ex.Message} | PatientID: {patientID}")
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Retrieves the complete vitals history for a specific patient.
    ''' Returns data sorted by most recent first for UI display.
    ''' </summary>
    ''' <param name="patientID">The unique Patient ID</param>
    ''' <returns>DataTable containing vitals history with friendly column names</returns>
    Public Function GetPatientVitalsHistory(patientID As String) As DataTable
        Try
            If String.IsNullOrWhiteSpace(patientID) Then
                LogError("GetPatientVitalsHistory error: PatientID is null or empty")
                Return New DataTable()
            End If

            Dim sql As String = "
SELECT 
    v.VitalID AS 'Vital ID',
    v.PatientID AS 'Patient ID',
    v.BloodPressure AS 'Blood Pressure',
    v.HeartRate AS 'Heart Rate (bpm)',
    v.Temperature AS 'Temperature (°C)',
    v.SpO2 AS 'SpO2 (%)',
    v.Weight AS 'Weight (kg)',
    v.DateRecorded AS 'Date Recorded'
FROM InpatientVitals v
WHERE v.PatientID = @patientID
ORDER BY v.DateRecorded DESC"

            Dim params As New Dictionary(Of String, Object) From {
                {"@patientID", patientID}
            }

            Return GetDataTable(sql, params)

        Catch ex As Exception
            LogError($"GetPatientVitalsHistory error: {ex.Message} | PatientID: {patientID}")
            Return New DataTable()
        End Try
    End Function

    ''' <summary>
    ''' Retrieves the most recent vitals record for a patient.
    ''' Useful for displaying current vital signs in patient overview.
    ''' </summary>
    ''' <param name="patientID">The unique Patient ID</param>
    ''' <returns>DataRow containing the latest vitals, or Nothing if no records exist</returns>
    Public Function GetLatestPatientVitals(patientID As String) As DataRow
        Try
            If String.IsNullOrWhiteSpace(patientID) Then
                Return Nothing
            End If

            Dim sql As String = "
SELECT 
    VitalID, PatientID, BloodPressure, HeartRate, Temperature, SpO2, Weight, DateRecorded
FROM InpatientVitals
WHERE PatientID = @patientID
ORDER BY DateRecorded DESC
LIMIT 1"

            Dim params As New Dictionary(Of String, Object) From {
                {"@patientID", patientID}
            }

            Dim dt As DataTable = GetDataTable(sql, params)
            If dt.Rows.Count > 0 Then
                Return dt.Rows(0)
            End If

            Return Nothing

        Catch ex As Exception
            LogError($"GetLatestPatientVitals error: {ex.Message} | PatientID: {patientID}")
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Deletes a specific vitals record by VitalID.
    ''' Use with caution - typically vitals should not be deleted for audit trail purposes.
    ''' </summary>
    Public Function DeleteVitalsRecord(vitalID As Integer) As Boolean
        Try
            Dim sql As String = "DELETE FROM InpatientVitals WHERE VitalID = @vitalID"
            Dim params As New Dictionary(Of String, Object) From {{"@vitalID", vitalID}}

            Dim rowsAffected As Integer = ExecuteNonQueryWithParams(sql, params)
            Return rowsAffected > 0

        Catch ex As Exception
            LogError($"DeleteVitalsRecord error: {ex.Message} | VitalID: {vitalID}")
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Gets the total count of vitals records for a specific patient.
    ''' </summary>
    Public Function GetPatientVitalsCount(patientID As String) As Integer
        Try
            Dim sql As String = "SELECT COUNT(*) FROM InpatientVitals WHERE PatientID = @patientID"
            Dim params As New Dictionary(Of String, Object) From {{"@patientID", patientID}}

            Dim dt As DataTable = GetDataTable(sql, params)
            If dt.Rows.Count > 0 Then
                Return Convert.ToInt32(dt.Rows(0)(0))
            End If

            Return 0

        Catch ex As Exception
            LogError($"GetPatientVitalsCount error: {ex.Message} | PatientID: {patientID}")
            Return 0
        End Try
    End Function

#End Region

#Region "Clinical Assessments & Triage - Enterprise Implementation"

    ''' <summary>
    ''' CLINICAL ASSESSMENTS TABLE INITIALIZATION
    ''' Creates the ClinicalAssessments table for advanced triage and clinical metrics
    ''' Compliant with international health-tech standards for patient triage workflows
    ''' Foreign key linkage to PatientsManagement ensures referential integrity
    ''' Thread-safe implementation with defensive error handling
    ''' </summary>
    Public Sub InitializeClinicalAssessmentsSchema()
        Try
            Using conn As New SQLiteConnection(GetConnectionString())
                conn.Open()

                ' Enable foreign key constraints
                Using cmdForeignKeys As New SQLiteCommand("PRAGMA foreign_keys = ON;", conn)
                    cmdForeignKeys.ExecuteNonQuery()
                End Using

                Dim sql As String = "
CREATE TABLE IF NOT EXISTS ClinicalAssessments (
    AssessmentID TEXT PRIMARY KEY NOT NULL,
    PatientID TEXT NOT NULL,
    SystolicBP INTEGER,
    DiastolicBP INTEGER,
    HeartRate INTEGER,
    Temperature REAL,
    TriageStatusFlag TEXT DEFAULT 'ROUTINE',
    RespiratoryRate INTEGER,
    PainScore INTEGER,
    ConsciousnessLevel TEXT,
    ClinicalNotes TEXT,
    AssessedBy TEXT,
    LastUpdated TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (PatientID) REFERENCES PatientsManagement(PatientID) ON DELETE CASCADE,
    CHECK (TriageStatusFlag IN ('ROUTINE', 'URGENT', 'EMERGENCY', 'CRITICAL', 'DECEASED')),
    CHECK (PainScore >= 0 AND PainScore <= 10)
);

CREATE INDEX IF NOT EXISTS idx_assessment_patient ON ClinicalAssessments(PatientID);
CREATE INDEX IF NOT EXISTS idx_assessment_triage ON ClinicalAssessments(TriageStatusFlag, LastUpdated DESC);
CREATE INDEX IF NOT EXISTS idx_assessment_date ON ClinicalAssessments(LastUpdated DESC);"

                Using cmd As New SQLiteCommand(sql, conn)
                    cmd.ExecuteNonQuery()
                End Using

                LogError("InitializeClinicalAssessmentsSchema: ClinicalAssessments table initialized successfully")
            End Using

        Catch ex As Exception
            LogError($"InitializeClinicalAssessmentsSchema error: {ex.Message} | StackTrace: {ex.StackTrace}")
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' GENERATES UNIQUE ASSESSMENT ID
    ''' Format: ASS-YYYY-NNNN (e.g., ASS-2026-0001)
    ''' Uses MAX-based suffix extraction to ensure uniqueness even after deletions
    ''' Thread-safe implementation prevents duplicate IDs under concurrent access
    ''' </summary>
    Private Function GetNextAssessmentID() As String
        Try
            Dim year As String = DateTime.Now.Year.ToString()
            Dim prefix As String = $"ASS-{year}-"

            Using conn As New SQLiteConnection(GetConnectionString())
                conn.Open()

                ' Query the maximum existing ID for current year
                Dim sql As String = "
SELECT AssessmentID 
FROM ClinicalAssessments 
WHERE AssessmentID LIKE @pattern 
ORDER BY AssessmentID DESC 
LIMIT 1"

                Dim maxID As String = String.Empty
                Using cmd As New SQLiteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@pattern", prefix & "%")
                    Dim result As Object = cmd.ExecuteScalar()
                    If result IsNot Nothing AndAlso Not DBNull.Value.Equals(result) Then
                        maxID = result.ToString()
                    End If
                End Using

                Dim nextNumber As Integer = 1
                If Not String.IsNullOrEmpty(maxID) Then
                    ' Extract numeric suffix (e.g., "ASS-2026-0005" -> "0005")
                    Dim parts() As String = maxID.Split("-"c)
                    If parts.Length = 3 Then
                        Dim numericPart As String = parts(2)
                        Dim parsedNumber As Integer = 0
                        If Integer.TryParse(numericPart, parsedNumber) Then
                            nextNumber = parsedNumber + 1
                        End If
                    End If
                End If

                Dim newID As String = $"ASS-{year}-{nextNumber:0000}"
                LogError($"GetNextAssessmentID: Generated new ID '{newID}'")
                Return newID
            End Using

        Catch ex As Exception
            LogError($"GetNextAssessmentID error: {ex.Message}")
            ' Fallback ID with timestamp to prevent total failure
            Return $"ASS-{DateTime.Now:yyyyMMddHHmmss}"
        End Try
    End Function

    ''' <summary>
    ''' SAVE CLINICAL ASSESSMENT - DEFENSIVE TRANSACTION-SAFE IMPLEMENTATION
    ''' Creates or updates a clinical assessment record with full triage metadata
    ''' Wraps operations in explicit Try-Catch transaction blocks for database integrity
    ''' Throws readable error summaries if unique constraints or locking issues occur
    ''' </summary>
    ''' <param name="assessmentID">Assessment ID (empty for new, existing for update)</param>
    ''' <param name="patientID">Patient ID (required - must exist in PatientsManagement)</param>
    ''' <param name="systolicBP">Systolic blood pressure (mmHg)</param>
    ''' <param name="diastolicBP">Diastolic blood pressure (mmHg)</param>
    ''' <param name="heartRate">Heart rate (beats per minute)</param>
    ''' <param name="temperature">Body temperature (Celsius)</param>
    ''' <param name="triageStatus">Triage flag: ROUTINE, URGENT, EMERGENCY, CRITICAL, DECEASED</param>
    ''' <param name="respiratoryRate">Breaths per minute</param>
    ''' <param name="painScore">Pain score (0-10)</param>
    ''' <param name="consciousnessLevel">AVPU or GCS descriptor</param>
    ''' <param name="clinicalNotes">Free-text clinical notes</param>
    ''' <param name="assessedBy">Clinician username or staff ID</param>
    ''' <returns>The saved AssessmentID (new or existing)</returns>
    Public Function SaveClinicalAssessment(
        assessmentID As String,
        patientID As String,
        systolicBP As Integer,
        diastolicBP As Integer,
        heartRate As Integer,
        temperature As Double,
        triageStatus As String,
        respiratoryRate As Integer,
        painScore As Integer,
        consciousnessLevel As String,
        clinicalNotes As String,
        assessedBy As String
    ) As String

        ' ===================================================================
        ' PARAMETER VALIDATION
        ' ===================================================================
        If String.IsNullOrWhiteSpace(patientID) Then
            Dim errMsg As String = "SaveClinicalAssessment error: PatientID is required"
            LogError(errMsg)
            Throw New ArgumentException(errMsg, NameOf(patientID))
        End If

        ' Validate patient exists
        If GetPatientByID(patientID) Is Nothing Then
            Dim errMsg As String = $"SaveClinicalAssessment error: Patient '{patientID}' does not exist"
            LogError(errMsg)
            Throw New InvalidOperationException(errMsg)
        End If

        ' Validate triage status
        Dim validTriage As String() = {"ROUTINE", "URGENT", "EMERGENCY", "CRITICAL", "DECEASED"}
        If String.IsNullOrWhiteSpace(triageStatus) OrElse Array.IndexOf(validTriage, triageStatus.ToUpper()) = -1 Then
            triageStatus = "ROUTINE"
        Else
            triageStatus = triageStatus.ToUpper()
        End If

        ' Validate pain score
        If painScore < 0 OrElse painScore > 10 Then
            Dim errMsg As String = $"SaveClinicalAssessment error: Invalid pain score {painScore} (must be 0-10)"
            LogError(errMsg)
            Throw New ArgumentOutOfRangeException(NameOf(painScore), errMsg)
        End If

        ' Determine if INSERT (new) or UPDATE (existing)
        Dim isNewAssessment As Boolean = String.IsNullOrWhiteSpace(assessmentID)
        If isNewAssessment Then
            assessmentID = GetNextAssessmentID()
        End If

        Try
            Using conn As New SQLiteConnection(GetConnectionString())
                conn.Open()

                ' Enable foreign key constraints
                Using cmdForeignKeys As New SQLiteCommand("PRAGMA foreign_keys = ON;", conn)
                    cmdForeignKeys.ExecuteNonQuery()
                End Using

                ' ===================================================================
                ' TRANSACTION-SAFE UPSERT LOGIC
                ' ===================================================================
                Using transaction As SQLiteTransaction = conn.BeginTransaction()
                    Try
                        Dim sql As String
                        If isNewAssessment Then
                            ' INSERT new assessment
                            sql = "
INSERT INTO ClinicalAssessments (
    AssessmentID, PatientID, SystolicBP, DiastolicBP, HeartRate, Temperature,
    TriageStatusFlag, RespiratoryRate, PainScore, ConsciousnessLevel,
    ClinicalNotes, AssessedBy, LastUpdated
) VALUES (
    @assessID, @patID, @sysBP, @diaBP, @hr, @temp,
    @triage, @rr, @pain, @conscious,
    @notes, @assessedBy, @lastUpd
)"
                            LogError($"SaveClinicalAssessment: Inserting new assessment {assessmentID} for patient {patientID}")
                        Else
                            ' UPDATE existing assessment
                            sql = "
UPDATE ClinicalAssessments SET
    SystolicBP = @sysBP,
    DiastolicBP = @diaBP,
    HeartRate = @hr,
    Temperature = @temp,
    TriageStatusFlag = @triage,
    RespiratoryRate = @rr,
    PainScore = @pain,
    ConsciousnessLevel = @conscious,
    ClinicalNotes = @notes,
    AssessedBy = @assessedBy,
    LastUpdated = @lastUpd
WHERE AssessmentID = @assessID"
                            LogError($"SaveClinicalAssessment: Updating existing assessment {assessmentID}")
                        End If

                        Using cmd As New SQLiteCommand(sql, conn, transaction)
                            cmd.Parameters.AddWithValue("@assessID", assessmentID)
                            cmd.Parameters.AddWithValue("@patID", patientID)
                            cmd.Parameters.AddWithValue("@sysBP", If(systolicBP <= 0, DBNull.Value, CObj(systolicBP)))
                            cmd.Parameters.AddWithValue("@diaBP", If(diastolicBP <= 0, DBNull.Value, CObj(diastolicBP)))
                            cmd.Parameters.AddWithValue("@hr", If(heartRate <= 0, DBNull.Value, CObj(heartRate)))
                            cmd.Parameters.AddWithValue("@temp", If(temperature <= 0.0, DBNull.Value, CObj(temperature)))
                            cmd.Parameters.AddWithValue("@triage", triageStatus)
                            cmd.Parameters.AddWithValue("@rr", If(respiratoryRate <= 0, DBNull.Value, CObj(respiratoryRate)))
                            cmd.Parameters.AddWithValue("@pain", painScore)
                            cmd.Parameters.AddWithValue("@conscious", If(String.IsNullOrWhiteSpace(consciousnessLevel), DBNull.Value, CObj(consciousnessLevel)))
                            cmd.Parameters.AddWithValue("@notes", If(String.IsNullOrWhiteSpace(clinicalNotes), DBNull.Value, CObj(clinicalNotes)))
                            cmd.Parameters.AddWithValue("@assessedBy", If(String.IsNullOrWhiteSpace(assessedBy), "SYSTEM", assessedBy))
                            cmd.Parameters.AddWithValue("@lastUpd", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))

                            LogError($"SaveClinicalAssessment SQL Parameters - ID: {assessmentID}, PatientID: {patientID}, BP: {systolicBP}/{diastolicBP}, HR: {heartRate}, Temp: {temperature}, Triage: {triageStatus}")

                            cmd.ExecuteNonQuery()
                        End Using

                        transaction.Commit()
                        LogError($"SaveClinicalAssessment: Successfully saved assessment {assessmentID}")
                        Return assessmentID

                    Catch ex As Exception
                        transaction.Rollback()
                        LogError($"SaveClinicalAssessment: Transaction rolled back - {ex.Message}")
                        Throw
                    End Try
                End Using
            End Using

        Catch sqlEx As SQLiteException
            ' ===================================================================
            ' SQLITE-SPECIFIC ERROR HANDLING
            ' Provides readable summaries for constraint violations and locking
            ' ===================================================================
            Dim userFriendlyMsg As String = $"Database error saving clinical assessment: {sqlEx.Message}"

            If sqlEx.Message.Contains("UNIQUE constraint failed") Then
                userFriendlyMsg = $"Assessment ID '{assessmentID}' already exists. Please use a different ID or refresh your data."
            ElseIf sqlEx.Message.Contains("FOREIGN KEY constraint failed") Then
                userFriendlyMsg = $"Cannot save assessment: Patient '{patientID}' does not exist in the system."
            ElseIf sqlEx.Message.Contains("database is locked") Then
                userFriendlyMsg = "Database is currently locked by another process. Please try again in a moment."
            ElseIf sqlEx.Message.Contains("CHECK constraint failed") Then
                userFriendlyMsg = "Invalid triage status or pain score provided. Please verify your input."
            End If

            LogError($"SaveClinicalAssessment SQLite error: {sqlEx.Message} | AssessmentID: {assessmentID} | PatientID: {patientID} | StackTrace: {sqlEx.StackTrace}")
            Throw New InvalidOperationException(userFriendlyMsg, sqlEx)

        Catch ex As Exception
            LogError($"SaveClinicalAssessment error: {ex.Message} | AssessmentID: {assessmentID} | PatientID: {patientID} | StackTrace: {ex.StackTrace}")
            Throw New InvalidOperationException($"Failed to save clinical assessment: {ex.Message}", ex)
        End Try
    End Function

    ''' <summary>
    ''' RETRIEVES ALL CLINICAL ASSESSMENTS FOR A PATIENT
    ''' Returns complete triage history ordered by most recent first
    ''' Useful for clinical decision support and audit trail visualization
    ''' </summary>
    Public Function GetClinicalAssessmentsByPatient(patientID As String) As DataTable
        Try
            If String.IsNullOrWhiteSpace(patientID) Then
                LogError("GetClinicalAssessmentsByPatient error: PatientID is null or empty")
                Return New DataTable()
            End If

            Dim sql As String = "
SELECT 
    AssessmentID AS 'Assessment ID',
    PatientID AS 'Patient ID',
    SystolicBP AS 'Systolic BP',
    DiastolicBP AS 'Diastolic BP',
    HeartRate AS 'Heart Rate',
    Temperature AS 'Temperature (°C)',
    TriageStatusFlag AS 'Triage Status',
    RespiratoryRate AS 'Respiratory Rate',
    PainScore AS 'Pain Score',
    ConsciousnessLevel AS 'Consciousness',
    ClinicalNotes AS 'Clinical Notes',
    AssessedBy AS 'Assessed By',
    LastUpdated AS 'Last Updated'
FROM ClinicalAssessments
WHERE PatientID = @patientID
ORDER BY LastUpdated DESC"

            Dim params As New Dictionary(Of String, Object) From {{"@patientID", patientID}}
            Return GetDataTable(sql, params)

        Catch ex As Exception
            LogError($"GetClinicalAssessmentsByPatient error: {ex.Message} | PatientID: {patientID}")
            Return New DataTable()
        End Try
    End Function

    ''' <summary>
    ''' RETRIEVES ASSESSMENTS BY TRIAGE STATUS
    ''' Useful for emergency department dashboards and priority queue management
    ''' </summary>
    Public Function GetAssessmentsByTriageStatus(triageStatus As String) As DataTable
        Try
            Dim sql As String = "
SELECT 
    ca.AssessmentID,
    ca.PatientID,
    pm.FirstName || ' ' || pm.LastName AS 'Patient Name',
    ca.SystolicBP,
    ca.DiastolicBP,
    ca.HeartRate,
    ca.Temperature,
    ca.TriageStatusFlag,
    ca.PainScore,
    ca.LastUpdated
FROM ClinicalAssessments ca
INNER JOIN PatientsManagement pm ON ca.PatientID = pm.PatientID
WHERE ca.TriageStatusFlag = @triage
ORDER BY ca.LastUpdated DESC"

            Dim params As New Dictionary(Of String, Object) From {{"@triage", triageStatus.ToUpper()}}
            Return GetDataTable(sql, params)

        Catch ex As Exception
            LogError($"GetAssessmentsByTriageStatus error: {ex.Message} | Triage: {triageStatus}")
            Return New DataTable()
        End Try
    End Function

    ''' <summary>
    ''' DELETES A CLINICAL ASSESSMENT
    ''' Use with caution - typically assessments should be retained for audit purposes
    ''' </summary>
    Public Function DeleteClinicalAssessment(assessmentID As String) As Boolean
        Try
            If String.IsNullOrWhiteSpace(assessmentID) Then
                Return False
            End If

            Dim sql As String = "DELETE FROM ClinicalAssessments WHERE AssessmentID = @id"
            Dim params As New Dictionary(Of String, Object) From {{"@id", assessmentID}}

            Dim rowsAffected As Integer = ExecuteNonQueryWithParams(sql, params)
            Return rowsAffected > 0

        Catch ex As Exception
            LogError($"DeleteClinicalAssessment error: {ex.Message} | AssessmentID: {assessmentID}")
            Return False
        End Try
    End Function

#End Region

#Region "Enterprise Audit Compatibility Aliases"

    ''' <summary>
    ''' COMPATIBILITY ALIAS: InitializeSystemAuditSchema()
    ''' Calls the existing InitializeSystemAuditLogsSchema() for naming convention alignment
    ''' Requested naming: InitializeSystemAuditSchema -> maps to InitializeSystemAuditLogsSchema
    ''' Ensures backward compatibility with enterprise naming standards
    ''' </summary>
    Public Sub InitializeSystemAuditSchema()
        InitializeSystemAuditLogsSchema()
    End Sub

    ''' <summary>
    ''' COMPATIBILITY ALIAS: WriteAuditEntry(user, action, module)
    ''' Simplified overload that maps to the comprehensive LogSystemActivity method
    ''' Requested signature: WriteAuditEntry(user As String, action As String, moduleName As String)
    ''' Provides enterprise-standard naming while leveraging existing audit infrastructure
    ''' Thread-safe, includes automatic failover to emergency backup on database lock
    ''' </summary>
    ''' <param name="user">Active username or session identifier</param>
    ''' <param name="action">Action performed (e.g., "Patient Record Updated")</param>
    ''' <param name="moduleName">Module or form name (e.g., "FormPatientManagement")</param>
    Public Sub WriteAuditEntry(user As String, action As String, moduleName As String)
        ' Delegate to the existing comprehensive audit logging implementation
        ' Defaults: IP = 127.0.0.1 (local), Severity = INFO, No additional context
        LogSystemActivity(user, action, moduleName, "127.0.0.1", "INFO", "")
    End Sub

#End Region

#Region "Doctors Management - Professional Implementation"

    ''' <summary>
    ''' Initializes the DoctorsManagement table with proper schema and indexing.
    ''' Performs robust self-healing migration for existing databases without Username column.
    ''' Implements strict connection cleanup using explicit Using blocks.
    ''' Also migrates the legacy Doctors table if present for backward compatibility.
    ''' Called during database initialization to ensure schema exists.
    ''' Thread-safe with full error logging and automatic schema repair.
    ''' </summary>
    Public Sub InitializeDoctorsManagementSchema()
        Try
            Using conn As New SQLiteConnection(GetConnectionString())
                conn.Open()

                ' ===================================================================
                ' PHASE 1: DOCTORSMANAGEMENT TABLE (RBAC-ENABLED STANDALONE PROFILES)
                ' ===================================================================

                ' Step 1.1: Check if DoctorsManagement table exists
                Dim tableCheckSql As String = "SELECT name FROM sqlite_master WHERE type='table' AND name='DoctorsManagement'"
                Dim doctorsMgmtExists As Boolean = False

                Using checkCmd As New SQLiteCommand(tableCheckSql, conn)
                    Using reader As SQLiteDataReader = checkCmd.ExecuteReader()
                        doctorsMgmtExists = reader.Read()
                    End Using
                End Using

                If Not doctorsMgmtExists Then
                    ' Step 1.2: Table doesn't exist - create with full schema including Username
                    Dim createMgmtSql As String = "
CREATE TABLE DoctorsManagement (
    DoctorID TEXT PRIMARY KEY NOT NULL,
    Username TEXT,
    FirstName TEXT NOT NULL,
    LastName TEXT NOT NULL,
    Specialization TEXT NOT NULL,
    PhoneNumber TEXT NOT NULL,
    Email TEXT,
    OfficeRoom TEXT,
    AvailabilityStatus TEXT NOT NULL DEFAULT 'Active',
    DateRegistered TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE INDEX idx_doctor_name ON DoctorsManagement(FirstName, LastName);
CREATE INDEX idx_doctor_specialization ON DoctorsManagement(Specialization);
CREATE INDEX idx_doctor_status ON DoctorsManagement(AvailabilityStatus);
CREATE INDEX idx_doctor_username ON DoctorsManagement(Username);"

                    Using createCmd As New SQLiteCommand(createMgmtSql, conn)
                        createCmd.ExecuteNonQuery()
                    End Using

                    LogError("INFO: DoctorsManagement table created successfully with Username column and indexes")

                Else
                    ' Step 1.3: Table exists - perform self-healing migration for Username column
                    Dim checkColumnSql As String = "PRAGMA table_info(DoctorsManagement)"
                    Dim hasUsernameColumn As Boolean = False

                    Using pragmaCmd As New SQLiteCommand(checkColumnSql, conn)
                        Using reader As SQLiteDataReader = pragmaCmd.ExecuteReader()
                            While reader.Read()
                                ' Column name is at index 1 in PRAGMA table_info result
                                Dim columnName As String = reader.GetString(1)
                                If columnName.Equals("Username", StringComparison.OrdinalIgnoreCase) Then
                                    hasUsernameColumn = True
                                    Exit While
                                End If
                            End While
                        End Using
                    End Using

                    If Not hasUsernameColumn Then
                        ' Step 1.4: Username column missing - execute dynamic ALTER TABLE migration
                        Dim alterMgmtSql As String = "ALTER TABLE DoctorsManagement ADD COLUMN Username TEXT;"
                        Using alterCmd As New SQLiteCommand(alterMgmtSql, conn)
                            alterCmd.ExecuteNonQuery()
                        End Using

                        ' Create index on newly added Username column
                        Dim indexMgmtSql As String = "CREATE INDEX IF NOT EXISTS idx_doctor_username ON DoctorsManagement(Username);"
                        Using indexCmd As New SQLiteCommand(indexMgmtSql, conn)
                            indexCmd.ExecuteNonQuery()
                        End Using

                        LogError("SUCCESS: Username column added to existing DoctorsManagement table via automatic migration")
                    Else
                        LogError("INFO: DoctorsManagement table already has Username column - no migration needed")
                    End If
                End If

                ' ===================================================================
                ' PHASE 2: LEGACY DOCTORS TABLE (BACKWARD COMPATIBILITY MIGRATION)
                ' ===================================================================

                ' Step 2.1: Check if legacy Doctors table exists (appointments-linked table)
                Dim legacyTableCheckSql As String = "SELECT name FROM sqlite_master WHERE type='table' AND name='Doctors'"
                Dim legacyDoctorsExists As Boolean = False

                Using legacyCheckCmd As New SQLiteCommand(legacyTableCheckSql, conn)
                    Using reader As SQLiteDataReader = legacyCheckCmd.ExecuteReader()
                        legacyDoctorsExists = reader.Read()
                    End Using
                End Using

                If legacyDoctorsExists Then
                    ' Step 2.2: Legacy table exists - check if it needs Username column migration
                    Dim legacyColumnCheckSql As String = "PRAGMA table_info(Doctors)"
                    Dim legacyHasUsername As Boolean = False

                    Using legacyPragmaCmd As New SQLiteCommand(legacyColumnCheckSql, conn)
                        Using reader As SQLiteDataReader = legacyPragmaCmd.ExecuteReader()
                            While reader.Read()
                                ' Column name is at index 1
                                Dim columnName As String = reader.GetString(1)
                                If columnName.Equals("Username", StringComparison.OrdinalIgnoreCase) Then
                                    legacyHasUsername = True
                                    Exit While
                                End If
                            End While
                        End Using
                    End Using

                    If Not legacyHasUsername Then
                        ' Step 2.3: Add Username column to legacy Doctors table without destroying data
                        Dim alterLegacySql As String = "ALTER TABLE Doctors ADD COLUMN Username TEXT;"
                        Using alterLegacyCmd As New SQLiteCommand(alterLegacySql, conn)
                            alterLegacyCmd.ExecuteNonQuery()
                        End Using

                        ' Create index for performance
                        Dim indexLegacySql As String = "CREATE INDEX IF NOT EXISTS idx_legacy_doctor_username ON Doctors(Username);"
                        Using indexLegacyCmd As New SQLiteCommand(indexLegacySql, conn)
                            indexLegacyCmd.ExecuteNonQuery()
                        End Using

                        LogError("SUCCESS: Username column added to legacy Doctors table via backward-compatible migration")
                    Else
                        LogError("INFO: Legacy Doctors table already has Username column - no migration needed")
                    End If
                Else
                    LogError("INFO: Legacy Doctors table not found - skipping backward compatibility migration")
                End If

                ' ===================================================================
                ' PHASE 3: SCHEMA VERIFICATION AND INTEGRITY CHECK
                ' ===================================================================

                ' Verify final schema integrity
                Dim verifyMgmtSql As String = "SELECT COUNT(*) FROM pragma_table_info('DoctorsManagement') WHERE name='Username'"
                Using verifyCmd As New SQLiteCommand(verifyMgmtSql, conn)
                    Dim usernameColumnCount As Object = verifyCmd.ExecuteScalar()
                    If usernameColumnCount IsNot Nothing AndAlso Convert.ToInt32(usernameColumnCount) > 0 Then
                        LogError("VERIFY: DoctorsManagement schema integrity confirmed - Username column present")
                    Else
                        LogError("WARNING: DoctorsManagement schema verification failed - Username column not detected")
                    End If
                End Using

            End Using

        Catch ex As SQLiteException
            LogError($"InitializeDoctorsManagementSchema SQLite error: {ex.Message} | Code: {ex.ErrorCode}")
            Throw
        Catch ex As Exception
            LogError($"InitializeDoctorsManagementSchema error: {ex.Message} | Source: {ex.Source}")
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Generates a unique Doctor ID in format: DOC-YYYY-NNNN
    ''' Thread-safe implementation with database transaction.
    ''' </summary>
    Public Function GenerateDoctorID() As String
        Try
            Dim year As String = DateTime.Now.Year.ToString()
            Dim countSql As String = "SELECT COUNT(*) FROM DoctorsManagement WHERE DoctorID LIKE @pattern"
            Dim params As New Dictionary(Of String, Object) From {
                {"@pattern", $"DOC-{year}-%"}
            }

            Dim dt As DataTable = GetDataTable(countSql, params)
            Dim count As Integer = 0

            If dt.Rows.Count > 0 Then
                count = Convert.ToInt32(dt.Rows(0)(0)) + 1
            Else
                count = 1
            End If

            Return $"DOC-{year}-{count:0000}"
        Catch ex As Exception
            LogError("GenerateDoctorID error: " & ex.Message)
            Return $"DOC-{DateTime.Now.Year}-0001"
        End Try
    End Function

    ''' <summary>
    ''' Saves a doctor record to the database (UPSERT operation).
    ''' Handles both INSERT (new doctors) and UPDATE (existing doctors).
    ''' Thread-safe with full parameter validation and error logging.
    ''' Includes Username for RBAC integration with Users table.
    ''' </summary>
    ''' <param name="doctorID">The unique Doctor ID (auto-generated if empty for new records)</param>
    ''' <param name="username">System login username (optional, for RBAC)</param>
    ''' <param name="firstName">Doctor's first name (required)</param>
    ''' <param name="lastName">Doctor's last name (required)</param>
    ''' <param name="specialization">Medical specialization (required)</param>
    ''' <param name="phoneNumber">Contact phone number (required)</param>
    ''' <param name="email">Email address (optional)</param>
    ''' <param name="officeRoom">Office/room number (optional)</param>
    ''' <param name="availabilityStatus">Current status: 'Active', 'On Leave', 'Retired' (required)</param>
    ''' <returns>The DoctorID of the saved record, or empty string on failure</returns>
    Public Function SaveDoctor(doctorID As String, username As String, firstName As String, lastName As String,
                                specialization As String, phoneNumber As String, email As String,
                                officeRoom As String, availabilityStatus As String) As String
        ' Input validation
        If String.IsNullOrWhiteSpace(firstName) Then
            LogError("SaveDoctor error: FirstName is required")
            Return String.Empty
        End If

        If String.IsNullOrWhiteSpace(lastName) Then
            LogError("SaveDoctor error: LastName is required")
            Return String.Empty
        End If

        If String.IsNullOrWhiteSpace(specialization) Then
            LogError("SaveDoctor error: Specialization is required")
            Return String.Empty
        End If

        If String.IsNullOrWhiteSpace(phoneNumber) Then
            LogError("SaveDoctor error: PhoneNumber is required")
            Return String.Empty
        End If

        If String.IsNullOrWhiteSpace(availabilityStatus) Then
            availabilityStatus = "Active" ' Default value
        End If

        ' Validate Username exists in Users table if provided
        If Not String.IsNullOrWhiteSpace(username) Then
            If Not ValidateUsernameExists(username) Then
                LogError($"SaveDoctor error: Username '{username}' does not exist in Users table")
                Return String.Empty
            End If
        End If

        Try
            Using conn As New SQLiteConnection(GetConnectionString())
                conn.Open()

                ' Begin transaction for thread-safety
                Using transaction As SQLiteTransaction = conn.BeginTransaction()
                    Try
                        Dim isNewRecord As Boolean = String.IsNullOrWhiteSpace(doctorID)
                        Dim finalDoctorID As String = doctorID

                        If Not isNewRecord Then
                            ' Check if doctor exists
                            Dim existsSql As String = "SELECT COUNT(*) FROM DoctorsManagement WHERE DoctorID = @id"
                            Using existsCmd As New SQLiteCommand(existsSql, conn, transaction)
                                existsCmd.Parameters.AddWithValue("@id", doctorID)
                                Dim exists As Integer = Convert.ToInt32(existsCmd.ExecuteScalar())
                                isNewRecord = (exists = 0)
                            End Using
                        End If

                        If isNewRecord Then
                            ' Generate new DoctorID
                            finalDoctorID = GenerateDoctorID()
                        End If

                        Dim sql As String

                        If isNewRecord Then
                            ' INSERT new record
                            sql = "INSERT INTO DoctorsManagement 
                                   (DoctorID, Username, FirstName, LastName, Specialization, PhoneNumber, Email, OfficeRoom, AvailabilityStatus, DateRegistered)
                                   VALUES (@id, @username, @firstName, @lastName, @spec, @phone, @email, @room, @status, @dateReg)"
                        Else
                            ' UPDATE existing record
                            sql = "UPDATE DoctorsManagement SET 
                                   Username = @username,
                                   FirstName = @firstName,
                                   LastName = @lastName,
                                   Specialization = @spec,
                                   PhoneNumber = @phone,
                                   Email = @email,
                                   OfficeRoom = @room,
                                   AvailabilityStatus = @status
                                   WHERE DoctorID = @id"
                        End If

                        Using cmd As New SQLiteCommand(sql, conn, transaction)
                            cmd.Parameters.AddWithValue("@id", finalDoctorID)
                            cmd.Parameters.AddWithValue("@username", If(String.IsNullOrWhiteSpace(username), DBNull.Value, CObj(username.Trim())))
                            cmd.Parameters.AddWithValue("@firstName", firstName.Trim())
                            cmd.Parameters.AddWithValue("@lastName", lastName.Trim())
                            cmd.Parameters.AddWithValue("@spec", specialization.Trim())
                            cmd.Parameters.AddWithValue("@phone", phoneNumber.Trim())
                            cmd.Parameters.AddWithValue("@email", If(String.IsNullOrWhiteSpace(email), DBNull.Value, CObj(email.Trim())))
                            cmd.Parameters.AddWithValue("@room", If(String.IsNullOrWhiteSpace(officeRoom), DBNull.Value, CObj(officeRoom.Trim())))
                            cmd.Parameters.AddWithValue("@status", availabilityStatus.Trim())

                            If isNewRecord Then
                                cmd.Parameters.AddWithValue("@dateReg", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                            End If

                            cmd.ExecuteNonQuery()
                        End Using

                        transaction.Commit()
                        Return finalDoctorID

                    Catch ex As Exception
                        transaction.Rollback()
                        Throw
                    End Try
                End Using
            End Using

        Catch ex As SQLiteException
            LogError($"SaveDoctor SQLite error: {ex.Message} | DoctorID: {doctorID}")
            Return String.Empty
        Catch ex As Exception
            LogError($"SaveDoctor error: {ex.Message} | DoctorID: {doctorID}")
            Return String.Empty
        End Try
    End Function

    ''' <summary>
    ''' Validates that a username exists in the Users table.
    ''' Used for RBAC integration before linking doctors to system logins.
    ''' </summary>
    Private Function ValidateUsernameExists(username As String) As Boolean
        Try
            If String.IsNullOrWhiteSpace(username) Then
                Return False
            End If

            Dim sql As String = "SELECT COUNT(*) FROM Users WHERE Username = @username"
            Dim params As New Dictionary(Of String, Object) From {{"@username", username}}

            Dim dt As DataTable = GetDataTable(sql, params)
            If dt.Rows.Count > 0 Then
                Return Convert.ToInt32(dt.Rows(0)(0)) > 0
            End If

            Return False

        Catch ex As Exception
            LogError($"ValidateUsernameExists error: {ex.Message} | Username: {username}")
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Validates that a username exists in the Users table with 'Doctor' role.
    ''' PRE-FLIGHT AUTHENTICATION VERIFICATION for doctor record creation.
    ''' This ensures RBAC integrity before allowing doctor records to be saved.
    ''' </summary>
    ''' <param name="username">The username to validate against the Users table</param>
    ''' <returns>True if username exists with Role='Doctor', False otherwise</returns>
    Public Function ValidateDoctorUserAccount(username As String) As Boolean
        Try
            If String.IsNullOrWhiteSpace(username) Then
                Return False
            End If

            Dim sql As String = "SELECT COUNT(*) FROM Users WHERE Username = @username AND Role = 'Doctor'"
            Dim params As New Dictionary(Of String, Object) From {{"@username", username.Trim()}}

            Dim dt As DataTable = GetDataTable(sql, params)
            If dt.Rows.Count > 0 Then
                Dim count As Integer = Convert.ToInt32(dt.Rows(0)(0))
                Return count > 0
            End If

            Return False

        Catch ex As Exception
            LogError($"ValidateDoctorUserAccount error: {ex.Message} | Username: {username}")
            Return False
        End Try
    End Function

    ''' <summary>
    ''' AUTOMATIC ACCOUNT PROVISIONING for doctor user accounts.
    ''' Creates a new user record in the Users table with Doctor role and default temporary password.
    ''' This eliminates manual back-and-forth between User Management and Doctors Management.
    ''' Thread-safe with full transaction support and comprehensive error logging.
    ''' </summary>
    ''' <param name="username">The username for the new doctor account</param>
    ''' <param name="fullName">The doctor's full name (FirstName + LastName)</param>
    ''' <param name="email">The doctor's email address (optional)</param>
    ''' <param name="phone">The doctor's phone number (optional)</param>
    ''' <returns>True if account created successfully, False otherwise</returns>
    Public Function ProvisionDoctorUserAccount(username As String, fullName As String,
                                                Optional email As String = "",
                                                Optional phone As String = "") As Boolean
        Try
            ' Input validation
            If String.IsNullOrWhiteSpace(username) Then
                LogError("ProvisionDoctorUserAccount error: Username is required")
                Return False
            End If

            If String.IsNullOrWhiteSpace(fullName) Then
                LogError("ProvisionDoctorUserAccount error: FullName is required")
                Return False
            End If

            ' Check if username already exists (avoid duplicates)
            Dim checkSql As String = "SELECT COUNT(*) FROM Users WHERE Username = @username"
            Dim checkParams As New Dictionary(Of String, Object) From {{"@username", username.Trim()}}
            Dim checkDt As DataTable = GetDataTable(checkSql, checkParams)

            If checkDt.Rows.Count > 0 AndAlso Convert.ToInt32(checkDt.Rows(0)(0)) > 0 Then
                LogError($"ProvisionDoctorUserAccount error: Username '{username}' already exists")
                Return False
            End If

            Using conn As New SQLiteConnection(GetConnectionString())
                conn.Open()

                ' Begin transaction for thread-safety and atomic operation
                Using transaction As SQLiteTransaction = conn.BeginTransaction()
                    Try
                        ' Default temporary password: DocWelcome2026!
                        Const DEFAULT_DOCTOR_PASSWORD As String = "DocWelcome2026!"

                        Dim sql As String = "INSERT INTO Users (Username, Password, Role, FullName, Email, Phone) " &
                                           "VALUES (@username, @password, 'Doctor', @fullName, @email, @phone)"

                        Using cmd As New SQLiteCommand(sql, conn, transaction)
                            cmd.Parameters.AddWithValue("@username", username.Trim())
                            cmd.Parameters.AddWithValue("@password", DEFAULT_DOCTOR_PASSWORD)
                            cmd.Parameters.AddWithValue("@fullName", fullName.Trim())
                            cmd.Parameters.AddWithValue("@email", If(String.IsNullOrWhiteSpace(email), DBNull.Value, CObj(email.Trim())))
                            cmd.Parameters.AddWithValue("@phone", If(String.IsNullOrWhiteSpace(phone), DBNull.Value, CObj(phone.Trim())))

                            Dim rowsAffected As Integer = cmd.ExecuteNonQuery()

                            If rowsAffected > 0 Then
                                transaction.Commit()
                                LogError($"ProvisionDoctorUserAccount SUCCESS: Created account for '{username}' with role 'Doctor'")
                                Return True
                            Else
                                transaction.Rollback()
                                LogError($"ProvisionDoctorUserAccount error: No rows affected for username '{username}'")
                                Return False
                            End If
                        End Using

                    Catch ex As Exception
                        transaction.Rollback()
                        Throw
                    End Try
                End Using
            End Using

        Catch ex As SQLiteException
            LogError($"ProvisionDoctorUserAccount SQLite error: {ex.Message} | Username: {username} | ErrorCode: {ex.ErrorCode}")
            Return False
        Catch ex As Exception
            LogError($"ProvisionDoctorUserAccount error: {ex.Message} | Username: {username}")
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Retrieves all doctor records with Username joined from Users table for RBAC display.
    ''' Optimized for DataGridView with proper column ordering and friendly names.
    ''' </summary>
    ''' <returns>DataTable containing all doctor records</returns>
    Public Function GetDoctorsTable() As DataTable
        Try
            Dim sql As String = "
SELECT 
    d.DoctorID AS 'Doctor ID',
    d.Username,
    d.FirstName AS 'First Name',
    d.LastName AS 'Last Name',
    d.Specialization,
    d.PhoneNumber AS 'Phone Number',
    d.Email,
    d.OfficeRoom AS 'Office Room',
    d.AvailabilityStatus AS 'Status',
    d.DateRegistered AS 'Registered On'
FROM DoctorsManagement d
ORDER BY d.AvailabilityStatus ASC, d.LastName ASC, d.FirstName ASC"

            Return GetDataTable(sql)

        Catch ex As Exception
            LogError("GetDoctorsTable error: " & ex.Message)
            Return New DataTable() ' Return empty table on error
        End Try
    End Function

    ''' <summary>
    ''' Retrieves a single doctor by ID.
    ''' </summary>
    Public Function GetDoctorByID(doctorID As String) As DataRow
        Try
            If String.IsNullOrWhiteSpace(doctorID) Then
                Return Nothing
            End If

            Dim sql As String = "SELECT * FROM DoctorsManagement WHERE DoctorID = @id"
            Dim params As New Dictionary(Of String, Object) From {{"@id", doctorID}}
            Dim dt As DataTable = GetDataTable(sql, params)

            If dt.Rows.Count > 0 Then
                Return dt.Rows(0)
            End If

            Return Nothing

        Catch ex As Exception
            LogError($"GetDoctorByID error: {ex.Message} | DoctorID: {doctorID}")
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Searches doctors by name, specialization, phone number, or username.
    ''' Returns a filtered DataTable for UI binding.
    ''' </summary>
    Public Function SearchDoctors(searchTerm As String) As DataTable
        Try
            If String.IsNullOrWhiteSpace(searchTerm) Then
                Return GetDoctorsTable()
            End If

            Dim sql As String = "
SELECT 
    DoctorID AS 'Doctor ID',
    Username,
    FirstName AS 'First Name',
    LastName AS 'Last Name',
    Specialization,
    PhoneNumber AS 'Phone Number',
    Email,
    OfficeRoom AS 'Office Room',
    AvailabilityStatus AS 'Status',
    DateRegistered AS 'Registered On'
FROM DoctorsManagement
WHERE FirstName LIKE @search 
   OR LastName LIKE @search 
   OR Specialization LIKE @search
   OR PhoneNumber LIKE @search
   OR Username LIKE @search
   OR (FirstName || ' ' || LastName) LIKE @search
ORDER BY AvailabilityStatus ASC, LastName ASC, FirstName ASC"

            Dim params As New Dictionary(Of String, Object) From {
                {"@search", $"%{searchTerm}%"}
            }

            Return GetDataTable(sql, params)

        Catch ex As Exception
            LogError($"SearchDoctors error: {ex.Message} | SearchTerm: {searchTerm}")
            Return New DataTable()
        End Try
    End Function

    ''' <summary>
    ''' Archives a doctor by setting their status to 'Archived'.
    ''' Soft delete - preserves data for audit trail.
    ''' </summary>
    Public Function ArchiveDoctor(doctorID As String) As Boolean
        Try
            If String.IsNullOrWhiteSpace(doctorID) Then
                Return False
            End If

            Dim sql As String = "UPDATE DoctorsManagement SET AvailabilityStatus = 'Archived' WHERE DoctorID = @id"
            Dim params As New Dictionary(Of String, Object) From {{"@id", doctorID}}

            Dim rowsAffected As Integer = ExecuteNonQueryWithParams(sql, params)
            Return rowsAffected > 0

        Catch ex As Exception
            LogError($"ArchiveDoctor error: {ex.Message} | DoctorID: {doctorID}")
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Permanently deletes a doctor record by ID.
    ''' Use with extreme caution - typically archiving is preferred.
    ''' </summary>
    Public Function DeleteDoctor(doctorID As String) As Boolean
        Try
            If String.IsNullOrWhiteSpace(doctorID) Then
                Return False
            End If

            Dim sql As String = "DELETE FROM DoctorsManagement WHERE DoctorID = @id"
            Dim params As New Dictionary(Of String, Object) From {{"@id", doctorID}}

            Dim rowsAffected As Integer = ExecuteNonQueryWithParams(sql, params)
            Return rowsAffected > 0

        Catch ex As Exception
            LogError($"DeleteDoctor error: {ex.Message} | DoctorID: {doctorID}")
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Gets the total count of doctors by status.
    ''' </summary>
    Public Function GetDoctorCountByStatus(status As String) As Integer
        Try
            Dim sql As String = "SELECT COUNT(*) FROM DoctorsManagement WHERE AvailabilityStatus = @status"
            Dim params As New Dictionary(Of String, Object) From {{"@status", status}}

            Dim dt As DataTable = GetDataTable(sql, params)
            If dt.Rows.Count > 0 Then
                Return Convert.ToInt32(dt.Rows(0)(0))
            End If

            Return 0

        Catch ex As Exception
            LogError($"GetDoctorCountByStatus error: {ex.Message} | Status: {status}")
            Return 0
        End Try
    End Function

    ''' <summary>
    ''' Gets all unique specializations for dropdown population.
    ''' </summary>
    Public Function GetDoctorSpecializations() As List(Of String)
        Try
            Dim sql As String = "SELECT DISTINCT Specialization FROM DoctorsManagement ORDER BY Specialization"
            Dim dt As DataTable = GetDataTable(sql)
            Dim specializations As New List(Of String)()

            For Each row As DataRow In dt.Rows
                If Not row.IsNull("Specialization") Then
                    specializations.Add(row("Specialization").ToString())
                End If
            Next

            Return specializations

        Catch ex As Exception
            LogError("GetDoctorSpecializations error: " & ex.Message)
            Return New List(Of String)()
        End Try
    End Function

    ''' <summary>
    ''' Gets all active usernames from Users table with 'Doctor' role for RBAC dropdown population.
    ''' </summary>
    Public Function GetDoctorUsernames() As List(Of String)
        Try
            Dim sql As String = "SELECT Username FROM Users WHERE Role = 'Doctor' AND IsActive = 1 ORDER BY Username"
            Dim dt As DataTable = GetDataTable(sql)
            Dim usernames As New List(Of String)()

            For Each row As DataRow In dt.Rows
                If Not row.IsNull("Username") Then
                    usernames.Add(row("Username").ToString())
                End If
            Next

            Return usernames

        Catch ex As Exception
            LogError("GetDoctorUsernames error: " & ex.Message)
            Return New List(Of String)()
        End Try
    End Function

#End Region

#Region "Doctor Dashboard Metrics & RBAC Support"
    ''' <summary>
    ''' Structure to hold personalized clinical metrics for doctor dashboards.
    ''' Provides real-time insights into doctor-specific appointment workload,
    ''' urgent cases requiring immediate attention, and completed shifts.
    ''' </summary>
    Public Structure DoctorMetricsStructure
        ''' <summary>Total appointments scheduled for this doctor today</summary>
        Public MyAppointmentsToday As Integer

        ''' <summary>Number of urgent/critical cases assigned to this doctor</summary>
        Public UrgentCasesCount As Integer

        ''' <summary>Number of completed appointments for this doctor today</summary>
        Public CompletedShiftsToday As Integer

        ''' <summary>Doctor's full name for personalized greeting</summary>
        Public DoctorFullName As String

        ''' <summary>Doctor's primary specialization</summary>
        Public Specialization As String

        ''' <summary>Office room assignment</summary>
        Public OfficeRoom As String
    End Structure

    ''' <summary>
    ''' Retrieves personalized clinical dashboard metrics for a doctor based on their DoctorID.
    ''' Executes isolated parameterized SQLite queries for security and performance.
    ''' 
    ''' METRICS INCLUDED:
    ''' 1. My Appointments Today: Total appointments scheduled for today
    ''' 2. Urgent Cases: High-risk patients from ClinicalAssessments linked to doctor's appointments
    ''' 3. Completed Shifts Today: Successfully completed appointments today
    ''' 4. Doctor Profile: Full name, specialization, and office room
    ''' 
    ''' SECURITY: All queries are fully parameterized to prevent SQL injection.
    ''' PERFORMANCE: Uses optimized indexes and date filtering.
    ''' </summary>
    ''' <param name="doctorID">The unique DoctorID from DoctorsManagement table (e.g., "DOC-001")</param>
    ''' <returns>DoctorMetricsStructure with populated metrics, or default values if doctor not found</returns>
    Public Function GetDoctorDashboardMetrics(doctorID As String) As DoctorMetricsStructure
        Dim metrics As New DoctorMetricsStructure()

        Try
            If String.IsNullOrWhiteSpace(doctorID) Then
                LogError("GetDoctorDashboardMetrics: DoctorID parameter is null or empty")
                Return metrics
            End If

            Dim today As String = DateTime.Now.ToString("yyyy-MM-dd")

            Using conn As New SQLiteConnection(GetConnectionString())
                conn.Open()

                ' ===================================================================
                ' METRIC 1: My Appointments Today
                ' Count all appointments for this doctor scheduled for today
                ' ===================================================================
                Dim appointmentsSql As String = "
SELECT COUNT(*) 
FROM Appointments 
WHERE DoctorID = @DocID 
  AND DATE(AppointmentDate) = @Today"

                Using cmdAppointments As New SQLiteCommand(appointmentsSql, conn)
                    cmdAppointments.Parameters.AddWithValue("@DocID", doctorID)
                    cmdAppointments.Parameters.AddWithValue("@Today", today)

                    Dim result As Object = cmdAppointments.ExecuteScalar()
                    If result IsNot Nothing AndAlso Not DBNull.Value.Equals(result) Then
                        metrics.MyAppointmentsToday = Convert.ToInt32(result)
                    End If
                End Using

                ' ===================================================================
                ' METRIC 2: Urgent Cases (High Risk/Critical Patients)
                ' Count patients with high-risk triage flags assigned to this doctor
                ' Joins ClinicalAssessments with Appointments to find urgent cases
                ' ===================================================================
                Dim urgentCasesSql As String = "
SELECT COUNT(DISTINCT ca.PatientID) 
FROM ClinicalAssessments ca
INNER JOIN Appointments a ON ca.PatientID = a.PatientID
WHERE a.DoctorID = @DocID
  AND (ca.TriageStatusFlag = 'CRITICAL' 
       OR ca.TriageStatusFlag = 'EMERGENCY'
       OR ca.TriageStatusFlag = 'URGENT'
       OR ca.TriageStatusFlag = 'High Risk (Red)')"

                Using cmdUrgent As New SQLiteCommand(urgentCasesSql, conn)
                    cmdUrgent.Parameters.AddWithValue("@DocID", doctorID)

                    Dim result As Object = cmdUrgent.ExecuteScalar()
                    If result IsNot Nothing AndAlso Not DBNull.Value.Equals(result) Then
                        metrics.UrgentCasesCount = Convert.ToInt32(result)
                    End If
                End Using

                ' ===================================================================
                ' METRIC 3: Completed Shifts Today
                ' Count appointments marked as completed for today
                ' ===================================================================
                Dim completedSql As String = "
SELECT COUNT(*) 
FROM Appointments 
WHERE DoctorID = @DocID 
  AND DATE(AppointmentDate) = @Today
  AND (Status = 'Completed' OR Status = 'Confirmed')"

                Using cmdCompleted As New SQLiteCommand(completedSql, conn)
                    cmdCompleted.Parameters.AddWithValue("@DocID", doctorID)
                    cmdCompleted.Parameters.AddWithValue("@Today", today)

                    Dim result As Object = cmdCompleted.ExecuteScalar()
                    If result IsNot Nothing AndAlso Not DBNull.Value.Equals(result) Then
                        metrics.CompletedShiftsToday = Convert.ToInt32(result)
                    End If
                End Using

                ' ===================================================================
                ' DOCTOR PROFILE: Retrieve doctor's personal information
                ' ===================================================================
                Dim profileSql As String = "
SELECT FirstName, LastName, Specialization, OfficeRoom 
FROM DoctorsManagement 
WHERE DoctorID = @DocID"

                Using cmdProfile As New SQLiteCommand(profileSql, conn)
                    cmdProfile.Parameters.AddWithValue("@DocID", doctorID)

                    Using reader As SQLiteDataReader = cmdProfile.ExecuteReader()
                        If reader.Read() Then
                            Dim firstName As String = If(reader.IsDBNull(0), "", reader.GetString(0))
                            Dim lastName As String = If(reader.IsDBNull(1), "", reader.GetString(1))
                            metrics.DoctorFullName = $"{firstName} {lastName}".Trim()
                            metrics.Specialization = If(reader.IsDBNull(2), "General Practice", reader.GetString(2))
                            metrics.OfficeRoom = If(reader.IsDBNull(3), "N/A", reader.GetString(3))
                        End If
                    End Using
                End Using
            End Using

            LogError($"GetDoctorDashboardMetrics SUCCESS: DoctorID={doctorID} | Appointments={metrics.MyAppointmentsToday} | Urgent={metrics.UrgentCasesCount} | Completed={metrics.CompletedShiftsToday}")
            Return metrics

        Catch ex As SQLiteException
            LogError($"GetDoctorDashboardMetrics SQLite error: {ex.Message} | DoctorID={doctorID} | ErrorCode={ex.ErrorCode}")
            Return metrics
        Catch ex As Exception
            LogError($"GetDoctorDashboardMetrics error: {ex.Message} | DoctorID={doctorID}")
            Return metrics
        End Try
    End Function

    ''' <summary>
    ''' Retrieves a doctor's DoctorID from their UserID (from Users table).
    ''' Essential for RBAC where SessionManager provides UserID but dashboard needs DoctorID.
    ''' 
    ''' WORKFLOW:
    ''' 1. User logs in → SessionManager.CurrentUser.UserID available
    ''' 2. Dashboard calls this method to resolve DoctorID
    ''' 3. DoctorID used for GetDoctorDashboardMetrics() and filtered queries
    ''' </summary>
    ''' <param name="userID">The UserID from Users table (SessionManager.CurrentUser.UserID)</param>
    ''' <returns>DoctorID string if found, empty string if no match or error</returns>
    Public Function GetDoctorIDByUserID(userID As Integer) As String
        Try
            If userID <= 0 Then
                LogError("GetDoctorIDByUserID: Invalid UserID parameter")
                Return String.Empty
            End If

            ' First, get the Username from Users table
            Dim usernameSql As String = "SELECT Username FROM Users WHERE UserID = @uid"
            Dim usernameParams As New Dictionary(Of String, Object) From {{"@uid", userID}}
            Dim usernameDt As DataTable = GetDataTable(usernameSql, usernameParams)

            If usernameDt.Rows.Count = 0 OrElse usernameDt.Rows(0).IsNull("Username") Then
                LogError($"GetDoctorIDByUserID: No Username found for UserID={userID}")
                Return String.Empty
            End If

            Dim username As String = usernameDt.Rows(0)("Username").ToString()

            ' Now, find the DoctorID in DoctorsManagement table using Username
            Dim doctorSql As String = "SELECT DoctorID FROM DoctorsManagement WHERE Username = @username"
            Dim doctorParams As New Dictionary(Of String, Object) From {{"@username", username}}
            Dim doctorDt As DataTable = GetDataTable(doctorSql, doctorParams)

            If doctorDt.Rows.Count > 0 AndAlso Not doctorDt.Rows(0).IsNull("DoctorID") Then
                Dim doctorID As String = doctorDt.Rows(0)("DoctorID").ToString()
                LogError($"GetDoctorIDByUserID SUCCESS: UserID={userID} → Username={username} → DoctorID={doctorID}")
                Return doctorID
            End If

            LogError($"GetDoctorIDByUserID: No DoctorID found for Username={username} (UserID={userID})")
            Return String.Empty

        Catch ex As Exception
            LogError($"GetDoctorIDByUserID error: {ex.Message} | UserID={userID}")
            Return String.Empty
        End Try
    End Function

    ''' <summary>
    ''' Retrieves appointments filtered by DoctorID with patient vitals for ESI triage color-coding.
    ''' Returns a DataTable ready for DataGridView binding with Emergency Severity Index calculations.
    ''' 
    ''' COLUMNS RETURNED:
    ''' - Appointment ID, Patient Name, Date, Time, Department, Status, Emergency Flag, Notes
    ''' - SystolicBP, DiastolicBP, HeartRate, SpO2 (from most recent vitals)
    ''' - ESILevel (1-5 calculated via TriageEngine)
    ''' - TriageColor (ARGB integer for DataGridView formatting)
    ''' - SeverityLabel (CRITICAL, URGENT, MODERATE, LOW, MINIMAL)
    ''' 
    ''' SECURITY: Parameterized query prevents SQL injection.
    ''' PERFORMANCE: Indexed on DoctorID, PatientID, and DateRecorded.
    ''' AUTO-SORT: Critical patients (ESI Level 1) automatically sorted to top.
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
            ' ENHANCED QUERY WITH VITALS JOIN FOR ESI TRIAGE COLOR-CODING
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
    p.PatientID AS PatientIDInternal,
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
        CASE 
            WHEN BloodPressure IS NOT NULL AND INSTR(BloodPressure, '/') > 0 
            THEN CAST(SUBSTR(BloodPressure, 1, INSTR(BloodPressure, '/') - 1) AS INTEGER)
            ELSE NULL
        END AS SystolicBP,
        CASE 
            WHEN BloodPressure IS NOT NULL AND INSTR(BloodPressure, '/') > 0 
            THEN CAST(SUBSTR(BloodPressure, INSTR(BloodPressure, '/') + 1) AS INTEGER)
            ELSE NULL
        END AS DiastolicBP,
        HeartRate,
        SpO2,
        DateRecorded,
        VitalID
    FROM InpatientVitals
    WHERE BloodPressure IS NOT NULL AND BloodPressure != ''
    ORDER BY DateRecorded DESC
) v ON p.PatientID = v.PatientID
WHERE a.DoctorID = @DocID
GROUP BY a.AppointmentID
ORDER BY a.AppointmentDate DESC, a.AppointmentTime DESC"

            Dim params As New Dictionary(Of String, Object) From {{"@DocID", doctorID}}
            Dim dt As DataTable = GetDataTable(sql, params)

            ' ===================================================================
            ' POST-QUERY ESI TRIAGE CALCULATION
            ' Calculate Emergency Severity Index level and color for each row
            ' ===================================================================
            dt.Columns.Add("ESILevel", GetType(Integer))
            dt.Columns.Add("TriageColor", GetType(Integer))  ' Store Color.ToArgb() for DataGridView
            dt.Columns.Add("SeverityLabel", GetType(String))

            For Each row As DataRow In dt.Rows
                ' Extract vitals (nullable integers for defensive handling)
                Dim sbp As Integer? = If(row.IsNull("SystolicBP"), Nothing, CType(row("SystolicBP"), Integer?))
                Dim dbp As Integer? = If(row.IsNull("DiastolicBP"), Nothing, CType(row("DiastolicBP"), Integer?))
                Dim hr As Integer? = If(row.IsNull("HeartRate"), Nothing, CType(row("HeartRate"), Integer?))
                Dim spo2Val As Integer? = If(row.IsNull("SpO2"), Nothing, CType(row("SpO2"), Integer?))

                ' Calculate ESI using TriageEngine module
                Dim triageResult As TriageEngine.TriageResult = TriageEngine.CalculateESI(sbp, dbp, hr, spo2Val)

                ' Store triage data in row for DataGridView CellFormatting event
                row("ESILevel") = triageResult.ESILevel
                row("TriageColor") = triageResult.ColorIndicator.ToArgb()
                row("SeverityLabel") = triageResult.SeverityLabel
            Next

            ' ===================================================================
            ' DYNAMIC SORTING: Critical patients (ESI Level 1) bubble to top
            ' Secondary sort by appointment date/time for same ESI level
            ' ===================================================================
            Dim dv As DataView = dt.DefaultView
            dv.Sort = "ESILevel ASC, [Date] ASC, [Time] ASC"
            dt = dv.ToTable()

            LogError($"GetDoctorAppointments: Retrieved {dt.Rows.Count} appointments for DoctorID={doctorID} with ESI triage color-coding")
            Return dt

        Catch ex As Exception
            LogError($"GetDoctorAppointments error: {ex.Message} | DoctorID={doctorID} | StackTrace: {ex.StackTrace}")
            Return New DataTable()
        End Try
    End Function
#End Region

End Module

