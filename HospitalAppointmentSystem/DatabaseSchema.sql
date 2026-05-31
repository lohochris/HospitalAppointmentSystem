-- ============================================================
-- Hospital Appointment System - SQLite Database Schema
-- CSC3226 Group Project
-- Group Members: Sa'id Umar, Aisha Ladan, Maryam Rabiu
-- ============================================================

-- Users table (Admin, Doctor, Patient, Receptionist)
CREATE TABLE IF NOT EXISTS Users (
    UserID      INTEGER PRIMARY KEY AUTOINCREMENT,
    Username    TEXT NOT NULL UNIQUE,
    Password    TEXT NOT NULL,
    Role        TEXT NOT NULL CHECK(Role IN ('Admin','Doctor','Patient','Receptionist')),
    FullName    TEXT NOT NULL,
    Email       TEXT,
    Phone       TEXT,
    IsActive    INTEGER DEFAULT 1,
    CreatedDate TEXT DEFAULT (datetime('now'))
);

-- Departments
CREATE TABLE IF NOT EXISTS Departments (
    DepartmentID   INTEGER PRIMARY KEY AUTOINCREMENT,
    DepartmentName TEXT NOT NULL UNIQUE,
    Description    TEXT
);

-- Doctors
CREATE TABLE IF NOT EXISTS Doctors (
    DoctorID        INTEGER PRIMARY KEY AUTOINCREMENT,
    UserID          INTEGER REFERENCES Users(UserID),
    DepartmentID    INTEGER REFERENCES Departments(DepartmentID),
    Specialization  TEXT NOT NULL,
    WorkingDays     TEXT DEFAULT 'Mon,Tue,Wed,Thu,Fri',
    StartTime       TEXT DEFAULT '09:00',
    EndTime         TEXT DEFAULT '17:00',
    LunchStart      TEXT DEFAULT '13:00',
    LunchEnd        TEXT DEFAULT '14:00',
    SlotDuration    INTEGER DEFAULT 30
);

-- Patients
CREATE TABLE IF NOT EXISTS Patients (
    PatientID        INTEGER PRIMARY KEY AUTOINCREMENT,
    UserID           INTEGER REFERENCES Users(UserID),
    DateOfBirth      TEXT,
    Gender           TEXT CHECK(Gender IN ('Male','Female','Other')),
    Address          TEXT,
    BloodGroup       TEXT,
    EmergencyContact TEXT,
    EmergencyPhone   TEXT,
    InsuranceNumber  TEXT,
    RegisteredDate   TEXT DEFAULT (datetime('now'))
);

-- Appointments
CREATE TABLE IF NOT EXISTS Appointments (
    AppointmentID   TEXT PRIMARY KEY,
    PatientID       INTEGER REFERENCES Patients(PatientID),
    DoctorID        INTEGER REFERENCES Doctors(DoctorID),
    DepartmentID    INTEGER REFERENCES Departments(DepartmentID),
    AppointmentDate TEXT NOT NULL,
    AppointmentTime TEXT NOT NULL,
    Status          TEXT DEFAULT 'Scheduled' CHECK(Status IN ('Scheduled','Completed','Cancelled','No-Show')),
    IsEmergency     INTEGER DEFAULT 0,
    Notes           TEXT,
    CreatedDate     TEXT DEFAULT (datetime('now')),
    ReminderSent    INTEGER DEFAULT 0
);

-- Queue
CREATE TABLE IF NOT EXISTS Queue (
    QueueID         INTEGER PRIMARY KEY AUTOINCREMENT,
    AppointmentID   TEXT REFERENCES Appointments(AppointmentID),
    TicketNumber    TEXT NOT NULL,
    QueuePosition   INTEGER,
    EstimatedWait   INTEGER,
    Status          TEXT DEFAULT 'Waiting' CHECK(Status IN ('Waiting','Called','Serving','Done')),
    CheckInTime     TEXT DEFAULT (datetime('now'))
);

-- Medical Records
CREATE TABLE IF NOT EXISTS MedicalRecords (
    RecordID      INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientID     INTEGER REFERENCES Patients(PatientID),
    DoctorID      INTEGER REFERENCES Doctors(DoctorID),
    AppointmentID TEXT REFERENCES Appointments(AppointmentID),
    Diagnosis     TEXT,
    Prescription  TEXT,
    TestResults   TEXT,
    Notes         TEXT,
    RecordDate    TEXT DEFAULT (datetime('now'))
);

-- Emergency Requests
CREATE TABLE IF NOT EXISTS EmergencyRequests (
    EmergencyID   INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientID     INTEGER REFERENCES Patients(PatientID),
    Description   TEXT NOT NULL,
    Priority      TEXT DEFAULT 'High' CHECK(Priority IN ('High','Critical','Low')),
    Status        TEXT DEFAULT 'Pending' CHECK(Status IN ('Pending','Assigned','Resolved')),
    RequestTime   TEXT DEFAULT (datetime('now')),
    AssignedTo    INTEGER REFERENCES Doctors(DoctorID)
);

-- ============================================================
-- SAMPLE DATA
-- ============================================================

-- Departments
INSERT OR IGNORE INTO Departments (DepartmentName, Description) VALUES
('General Medicine',  'Primary care and general health consultations'),
('Cardiology',        'Heart and cardiovascular diseases'),
('Pediatrics',        'Medical care for infants, children and adolescents'),
('Orthopedics',       'Bone, joint and muscle disorders'),
('Emergency',         'Urgent and emergency medical care');

-- Users (Passwords stored as plain text for demo; hash in production)
INSERT OR IGNORE INTO Users (Username, Password, Role, FullName, Email, Phone) VALUES
('admin',       'Admin@123',    'Admin',        'System Administrator',   'admin@hospital.com',      '08012345678'),
('dr_james',    'Doctor@123',   'Doctor',       'Dr. James Okafor',       'james@hospital.com',      '08023456789'),
('dr_fatima',   'Doctor@123',   'Doctor',       'Dr. Fatima Aliyu',       'fatima@hospital.com',     '08034567890'),
('dr_chukwu',   'Doctor@123',   'Doctor',       'Dr. Chukwuemeka Nwosu',  'chukwu@hospital.com',     '08045678901'),
('receptionist','Recep@123',    'Receptionist', 'Ngozi Adeyemi',          'ngozi@hospital.com',      '08056789012'),
('patient1',    'Patient@123',  'Patient',      'Emeka Obi',              'emeka@email.com',         '08067890123'),
('patient2',    'Patient@123',  'Patient',      'Halima Musa',            'halima@email.com',        '08078901234'),
('patient3',    'Patient@123',  'Patient',      'Tunde Bakare',           'tunde@email.com',         '08089012345'),
('patient4',    'Patient@123',  'Patient',      'Chioma Eze',             'chioma@email.com',        '08090123456'),
('patient5',    'Patient@123',  'Patient',      'Abubakar Sule',          'abubakar@email.com',      '08001234567');

-- Doctors
INSERT OR IGNORE INTO Doctors (UserID, DepartmentID, Specialization) VALUES
(2, 2, 'Interventional Cardiology'),
(3, 3, 'Neonatology and Paediatric Care'),
(4, 4, 'Sports Medicine and Orthopaedics');

-- Patients
INSERT OR IGNORE INTO Patients (UserID, DateOfBirth, Gender, Address, BloodGroup, EmergencyContact, EmergencyPhone) VALUES
(6,  '1990-05-14', 'Male',   '12 Adeola Street, Lagos',      'O+',  'Ngozi Obi',     '08011111111'),
(7,  '1985-11-22', 'Female', '5 Kano Road, Abuja',           'A+',  'Bello Musa',    '08022222222'),
(8,  '1978-03-09', 'Male',   '33 Ibadan Close, Oyo',         'B-',  'Amaka Bakare',  '08033333333'),
(9,  '2000-07-30', 'Female', '7 Enugu Avenue, Enugu',        'AB+', 'Emeka Eze',     '08044444444'),
(10, '1995-12-01', 'Male',   '21 Kaduna Lane, Kaduna',       'O-',  'Amina Sule',    '08055555555');

-- Appointments (mix of today and future dates)
INSERT OR IGNORE INTO Appointments (AppointmentID, PatientID, DoctorID, DepartmentID, AppointmentDate, AppointmentTime, Status, IsEmergency, Notes) VALUES
('APT-2024-0001', 1, 1, 2, date('now'),           '09:00', 'Scheduled',  0, 'Routine cardiac checkup'),
('APT-2024-0002', 2, 2, 3, date('now'),           '10:00', 'Scheduled',  0, 'Child vaccination follow-up'),
('APT-2024-0003', 3, 3, 4, date('now'),           '11:00', 'Scheduled',  0, 'Knee pain assessment'),
('APT-2024-0004', 4, 1, 2, date('now'),           '14:00', 'Scheduled',  1, 'Emergency - chest pain'),
('APT-2024-0005', 5, 2, 3, date('now'),           '15:00', 'Scheduled',  0, 'General consultation'),
('APT-2024-0006', 1, 3, 4, date('now','+1 day'),  '09:30', 'Scheduled',  0, 'Follow-up ortho review'),
('APT-2024-0007', 2, 1, 2, date('now','+1 day'),  '10:30', 'Scheduled',  0, 'ECG monitoring'),
('APT-2024-0008', 3, 2, 3, date('now','+2 days'), '11:30', 'Scheduled',  0, 'Paeds growth review'),
('APT-2024-0009', 4, 3, 4, date('now','+2 days'), '14:30', 'Completed',  0, 'Post-surgery review'),
('APT-2024-0010', 5, 1, 2, date('now','+3 days'), '09:00', 'Scheduled',  0, 'Hypertension management');

-- Queue entries for today's appointments
INSERT OR IGNORE INTO Queue (AppointmentID, TicketNumber, QueuePosition, EstimatedWait, Status) VALUES
('APT-2024-0001', 'TKT-001', 1, 0,  'Waiting'),
('APT-2024-0002', 'TKT-002', 2, 30, 'Waiting'),
('APT-2024-0003', 'TKT-003', 3, 60, 'Waiting'),
('APT-2024-0004', 'TKT-004', 1, 0,  'Waiting'),
('APT-2024-0005', 'TKT-005', 4, 90, 'Waiting');

-- Medical Records
INSERT OR IGNORE INTO MedicalRecords (PatientID, DoctorID, AppointmentID, Diagnosis, Prescription, TestResults, Notes) VALUES
(1, 1, 'APT-2024-0001', 'Mild hypertension',        'Amlodipine 5mg daily',           'BP: 145/90',  'Monitor weekly'),
(3, 3, 'APT-2024-0009', 'Post-knee replacement',    'Ibuprofen 400mg, Physiotherapy', 'X-ray normal','Recovery on track'),
(2, 2, 'APT-2024-0002', 'Routine vaccination check','None required',                  'All clear',   'Next visit in 6 months');
