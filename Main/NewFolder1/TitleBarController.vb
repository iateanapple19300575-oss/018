Imports System.Drawing
Imports System.Windows.Forms

Public Class TitleBarController

    Private ReadOnly mainForm As GHRUN_Main

    ' Designer が生成したコントロールを参照するだけ
    Private ReadOnly pnlTitle As Panel
    Private ReadOnly btnToggle As Button
    Private ReadOnly lblTitle As Label
    Private ReadOnly lblYM As Label
    Private ReadOnly lblLock As Label
    Private ReadOnly lblUser As Label
    Private ReadOnly lblSystemDate As Label
    Private ReadOnly btnMinimize As Button
    Private ReadOnly btnMaximize As Button
    Private ReadOnly btnAppClose As Button

    ' アニメーション制御
    Private isExpanding As Boolean = False
    Private isCollapsing As Boolean = False
    Private sidePanelWidth As Integer = GHRUN_Main.SIDE_PANEL_WIDTH
    Private minWidth As Integer = 0
    Private animationStep As Integer = 10

    Public Sub New(form As GHRUN_Main)
        Me.mainForm = form

        ' ★ Designer が生成したコントロールを参照
        pnlTitle = form.pnlTitle
        btnToggle = form.btnToggle
        lblTitle = form.lblTitle
        lblYM = form.lblYM
        lblLock = form.lblLock
        lblUser = form.lblUser
        lblSystemDate = form.lblSystemDate
        btnMinimize = form.btnMinimize
        btnMaximize = form.btnMaximize
        btnAppClose = form.btnAppClose

        ' ★ Designer の animationTimer を使う
        AddHandler form.animationTimer.Tick, AddressOf AnimationTimer_Tick
    End Sub

    '===========================================================
    ' 初期化
    '===========================================================
    Public Sub InitializeTitleBar()

        ' イベント登録
        AddHandler btnToggle.Click, AddressOf toggleButton_Click
        AddHandler btnMinimize.Click, Sub() mainForm.WindowState = FormWindowState.Minimized
        AddHandler btnMaximize.Click, AddressOf MaximizeWindow
        AddHandler btnAppClose.Click, Sub() mainForm.Close()

        AddMouseEventHandler()

        TitleLayoutRefresh()
    End Sub

    '===========================================================
    ' タイトルバーのレイアウト調整
    '===========================================================
    Public Sub TitleLayoutRefresh()

        If GHRUN_Main.IsImportComplete Then Return

        btnToggle.Left = GHRUN_Main.TITLE_PANEL_MENU_CTRL_LEFT
        lblTitle.Left = GHRUN_Main.TITLE_PANEL_APPL_NAME_LEFT
        lblYM.Left = GHRUN_Main.TITLE_PANEL_YEAR_MONTH_LEFT
        lblLock.Left = GHRUN_Main.TITLE_PANEL_LOCK_STATUS_LEFT

#If DEBUG_DEV Then
        lblSystemDate.Left = mainForm.Width - GHRUN_Main.TITLE_PANEL_SYSTEM_DATE_RLEFT
#End If

        lblUser.Left = mainForm.Width - GHRUN_Main.TITLE_PANEL_USER_NAME_RLEFT
        btnMinimize.Left = mainForm.Width - GHRUN_Main.TITLE_PANEL_MIN_RLEFT
        btnMaximize.Left = mainForm.Width - GHRUN_Main.TITLE_PANEL_MAX_RLEFT
        btnAppClose.Left = mainForm.Width - GHRUN_Main.TITLE_PANEL_CLOSE_RLEFT
    End Sub

    '===========================================================
    ' 最大化ボタン
    '===========================================================
    Private Sub MaximizeWindow(sender As Object, e As EventArgs)
        If mainForm.WindowState = FormWindowState.Maximized Then
            mainForm.WindowState = FormWindowState.Normal
            mainForm.Location = ScreenUtil.ScreenWorkAreaLocation(mainForm)
            mainForm.Size = ScreenUtil.ScreenWorkAreaSize(mainForm)
        Else
            mainForm.WindowState = FormWindowState.Maximized
        End If
    End Sub

    '===========================================================
    ' ハンバーガーボタン（サイドメニュー開閉）
    '===========================================================
    Private Sub toggleButton_Click(sender As Object, e As EventArgs)

        Dim pnlMenu = mainForm.SideMenuPanel

        If pnlMenu.Width > minWidth Then
            isCollapsing = False
            isExpanding = True
        Else
            isCollapsing = True
            isExpanding = False
        End If

        mainForm.animationTimer.Start()
    End Sub

    '===========================================================
    ' サイドメニュー開閉アニメーション
    '===========================================================
    Private Sub AnimationTimer_Tick(sender As Object, e As EventArgs)

        Dim pnlMenu = mainForm.SideMenuPanel

        If isCollapsing Then

            pnlMenu.Width += animationStep

            If pnlMenu.Width >= sidePanelWidth Then
                pnlMenu.Width = sidePanelWidth
                mainForm.animationTimer.Stop()
                isExpanding = False
                btnToggle.Text = "Ξ"
            End If

        ElseIf isExpanding Then

            pnlMenu.Width -= animationStep

            If pnlMenu.Width <= minWidth Then
                pnlMenu.Width = minWidth
                mainForm.animationTimer.Stop()
                isCollapsing = False
                btnToggle.Text = "Ξ"
            End If

        End If

        mainForm.menuController.LayoutMenu()
    End Sub

    '===========================================================
    ' マウスドラッグによるウィンドウ移動
    '===========================================================
    Private Sub AddMouseEventHandler()

        ' タイトルバーの全コントロールにドラッグイベントを付与
        AddHandler pnlTitle.MouseDown, AddressOf AllControls_MouseDown
        AddHandler pnlTitle.MouseMove, AddressOf AllControls_MouseMove

        AddHandler lblTitle.MouseDown, AddressOf AllControls_MouseDown
        AddHandler lblYM.MouseDown, AddressOf AllControls_MouseDown
        AddHandler lblLock.MouseDown, AddressOf AllControls_MouseDown
        AddHandler lblUser.MouseDown, AddressOf AllControls_MouseDown

#If DEBUG_DEV Then
        AddHandler lblSystemDate.MouseDown, AddressOf AllControls_MouseDown
#End If

        ' MDI クライアントにも付与
        For Each ctrl As Control In mainForm.Controls
            If TypeOf ctrl Is MdiClient Then
                AddHandler ctrl.MouseDown, AddressOf AllControls_MouseDown
                AddHandler ctrl.MouseMove, AddressOf AllControls_MouseMove
                Exit For
            End If
        Next
    End Sub

    Private Sub AllControls_MouseDown(sender As Object, e As MouseEventArgs)

        If e.Button <> MouseButtons.Left Then Return

        Dim screenPoint As Point = CType(sender, Control).PointToScreen(e.Location)
        Dim formPoint As Point = mainForm.PointToClient(screenPoint)

        Dim resizeDir As UInteger = mainForm.GetHitTestResult(formPoint)

        GHRUN_Main.ReleaseCapture()
        GHRUN_Main.SendMessage(mainForm.Handle, GHRUN_Main.WM_NCLBUTTONDOWN, CType(resizeDir, IntPtr), IntPtr.Zero)
    End Sub

    Private Sub AllControls_MouseMove(sender As Object, e As MouseEventArgs)

        If e.Button = MouseButtons.Left Then Return

        Dim screenPoint As Point = CType(sender, Control).PointToScreen(e.Location)
        Dim formPoint As Point = mainForm.PointToClient(screenPoint)

        Dim resizeDir As UInteger = mainForm.GetHitTestResult(formPoint)

        Select Case resizeDir
            Case GHRUN_Main.HT_LEFT, GHRUN_Main.HT_RIGHT
                mainForm.Cursor = Cursors.SizeWE
            Case GHRUN_Main.HT_TOP, GHRUN_Main.HT_BOTTOM
                mainForm.Cursor = Cursors.SizeNS
            Case GHRUN_Main.HT_TOPLEFT, GHRUN_Main.HT_BOTTOMRIGHT
                mainForm.Cursor = Cursors.SizeNWSE
            Case GHRUN_Main.HT_TOPRIGHT, GHRUN_Main.HT_BOTTOMLEFT
                mainForm.Cursor = Cursors.SizeNESW
            Case Else
                mainForm.Cursor = Cursors.Default
        End Select
    End Sub

End Class
