Option Strict On
Option Explicit On

' ============================================================
' ModuleValidation.vb
' CSC3226 - Hospital Appointment System
' Shared validation functions and utilities
' Group: Sa'id Umar, Aisha Ladan, Maryam Rabiu
' ============================================================

Imports System.Text.RegularExpressions

Public Module ModuleValidation

#Region "Field Validation Functions"

    ''' <summary>
    ''' Validates that a string is not empty or whitespace.
    ''' Demonstrates: Function returning Boolean
    ''' </summary>
    Public Function IsNotEmpty(value As String) As Boolean
        Return Not String.IsNullOrWhiteSpace(value)
    End Function

    ''' <summary>
    ''' Validates email address format using Regex.
    ''' Demonstrates: Function, conditional statement
    ''' </summary>
    Public Function IsValidEmail(email As String) As Boolean
        If String.IsNullOrWhiteSpace(email) Then Return False
        Try
            Dim pattern As String = "^[^@\s]+@[^@\s]+\.[^@\s]+$"
            Return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase)
        Catch ex As Exception
            ModuleDatabase.LogError("IsValidEmail error: " & ex.Message)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Validates Nigerian phone number (11 digits starting with 0).
    ''' Demonstrates: Function, conditional statements
    ''' </summary>
    Public Function IsValidPhone(phone As String) As Boolean
        If String.IsNullOrWhiteSpace(phone) Then Return False
        ' Remove spaces and dashes
        Dim cleaned As String = phone.Replace(" ", "").Replace("-", "")
        ' Check Nigerian format: 11 digits starting with 0
        Return Regex.IsMatch(cleaned, "^0[789][01]\d{8}$")
    End Function

    ''' <summary>
    ''' Validates date is not in the past.
    ''' Demonstrates: Function, conditional statement
    ''' </summary>
    Public Function IsValidFutureDate(dateValue As DateTime) As Boolean
        ' Demonstrates: Conditional statement
        If dateValue.Date < DateTime.Now.Date Then
            Return False
        End If
        Return True
    End Function

    ''' <summary>
    ''' Validates appointment is within working hours.
    ''' Demonstrates: Function, Select Case, conditional statements
    ''' </summary>
    Public Function IsWithinWorkingHours(appointmentTime As String) As Boolean
        Try
            Dim timeSpan As TimeSpan = TimeSpan.Parse(appointmentTime)
            Dim startWork As New TimeSpan(9, 0, 0)
            Dim endWork As New TimeSpan(17, 0, 0)
            Dim lunchStart As New TimeSpan(13, 0, 0)
            Dim lunchEnd As New TimeSpan(14, 0, 0)

            ' Must be within working hours and not during lunch
            If timeSpan < startWork OrElse timeSpan >= endWork Then Return False
            If timeSpan >= lunchStart And timeSpan < lunchEnd Then Return False
            Return True
        Catch
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Validates password strength (min 8 chars, 1 uppercase, 1 digit).
    ''' Demonstrates: Function, looping, conditional statements
    ''' </summary>
    Public Function IsValidPassword(password As String) As Boolean
        If String.IsNullOrEmpty(password) OrElse password.Length < 8 Then Return False

        Dim hasUpper As Boolean = False
        Dim hasDigit As Boolean = False

        ' Demonstrates: For Each loop
        For Each ch As Char In password
            If Char.IsUpper(ch) Then hasUpper = True
            If Char.IsDigit(ch) Then hasDigit = True
        Next

        Return hasUpper And hasDigit
    End Function

    ''' <summary>
    ''' Validates a Patient ID number format.
    ''' Demonstrates: Function, conditional statement
    ''' </summary>
    Public Function IsValidPatientID(patientID As String) As Boolean
        If String.IsNullOrWhiteSpace(patientID) Then Return False
        Return Regex.IsMatch(patientID, "^\d{1,6}$")
    End Function

    ''' <summary>
    ''' Validates date of birth (must be at least 0 years old, not future).
    ''' Demonstrates: Function, conditional statements
    ''' </summary>
    Public Function IsValidDateOfBirth(dob As DateTime) As Boolean
        ' Demonstrates: Conditional statement
        If dob > DateTime.Now Then Return False
        If dob < New DateTime(1900, 1, 1) Then Return False
        Return True
    End Function

    ''' <summary>
    ''' Calculates age from date of birth.
    ''' Demonstrates: Function returning Integer, conditional statement
    ''' </summary>
    Public Function CalculateAge(dob As DateTime) As Integer
        Dim age As Integer = DateTime.Now.Year - dob.Year
        ' Demonstrates: Conditional statement
        If DateTime.Now.DayOfYear < dob.DayOfYear Then
            age -= 1
        End If
        Return age
    End Function

#End Region

#Region "Waiting Time Calculations"

    ''' <summary>
    ''' Calculates estimated waiting time based on queue position.
    ''' Demonstrates: Function returning Integer, arithmetic
    ''' </summary>
    Public Function CalculateWaitingTime(queuePosition As Integer) As Integer
        Const MINUTES_PER_PATIENT As Integer = 30
        Return (queuePosition - 1) * MINUTES_PER_PATIENT
    End Function

    ''' <summary>
    ''' Formats minutes as "X hrs Y mins" or "Y mins".
    ''' Demonstrates: Function, conditional statement, Select Case
    ''' </summary>
    Public Function FormatWaitingTime(minutes As Integer) As String
        ' Demonstrates: Conditional statement
        If minutes = 0 Then Return "Now"
        If minutes < 60 Then Return $"{minutes} min(s)"
        Dim hrs As Integer = minutes \ 60
        Dim mins As Integer = minutes Mod 60
        If mins = 0 Then Return $"{hrs} hr(s)"
        Return $"{hrs} hr(s) {mins} min(s)"
    End Function

#End Region

#Region "Appointment ID Validation"

    ''' <summary>
    ''' Validates appointment ID format (APT-YYYY-NNNN).
    ''' Demonstrates: Function, Regex
    ''' </summary>
    Public Function IsValidAppointmentID(apptID As String) As Boolean
        If String.IsNullOrWhiteSpace(apptID) Then Return False
        Return Regex.IsMatch(apptID, "^APT-\d{4}-\d{4}$")
    End Function

#End Region

#Region "Symptom Checker Logic"

    ''' <summary>
    ''' Suggests a department based on selected symptoms.
    ''' Demonstrates: Function, Select Case, List(Of T) collection
    ''' </summary>
    Public Function SuggestDepartment(symptoms As List(Of String)) As String
        ' Demonstrates: conditional statements with collections
        If symptoms.Contains("Chest Pain") OrElse symptoms.Contains("Palpitations") OrElse symptoms.Contains("Shortness of Breath") Then
            Return "Cardiology"
        ElseIf symptoms.Contains("Fever") AndAlso symptoms.Contains("Cough") Then
            Return "General Medicine"
        ElseIf symptoms.Contains("Child Fever") OrElse symptoms.Contains("Child Vomiting") Then
            Return "Pediatrics"
        ElseIf symptoms.Contains("Joint Pain") OrElse symptoms.Contains("Back Pain") OrElse symptoms.Contains("Fracture") Then
            Return "Orthopedics"
        ElseIf symptoms.Contains("Severe Pain") OrElse symptoms.Contains("Unconsciousness") Then
            Return "Emergency"
        Else
            Return "General Medicine"
        End If
    End Function

    ''' <summary>
    ''' Determines urgency level from symptoms.
    ''' Demonstrates: Function, Select Case
    ''' </summary>
    Public Function DetermineUrgency(symptoms As List(Of String)) As String
        ' Demonstrates: Select Case statement
        Dim highUrgency As New List(Of String) From {"Chest Pain", "Unconsciousness", "Severe Pain", "Difficulty Breathing"}
        Dim medUrgency As New List(Of String) From {"Fever", "Vomiting", "Dizziness", "Palpitations"}

        ' Demonstrates: For Each loop with conditional
        For Each symptom As String In symptoms
            If highUrgency.Contains(symptom) Then
                Return "HIGH - Immediate Attention Required"
            End If
        Next

        For Each symptom As String In symptoms
            If medUrgency.Contains(symptom) Then
                Return "MEDIUM - Seek Care Today"
            End If
        Next

        Return "LOW - Schedule Regular Appointment"
    End Function

#End Region

#Region "Form Validation Helpers"

    ''' <summary>
    ''' Validates an entire patient registration form.
    ''' Demonstrates: Procedure, multiple conditional statements
    ''' </summary>
    Public Function ValidatePatientForm(fullName As String, username As String, password As String,
                                          email As String, phone As String,
                                          ByRef errorMessage As String) As Boolean
        ' Demonstrates: Multiple conditional statements (If...Then...ElseIf)
        If Not IsNotEmpty(fullName) Then
            errorMessage = "Full name is required."
            Return False
        End If

        If Not IsNotEmpty(username) OrElse username.Length < 4 Then
            errorMessage = "Username must be at least 4 characters."
            Return False
        End If

        If Not IsValidPassword(password) Then
            errorMessage = "Password must be at least 8 characters with 1 uppercase letter and 1 digit."
            Return False
        End If

        If Not IsValidEmail(email) Then
            errorMessage = "Please enter a valid email address."
            Return False
        End If

        If Not IsValidPhone(phone) Then
            errorMessage = "Please enter a valid Nigerian phone number (e.g. 08012345678)."
            Return False
        End If

        errorMessage = ""
        Return True
    End Function

    ''' <summary>
    ''' Validates appointment booking fields.
    ''' Demonstrates: Procedure, conditional statements
    ''' </summary>
    Public Function ValidateAppointmentForm(appointmentDate As DateTime, appointmentTime As String,
                                              doctorID As Integer, patientID As Integer,
                                              ByRef errorMessage As String) As Boolean
        If Not IsValidFutureDate(appointmentDate) Then
            errorMessage = "Appointment date cannot be in the past."
            Return False
        End If

        If Not IsWithinWorkingHours(appointmentTime) Then
            errorMessage = "Appointment time must be within working hours (09:00-17:00, excluding 13:00-14:00 lunch)."
            Return False
        End If

        If doctorID <= 0 Then
            errorMessage = "Please select a doctor."
            Return False
        End If

        If patientID <= 0 Then
            errorMessage = "Please select or register a patient."
            Return False
        End If

        errorMessage = ""
        Return True
    End Function

#End Region

End Module
