# TEST SCENARIOS - Patient Save Error Diagnostics

## 🧪 TEST CASE 1: Duplicate Phone Number

### Setup:
1. Open Patient Management form
2. Save a patient with phone: `07012345678`
3. Try to save another patient with the same phone

### Expected Error Message:
```
Failed to save patient. Error: constraint failed
UNIQUE constraint failed: PatientsManagement.PhoneNumber

Stack Trace:
   at System.Data.SQLite.SQLiteStatement.ExecuteNonQuery()
   at System.Data.SQLite.SQLiteCommand.ExecuteNonQuery(CommandBehavior behavior)
   ...
```

### Error Log Entry:
```
SavePatient: Inserting new patient PAT-2024-0002
SavePatient SQL Parameters - ID: PAT-2024-0002, FirstName: Test, LastName: User, Phone: 07012345678, Email: NULL
SavePatient SQLite error: constraint failed
UNIQUE constraint failed: PatientsManagement.PhoneNumber | ErrorCode: 19 | Patient: PAT-2024-0002
```

### ✅ PASS CRITERIA:
- Error message shows "UNIQUE constraint failed: PatientsManagement.PhoneNumber"
- Error log shows the duplicate phone number value
- ErrorCode: 19 is logged

---

## 🧪 TEST CASE 2: Empty Required Field (Edge Case)

### Setup:
1. Modify the form validation temporarily (comment out ValidateInputs)
2. Try to save with empty FirstName

### Expected Error Message:
```
Failed to save patient. Error: constraint failed
NOT NULL constraint failed: PatientsManagement.FirstName

Stack Trace: ...
```

### Error Log Entry:
```
SavePatient: Inserting new patient PAT-2024-0003
SavePatient SQL Parameters - ID: PAT-2024-0003, FirstName: , LastName: Smith, Phone: 12345, Email: NULL
SavePatient SQLite error: NOT NULL constraint failed: PatientsManagement.FirstName | ErrorCode: 19
```

### ✅ PASS CRITERIA:
- Error message identifies the missing field (FirstName)
- Empty FirstName value is visible in log

---

## 🧪 TEST CASE 3: Valid Save (Success Path)

### Setup:
1. Fill in all required fields:
   - First Name: John
   - Last Name: Doe
   - Phone: 07098765432 (unique)
   - Email: john@example.com
2. Click Save

### Expected Behavior:
- Success message: "Patient registered successfully!"
- Patient appears in DataGridView
- Form clears

### Error Log Entry:
```
SavePatient: Inserting new patient PAT-2024-0004
SavePatient SQL Parameters - ID: PAT-2024-0004, FirstName: John, LastName: Doe, Phone: 07098765432, Email: john@example.com
```

### ✅ PASS CRITERIA:
- No error message
- Success MessageBox appears
- Patient visible in grid
- Log shows successful parameter binding

---

## 🧪 TEST CASE 4: Update Existing Patient

### Setup:
1. Click on an existing patient in the grid
2. Modify the first name
3. Click Save (should show "Update" button text)

### Expected Behavior:
- Success message: "Patient updated successfully!"
- Changes reflected in DataGridView

### Error Log Entry:
```
SavePatient: Updating existing patient PAT-2024-0001
SavePatient SQL Parameters - ID: PAT-2024-0001, FirstName: Jane, LastName: Doe, Phone: 07012345678, Email: jane@example.com
```

### ✅ PASS CRITERIA:
- Log shows "Updating existing patient" (not "Inserting new patient")
- Original PatientID is preserved
- Changes saved successfully

---

## 🧪 TEST CASE 5: NULL Email (Optional Field)

### Setup:
1. Fill required fields
2. Leave Email field **blank**
3. Click Save

### Expected Behavior:
- Success message
- Patient saved with NULL email

### Error Log Entry:
```
SavePatient: Inserting new patient PAT-2024-0005
SavePatient SQL Parameters - ID: PAT-2024-0005, FirstName: Bob, LastName: Smith, Phone: 07011112222, Email: NULL
```

### ✅ PASS CRITERIA:
- Log shows "Email: NULL"
- No constraint error
- Patient saved successfully

---

## 🧪 TEST CASE 6: Database Locked (Concurrency)

### Setup:
1. Open the database file in DB Browser for SQLite
2. Start a transaction (Begin Transaction) but don't commit
3. Try to save a patient in your app

### Expected Error Message:
```
Failed to save patient. Error: database is locked
database is locked

Stack Trace: ...
```

### Error Log Entry:
```
SavePatient: Inserting new patient PAT-2024-0006
SavePatient SQL Parameters - ID: PAT-2024-0006, FirstName: Test, LastName: Lock, Phone: 12345, Email: NULL
SavePatient SQLite error: database is locked | ErrorCode: 5 | Patient: PAT-2024-0006
```

### ✅ PASS CRITERIA:
- Error message clearly states "database is locked"
- ErrorCode: 5 (SQLITE_BUSY) is logged
- Application doesn't crash

---

## 🧪 TEST CASE 7: Special Characters in Name

### Setup:
1. Enter name with special characters: O'Brien, José, François
2. Phone: unique number
3. Click Save

### Expected Behavior:
- Success message
- Special characters preserved

### Error Log Entry:
```
SavePatient: Inserting new patient PAT-2024-0007
SavePatient SQL Parameters - ID: PAT-2024-0007, FirstName: José, LastName: O'Brien, Phone: 07099998888, Email: NULL
```

### ✅ PASS CRITERIA:
- No SQL injection errors (parameterized queries work)
- Special characters saved correctly
- Apostrophes handled properly

---

## 📊 TESTING CHECKLIST

Use this checklist when running tests:

- [ ] Test Case 1: Duplicate Phone Number - ERROR VISIBLE ✅
- [ ] Test Case 2: Empty Required Field - ERROR VISIBLE ✅
- [ ] Test Case 3: Valid Save - SUCCESS ✅
- [ ] Test Case 4: Update Existing - SUCCESS ✅
- [ ] Test Case 5: NULL Email - SUCCESS ✅
- [ ] Test Case 6: Database Locked - ERROR VISIBLE ✅
- [ ] Test Case 7: Special Characters - SUCCESS ✅

---

## 🔧 TROUBLESHOOTING TIPS

### If No Error Message Appears:
1. Check if ValidateInputs() is preventing database call
2. Verify exception is being thrown
3. Check MessageBox.Show is not commented out

### If Error Log is Empty:
1. Check error_log.txt exists in bin\Debug folder
2. Verify LogError() method is working
3. Check file permissions

### If Error Message is Still Generic:
1. Verify you're running the latest build (F5, not old .exe)
2. Check the code changes were saved
3. Rebuild solution (Ctrl+Shift+B)

---

## 📝 MANUAL TEST LOG TEMPLATE

Use this template to document your test results:

```
TEST DATE: _______________
TESTER: _______________
APPLICATION VERSION: 1.0.0

Test Case 1 - Duplicate Phone:
[ ] PASS  [ ] FAIL
Error Message Shown: ______________________________
Notes: ___________________________________________

Test Case 2 - Empty Field:
[ ] PASS  [ ] FAIL
Error Message Shown: ______________________________
Notes: ___________________________________________

Test Case 3 - Valid Save:
[ ] PASS  [ ] FAIL
Patient ID Created: ______________________________
Notes: ___________________________________________

Test Case 4 - Update Patient:
[ ] PASS  [ ] FAIL
Patient ID Updated: ______________________________
Notes: ___________________________________________

Test Case 5 - NULL Email:
[ ] PASS  [ ] FAIL
Email Value in Log: ______________________________
Notes: ___________________________________________

Test Case 6 - Database Locked:
[ ] PASS  [ ] FAIL
Error Code Shown: ______________________________
Notes: ___________________________________________

Test Case 7 - Special Characters:
[ ] PASS  [ ] FAIL
Characters Saved: ______________________________
Notes: ___________________________________________

OVERALL STATUS: [ ] ALL TESTS PASS  [ ] FAILURES FOUND

NOTES/ISSUES:
_________________________________________________
_________________________________________________
_________________________________________________
```

---

## 🎯 SUCCESS CRITERIA

Your fix is successful if:

1. ✅ Error messages are **specific** (not generic)
2. ✅ Error log contains **parameter values**
3. ✅ Stack traces are **visible**
4. ✅ Constraint violations are **identified by column name**
5. ✅ Valid saves **work without errors**
6. ✅ Updates **preserve PatientID**
7. ✅ NULL values are **handled correctly**

---

**Run through these test cases and document your results!** 🚀
