Public Class Markdown
    Dim CSS As New System.Text.StringBuilder
    Public Result As New System.Text.StringBuilder
    Dim SelLine As Integer = 0

    Enum State
        Regular = 0
        CodeBlock = 1
        List = 2
    End Enum
    Enum Blocks
        ' -----------
        ' Multi-Lines
        ' -----------
        None = 0
        Text = 24
        P = 1
        Blank = 23

        ListDots = 3
        ListNumb = 4

        ' -----------
        ' Single-Lines
        ' -----------
        CodeLine = 10
        TextLine = 11
        Checkbox = 18
        Include = 19
        H1 = 12
        H2 = 13
        H3 = 14
        H4 = 15
        H5 = 16
        H6 = 17
        HR = 22

        CodeBlock1 = 40
        CodeBlock2 = 41
        CodeBlock3 = 42

        Table = 50

        BOLD = 25
    End Enum

    Sub New(ByVal MD As String)
        CSS.Clear() : CSS.Append(My.Computer.FileSystem.ReadAllText("MD.css"))
        GenHTML(MD)
    End Sub

    Public Function GenHTML(ByVal Text As String)
        Result.Clear()
        Result.AppendLine("<HTML>")
        Result.AppendLine("<HEAD>")
        Result.AppendLine("<STYLE>")
        Result.AppendLine(CSS.ToString)
        Result.AppendLine("</STYLE>")
        Result.AppendLine("</HEAD>")
        Result.AppendLine("</BODY>")

        Dim State As State = State.Regular
        Dim ThisLine As Blocks = Blocks.None
        Dim LastLine As Blocks = Blocks.None

        Dim Splitter As List(Of String) = Split(Text, vbLf).ToList

        For Each Line In Splitter
            ThisLine = LineDesc(Line)   ' Diese Zeile Typ

            Select Case State
                Case State.CodeBlock
                    Select Case ThisLine
                        Case Blocks.None : Result.AppendLine("<BR>")
                        Case Blocks.Text : Result.AppendLine(Line.Trim)
                        Case Blocks.CodeBlock1 : State = State.Regular : Result.AppendLine("</code></pre>")
                        Case Blocks.CodeBlock2 : Result.AppendLine(Line.Trim)
                        Case Blocks.Table : Result.AppendLine("<TR><TD>" & String.Join("</TD><TD>", Line.Split("|")) & "</TD></TR>")
                    End Select
                Case State.List
                    Select Case ThisLine
                        Case Blocks.CodeLine
                            State = State.Regular : Result.AppendLine("</ul>")
                            Result.AppendLine("<blockquote><code>" & Mid(Line, 2) & "</code></blockquote>")
                        Case Blocks.ListDots
                            Dim Info As LineLink = New LineLink(Mid(Line, 2))
                            Result.AppendLine("<li><a href='" & Info.Link & "'>" & Info.Text & "</a></li>") ' * [Text](#Anker)
                        Case Blocks.ListNumb : Result.AppendLine("<li>" & Mid(Line, 2) & "</li>")
                        Case Else : State = State.Regular : Result.AppendLine("</ul>")
                    End Select
                Case State.Regular
                    Select Case ThisLine
                        Case Blocks.None : Result.AppendLine("<BR>")
                        Case Blocks.Blank : Result.AppendLine("<BR>")
                        Case Blocks.Text : Result.AppendLine(Line.Trim & "</BR>")
                        Case Blocks.BOLD : Result.AppendLine(SingleLineStyle(Line))

                        Case Blocks.P : Result.AppendLine("<p>" & SingleLineStyle(Mid(Line, 1)))
                        Case Blocks.HR : Result.AppendLine("</HR>")

                        Case Blocks.H1 : Result.AppendLine("<H1 id='" & LineID(Line) & "'>" & LineID(Line) & "</H1>")
                        Case Blocks.H2 : Result.AppendLine("<H2 id='" & LineID(Line) & "'>" & LineID(Line) & "</H2>")
                        Case Blocks.H3 : Result.AppendLine("<H3 id='" & LineID(Line) & "'>" & LineID(Line) & "</H3>")
                        Case Blocks.H4 : Result.AppendLine("<H4 id='" & LineID(Line) & "'>" & LineID(Line) & "</H4>")
                        Case Blocks.H5 : Result.AppendLine("<H5 id='" & LineID(Line) & "'>" & LineID(Line) & "</H5>")
                        Case Blocks.H6 : Result.AppendLine("<H6 id='" & LineID(Line) & "'>" & LineID(Line) & "</H6>")

                        Case Blocks.CodeLine : Result.AppendLine("<blockquote><code>" & Mid(Line, 2) & "</code></blockquote>")
                        Case Blocks.ListDots
                            State = State.List
                            Dim Info As LineLink = New LineLink(Mid(Line, 2))
                            Result.AppendLine("<ul><li><a href='" & Info.Link & "'>" & Info.Text & "</a></li>") ' * [Text](#Anker)
                        Case Blocks.ListNumb
                            State = State.List
                            Dim Info As LineLink = New LineLink(Mid(Line, 2))
                            Result.AppendLine("<ul><li><a href='" & Info.Link & "'>" & Info.Text & "</a></li>") ' * [Text](#Anker)
                        Case Blocks.Checkbox : Result.AppendLine("<p><input type='checkbox' disabled" & IIf(Line.ToUpper.Contains("[X]"), " checked ", "") & ">" & Mid(Line, 6) & "</input></p>")
                        Case Blocks.Include : Result.AppendLine(Mid(Line, 2))

                        Case Blocks.CodeBlock1 : State = State.CodeBlock : Result.AppendLine("<pre><code>")
                        Case Blocks.CodeBlock2 : State = State.CodeBlock : Result.AppendLine("<pre><code>")
                        Case Blocks.CodeBlock3 : State = State.CodeBlock : Result.AppendLine("<pre><code>")
                    End Select
            End Select
        Next


        Result.AppendLine("</BODY>")
        Result.AppendLine("</HTML>")

        Return Result.ToString
    End Function

    Public Function LineID(ByVal Line As String) As String
        Dim Result As String = Line
        Result = Result.Replace("*", "")
        Result = Result.Replace("#", "")
        Result = Result.Replace(" ", "")
        Return Result.Trim
    End Function

    Public Function LineDesc(ByVal Line As String) As Blocks
        If Line.Trim = "" Then Return Blocks.None
        Dim Match As String = System.Text.RegularExpressions.Regex.Match(Line, "^(\*{1,6}|#{1,6}|>| {4}|\-{1,6}( \[.\])?|[0-9]*\.|!|```[A-Za-z0-9]*)", System.Text.RegularExpressions.RegexOptions.Multiline).Value
        If Line.Contains("|") Then Return Blocks.Table
        Select Case Match
            Case "" : Return Blocks.Text

            Case "*" : Return Blocks.ListDots       ' Auflistung
            Case "-" : Return Blocks.ListDots       ' Auflistung
            Case "+" : Return Blocks.ListDots       ' Auflistung

            Case "***" : Return Blocks.HR           ' Linie (HR)
            Case "---" : Return Blocks.HR           ' Linie (HR)
            Case "___" : Return Blocks.HR           ' Linie (HR)

            Case "**" : Return Blocks.BOLD          ' Font-Style: Bold

            Case "#" : Return Blocks.H1
            Case "##" : Return Blocks.H2
            Case "###" : Return Blocks.H3
            Case "####" : Return Blocks.H4
            Case "#####" : Return Blocks.H5
            Case "######" : Return Blocks.H6

            Case ">" : Return Blocks.CodeLine       ' Code-Zeile
            Case "    " : Return Blocks.CodeBlock2   ' Code-Block Variante2
            Case vbTab : Return Blocks.CodeBlock3   ' Code-Block Variante3
            Case "!" : Return Blocks.Include        ' Eine Markdown Datei einbinden

            Case Else
                If Line.StartsWith("```") Then Return Blocks.CodeBlock1
                If IsNumeric(Match.Replace(".", "")) Then Return Blocks.ListNumb
                If Match.Contains("[") And Match.Contains("]") Then Return Blocks.Checkbox
        End Select
        Return Blocks.None
    End Function
    Class LineLink
        Public Text As String
        Public Link As String
        Sub New(ByVal Source As String)
            Dim RGX As System.Text.RegularExpressions.Match = System.Text.RegularExpressions.Regex.Match(Source, "\[(.*?)\]\((.*?)\)")
            Text = RGX.Groups(1).Value
            Link = RGX.Groups(2).Value
        End Sub
    End Class
    Public Function SingleLineStyle(ByVal Text As String) As String
        If Text.Trim = "" Then Return ""
        Dim Result As String = Text

        ' -------------------------------------------------------------------------
        ' ersetzt ** Text ** durch <B> Text </B>
        ' -------------------------------------------------------------------------
        Do Until Result.Contains("**") = False
            Dim Bolds As System.Text.RegularExpressions.MatchCollection = System.Text.RegularExpressions.Regex.Matches(Result, "(_{2}(.*)_{2}|\*{2}(.*)\*{2})", System.Text.RegularExpressions.RegexOptions.Multiline)
            For Each eintrag As System.Text.RegularExpressions.Match In Bolds
                If Len(eintrag.Value) < 4 Then Continue For
                Result = Result.Replace(eintrag.Value, "<B>" & Mid(eintrag.Value, 3, Len(eintrag.Value) - 4) & "</B>")
            Next
        Loop

        ' -------------------------------------------------------------------------
        ' ersetzt * Text * durch <I> Text </I>
        ' -------------------------------------------------------------------------
        Dim Itals As System.Text.RegularExpressions.MatchCollection = System.Text.RegularExpressions.Regex.Matches(Result, "(_{1}(.*)_{1}|\*{1}(.*)\*{1})", System.Text.RegularExpressions.RegexOptions.Multiline)
        For Each eintrag As System.Text.RegularExpressions.Match In Itals
            If Len(eintrag.Value) < 2 Then Continue For
            Result = Result.Replace(eintrag.Value, "<em>" & Mid(eintrag.Value, 2, Len(eintrag.Value) - 2) & "</em>")
        Next

        Dim Delets As System.Text.RegularExpressions.MatchCollection = System.Text.RegularExpressions.Regex.Matches(Result, "~{2}(.*)~{2}", System.Text.RegularExpressions.RegexOptions.Multiline)
        For Each eintrag As System.Text.RegularExpressions.Match In Delets
            Result = Result.Replace(eintrag.Value, "<del>" & Mid(eintrag.Value, 3, Len(eintrag.Value) - 4) & "</del>")
        Next

        Dim Codes As System.Text.RegularExpressions.MatchCollection = System.Text.RegularExpressions.Regex.Matches(Result, "`(.*)`", System.Text.RegularExpressions.RegexOptions.Multiline)
        For Each eintrag As System.Text.RegularExpressions.Match In Codes
            Result = Result.Replace(eintrag.Value, "<code>" & eintrag.Groups(1).Value & "</code>")
        Next

        Dim Links As System.Text.RegularExpressions.MatchCollection = System.Text.RegularExpressions.Regex.Matches(Result, "(\[.*?\])(\(.*?\))", System.Text.RegularExpressions.RegexOptions.Multiline)
        For Each eintrag As System.Text.RegularExpressions.Match In Links
            Result = Result.Replace(eintrag.Value, "<a href='" & eintrag.Groups(2).Value & "'>" & eintrag.Groups(1).Value & "</a>")
        Next
        Return Result
    End Function

End Class
