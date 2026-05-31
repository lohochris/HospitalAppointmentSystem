Option Strict On
Option Explicit On

' ============================================================
' ClassModels.vb
' CSC3226 - Hospital Appointment System
' Central Data Models & Business Entities
' Group: Sa'id Umar, Aisha Ladan, Maryam Rabiu
' ============================================================

Imports System

' ============================================================
' Patient Entity Class
' ============================================================
Public Class Patient
    Public Property PatientID As Integer
    Public Property UserID As Integer
    Public Property FullName As String = ""
    Public Property Username As String = ""
    Public Property Email As String = ""
    Public Property Phone As String = ""
    Public Property DateOfBirth As DateTime
    Public Property Gender As String = ""
    Public Property Address As String = ""
    Public Property BloodGroup As String = ""
    Public Property EmergencyContact As String = ""
    Public Property EmergencyPhone As String = ""
    Public Property InsuranceNumber As String = ""
    Public Property RegisteredDate As DateTime = DateTime.Now

    ''' <summary>
    ''' Calculates patient age via validation module.
    ''' </summary>
    Public Function GetAge() As Integer
        Return ModuleValidation.CalculateAge(DateOfBirth)
    End Function

    Public Overrides Function ToString() As String
        Return $"[{PatientID}] {FullName}"
    End Function
End Class

' ============================================================
' Doctor Entity Class
' ============================================================
Public Class Doctor
    Public Property DoctorID As Integer
    Public Property UserID As Integer
    Public Property FullName As String = ""
    Public Property DepartmentID As Integer
    Public Property DepartmentName As String = ""
    Public Property Specialization As String = ""
    Public Property WorkingDays As String = "Mon,Tue,Wed,Thu,Fri"
    Public Property StartTime As String = "09:00"
    Public Property EndTime As String = "17:00"
    Public Property LunchStart As String = "13:00"
    Public Property LunchEnd As String = "14:00"
    Public Property SlotDuration As Integer = 30
    Public Property Phone As String = ""
    Public Property Email As String = ""

    ''' <summary>
    ''' Verifies if a doctor actively shifts on a given week day name.
    ''' </summary>
    Public Function WorksOnDay(dayName As String) As Boolean
        If String.IsNullOrWhiteSpace(WorkingDays) Then Return False

        Dim days As String() = WorkingDays.Split(","c)
        For Each day As String In days
            If day.Trim().Equals(dayName, StringComparison.OrdinalIgnoreCase) Then
                Return True
            End If
        Next
        Return False
    End Function

    Public Overrides Function ToString() As String
        Return $"Dr. {FullName} ({DepartmentName})"
    End Function
End Class

' ============================================================
' Appointment Entity Class
' ============================================================
Public Class Appointment
    Public Property AppointmentID As String = ""
    Public Property PatientID As Integer
    Public Property DoctorID As Integer
    Public Property DepartmentID As Integer
    Public Property AppointmentDate As DateTime
    Public Property AppointmentTime As String = ""
    Public Property Status As String = "Scheduled"
    Public Property IsEmergency As Boolean = False
    Public Property Notes As String = ""
    Public Property CreatedDate As DateTime = DateTime.Now
    Public Property ReminderSent As Boolean = False

    ' Data binding display helpers
    Public Property PatientName As String = ""
    Public Property DoctorName As String = ""
    Public Property DepartmentName As String = ""

    ''' <summary>
    ''' Maps explicit state definitions to friendly status string outputs.
    ''' </summary>
    Public Function GetStatusDisplay() As String
        Select Case Status
            Case "Scheduled"
                Return "📅 Scheduled"
            Case "Completed"
                Return "✅ Completed"
            Case "Cancelled"
                Return "❌ Cancelled"
            Case "No-Show"
                Return "⚠ No-Show"
            Case Else
                Return Status
        End Select
    End Function

    Public Overrides Function ToString() As String
        Dim emgTag As String = If(IsEmergency, " [EMERGENCY]", "")
        Return $"{AppointmentID}{emgTag} - {AppointmentDate:dd/MM/yyyy} {AppointmentTime}"
    End Function
End Class

' ============================================================
' Queue Entry Entity Class
' ============================================================
Public Class QueueEntry
    Public Property QueueID As Integer
    Public Property AppointmentID As String = ""
    Public Property TicketNumber As String = ""
    Public Property QueuePosition As Integer
    Public Property EstimatedWait As Integer
    Public Property Status As String = "Waiting"
    Public Property CheckInTime As DateTime = DateTime.Now

    ' Data binding display helpers
    Public Property PatientName As String = ""
    Public Property DoctorName As String = ""
    Public Property DepartmentName As String = ""
    Public Property IsEmergency As Boolean = False
    Public Property AppointmentTime As String = ""

    ''' <summary>
    ''' Transforms raw waiting time integers to clean UI formats.
    ''' </summary>
    Public Function GetWaitDisplay() As String
        Return ModuleValidation.FormatWaitingTime(EstimatedWait)
    End Function

    Public Overrides Function ToString() As String
        Dim emgTag As String = If(IsEmergency, " 🚨", "")
        Return $"{TicketNumber}{emgTag} - {PatientName} ({GetWaitDisplay()})"
    End Function
End Class