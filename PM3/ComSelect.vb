Imports System.Windows.Forms

Public Class ComSelect

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub ComSelect_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Dim LST = System.IO.Ports.SerialPort.GetPortNames()
        For Each Port In LST : ComboBox1.Items.Add(Port) : Next
        ComboBox1.Text = ComboBox1.Items(ComboBox1.Items.Count - 1)
    End Sub
End Class
