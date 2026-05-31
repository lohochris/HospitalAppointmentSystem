Option Strict On
Option Explicit On

' ============================================================
' PatientModel.vb
' CSC3226 - Hospital Appointment System
' Patient entity model for clean architecture
' Group: Sa'id Umar, Aisha Ladan, Maryam Rabiu
' ============================================================

Imports System

''' <summary>
''' Represents a patient entity in the hospital system.
''' </summary>
Public Class PatientModel

#Region "Properties"

    ''' <summary>
    ''' Unique identifier for the patient (format: PAT-YYYY-NNNN)
    ''' </summary>
    Public Property PatientID As String

    ''' <summary>
    ''' Patient's first name (required)
    ''' </summary>
    Public Property FirstName As String

    ''' <summary>
    ''' Patient's last name (required)
    ''' </summary>
    Public Property LastName As String

    ''' <summary>
    ''' Date of birth in ISO format (YYYY-MM-DD)
    ''' </summary>
    Public Property DateOfBirth As String

    ''' <summary>
    ''' Gender: Male, Female, or Other
    ''' </summary>
    Public Property Gender As String

    ''' <summary>
    ''' Contact phone number (required)
    ''' </summary>
    Public Property PhoneNumber As String

    ''' <summary>
    ''' Email address (optional)
    ''' </summary>
    Public Property Email As String

    ''' <summary>
    ''' Registration date in ISO format (YYYY-MM-DD HH:mm:ss)
    ''' </summary>
    Public Property DateRegistered As String

#End Region

#Region "Constructors"

    ''' <summary>
    ''' Default constructor
    ''' </summary>
    Public Sub New()
        PatientID = String.Empty
        FirstName = String.Empty
        LastName = String.Empty
        DateOfBirth = String.Empty
        Gender = String.Empty
        PhoneNumber = String.Empty
        Email = String.Empty
        DateRegistered = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
    End Sub

    ''' <summary>
    ''' Parameterized constructor
    ''' </summary>
    Public Sub New(firstName As String, lastName As String, phone As String)
        Me.New()
        Me.FirstName = firstName
        Me.LastName = lastName
        Me.PhoneNumber = phone
    End Sub

#End Region

#Region "Methods"

    ''' <summary>
    ''' Returns the patient's full name
    ''' </summary>
    Public Function GetFullName() As String
        Return $"{FirstName} {LastName}".Trim()
    End Function

    ''' <summary>
    ''' Validates required fields
    ''' </summary>
    Public Function IsValid() As Boolean
        Return Not String.IsNullOrWhiteSpace(FirstName) AndAlso
               Not String.IsNullOrWhiteSpace(LastName) AndAlso
               Not String.IsNullOrWhiteSpace(PhoneNumber)
    End Function

    ''' <summary>
    ''' Returns a string representation of the patient
    ''' </summary>
    Public Overrides Function ToString() As String
        Return $"[{PatientID}] {GetFullName()} - {PhoneNumber}"
    End Function

#End Region

End Class
