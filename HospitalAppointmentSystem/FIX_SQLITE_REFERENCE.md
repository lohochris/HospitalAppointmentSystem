# QUICK FIX: Add System.Data.SQLite Reference

## Problem
You're getting 23 BC30002 errors because the main `System.Data.SQLite.dll` reference is missing from your project, even though the NuGet package is installed.

## Solution (Choose ONE method)

### ✅ Method 1: Add Reference in Visual Studio (RECOMMENDED)

1. In Visual Studio, go to: **Project → Add Reference...**
2. Click the **Browse** button
3. Navigate to:
   ```
   C:\Users\Loho Christopher\source\repos\HospitalAppointmentSystem\packages\Stub.System.Data.SQLite.Core.NetFramework.1.0.118.0\lib\net46\System.Data.SQLite.dll
   ```
4. Select **System.Data.SQLite.dll**
5. Click **Add**, then **OK**
6. **Build** the project (Ctrl+Shift+B)

### ✅ Method 2: Edit Project File Manually

1. **Close Visual Studio** completely
2. Open this file in Notepad:
   ```
   C:\Users\Loho Christopher\source\repos\HospitalAppointmentSystem\HospitalAppointmentSystem\HospitalAppointmentSystem.vbproj
   ```
3. Find this section (around line 81-84):
   ```xml
   <Reference Include="System.Data" />
   <Reference Include="System.Data.DataSetExtensions" />
   <Reference Include="System.Data.SQLite.EF6, Version=1.0.118.0...
   ```

4. **Add these lines BEFORE** the `System.Data.SQLite.EF6` reference:
   ```xml
   <Reference Include="System.Data.SQLite, Version=1.0.118.0, Culture=neutral, PublicKeyToken=db937bc2d44ff139, processorArchitecture=MSIL">
	 <HintPath>..\packages\Stub.System.Data.SQLite.Core.NetFramework.1.0.118.0\lib\net46\System.Data.SQLite.dll</HintPath>
	 <Private>True</Private>
   </Reference>
   ```

5. Save the file
6. Open Visual Studio and reload the solution
7. Build the project

### ✅ Method 3: Reinstall Package via NuGet

1. In Visual Studio: **Tools → NuGet Package Manager → Package Manager Console**
2. Run these commands:
   ```powershell
   Update-Package -reinstall System.Data.SQLite
   ```
3. Build the project

## Verification

After adding the reference, verify it appears in:
- **Solution Explorer → References → System.Data.SQLite**

Then build the project - all 23 errors should be resolved!

## What Was Wrong?

Your `packages.config` has the package listed, but the `.vbproj` file is missing the `<Reference>` entry for the core `System.Data.SQLite.dll`. This causes the compiler to not find the SQLite types even though your code has the correct `Imports System.Data.SQLite` statement.
