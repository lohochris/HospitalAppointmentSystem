Option Strict On
Option Explicit On

' ============================================================
' UserAccount.vb
' CSC3226 - Hospital Appointment System
' User account data model - SINGLE DEFINITION
' Group: Sa'id Umar, Aisha Ladan, Maryam Rabiu
' ============================================================

Imports System

Public Class UserAccount

#Region "Public Properties"
    ' Auto-implemented properties automatically handle their own unique internal backing fields
    Public Property UserID As Integer = 0
    Public Property Username As String = String.Empty
    Public Property FullName As String = String.Empty
    Public Property Role As String = String.Empty
    Public Property IsAuthenticated As Boolean = False
    Public Property Email As String = String.Empty
    Public Property PatientID As Integer = 0
#End Region

#Region "Constructors"
    ' Default constructor
    Public Sub New()
        Me.UserID = 0
        Me.IsAuthenticated = False
        Me.PatientID = 0
        Me.Username = String.Empty
        Me.FullName = String.Empty
        Me.Role = String.Empty
        Me.Email = String.Empty
    End Sub

    ' Parameterized constructor
    Public Sub New(userID As Integer, username As String, fullName As String, role As String, email As String)
        Me.UserID = userID
        Me.Username = username
        Me.FullName = fullName
        Me.Role = role
        Me.Email = email
        Me.IsAuthenticated = True
        Me.PatientID = 0
    End Sub
#End Region

#Region "Methods"
    Public Overrides Function ToString() As String
        Return String.Format("{0} ({1})", Me.FullName, Me.Role)
    End Function

    Public Function IsAdmin() As Boolean
        Return String.Equals(Me.Role, "Admin", StringComparison.OrdinalIgnoreCase)
    End Function

    Public Function IsDoctor() As Boolean
        Return String.Equals(Me.Role, "Doctor", StringComparison.OrdinalIgnoreCase)
    End Function

    Public Function IsReceptionist() As Boolean
        Return String.Equals(Me.Role, "Receptionist", StringComparison.OrdinalIgnoreCase)
    End Function

    Public Function IsPatient() As Boolean
        Return String.Equals(Me.Role, "Patient", StringComparison.OrdinalIgnoreCase)
    End Function
#End Region

End Class