Public Class P3GUI

    Dim HTMLTab As WebBrowser
    Dim HelpTab As TabPage
    Dim Commands As Commands
    Dim Process As PM3Process

    Sub CleanFSLogs()
        Try
            If My.Computer.FileSystem.DirectoryExists(".proxmark3") Then
                My.Computer.FileSystem.DeleteDirectory(".proxmark3", FileIO.DeleteDirectoryOption.DeleteAllContents)
                My.Computer.FileSystem.CreateDirectory(".proxmark3")
            End If
        Catch : End Try
    End Sub
    Sub KillOldProcesses()
        For Each PROC In System.Diagnostics.Process.GetProcesses
            If PROC.ProcessName.Contains("Proxmark3") Then PROC.Kill()
        Next
    End Sub

    Private Sub P3GUI_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        Process.RECVExit()
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        KillOldProcesses()
        CleanFSLogs()

        Commands = New Commands
        BuildTree(Commands.TREE, TreeView1.Nodes(0), Commands.BONE)
        TreeView1.Nodes(0).Expand()
        Dim Serial As New ComSelect : Serial.ShowDialog()
        Process = New PM3Process(Serial.ComboBox1.Text)
        AddHandler Process.NewLogEntry, AddressOf ListAdd
        AddHandler Process.OutputRecv, AddressOf ListAdd
        AddHandler Process.Processing, Sub(State As Boolean)
                                           LBLConn.BackColor = IIf(State = False, Color.Green, Color.Red)
                                           Application.DoEvents()
                                       End Sub

        Dim ProxWin As New FrameGrabber(TabControl1, "proxmark", True)
        Dim ProxOvr As New FrameGrabber(TabControl1, "overlays", True)

        HelpTab = New TabPage("Help")
        HTMLTab = New WebBrowser : HTMLTab.Dock = DockStyle.Fill
        HelpTab.Controls.Add(HTMLTab)

    End Sub

    Sub BuildTree(ByVal Tree As List(Of Commands.NodeClass), ByVal ParentNode As TreeNode, ByVal One As Boolean)
        For Each Entry In Tree
            If IsNothing(Entry.SubEntrys) Then
                Dim Node = ParentNode.Nodes.Add(Entry.ToString)
                Node.Tag = Entry.Entry
                Node.ToolTipText = Entry.ToString
                If One = True Then Node.TreeView.SelectedNode = Node
            Else
                Dim NewNode = ParentNode.Nodes.Add(Entry.ToString)
                BuildTree(Entry.SubEntrys, NewNode, One)
            End If
        Next
    End Sub
    Private Sub TreeView1_AfterSelect(ByVal sender As System.Object, ByVal e As System.Windows.Forms.TreeViewEventArgs) Handles TreeView1.AfterSelect
        CreateMenu(TreeView1.SelectedNode.Tag)
        ProvideHelp(TreeView1.SelectedNode.FullPath)
    End Sub

    Sub ProvideHelp(ByVal Path As String)
        TabControl1.TabPages.Remove(HelpTab)
        If My.Computer.FileSystem.DirectoryExists(Path) = False Then Exit Sub
        If My.Computer.FileSystem.GetFiles(Path).Count > 0 Then
            Dim FName As String = My.Computer.FileSystem.GetFiles(Path)(0)
            HelpTab.Text = Mid(FName, InStrRev(FName, "\") + 1)
            TabControl1.TabPages.Add(HelpTab)
            TabControl1.SelectedTab = HelpTab
            Dim MD As New Markdown(My.Computer.FileSystem.GetFiles(Path)(0))
            HTMLTab.DocumentText = MD.Result.ToString
        End If
    End Sub

    Sub CreateMenu(ByVal Command As Commands.Command)
        Dim OPTS As New List(Of Control)
        Dim ViewI As Integer = 0

        If IsNothing(Command) Then TxtRaw.Text = TreeView1.SelectedNode.FullPath.Replace(TreeView1.Nodes(0).Text, " ").Replace("/", " ")
        If Not IsNothing(Command) Then TxtRaw.Text = String.Join(" ", Command.ModPath)
        SplitContainer2.Panel1.Controls.Clear()
        If IsNothing(Command) Then Exit Sub

        ' -------------------------------------------------------------------------------------------------------
        ' Provide help from commandline and the command specific help
        ' -------------------------------------------------------------------------------------------------------
        Dim HLP As New TextBox
        HLP.ScrollBars = ScrollBars.Vertical
        HLP.Left = 10 : HLP.Top = 5
        HLP.Height = 100
        HLP.Width = SplitContainer2.Panel1.Width * 0.97
        HLP.Multiline = True : HLP.BorderStyle = BorderStyle.None
        HLP.Font = New Font("Arial", 10)
        HLP.BackColor = Color.LightYellow : SplitContainer2.Panel1.Controls.Add(HLP)

        Dim XNOD = Commands.XDOC.SelectSingleNode("PM3/" & TxtRaw.Text.Replace(" ", ".") & "/Usage")
        If String.IsNullOrEmpty(XNOD.InnerText) Then HLP.Text = Command.HelpTxt
        If Not String.IsNullOrEmpty(XNOD.InnerText) Then HLP.Text = XNOD.InnerText

        Dim BTNHeight As Integer = 25

        ' -------------------------------------------------------------------------------------------------------
        ' Group all Controls
        ' -------------------------------------------------------------------------------------------------------
        Dim GRP As New GroupBox
        GRP.Text = "Parameters"
        GRP.Left = 10
        GRP.Height = SplitContainer2.Panel1.Height - HLP.Height - (3 * BTNHeight)
        GRP.Top = HLP.Top + HLP.Height + 15
        GRP.Width = SplitContainer2.Panel1.Width * 0.97
        SplitContainer2.Panel1.Controls.Add(GRP)

        ' -------------------------------------------------------------------------------------------------------
        ' Control positioning
        ' -------------------------------------------------------------------------------------------------------
        Dim ACTTop As Integer = 20
        Dim ACTLeft1 As Integer = 10
        Dim ACTLeft2 As Integer = GRP.Width * 0.3
        Dim ACTLeft3 As Integer = GRP.Width * 0.9
        Dim ACTWidth1 As Integer = ACTLeft2 - ACTLeft1 - 10
        Dim ACTWidth2 As Integer = ACTLeft3 - ACTLeft2 - 10
        Dim ACTWidth3 As Integer = GRP.Width - ACTLeft3 - 20

        ' -------------------------------------------------------------------------------------------------------
        ' Parameters
        ' -------------------------------------------------------------------------------------------------------
        For Each Opt In Command.OptPara

            ACTTop = 20 + (ViewI * 30)

            If Opt.Name.ToLower = "filename" Then
                Dim BTNDIR As New Button
                BTNDIR.Text = "Save/Load Filename"
                BTNDIR.Top = ACTTop
                BTNDIR.Left = ACTLeft1
                BTNDIR.Width = ACTWidth2
                GRP.Controls.Add(BTNDIR)
                Continue For
            End If

            Dim LBL As New Label
            LBL.Text = Opt.Name
            LBL.Top = ACTTop
            LBL.Left = ACTLeft1
            LBL.Width = ACTWidth1
            GRP.Controls.Add(LBL)

            Dim CBO As New ComboBox
            If Not IsNothing(Opt.Values) Then CBO.Items.AddRange(Opt.Values)
            CBO.Top = ACTTop
            CBO.Left = ACTLeft2
            CBO.Width = ACTWidth2
            If Not IsNothing(Opt.Default) Then
                CBO.Text = Opt.Default
            Else
                If CBO.Items.Count > 0 Then
                    CBO.Text = CBO.Items(0)
                    TxtRaw.Text += " " & CBO.Text
                Else
                    CBO.Text = 0
                    TxtRaw.Text += " " & CBO.Text
                End If
            End If
            GRP.Controls.Add(CBO) : OPTS.Add(CBO)
            AddHandler CBO.TextChanged, Sub(S, E)
                                            TxtRaw.Text = String.Join(" ", Command.ModPath)
                                            For Each Entry In OPTS
                                                TxtRaw.Text += " " & Entry.Text
                                            Next
                                        End Sub

            Dim PARBTN As New Button
            PARBTN.Text = "X"
            PARBTN.Height = 22
            PARBTN.Top = ACTTop
            PARBTN.Left = ACTLeft3
            PARBTN.Width = ACTWidth3
            PARBTN.Tag = Opt
            GRP.Controls.Add(PARBTN)
            AddHandler PARBTN.Click, Sub()
                                         Commands.DelParameter(TreeView1.SelectedNode, Opt)
                                         CreateMenu(Command) : Exit Sub
                                     End Sub

            ViewI += 1
        Next

        Dim BTNADD As New Button
        BTNADD.Text = "Add Parameter"
        BTNADD.Top = 20 + (ViewI * 30)
        BTNADD.Left = ACTLeft3
        BTNADD.Width = ACTWidth3
        AddHandler BTNADD.Click, Sub()
                                     Dim ParNam As String = InputBox("Name")
                                     Dim ParVal As String = InputBox("Values (Comma-seperated)")
                                     Commands.AddParameter(TreeView1.SelectedNode, ParNam, ParVal)
                                     CreateMenu(Command) : Exit Sub
                                 End Sub
        GRP.Controls.Add(BTNADD)


        Dim BTN As New Button
        BTN.Text = "Run"
        BTN.Left = 10
        BTN.Top = SplitContainer2.Panel1.Height - BTN.Height - BTN.Height - 5
        BTN.Width = SplitContainer2.Panel1.Width - 20
        BTN.Enabled = Process.IsReady
        AddHandler BTN.Click, Sub()
                                  ListView1.Items.Clear() : TabControl1.SelectedTab = TabControl1.TabPages(0)
                                  Dim SendText As String = TreeView1.SelectedNode.FullPath.Replace(TreeView1.Nodes(0).Text & "/", "")
                                  Process.SendCommand(SendText.Replace("/", " "))
                              End Sub
        SplitContainer2.Panel1.Controls.Add(BTN)

        Dim BTNH As New Button
        BTNH.Text = "Usage and Examples"
        BTNH.Left = 10
        BTNH.Top = SplitContainer2.Panel1.Height - BTN.Height - 5
        BTNH.Width = SplitContainer2.Panel1.Width - 20
        BTNH.Enabled = Process.IsReady
        AddHandler BTNH.Click, Sub()
                                   ListView1.Items.Clear() : TabControl1.SelectedTab = TabControl1.TabPages(0)
                                   Dim SendText As String = TreeView1.SelectedNode.FullPath.Replace(TreeView1.Nodes(0).Text & "/", "")
                                   Dim Res = Process.SendCommand(SendText.Replace("/", " ") & " h")
                                   Dim NOD = Commands.XDOC.SelectSingleNode("PM3/" & SendText.Replace("/", ".") & "/Usage")
                                   NOD.InnerText = Res : NOD.OwnerDocument.Save("PMConfig.xml")
                                   Command.HelpTxt += vbCrLf & vbCrLf & Res : HLP.Text = Res
                               End Sub
        SplitContainer2.Panel1.Controls.Add(BTNH)

    End Sub

#Region "Resize"
    Private Sub Form1_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
        If IsNothing(TreeView1.SelectedNode) Then Exit Sub
        CreateMenu(TreeView1.SelectedNode.Tag)
    End Sub
    Private Sub SplitContainer1_SplitterMoved(ByVal sender As Object, ByVal e As System.Windows.Forms.SplitterEventArgs) Handles SplitContainer1.SplitterMoved
        If IsNothing(TreeView1.SelectedNode) Then Exit Sub
        CreateMenu(TreeView1.SelectedNode.Tag)
    End Sub
    Private Sub SplitContainer2_SplitterMoved(ByVal sender As Object, ByVal e As System.Windows.Forms.SplitterEventArgs) Handles SplitContainer2.SplitterMoved
        If IsNothing(TreeView1.SelectedNode) Then Exit Sub
        CreateMenu(TreeView1.SelectedNode.Tag)
    End Sub
#End Region

    Delegate Sub ListAddDelegate(ByVal Text As String)
    Private Sub ListAdd(ByVal Text As String)
        If Me.InvokeRequired = True Then
            ListView1.Invoke(New ListAddDelegate(AddressOf ListAdd), Text)
        Else
            ListView1.Items.Add(Text)
        End If
    End Sub

    Private Sub P3GUI_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize, SplitContainer1.SplitterMoved
        Application.DoEvents()
        ToolStripTextBox1.Size = New Drawing.Size(SplitContainer1.Panel1.Width, ToolStripTextBox1.Height)
        TxtRaw.Size = New Drawing.Size(SplitContainer1.Panel2.Width - BtnRawSend.Width, TxtRaw.Height)
    End Sub
    Private Sub ToolStripTextBox1_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles ToolStripTextBox1.KeyUp
        TxtRaw.Text = TreeView1.SelectedNode.FullPath.Replace(TreeView1.Nodes(0).Text & "/", "") : SplitContainer2.Panel1.Controls.Clear()

        TreeView1.Nodes(0).Nodes.Clear()
        TreeView1.BeginUpdate()
        Commands.SetFilter(ToolStripTextBox1.Text)
        BuildTree(Commands.TREE, TreeView1.Nodes(0), Commands.BONE)

        If ToolStripTextBox1.Text <> "" Then TreeView1.Nodes(0).ExpandAll()
        If ToolStripTextBox1.Text = "" Then TreeView1.Nodes(0).Collapse(False) : TreeView1.Nodes(0).Expand()
        TreeView1.EndUpdate()
    End Sub
    Private Sub BtnRawSend_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BtnRawSend.Click
        ListView1.Items.Clear()
        Process.SendCommand(TxtRaw.Text)
    End Sub

End Class
