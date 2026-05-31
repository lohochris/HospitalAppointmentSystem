Option Strict On
Option Explicit On

' ============================================================
' TelemedicineManager.vb
' CSC3226 - Hospital Appointment System
' Telemedicine Room URL Generator & Session Manager
' Group: Sa'id Umar, Aisha Ladan, Maryam Rabiu
' ============================================================

Imports System.Text.RegularExpressions

''' <summary>
''' TELEMEDICINE MANAGER - HELPER CLASS
''' Provides utility functions for video consultation room generation and validation
''' 
''' FEATURES:
''' - Platform-specific URL generation (Daily.co, Jitsi, Zoom)
''' - Unique room ID creation with collision prevention
''' - URL validation and security checks
''' - Session token generation for authenticated rooms
''' - Platform capability detection
''' </summary>
Public Class TelemedicineManager

#Region "Constants"
    ' Supported Telemedicine Platforms
    Public Const PLATFORM_DAILY As String = "Daily"
    Public Const PLATFORM_JITSI As String = "Jitsi"
    Public Const PLATFORM_ZOOM As String = "Zoom"

    ' Default Room Configuration
    Private Const DEFAULT_PLATFORM As String = PLATFORM_JITSI  ' Open-source, no account required
    Private Const ROOM_ID_PREFIX As String = "MEDICARE"
    Private Const ROOM_ID_LENGTH As Integer = 32  ' Maximum room name length for most platforms
#End Region

#Region "Room ID Generation"
    ''' <summary>
    ''' GENERATES UNIQUE TELEMEDICINE ROOM IDENTIFIER
    ''' 
    ''' FORMAT: MEDICARE-{AppointmentID}-{Timestamp}-{Random}
    ''' EXAMPLE: MEDICARE-APT2026001-20260115143022-A7B9
    ''' 
    ''' FEATURES:
    ''' - Collision-resistant (timestamp + random GUID suffix)
    ''' - URL-safe characters only (no spaces, special chars)
    ''' - Platform-compatible length (≤32 chars for Daily.co)
    ''' - Traceable back to appointment via embedded ID
    ''' </summary>
    Public Shared Function GenerateRoomID(appointmentID As String) As String
        Try
            ' Sanitize appointment ID (remove dashes, spaces)
            Dim cleanAppointmentID As String = Regex.Replace(appointmentID, "[^a-zA-Z0-9]", "")

            ' Generate timestamp (yyyyMMddHHmmss)
            Dim timestamp As String = DateTime.Now.ToString("yyyyMMddHHmmss")

            ' Generate random suffix (4-char hex from GUID)
            Dim randomSuffix As String = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper()

            ' Combine components
            Dim roomID As String = $"{ROOM_ID_PREFIX}-{cleanAppointmentID}-{timestamp}-{randomSuffix}"

            ' Truncate if exceeds platform limits (keep prefix + random for uniqueness)
            If roomID.Length > ROOM_ID_LENGTH Then
                roomID = $"{ROOM_ID_PREFIX}-{timestamp}-{randomSuffix}"
            End If

            Return roomID

        Catch ex As Exception
            LogError($"TelemedicineManager.GenerateRoomID error: {ex.Message}")
            ' Fallback: timestamp + GUID only
            Return $"{ROOM_ID_PREFIX}-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid():N}".Substring(0, ROOM_ID_LENGTH)
        End Try
    End Function
#End Region

#Region "Platform URL Generation"
    ''' <summary>
    ''' CONSTRUCTS PLATFORM-SPECIFIC VIDEO ROOM URL
    ''' 
    ''' SUPPORTED PLATFORMS:
    ''' - Daily.co: https://{subdomain}.daily.co/{roomID}
    ''' - Jitsi Meet: https://meet.jit.si/{roomID}
    ''' - Zoom: Requires OAuth (returns placeholder)
    ''' </summary>
    Public Shared Function GenerateRoomURL(roomID As String, platform As String, Optional subdomain As String = "medicare-hospital") As String
        Try
            If String.IsNullOrWhiteSpace(roomID) Then
                Throw New ArgumentException("Room ID cannot be null or empty")
            End If

            ' Sanitize room ID for URL compatibility
            Dim safeRoomID As String = Uri.EscapeDataString(roomID)

            ' Generate platform-specific URL
            Select Case platform.ToLower()
                Case "daily", "daily.co"
                    Return $"https://{subdomain}.daily.co/{safeRoomID}"

                Case "jitsi", "jitsi.meet"
                    Return $"https://meet.jit.si/{safeRoomID}"

                Case "zoom"
                    ' Zoom requires OAuth integration (future enhancement)
                    LogError("TelemedicineManager.GenerateRoomURL: Zoom requires OAuth configuration")
                    Return String.Empty  ' Signal that Zoom is not configured

                Case Else
                    ' Default to Jitsi (open-source, no setup required)
                    LogError($"TelemedicineManager.GenerateRoomURL: Unknown platform '{platform}', defaulting to Jitsi")
                    Return $"https://meet.jit.si/{safeRoomID}"
            End Select

        Catch ex As Exception
            LogError($"TelemedicineManager.GenerateRoomURL error: {ex.Message}")
            Return String.Empty
        End Try
    End Function
#End Region

#Region "URL Validation"
    ''' <summary>
    ''' VALIDATES TELEMEDICINE URL FOR SECURITY & COMPLIANCE
    ''' 
    ''' CHECKS:
    ''' - HTTPS protocol (medical encryption requirement)
    ''' - Well-formed URI syntax
    ''' - Allowed domain whitelist (optional)
    ''' - Exception for internal browser states (about:blank, about:*)
    ''' </summary>
    Public Shared Function IsValidTelemedicineURL(url As String, Optional strictDomainCheck As Boolean = False) As Boolean
        Try
            ' Null/empty check
            If String.IsNullOrWhiteSpace(url) Then
                Return False
            End If

            Dim trimmedUrl As String = url.Trim()

            ' Allow safe internal browser states (about:blank, about:srcdoc, etc.)
            ' These are used by WebView2 during initialization and do not pose security risks
            If trimmedUrl.StartsWith("about:", StringComparison.OrdinalIgnoreCase) Then
                LogError($"IsValidTelemedicineURL: ALLOWED - Internal browser state | URL={trimmedUrl}")
                Return True
            End If

            ' HTTPS enforcement for all external URLs
            If Not trimmedUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) Then
                LogError($"IsValidTelemedicineURL: REJECTED - HTTPS required | URL={url}")
                Return False
            End If

            ' URI format validation
            Dim validUri As Uri = Nothing
            If Not Uri.TryCreate(trimmedUrl, UriKind.Absolute, validUri) Then
                LogError($"IsValidTelemedicineURL: REJECTED - Invalid URI format | URL={url}")
                Return False
            End If

            ' Optional: Domain whitelist check
            If strictDomainCheck Then
                Dim allowedDomains As String() = {
                    "daily.co",
                    "jit.si",
                    "zoom.us",
                    "localhost"  ' For testing
                }

                Dim hostIsAllowed As Boolean = False
                For Each domain As String In allowedDomains
                    If validUri.Host.EndsWith(domain, StringComparison.OrdinalIgnoreCase) Then
                        hostIsAllowed = True
                        Exit For
                    End If
                Next

                If Not hostIsAllowed Then
                    LogError($"IsValidTelemedicineURL: REJECTED - Domain not whitelisted | Host={validUri.Host}")
                    Return False
                End If
            End If

            Return True

        Catch ex As Exception
            LogError($"IsValidTelemedicineURL error: {ex.Message}")
            Return False
        End Try
    End Function
#End Region

#Region "Session Token Generation"
    ''' <summary>
    ''' GENERATES SECURE SESSION TOKEN FOR AUTHENTICATED VIDEO ROOMS
    ''' 
    ''' FORMAT: Base64(UserID|AppointmentID|Timestamp|HMAC)
    ''' USE CASE: Embed in URL query string for server-side validation
    ''' 
    ''' EXAMPLE: https://meet.jit.si/Room123?token=eyJ1c2VySWQiOiIxMjMi...
    ''' 
    ''' NOTE: This is a simplified token. Production systems should use JWT (JSON Web Tokens)
    ''' with proper signing keys and expiration timestamps.
    ''' </summary>
    Public Shared Function GenerateSessionToken(userID As String, appointmentID As String) As String
        Try
            ' Create token payload
            Dim timestamp As String = DateTime.UtcNow.ToString("O")  ' ISO 8601 format
            Dim payload As String = $"{userID}|{appointmentID}|{timestamp}"

            ' Simple Base64 encoding (production should use JWT with HMAC signature)
            Dim tokenBytes As Byte() = System.Text.Encoding.UTF8.GetBytes(payload)
            Dim token As String = Convert.ToBase64String(tokenBytes)

            Return token

        Catch ex As Exception
            LogError($"TelemedicineManager.GenerateSessionToken error: {ex.Message}")
            Return String.Empty
        End Try
    End Function
#End Region

#Region "Platform Capability Check"
    ''' <summary>
    ''' CHECKS IF TELEMEDICINE PLATFORM IS CONFIGURED AND READY
    ''' 
    ''' VALIDATION:
    ''' - Daily.co: Check if subdomain is configured
    ''' - Jitsi: Always available (public instance)
    ''' - Zoom: Check OAuth credentials (future)
    ''' </summary>
    Public Shared Function IsPlatformAvailable(platform As String) As Boolean
        Try
            Select Case platform.ToLower()
                Case "jitsi", "jitsi.meet"
                    ' Jitsi is always available (public open-source instance)
                    Return True

                Case "daily", "daily.co"
                    ' Daily.co requires account setup (check if subdomain configured)
                    ' For now, assume configured (production should validate API key)
                    Return True

                Case "zoom"
                    ' Zoom requires OAuth configuration
                    ' Check if credentials exist (placeholder for future implementation)
                    LogError("IsPlatformAvailable: Zoom requires OAuth setup")
                    Return False

                Case Else
                    Return False
            End Select

        Catch ex As Exception
            LogError($"TelemedicineManager.IsPlatformAvailable error: {ex.Message}")
            Return False
        End Try
    End Function
#End Region

#Region "Room URL Parser"
    ''' <summary>
    ''' EXTRACTS ROOM ID FROM TELEMEDICINE URL
    ''' 
    ''' EXAMPLES:
    ''' - https://meet.jit.si/RoomName → RoomName
    ''' - https://subdomain.daily.co/RoomName → RoomName
    ''' </summary>
    Public Shared Function ExtractRoomIDFromURL(url As String) As String
        Try
            If String.IsNullOrWhiteSpace(url) Then
                Return String.Empty
            End If

            Dim validUri As Uri = Nothing
            If Not Uri.TryCreate(url, UriKind.Absolute, validUri) Then
                Return String.Empty
            End If

            ' Extract last path segment as room ID
            Dim segments As String() = validUri.AbsolutePath.Split("/"c)
            If segments.Length > 0 Then
                Dim roomID As String = segments(segments.Length - 1).Trim()
                Return Uri.UnescapeDataString(roomID)
            End If

            Return String.Empty

        Catch ex As Exception
            LogError($"TelemedicineManager.ExtractRoomIDFromURL error: {ex.Message}")
            Return String.Empty
        End Try
    End Function
#End Region

#Region "Platform Display Name"
    ''' <summary>
    ''' RETURNS USER-FRIENDLY PLATFORM NAME
    ''' </summary>
    Public Shared Function GetPlatformDisplayName(platform As String) As String
        Select Case platform.ToLower()
            Case "daily", "daily.co"
                Return "Daily.co (Enterprise)"
            Case "jitsi", "jitsi.meet"
                Return "Jitsi Meet (Open-Source)"
            Case "zoom"
                Return "Zoom (OAuth Required)"
            Case Else
                Return "Unknown Platform"
        End Select
    End Function
#End Region
End Class
