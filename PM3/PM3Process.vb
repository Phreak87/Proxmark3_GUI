Imports System.Threading

Public Class PM3Process

    Event Exited()
    Event Processing(ByVal State As Boolean)
    Event ErrorRecv(ByVal Text As String)
    Event OutputRecv(ByVal Text As String)
    Event NewLogEntry(ByVal Text As String)

    Dim THS As Threading.Thread
    Dim LastPos As Integer = 0              ' Position of the internal Logfile Position
    Dim LogFile As String = ""              ' Main-Application gives a hint to the Log-File
    Dim Ready As Boolean = True             ' False until we got a Ping Response after a Command
    Dim FSL As System.IO.FileStream         ' Logfile to Read the Outputs. STDOut doesn´t work 
    Dim PRC As System.Diagnostics.Process   ' Process to Send the Commands
    Dim PRCLog As New System.Text.StringBuilder

    Sub New(ByVal Port As String)
        If My.Computer.FileSystem.FileExists("Proxmark3.exe") = False Then
            MsgBox("Anwendung 'Proxmark3.exe' nicht gefunden." & vbCrLf & _
                   "Programm läuft im Offline-Modus", MsgBoxStyle.Exclamation) : Exit Sub
        End If
        Dim PI As New ProcessStartInfo("Proxmark3.exe", Port)
        PI.WindowStyle = ProcessWindowStyle.Hidden
        PI.CreateNoWindow = True
        PI.RedirectStandardError = True
        PI.RedirectStandardInput = True
        PI.RedirectStandardOutput = True
        PI.UseShellExecute = False

        PRC = New System.Diagnostics.Process
        PRC.StartInfo = PI
        PRC.EnableRaisingEvents = True

        AddHandler PRC.Exited, AddressOf RECVExit
        AddHandler PRC.ErrorDataReceived, Sub(sender As Object, e As DataReceivedEventArgs) RaiseEvent ErrorRecv(e.Data)
        AddHandler PRC.OutputDataReceived, AddressOf RECVOutput

        PRC.Start()
        PRC.BeginOutputReadLine()
        PRC.BeginErrorReadLine()
    End Sub

    Sub RECVExit()
        If Not IsNothing(THS) Then THS.Abort()
        If Not IsNothing(PRC) Then If PRC.HasExited = False Then PRC.Kill()
        PRC = Nothing
        FSL = Nothing
        RaiseEvent Exited()
    End Sub
    Sub RECVOutput(ByVal sender As Object, ByVal e As DataReceivedEventArgs)
        If IsNothing(e) Then Exit Sub
        If IsNothing(e.Data) Then Exit Sub

        If e.Data.ToString.Contains(".proxmark3/log_") Then
            LogFile = Mid(e.Data, InStrRev(e.Data.ToString, "/") + 1)
            FSL = New IO.FileStream(".proxmark3\" & LogFile, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.ReadWrite)
            FSL.Position = FSL.Length : THS = New Threading.Thread(AddressOf StreamWatcher) : THS.Start()
        End If

        ' RaiseEvent OutputRecv(e.Data.ToString)
    End Sub

    Sub StreamWatcher()
        Do
            If IsNothing(FSL) Then Exit Sub
            If FSL.Position = FSL.Length Then
                Threading.Thread.Sleep(300)
            Else
                AddNewLogs()
            End If
        Loop
    End Sub
    Sub AddNewLogs()
        Dim BY(FSL.Length - FSL.Position - 1) As Byte
        FSL.Read(BY, 0, BY.Length) : LastPos = FSL.Position
        Dim AR As String() = Split(System.Text.Encoding.UTF8.GetString(BY), vbCrLf)
        For Each Line In AR

            If Line.Contains("[=] Ping sent") Then Continue For
            If Line.Contains("[con] pm3 --> hw ping") Then Continue For
            If Line.Contains("[+] Ping response received") Then Ready = True : Continue For

            If Line <> "" Then RaiseEvent NewLogEntry(Line)
            PRCLog.AppendLine(Line)
        Next
    End Sub

    Function SendCommand(ByVal Command As String) As String
        ' Pre-Exit
        If IsNothing(PRC) Then RaiseEvent NewLogEntry("Not Connected") : Return ""
        If IsNothing(FSL) = True Then RaiseEvent NewLogEntry("No Logfile") : Return ""
        If PRC.HasExited = True Then RaiseEvent NewLogEntry("Process exited") : Return ""

        ' Clean Logs
        FSL.Position = FSL.Length
        PRCLog.Clear()
        Ready = False

        ' perform action
        RaiseEvent Processing(True)
        PRC.StandardInput.WriteLine(Command)
        PRC.StandardInput.WriteLine("hw ping")

        ' end action and Return
        Do Until Ready = True : Application.DoEvents() : Thread.Sleep(100) : Loop
        RaiseEvent Processing(False) : Ready = True
        Return PRCLog.ToString
    End Function

    Function IsReady() As Boolean
        Return Ready = True And Not IsNothing(FSL) And Not IsNothing(PRC)
    End Function
End Class
