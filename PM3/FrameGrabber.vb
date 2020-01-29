Imports System.Runtime.InteropServices

Public Class FrameGrabber

    Private CANCEL As Boolean = False

    Private ACTTAB As TabPage
    Private WHANDLE As IntPtr
    Private PROCESS As Process
    Private TABCTRL As TabControl

#Region "Internals"
    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)> _
    Public Shared Function ShowWindow(ByVal hwnd As IntPtr, ByVal nCmdShow As Integer) As Boolean
    End Function
    <DllImport("user32.dll")> _
    Public Shared Function SetParent(ByVal Child As IntPtr, ByVal NewParent As IntPtr) As Integer
    End Function
    <DllImport("user32.dll")> _
    Public Shared Function GetWindowLong( _
     ByVal hWnd As IntPtr, _
     ByVal nIndex As Integer) As Integer
    End Function
    <DllImport("user32.dll")> _
    Public Shared Function SetWindowLong( _
    ByVal hWnd As IntPtr, _
    ByVal nIndex As Integer, _
    ByVal dwNewLong As IntPtr) As Integer
    End Function
    Public Enum WindowStyles As Long
        WS_OVERLAPPED = 0
        WS_POPUP = 2147483648
        WS_CHILD = 1073741824
        WS_MINIMIZE = 536870912
        WS_VISIBLE = 268435456
        WS_DISABLED = 134217728
        WS_CLIPSIBLINGS = 67108864
        WS_CLIPCHILDREN = 33554432
        WS_MAXIMIZE = 16777216
        WS_BORDER = 8388608
        WS_DLGFRAME = 4194304
        WS_VSCROLL = 2097152
        WS_HSCROLL = 1048576
        WS_SYSMENU = 524288
        WS_THICKFRAME = 262144
        WS_GROUP = 131072
        WS_TABSTOP = 65536

        WS_MINIMIZEBOX = 131072
        WS_MAXIMIZEBOX = 65536

        WS_CAPTION = WS_BORDER Or WS_DLGFRAME
        WS_TILED = WS_OVERLAPPED
        WS_ICONIC = WS_MINIMIZE
        WS_SIZEBOX = WS_THICKFRAME
        WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW

        WS_OVERLAPPEDWINDOW = WS_OVERLAPPED Or WS_CAPTION Or WS_SYSMENU Or WS_THICKFRAME Or WS_MINIMIZEBOX Or WS_MAXIMIZEBOX
        WS_POPUPWINDOW = WS_POPUP Or WS_BORDER Or WS_SYSMENU
        WS_CHILDWINDOW = WS_CHILD

        WS_EX_DLGMODALFRAME = 1
        WS_EX_NOPARENTNOTIFY = 4
        WS_EX_TOPMOST = 8
        WS_EX_ACCEPTFILES = 16
        WS_EX_TRANSPARENT = 32

        '#If (WINVER >= 400) Then
        WS_EX_MDICHILD = 64
        WS_EX_TOOLWINDOW = 128
        WS_EX_WINDOWEDGE = 256
        WS_EX_CLIENTEDGE = 512
        WS_EX_CONTEXTHELP = 1024

        WS_EX_RIGHT = 4096
        WS_EX_LEFT = 0
        WS_EX_RTLREADING = 8192
        WS_EX_LTRREADING = 0
        WS_EX_LEFTSCROLLBAR = 16384
        WS_EX_RIGHTSCROLLBAR = 0

        WS_EX_CONTROLPARENT = 65536
        WS_EX_STATICEDGE = 131072
        WS_EX_APPWINDOW = 262144

        WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE Or WS_EX_CLIENTEDGE
        WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE Or WS_EX_TOOLWINDOW Or WS_EX_TOPMOST
        '#End If

        '#If (WIN32WINNT >= 500) Then
        WS_EX_LAYERED = 524288
        '#End If

        '#If (WINVER >= 500) Then
        WS_EX_NOINHERITLAYOUT = 1048576 ' Disable inheritence of mirroring by children
        WS_EX_LAYOUTRTL = 4194304 ' Right to left mirroring
        '#End If

        '#If (WIN32WINNT >= 500) Then
        WS_EX_COMPOSITED = 33554432
        WS_EX_NOACTIVATE = 67108864
        '#End If

    End Enum
    Enum ShowWindowCommands As Integer
        Hide = 0
        Normal = 1
        ShowMinimized = 2
        ShowMaximized = 3
        ShowNoActivate = 4
        Show = 5
        Minimize = 6
        ShowMinNoActive = 7
        ShowNA = 8
        Restore = 9
        ShowDefault = 10
        ForceMinimize = 11
    End Enum
#End Region

    Function GetMainWindowName(ByVal Name As String) As IntPtr
        Dim PRCL As Process() = System.Diagnostics.Process.GetProcesses
        For Each PRC In PRCL
            If PRC.ProcessName.ToLower.Contains(Name.ToLower) Then
                PROCESS = PRC
                WHANDLE = PRC.MainWindowHandle
                If CInt(WHANDLE) > 0 Then Return WHANDLE
            End If
        Next
        Return 0
    End Function
    Function WaitMainWindowHandle(ByVal Process As Process) As Boolean
        For i As Integer = 1 To 10
            If CInt(Process.MainWindowHandle) > 0 Then
                Return True
            Else
                Threading.Thread.Sleep(100)
                Application.DoEvents()
            End If
        Next
        Return False
    End Function
    Function CreateTabPage(ByVal Name As String) As TabPage
        If Not IsNothing(TABCTRL) Then
            TABCTRL.TabPages.Add(Name)
            TABCTRL.SelectTab(TABCTRL.TabPages.Count - 1)
            Return TABCTRL.TabPages(TABCTRL.TabPages.Count - 1)
        End If
        Return Nothing
    End Function

    Delegate Sub GrabWindowDelegate(ByVal Handle As IntPtr)
    Sub GrabWindow(ByVal Handle As IntPtr)
        If CInt(Handle) = 0 Then Exit Sub
        If TABCTRL.InvokeRequired = True Then
            TABCTRL.Invoke(New GrabWindowDelegate(AddressOf GrabWindow), Handle)
        Else
            Dim Name As String = Handle
            ACTTAB = CreateTabPage(PROCESS.ProcessName)
            SetParent(Handle, ACTTAB.Handle)

            Dim styles As WindowStyles = GetWindowLong(Handle, -16)
            SetWindowLong(Handle, _
                          -16,
                          styles _
                          And Not WindowStyles.WS_MAXIMIZEBOX _
                          And Not WindowStyles.WS_MINIMIZEBOX _
                          And Not WindowStyles.WS_SYSMENU _
                          And Not WindowStyles.WS_BORDER _
                          And Not WindowStyles.WS_SIZEBOX _
                          And Not WindowStyles.WS_EX_TRANSPARENT)
            ShowWindow(Handle, ShowWindowCommands.ShowDefault)
            ShowWindow(Handle, ShowWindowCommands.ShowMaximized)
        End If
    End Sub

    Sub New(ByVal _TABCTRL As TabControl, ByVal NAME As String, Optional ByVal UPDATE As Boolean = False)

        TABCTRL = _TABCTRL
        PROCESS = Nothing
        HandleChange()

        ' ----------------------------------------
        ' Einmalig Fenster fangen.
        ' ----------------------------------------
        If CInt(GetMainWindowName(NAME)) > 0 Then
            GrabWindow(WHANDLE)
            HandleResize()
            HandleExit()
        End If

    End Sub
    Sub New(ByVal _TABCTRL As TabControl, ByVal PROZESS As Process)
        PROCESS = PROZESS
        TABCTRL = _TABCTRL

        WaitMainWindowHandle(PROZESS)
        WHANDLE = PROCESS.MainWindowHandle
        GrabWindow(WHANDLE)

        HandleChange()
        HandleResize()
        HandleExit()
    End Sub

#Region "Change Window"
    Private HANDTIMER As Timers.Timer
    Delegate Sub HandleCheck()
    Sub HandleChange()
        HANDTIMER = New System.Timers.Timer(1000)
        AddHandler HANDTIMER.Elapsed, Sub() CheckHandle()
        HANDTIMER.AutoReset = True : HANDTIMER.Start()
    End Sub
    Sub CheckHandle()
        If IsNothing(PROCESS) Then
            HANDTIMER.Stop() : Exit Sub
        End If
        If TABCTRL.InvokeRequired = True Then
            TABCTRL.Invoke(New HandleCheck(AddressOf CheckHandle), Nothing)
        Else
            If CInt(WHANDLE) = 0 Or PROCESS.MainWindowHandle <> WHANDLE Then
                If Not IsNothing(ACTTAB) Then TABCTRL.TabPages.Remove(ACTTAB)
                GrabWindow(PROCESS.MainWindowHandle)
                WHANDLE = PROCESS.MainWindowHandle
            End If
        End If
    End Sub
#End Region

#Region "Exit Process"
    Private PRCTIMER As Timers.Timer
    Delegate Sub PRCCheck()
    Sub HandleExit()
        PRCTIMER = New System.Timers.Timer(1000)
        AddHandler PRCTIMER.Elapsed, Sub() CheckPRC()
        PRCTIMER.AutoReset = True : PRCTIMER.Start()
    End Sub
    Sub CheckPRC()
        If PROCESS.HasExited = True Then
            If TABCTRL.InvokeRequired = True Then
                TABCTRL.Invoke(New PRCCheck(AddressOf CheckPRC), Nothing)
            Else
                TABCTRL.TabPages.Remove(ACTTAB)
                PRCTIMER.Stop() : PRCTIMER.Dispose()
                PROCESS.Dispose() : PROCESS = Nothing
            End If
        End If
    End Sub
#End Region

#Region "Resize Controls"
    Sub HandleResize()
        Dim CTRL As Control = TABCTRL
        Do Until IsNothing(CTRL.Parent)
            AddHandler CTRL.MouseDown, Sub() Cancel = True
            AddHandler CTRL.MouseUp, Sub() Cancel = False
            AddHandler CTRL.Resize, Sub() DoResize()
            CTRL = CTRL.Parent
        Loop

        Try
            AddHandler CType(CTRL, Form).ResizeBegin, Sub() Cancel = True
            AddHandler CType(CTRL, Form).ResizeEnd, Sub() DoResize(True)
            AddHandler CType(CTRL, Form).Resize, Sub() DoResize()
        Catch : End Try
    End Sub
    Sub DoResize(Optional ByVal Force As Boolean = False)
        If Force = False Then If Cancel = True Then Exit Sub
        ShowWindow(WHANDLE, ShowWindowCommands.Normal)
        ShowWindow(WHANDLE, ShowWindowCommands.ShowMaximized)
    End Sub
#End Region

End Class
