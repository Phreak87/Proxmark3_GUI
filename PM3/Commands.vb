Public Class Commands
    Public XDoc As Xml.XmlDocument
    Public ONE As Boolean = False
    Public TEXT As String
    Public COMM As List(Of Command)
    Public TREE As List(Of NodeClass)

    Sub New()
        TEXT = ExtractHelpText()
        TREE = CreateTree(Nothing)

        If My.Computer.FileSystem.FileExists("PMConfig.xml") = False Then
            XDoc = New Xml.XmlDocument
            Dim XNod = XDoc.CreateElement("PM3")
            XDoc.AppendChild(XNod)
            BuildXML(TREE, XNod, False)
            XDoc.Save("PMConfig.xml")
        Else
            XDoc = New Xml.XmlDocument
            XDoc.Load("PMConfig.xml")
        End If
    End Sub

    Sub BuildXML(ByVal Tree As List(Of Commands.NodeClass), ByVal ParentNode As Xml.XmlNode, ByVal One As Boolean)
        For Each Entry In Tree
            If IsNothing(Entry.SubEntrys) Then
                Dim Node = ParentNode.AppendChild(ParentNode.OwnerDocument.CreateElement("_" & Entry.ToString))
                Dim Help = Node.AppendChild(Node.OwnerDocument.CreateElement("Help")) : Help.InnerText = Entry.Entry.HelpTxt
                Dim Offl = Node.AppendChild(Node.OwnerDocument.CreateElement("Offline")) : Offl.InnerText = Entry.Entry.Offline
                Dim Exte = Node.AppendChild(Node.OwnerDocument.CreateElement("Extended")) : Exte.InnerText = False
                Dim Usag = Node.AppendChild(Node.OwnerDocument.CreateElement("Usage")) : Usag.InnerText = ""
                Dim Pars = Node.AppendChild(Node.OwnerDocument.CreateElement("Parameters"))
                For Each Para In Entry.Entry.OptPara
                    Dim Par = Pars.AppendChild(Node.OwnerDocument.CreateElement("Parameter")) : Par.InnerText = Para
                Next
            Else
                Dim Node = ParentNode.AppendChild(ParentNode.OwnerDocument.CreateElement("_" & Entry.ToString))
                BuildXML(Entry.SubEntrys, Node, One)
            End If
        Next
    End Sub

    Sub SetFilter(ByVal Text As String)
        TREE = CreateTree(Text)
    End Sub

    Function ExtractHelpText() As String
        If My.Computer.FileSystem.FileExists("Proxmark3.exe") Then
            Dim PI As New ProcessStartInfo("Proxmark3.exe", "-t")
            PI.CreateNoWindow = True
            PI.UseShellExecute = False
            PI.RedirectStandardOutput = True
            PI.WindowStyle = ProcessWindowStyle.Hidden

            Dim PRC As Process = New System.Diagnostics.Process
            PRC.StartInfo = PI : PRC.EnableRaisingEvents = True

            Dim SB As New System.Text.StringBuilder
            AddHandler PRC.OutputDataReceived, Sub(S, DAT) SB.AppendLine(DAT.data)
            PRC.Start() : PRC.BeginOutputReadLine()

            PRC.WaitForExit(2000)
            Return SB.ToString
        End If
        Return Nothing
    End Function

    Function CreateTree(ByVal Filter As String) As List(Of NodeClass)
        Dim TXT As List(Of String) = Split(TEXT, vbCrLf).ToList
        Dim LST As List(Of String) = TXT.FindAll(Function(s) Split(s, "|").Count >= 3).ToArray.ToList
        LST.RemoveAll(Function(s) Mid(s, 26, 1) <> "|" Or Mid(s, 1, 7) = "command" Or Mid(s, 1, 7) = "-------")
        COMM = New List(Of Command) : For Each Entry In LST : COMM.Add(New Command(Entry)) : Next
        If Not String.IsNullOrEmpty(Filter) Then COMM.RemoveAll(Function(s) s.OrgPath.Contains(Filter) = False)
        If COMM.Count = 1 Then ONE = True Else ONE = False
        Return BuildNodes(0, COMM)
    End Function
    Public Shared Function BuildNodes(ByVal Level As Integer, ByVal CList As List(Of Command)) As List(Of NodeClass)
        Dim Target As New List(Of NodeClass)
        Dim LimCnt As List(Of Command) = CList.FindAll(Function(s) s.ModPath.Count > Level)
        Dim Disstr As List(Of String) = LimCnt.Select(Function(s) s.ModPath(Level)).Distinct.ToList

        For Each Entry In Disstr
            Dim SEntry As String = Entry : If SEntry = "" Then Continue For
            Dim Subs = CList.FindAll(Function(s) s.ModPath(Level) = SEntry And s.ModPath.Length > Level + 1)
            Dim Command = LimCnt.FindAll(Function(s) s.ModPath(Level) = SEntry)(0)
            If Subs.Count = 0 Then
                Target.Add(New NodeClass(SEntry, Command))
            Else
                Target.Add(New NodeClass(SEntry, Subs, Level + 1))
            End If
        Next
        Return Target
    End Function

    Public Class Command
        Property OrgPath As String
        Property ModPath As String()
        Property Offline As Boolean
        Property HelpTxt As String
        Property OptPara As New List(Of String)
        Public Overrides Function ToString() As String
            Return String.Join(".", ModPath) & " :" & OptPara.Count
        End Function
        Sub New(ByVal Commandline As String)
            Dim SPLT As System.Text.RegularExpressions.Match = System.Text.RegularExpressions.Regex.Match(Commandline, "^([a-zA-Z0-9 ]*)\|([YN][ ]*)\|(.*)?$", System.Text.RegularExpressions.RegexOptions.Multiline)
            OrgPath = SPLT.Groups(1).Value.Trim : ModPath = Split(OrgPath, " ")
            Offline = SPLT.Groups(2).Value.Trim = "Y"
            HelpTxt = SPLT.Groups(3).Value.Trim

            Dim ReplText As String = HelpTxt
            Dim Params As System.Text.RegularExpressions.MatchCollection = System.Text.RegularExpressions.Regex.Matches(ReplText, "\[(.*?)\]")
            For i As Integer = 0 To Params.Count - 1
                OptPara.Add(Params(i).Groups(1).Value)
                ReplText = ReplText.Replace(Params(i).Value, "")
            Next

            Dim Params2 As System.Text.RegularExpressions.MatchCollection = System.Text.RegularExpressions.Regex.Matches(ReplText, "<(.*?)>")
            For i As Integer = 0 To Params2.Count - 1
                OptPara.Add(Params2(i).Groups(1).Value)
            Next
        End Sub
    End Class
    Public Class NodeClass
        Public Name As String
        Public Entry As Command
        Public SubEntrys As List(Of NodeClass)
        Public Overrides Function ToString() As String
            Return Name
        End Function
        Sub New(ByVal Text As String, ByVal Command As Command)
            Name = Text
            Entry = Command
        End Sub
        Sub New(ByVal _Entry As String, ByVal _Subs As List(Of Command), ByVal Level As Integer)
            Name = _Entry
            SubEntrys = BuildNodes(Level, _Subs)
        End Sub
    End Class
End Class
