'===========================
' MDI コントローラ
'===========================
Public Class MdiController

    Private ReadOnly mainForm As GHRUN_Main

    Public Sub New(form As GHRUN_Main)
        Me.mainForm = form
    End Sub

    Public Sub OnMdiChildActivated()
        Dim child = mainForm.ActiveMdiChild
        mainForm.SideMenuEnable()

        If child Is Nothing Then
            Return
        End If
    End Sub

    Public Sub ShowMdiChildForm(ByRef mdiChild As Form, Optional ByVal startPosition As FormStartPosition = FormStartPosition.Manual)
        Dim mdiChildren() As Form = mainForm.MdiChildren
        Mdihelper.ShowMdiChildFormStart(mainForm, mdiChild, mdiChildren)
    End Sub

    Public Function ShowChildForm(Of T As Form)() As Boolean
        For Each child As Form In mainForm.MdiChildren
            If TypeOf child Is T Then
                child.Activate()
                Return True
            End If
        Next
        Return False
    End Function

    Public Sub CloseMDIChildScreen(ByVal newScreen As Form)
        ShowMdiChildForm(newScreen)
        For Each frm As Form In mainForm.MdiChildren
            If Not frm.Name.Equals(newScreen.Name) Then
                frm.Close()
            End If
        Next
    End Sub

    Public Sub CloseMDIChildAll()
        For Each frm As Form In mainForm.MdiChildren
            frm.Close()
        Next
    End Sub

End Class