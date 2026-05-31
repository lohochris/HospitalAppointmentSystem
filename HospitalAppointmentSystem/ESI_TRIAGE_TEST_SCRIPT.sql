-- ============================================================
-- ESI TRIAGE TESTING SQL SCRIPT
-- Emergency Severity Index (ESI) Visual Triage System
-- Hospital Appointment System - Module 1
-- ============================================================

-- ===================================================================
-- PREREQUISITE: Ensure you have a doctor account and patient records
-- Run these queries in SQLite DB Browser or similar tool
-- ===================================================================

-- Check existing patients
SELECT PatientID, FullName FROM Patients 
INNER JOIN Users ON Patients.UserID = Users.UserID
LIMIT 5;

-- Check existing doctors
SELECT DoctorID, FullName, Specialization FROM Doctors
INNER JOIN Users ON Doctors.UserID = Users.UserID
LIMIT 5;

-- ===================================================================
-- TEST SCENARIO 1: CRITICAL PATIENT (ESI LEVEL 1) - SEVERE HYPOXEMIA
-- Expected: Soft Red background, bold font, auto-sorted to TOP
-- ===================================================================
INSERT INTO InpatientVitals (PatientID, BloodPressure, HeartRate, SpO2, Temperature, Weight, DateRecorded)
VALUES ('PAT-000001', '120/80', 75, 88, 98.6, 70.5, datetime('now'));

INSERT INTO Appointments (AppointmentID, PatientID, DoctorID, DepartmentID, AppointmentDate, AppointmentTime, Status, IsEmergency, Notes)
VALUES ('APT-TRIAGE-CRITICAL-01', 'PAT-000001', 1, 1, date('now', '+1 day'), '09:00', 'Scheduled', 1, 'ESI Level 1 Test - Severe Hypoxemia (SpO2=88%)');

-- ===================================================================
-- TEST SCENARIO 2: CRITICAL PATIENT (ESI LEVEL 1) - HYPERTENSIVE CRISIS
-- Expected: Soft Red background, bold font, auto-sorted to TOP
-- ===================================================================
INSERT INTO InpatientVitals (PatientID, BloodPressure, HeartRate, SpO2, DateRecorded)
VALUES ('PAT-000002', '185/125', 90, 96, datetime('now'));

INSERT INTO Appointments (AppointmentID, PatientID, DoctorID, DepartmentID, AppointmentDate, AppointmentTime, Status, IsEmergency, Notes)
VALUES ('APT-TRIAGE-CRITICAL-02', 'PAT-000002', 1, 4, date('now', '+1 day'), '09:30', 'Scheduled', 1, 'ESI Level 1 Test - Hypertensive Crisis (BP=185/125)');

-- ===================================================================
-- TEST SCENARIO 3: CRITICAL PATIENT (ESI LEVEL 1) - SEVERE TACHYCARDIA
-- Expected: Soft Red background, bold font, auto-sorted to TOP
-- ===================================================================
INSERT INTO InpatientVitals (PatientID, BloodPressure, HeartRate, SpO2, DateRecorded)
VALUES ('PAT-000003', '130/85', 145, 97, datetime('now'));

INSERT INTO Appointments (AppointmentID, PatientID, DoctorID, DepartmentID, AppointmentDate, AppointmentTime, Status, IsEmergency, Notes)
VALUES ('APT-TRIAGE-CRITICAL-03', 'PAT-000003', 1, 1, date('now', '+1 day'), '10:00', 'Scheduled', 1, 'ESI Level 1 Test - Severe Tachycardia (HR=145 bpm)');

-- ===================================================================
-- TEST SCENARIO 4: URGENT PATIENT (ESI LEVEL 2) - MODERATE HYPOXEMIA
-- Expected: Soft Amber background, regular font, below Level 1
-- ===================================================================
INSERT INTO InpatientVitals (PatientID, BloodPressure, HeartRate, SpO2, DateRecorded)
VALUES ('PAT-000004', '135/88', 85, 92, datetime('now'));

INSERT INTO Appointments (AppointmentID, PatientID, DoctorID, DepartmentID, AppointmentDate, AppointmentTime, Status, IsEmergency, Notes)
VALUES ('APT-TRIAGE-URGENT-01', 'PAT-000004', 1, 2, date('now', '+1 day'), '10:30', 'Scheduled', 0, 'ESI Level 2 Test - Moderate Hypoxemia (SpO2=92%)');

-- ===================================================================
-- TEST SCENARIO 5: URGENT PATIENT (ESI LEVEL 2) - STAGE 2 HYPERTENSION
-- Expected: Soft Amber background, regular font, below Level 1
-- ===================================================================
INSERT INTO InpatientVitals (PatientID, BloodPressure, HeartRate, SpO2, DateRecorded)
VALUES ('PAT-000005', '155/95', 105, 96, datetime('now'));

INSERT INTO Appointments (AppointmentID, PatientID, DoctorID, DepartmentID, AppointmentDate, AppointmentTime, Status, IsEmergency, Notes)
VALUES ('APT-TRIAGE-URGENT-02', 'PAT-000005', 1, 4, date('now', '+1 day'), '11:00', 'Scheduled', 0, 'ESI Level 2 Test - Stage 2 HTN + Tachycardia (BP=155/95, HR=105)');

-- ===================================================================
-- TEST SCENARIO 6: STABLE PATIENT (ESI LEVEL 3) - NORMAL VITALS
-- Expected: Soft Yellow background, regular font, at bottom of queue
-- ===================================================================
INSERT INTO InpatientVitals (PatientID, BloodPressure, HeartRate, SpO2, DateRecorded)
VALUES ('PAT-000006', '120/80', 75, 98, datetime('now'));

INSERT INTO Appointments (AppointmentID, PatientID, DoctorID, DepartmentID, AppointmentDate, AppointmentTime, Status, Notes)
VALUES ('APT-TRIAGE-STABLE-01', 'PAT-000006', 1, 2, date('now', '+1 day'), '11:30', 'Scheduled', 'ESI Level 3 Test - Normal Vitals (BP=120/80, HR=75, SpO2=98%)');

-- ===================================================================
-- TEST SCENARIO 7: STABLE PATIENT (ESI LEVEL 3) - NORMAL VITALS (VARIANT)
-- Expected: Soft Yellow background, regular font, at bottom of queue
-- ===================================================================
INSERT INTO InpatientVitals (PatientID, BloodPressure, HeartRate, SpO2, DateRecorded)
VALUES ('PAT-000007', '115/75', 70, 99, datetime('now'));

INSERT INTO Appointments (AppointmentID, PatientID, DoctorID, DepartmentID, AppointmentDate, AppointmentTime, Status, Notes)
VALUES ('APT-TRIAGE-STABLE-02', 'PAT-000007', 1, 3, date('now', '+1 day'), '01:00', 'Scheduled', 'ESI Level 3 Test - Normal Vitals (BP=115/75, HR=70, SpO2=99%)');

-- ===================================================================
-- TEST SCENARIO 8: PATIENT WITHOUT VITALS (INCOMPLETE DATA)
-- Expected: Soft Gray background, default to Level 3, no crash
-- ===================================================================
INSERT INTO Appointments (AppointmentID, PatientID, DoctorID, DepartmentID, AppointmentDate, AppointmentTime, Status, Notes)
VALUES ('APT-TRIAGE-INCOMPLETE-01', 'PAT-000008', 1, 2, date('now', '+1 day'), '01:30', 'Scheduled', 'ESI Incomplete Data Test - No vitals record');

-- ===================================================================
-- TEST SCENARIO 9: INVALID VITALS (PHYSIOLOGICAL VALIDATION)
-- Expected: Soft Gray background, invalid vitals rejected, default to Level 3
-- ===================================================================
INSERT INTO InpatientVitals (PatientID, BloodPressure, HeartRate, SpO2, DateRecorded)
VALUES ('PAT-000009', '120/80', 75, 105, datetime('now'));  -- SpO2 > 100% (impossible)

INSERT INTO Appointments (AppointmentID, PatientID, DoctorID, DepartmentID, AppointmentDate, AppointmentTime, Status, Notes)
VALUES ('APT-TRIAGE-INVALID-01', 'PAT-000009', 1, 2, date('now', '+1 day'), '02:00', 'Scheduled', 'ESI Invalid Vitals Test - SpO2=105% (physiologically impossible)');

-- ===================================================================
-- VERIFICATION QUERIES
-- ===================================================================

-- 1. Check all test appointments created
SELECT AppointmentID, PatientID, DepartmentID, AppointmentDate, AppointmentTime, Status, Notes
FROM Appointments
WHERE AppointmentID LIKE 'APT-TRIAGE-%'
ORDER BY AppointmentID;

-- 2. Check all test vitals inserted
SELECT v.PatientID, v.BloodPressure, v.HeartRate, v.SpO2, v.DateRecorded
FROM InpatientVitals v
WHERE v.PatientID IN ('PAT-000001', 'PAT-000002', 'PAT-000003', 'PAT-000004', 'PAT-000005', 
					  'PAT-000006', 'PAT-000007', 'PAT-000008', 'PAT-000009')
ORDER BY v.DateRecorded DESC;

-- 3. Preview triage query (same logic as GetDoctorAppointments)
SELECT 
	a.AppointmentID,
	(SELECT FullName FROM Users WHERE UserID = p.UserID) AS PatientName,
	a.AppointmentDate,
	a.AppointmentTime,
	a.Status,
	v.BloodPressure,
	v.SystolicBP,
	v.DiastolicBP,
	v.HeartRate,
	v.SpO2,
	CASE 
		WHEN v.SpO2 < 90 OR v.SystolicBP >= 180 OR v.DiastolicBP >= 120 OR v.HeartRate > 130 THEN 1  -- Critical
		WHEN (v.SpO2 >= 90 AND v.SpO2 <= 94) OR (v.SystolicBP >= 140 AND v.SystolicBP <= 179) OR (v.HeartRate >= 100 AND v.HeartRate <= 129) THEN 2  -- Urgent
		ELSE 3  -- Stable/Unknown
	END AS ESILevel,
	a.Notes
FROM Appointments a
INNER JOIN Patients p ON a.PatientID = p.PatientID
LEFT JOIN (
	SELECT 
		PatientID,
		BloodPressure,
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
		DateRecorded
	FROM InpatientVitals
	WHERE BloodPressure IS NOT NULL AND BloodPressure != ''
	ORDER BY DateRecorded DESC
) v ON p.PatientID = v.PatientID
WHERE a.DoctorID = 1
  AND a.AppointmentID LIKE 'APT-TRIAGE-%'
ORDER BY ESILevel ASC, a.AppointmentDate ASC, a.AppointmentTime ASC;

-- ===================================================================
-- EXPECTED SORT ORDER IN DOCTOR'S DASHBOARD
-- ===================================================================
-- ROW 1: APT-TRIAGE-CRITICAL-01 | PAT-000001 | SpO2=88%    (🔴 CRITICAL - Level 1)
-- ROW 2: APT-TRIAGE-CRITICAL-02 | PAT-000002 | BP=185/125  (🔴 CRITICAL - Level 1)
-- ROW 3: APT-TRIAGE-CRITICAL-03 | PAT-000003 | HR=145 bpm  (🔴 CRITICAL - Level 1)
-- ROW 4: APT-TRIAGE-URGENT-01   | PAT-000004 | SpO2=92%    (🟡 URGENT - Level 2)
-- ROW 5: APT-TRIAGE-URGENT-02   | PAT-000005 | BP=155/95   (🟡 URGENT - Level 2)
-- ROW 6: APT-TRIAGE-STABLE-01   | PAT-000006 | Normal      (🟢 STABLE - Level 3)
-- ROW 7: APT-TRIAGE-STABLE-02   | PAT-000007 | Normal      (🟢 STABLE - Level 3)
-- ROW 8: APT-TRIAGE-INCOMPLETE-01 | PAT-000008 | No Vitals (⚪ GRAY - Level 3)
-- ROW 9: APT-TRIAGE-INVALID-01  | PAT-000009 | Invalid     (⚪ GRAY - Level 3)

-- ===================================================================
-- CLEANUP (Optional - run only if you want to remove test data)
-- ===================================================================

-- Delete test appointments
-- DELETE FROM Appointments WHERE AppointmentID LIKE 'APT-TRIAGE-%';

-- Delete test vitals
-- DELETE FROM InpatientVitals WHERE PatientID IN ('PAT-000001', 'PAT-000002', 'PAT-000003', 
--     'PAT-000004', 'PAT-000005', 'PAT-000006', 'PAT-000007', 'PAT-000009');

-- ===================================================================
-- AUDIT LOG VERIFICATION
-- Check HospitalErrors.log file for triage calculation entries:
--
-- Example log entries to expect:
-- TriageEngine.CalculateESI: CRITICAL ALERT - Level 1 assigned | 🔴 CRITICAL: SpO2=88% (Severe Hypoxemia)
-- TriageEngine.CalculateESI: CRITICAL ALERT - Level 1 assigned | 🔴 CRITICAL: SBP=185 mmHg (Hypertensive Crisis), DBP=125 mmHg (Hypertensive Emergency)
-- TriageEngine.CalculateESI: CRITICAL ALERT - Level 1 assigned | 🔴 CRITICAL: HR=145 bpm (Severe Tachycardia)
-- TriageEngine.CalculateESI: URGENT ALERT - Level 2 assigned | 🟡 URGENT: SpO2=92% (Moderate Hypoxemia)
-- TriageEngine.CalculateESI: STABLE - Level 3 assigned | 🟢 STABLE: SBP=120, DBP=80, HR=75, SpO2=98%
-- TriageEngine.CalculateESI: WARNING - Incomplete vitals data (SBP=NULL, DBP=NULL, HR=NULL, SpO2=NULL)
-- TriageEngine.CalculateESI: ERROR - Physiologically invalid vitals (SBP=120, DBP=80, HR=75, SpO2=105)
-- ===================================================================

-- ===================================================================
-- HOW TO TEST IN THE APPLICATION
-- ===================================================================
-- 1. Run this SQL script in SQLite DB Browser to insert test data
-- 2. Login to application as doctor: dr_james_okafor / Doctor@123
-- 3. Navigate to main dashboard (automatic on login for doctor role)
-- 4. Observe the appointment queue DataGridView:
--    - Critical patients (SpO2=88%, BP=185/125, HR=145) appear at TOP with RED background and BOLD font
--    - Urgent patients (SpO2=92%, BP=155/95) appear in MIDDLE with AMBER background
--    - Stable patients appear at BOTTOM with YELLOW background
--    - Patients without vitals appear with GRAY background
-- 5. Select a row and verify the selection color is slightly darker for visibility
-- 6. Check HospitalErrors.log file for comprehensive audit trail
-- ===================================================================
