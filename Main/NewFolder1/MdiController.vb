Imports System.Windows.Forms

Public Class MdiController

    Private ReadOnly mainForm As GHRUN_Main

    Public Sub New(form As GHRUN_Main)
        Me.mainForm = form
    End Sub

    '===========================================================
    ' MDI 子フォームアクティブ化時処理
    '===========================================================
    Public Sub OnMdiChildActivated()

        Dim child = mainForm.ActiveMdiChild

        ' メニュー活性制御（GHRUN_Main 側）
        mainForm.SideMenuEnable()

        If child Is Nothing Then
            Return
        End If

        ' 必要ならここに子フォームごとの UI 更新処理を追加可能
    End Sub

    '===========================================================
    ' 既に開いているフォームを再利用
    '===========================================================
    Public Function ShowChildForm(Of T As Form)() As Boolean

        For Each child As Form In mainForm.MdiChildren
            If TypeOf child Is T Then
                child.Activate()
                Return True
            End If
        Next

        Return False
    End Function

    '===========================================================
    ' 新規フォームを表示（位置調整含む）
    '===========================================================
    Public Sub ShowMdiChildForm(ByRef mdiChild As Form,
                                Optional startPosition As FormStartPosition = FormStartPosition.Manual)

        Dim mdiChildren() As Form = mainForm.MdiChildren

        ' 元コードの Mdihelper をそのまま利用
        Mdihelper.ShowMdiChildFormStart(mainForm, mdiChild, mdiChildren)
    End Sub

    '===========================================================
    ' 新しい画面を開き、他の子フォームを閉じる
    '===========================================================
    Public Sub CloseMDIChildScreen(newScreen As Form)

        ' 新しい画面を表示
        ShowMdiChildForm(newScreen)

        ' 他の画面を閉じる
        For Each frm As Form In mainForm.MdiChildren
            If Not frm.Name.Equals(newScreen.Name) Then
                frm.Close()
            End If
        Next
    End Sub

    '===========================================================
    ' 全子フォームを閉じる
    '===========================================================
    Public Sub CloseMDIChildAll()

        For Each frm As Form In mainForm.MdiChildren
            frm.Close()
        Next
    End Sub

End Class
