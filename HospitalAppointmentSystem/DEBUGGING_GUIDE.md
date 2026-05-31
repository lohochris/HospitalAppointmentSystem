# Debugging Guide - Hospital Appointment System

## Issue: Pressing F5 in Visual Studio doesn't show the application

### ✅ What We've Fixed:
1. ✓ Added database initialization to `Program.vb`
2. ✓ Added connection test before launching the form
3. ✓ Enhanced error messages with stack traces
4. ✓ Verified the executable runs correctly outside Visual Studio

### 🔍 Troubleshooting Steps:

#### **Step 1: Verify Visual Studio Settings**
1. In Visual Studio, go to: **Project → HospitalAppointmentSystem Properties**
2. Check the **Application** tab:
   - **Startup object:** Should be `Sub Main` or `HospitalAppointmentSystem.Program`
   - **Application type:** Should be `Windows Forms Application` or `Windows Application`

#### **Step 2: Check Debug Settings**
1. Go to: **Debug → Options → Debugging → General**
2. Ensure these are checked:
   - ☑ Enable Just My Code (recommended)
   - ☑ Enable .NET Framework source stepping (optional)
3. Ensure this is **unchecked**:
   - ☐ Break when exceptions cross AppDomain or managed/native boundaries

#### **Step 3: Set Exception Settings**
1. Go to: **Debug → Windows → Exception Settings** (or press Ctrl+Alt+E)
2. Check **☑ Common Language Runtime Exceptions**
   - This will break on ANY exception, even caught ones
3. Press **F5** again - if there's ANY error, you'll see it now

#### **Step 4: Use Start Without Debugging**
Instead of pressing **F5**, try:
- Press **Ctrl+F5** (Start Without Debugging)
- This bypasses some debugger hooks and might show the window

#### **Step 5: Manual Debug Launch**
1. Close Visual Studio completely
2. Open Visual Studio again
3. Open the project
4. Press **F6** to rebuild
5. Press **F5** to start debugging

#### **Step 6: Check if Window is Hidden**
The application window might be opening off-screen or behind Visual Studio:
- Press **Alt+Tab** after pressing F5 to see all open windows
- Check the Windows taskbar for the application icon

#### **Step 7: Run from Command Line**
Open PowerShell in the project root and run:
```powershell
.\HospitalAppointmentSystem\bin\Debug\HospitalAppointmentSystem.exe
```
This will show any errors that occur.

#### **Step 8: Check Output Window**
When you press F5 in Visual Studio:
1. Go to **View → Output** (or press Ctrl+Alt+O)
2. In the "Show output from:" dropdown, select **Debug**
3. Look for any error messages

### 🎯 Most Common Causes:

1. **Visual Studio is set to "Start Without Debugging" mode**
   - Solution: Check that you're pressing **F5** (not Ctrl+F5)

2. **Debugger is attaching to wrong process**
   - Solution: Go to **Debug → Options → Debugging → General** and uncheck "Use Managed Compatibility Mode"

3. **Application is running but window is off-screen**
   - Solution: Press Alt+Tab or check taskbar

4. **AntiVirus is blocking the debugger**
   - Solution: Add exception for Visual Studio and your project folder

5. **Database initialization error**
   - Solution: Check if error message appears (we added error handling for this)

### 📝 What to Report:
If none of the above works, please check:
1. Any error messages in the **Output** window
2. Any error messages in the **Error List** window
3. Whether Ctrl+F5 works (Start Without Debugging)
4. Whether running the .exe directly works

### ✨ Quick Test:
Run this command to test if the app works outside Visual Studio:
```powershell
.\HospitalAppointmentSystem\bin\Debug\HospitalAppointmentSystem.exe
```

If it works, then it's a Visual Studio debugger configuration issue.
If it doesn't work, there's a runtime error we need to fix.
