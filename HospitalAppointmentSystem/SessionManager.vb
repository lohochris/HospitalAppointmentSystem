Option Strict On
Option Explicit On

' ============================================================
' SessionManager.vb
' CSC3226 - Hospital Appointment System
' Global session management - SINGLE DEFINITION
' Group: Sa'id Umar, Aisha Ladan, Maryam Rabiu
' ============================================================

Imports System

Public NotInheritable Class SessionManager

    Private Shared _currentUser As UserAccount

    ' Private constructor prevents instantiation
    Private Sub New()
    End Sub

    Public Shared Property CurrentUser() As UserAccount
        Get
            Return _currentUser
        End Get
        Set(value As UserAccount)
            _currentUser = value
        End Set
    End Property

    Public Shared Sub Logout()
        _currentUser = Nothing
    End Sub

    Public Shared Function IsLoggedIn() As Boolean
        Return _currentUser IsNot Nothing AndAlso _currentUser.IsAuthenticated
    End Function

    Public Shared Function GetCurrentRole() As String
        If _currentUser Is Nothing Then
            Return String.Empty
        End If
        Return _currentUser.Role
    End Function

    Public Shared Function GetCurrentUserName() As String
        If _currentUser Is Nothing Then
            Return String.Empty
        End If
        Return _currentUser.Username
    End Function

    Public Shared Function GetCurrentFullName() As String
        If _currentUser Is Nothing Then
            Return String.Empty
        End If
        Return _currentUser.FullName
    End Function

End Class