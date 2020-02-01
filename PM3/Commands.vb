Public Class Commands
    Public XDOC As Xml.XmlDocument
    Public COMM As List(Of Command)
    Public TREE As List(Of NodeClass)

    Public BONE As Boolean = False
    Public XNAM As String = "PMConfig.xml"

    Sub New()
        If My.Computer.FileSystem.FileExists("PMConfig.xml") = False Then
            COMM = TextToCommands(ExtractHelpText())
            TREE = CommandsToTree(COMM)
            XDOC = BuildXML(COMM)
            XDOC.Save("PMConfig.xml")
        Else
            XDOC = New Xml.XmlDocument
            XDOC.Load("PMConfig.xml")
            COMM = XDOCToCommands(XDOC)
            TREE = CommandsToTree(COMM)
        End If
    End Sub

    Sub SetFilter(ByVal Text As String)
        TREE = CommandsToTree(COMM, Text)
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
        Else
            If My.Computer.FileSystem.FileExists("proxmark3Help.txt") Then
                Return My.Computer.FileSystem.ReadAllText("proxmark3Help.txt")
            End If
        End If
        Return Nothing
    End Function
    Function TextToCommands(ByVal Text As String) As List(Of Command)
        Dim TXT As List(Of String) = Split(Text, vbCrLf).ToList
        Dim LST As List(Of String) = TXT.FindAll(Function(s) Split(s, "|").Count >= 3).ToArray.ToList
        LST.RemoveAll(Function(s) Mid(s, 26, 1) <> "|" Or Mid(s, 1, 7) = "command" Or Mid(s, 1, 7) = "-------" Or s.Contains("Proxmark3.exe"))
        Dim CMDL = New List(Of Command) : For Each Entry In LST : CMDL.Add(New Command(Entry)) : Next
        Return CMDL
    End Function
    Function XDOCToCommands(ByVal XDOC As Xml.XmlDocument) As List(Of Command)
        Dim COMML As New List(Of Command)
        For Each Entry As Xml.XmlNode In XDOC.SelectNodes("PM3/*")
            COMML.Add(New Command(Entry))
        Next
        Return COMML
    End Function
    Function CommandsToTree(ByVal Comm As List(Of Command), Optional ByVal Filter As String = Nothing) As List(Of NodeClass)
        Dim conns As New List(Of Command) : conns.AddRange(Comm)
        If Not String.IsNullOrEmpty(Filter) Then conns.RemoveAll(Function(s) s.OrgPath.Contains(Filter.Replace(" ", ".")) = False)
        If conns.Count = 1 Then BONE = True Else BONE = False
        Return BuildNodes(0, conns)
    End Function
    Function BuildXML(ByVal Commands As List(Of Command))
        Dim XDoc As New Xml.XmlDocument
        Dim XNod = XDoc.CreateElement("PM3")
        XDoc.AppendChild(XNod)

        For Each Command In Commands
            Dim Node = XNod.AppendChild(XNod.OwnerDocument.CreateElement(String.Join(".", Command.ModPath)))
            Dim Help = Node.AppendChild(XNod.OwnerDocument.CreateElement("Help")) : Help.InnerText = Command.HelpTxt
            Dim Offl = Node.AppendChild(XNod.OwnerDocument.CreateElement("Offline")) : Offl.InnerText = Command.Offline
            Dim Exte = Node.AppendChild(XNod.OwnerDocument.CreateElement("Extended")) : Exte.InnerText = False
            Dim Usag = Node.AppendChild(XNod.OwnerDocument.CreateElement("Usage")) : Usag.InnerText = ""
            Dim Pars = Node.AppendChild(XNod.OwnerDocument.CreateElement("Parameters"))
            For Each Para In Command.OptPara
                Dim Par = Pars.AppendChild(Node.OwnerDocument.CreateElement("Parameter"))
                Par.Attributes.Append(Node.OwnerDocument.CreateAttribute("Name")) : Par.Attributes("Name").Value = Para.Name
                Par.Attributes.Append(Node.OwnerDocument.CreateAttribute("Default")) : Par.Attributes("Default").Value = Para.Default
                If Not IsNothing(Para.Values) Then
                    For Each ParVal In Para.Values
                        Dim Val = Par.AppendChild(Node.OwnerDocument.CreateElement("Value")) : Val.InnerText = ParVal
                    Next
                End If
            Next
        Next
        Return XDoc
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

    Sub AddParameter(ByVal Node As TreeNode, ByVal Name As String, ByVal Options As String)
        Dim TreePath As String = Node.FullPath.Replace("/", ".").Replace(Node.TreeView.Nodes(0).Text & ".", "PM3/")
        Dim TreePathP As String = "/Parameters" : Dim XNODE = XDOC.SelectSingleNode(TreePath & TreePathP)
        Dim Par = XNODE.AppendChild(XNODE.OwnerDocument.CreateElement("Parameter"))

        Par.Attributes.Append(XNODE.OwnerDocument.CreateAttribute("Name")) : Par.Attributes("Name").Value = Name
        Par.Attributes.Append(XNODE.OwnerDocument.CreateAttribute("Default")) : Par.Attributes("Default").Value = Split(Options, ",")(0)
        For Each ParVal In Split(Options, ",")
            Dim Val = Par.AppendChild(Par.OwnerDocument.CreateElement("Value")) : Val.InnerText = ParVal
        Next : XNODE.OwnerDocument.Save("PMConfig.xml")
        CType(Node.Tag, Commands.Command).OptPara.Add(New Commands.Parameter(Par))
    End Sub
    Sub DelParameter(ByVal Node As TreeNode, ByVal Param As Parameter)
        Dim TreePath As String = Node.FullPath.Replace("/", ".").Replace(Node.TreeView.Nodes(0).Text & ".", "PM3/")
        Dim TreePathP As String = "/Parameters/Parameter[@Name='" & Param.Name & "']"
        Dim XNODE = XDOC.SelectSingleNode(TreePath & TreePathP)

        XNODE.ParentNode.RemoveChild(XNODE) : XNODE.OwnerDocument.Save("PMConfig.xml")
        CType(Node.Tag, Commands.Command).OptPara.Remove(Param)
    End Sub

    Public Class Command
        Property OrgPath As String
        Property ModPath As String()
        Property Offline As Boolean
        Property HelpTxt As String
        Property OptPara As New List(Of Parameter)
        Public Overrides Function ToString() As String
            Return String.Join(".", ModPath) & " :" & OptPara.Count
        End Function
        Sub New(ByVal XMLNode As Xml.XmlNode)
            OrgPath = XMLNode.Name
            ModPath = Split(OrgPath, ".")
            Offline = CBool(XMLNode.SelectSingleNode("Offline").InnerText)
            HelpTxt = XMLNode.SelectSingleNode("Help").InnerText
            Dim Parameters As Xml.XmlNodeList = XMLNode.SelectNodes("Parameters/Parameter")
            For Each Entry As Xml.XmlNode In Parameters
                OptPara.Add(New Commands.Parameter(Entry))
            Next
        End Sub
        Sub New(ByVal Commandline As String)
            Dim SPLT As System.Text.RegularExpressions.Match = System.Text.RegularExpressions.Regex.Match(Commandline, "^([a-zA-Z0-9_ ]*)\|([YN][ ]*)\|(.*)?$", System.Text.RegularExpressions.RegexOptions.Multiline)
            OrgPath = SPLT.Groups(1).Value.Trim : If OrgPath = "" Then OrgPath = Commandline.Split("|")(0).Trim
            'If OrgPath.Contains("lf") Then
            '    Dim a = 1
            'End If
            ModPath = Split(OrgPath, " ")
            Offline = SPLT.Groups(2).Value.Trim = "Y"
            HelpTxt = SPLT.Groups(3).Value.Trim

            Dim ReplText As String = HelpTxt
            Dim Params As System.Text.RegularExpressions.MatchCollection = System.Text.RegularExpressions.Regex.Matches(ReplText, "(\[(.*?)\]|<(.*?)>)")
            For i As Integer = 0 To Params.Count - 1
                Dim ParaVal As String = Params(i).Groups(1).Value
                If ParaVal.StartsWith("<") Then ParaVal = ParaVal.Remove(0, 1)
                If ParaVal.EndsWith(">") Then ParaVal = ParaVal.Remove(ParaVal.Length - 1, 1)
                If ParaVal.StartsWith("[") Then ParaVal = ParaVal.Remove(0, 1)
                If ParaVal.EndsWith("]") Then ParaVal = ParaVal.Remove(ParaVal.Length - 1, 1)
                OptPara.Add(New Parameter(ParaVal))
                ReplText = ReplText.Replace(Params(i).Value, "")
            Next

        End Sub
    End Class

    Public Class Parameter
        Public Name As String
        Public Values As String()
        Public [Default] As String = 0
        Sub New(ByVal XMLNode As Xml.XmlNode)
            Dim VALBuff As New List(Of String)
            Name = XMLNode.Attributes("Name").Value
            [Default] = XMLNode.Attributes("Default").Value
            For Each SubNode In XMLNode.ChildNodes
                VALBuff.Add(SubNode.innerText)
            Next
            Values = VALBuff.ToArray
        End Sub
        Sub New(ByVal _Name As String)
            Name = _Name
            If IsNothing(_Name) Then Exit Sub
            If _Name.Trim = "" Then Exit Sub
            If _Name.StartsWith("<") Then _Name = Mid(_Name, 2, Len(_Name) - 2)
            If System.Text.RegularExpressions.Regex.IsMatch(_Name, "<(.*?)>") Then
                Values = Split(System.Text.RegularExpressions.Regex.Match(Name, "<(.*?)>").Groups(1).Value, "|")
                [Default] = Values(0)
            Else
                If System.Text.RegularExpressions.Regex.IsMatch(_Name, "\((.*?)\)") Then
                    Values = Split(System.Text.RegularExpressions.Regex.Match(_Name, "\((.*?)\)").Groups(1).Value, "|")
                    [Default] = Values(0)
                End If
                If _Name.Contains("|") Then
                    Values = Split(_Name, "|")
                    [Default] = Values(0)
                End If
            End If
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
