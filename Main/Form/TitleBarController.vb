Imports System.ComponentModel
Imports App.Common.Data.Model
Imports LecturePay.Common.Infrastructure
Imports MasterImport
Imports Microsoft.Win32
Imports PunchDataImport
Imports System.Drawing
Imports System.Windows.Forms

'===========================
' タイトルバーコントローラ
'===========================
Public Class TitleBarController

    Private ReadOnly mainForm As GHRUN_Main

    Private pnlTitle As Panel

    Private btnToggle As Button
    Private lblTitle As Label
    Private lblYM As Label
    Private lblLock As Label
    Private lblUser As Label
    Private lblSystemDate As Label
    Private btnMinimize As Button
    Private btnMaximize As Button
    Private btnAppClose As Button

    Private animationTimer As New Timer()
    Private isExpanding As Boolean = False
    Private isCollapsing As Boolean = False
    Private sidePanelWidth As Integer = 240
    Private minWidth As Integer = 0
    Private animationStep As Integer = 10



    Private resizing As Boolean = False
    Private lastCursor As Cursor = Cursors.Default


    Public Sub New(form As GHRUN_Main)
        Me.mainForm = form

        animationTimer.Interval = 10
        AddHandler animationTimer.Tick, AddressOf AnimationTimer_Tick

        AddHandler lblTitle.MouseDoubleClick, AddressOf TitleBar_DoubleClick
        AddHandler lblYM.MouseDoubleClick, AddressOf TitleBar_DoubleClick
        AddHandler lblLock.MouseDoubleClick, AddressOf TitleBar_DoubleClick
        AddHandler lblUser.MouseDoubleClick, AddressOf TitleBar_DoubleClick

#If DEBUG_DEV Then
        AddHandler lblSystemDate.MouseDoubleClick, AddressOf TitleBar_DoubleClick
#End If

    End Sub

    Public ReadOnly Property TitlePanel As Panel
        Get
            Return pnlTitle
        End Get
    End Property

    Public Sub InitializeTitleBar()
        AddTitleButton()
        AddMouseEventHandler()
        TitleLayoutRefresh()
        AddHandler pnlTitle.MouseDoubleClick, AddressOf TitleBar_DoubleClick
    End Sub

    Private Sub AddTitleButton()

        pnlTitle = New Panel With {
            .Name = "HeaderTitlePanel",
            .Dock = DockStyle.Top,
            .Height = 40,
            .BackColor = Color.DarkSlateGray
        }

        btnToggle = New Button With {
            .Text = "Ξ",
            .Font = New Font("游ゴシック", 12, FontStyle.Regular),
            .Size = New Size(30, 30),
            .ForeColor = Color.Black,
            .BackColor = GHRUN_Main.COLOR_BUTTON_DEFAULT,
            .Location = New Point(10, 5),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left
        }
        AddHandler btnToggle.Click, AddressOf toggleButton_Click
        pnlTitle.Controls.Add(btnToggle)

        lblTitle = New Label With {
            .Name = "lblAppliName",
            .Text = AppCommon.ApplicationName,
            .Font = New Font("游ゴシック", 12, FontStyle.Regular),
            .Location = New Point(10, 10),
            .Height = 30,
            .ForeColor = Color.White,
            .BackColor = Color.DarkSlateGray,
            .AutoSize = True}
        pnlTitle.Controls.Add(lblTitle)

        lblYM = New Label With {
            .Name = "lblYearMonth",
            .Text = AppCommon.GetTargetYearMonth(),
            .Font = New Font("游ゴシック", 12, FontStyle.Regular),
            .Location = New Point(10, 10),
            .ForeColor = Color.White,
            .BackColor = Color.DarkSlateGray,
            .AutoSize = True
        }
        pnlTitle.Controls.Add(lblYM)

        lblLock = New Label With {
            .Name = "lblLockState",
            .Text = AppCommon.GetLockStatus(),
            .Font = New Font("游ゴシック", 12, FontStyle.Regular),
            .Location = New Point(10, 10),
            .ForeColor = Color.White,
            .BackColor = Color.DarkSlateGray,
            .AutoSize = True
        }
        pnlTitle.Controls.Add(lblLock)

        lblUser = New Label With {
            .Name = "lblUserName",
            .Text = AppCommon.GetUser(),
            .Font = New Font("游ゴシック", 12, FontStyle.Regular),
            .Location = New Point(10, 10),
            .ForeColor = Color.White,
            .BackColor = Color.DarkSlateGray,
            .AutoSize = True
        }
        pnlTitle.Controls.Add(lblUser)

#If DEBUG_DEV Then
        lblSystemDate = New Label With {
            .Text = "(デバッグ用)日付：",
            .Font = New Font("游ゴシック", 12, FontStyle.Regular),
            .Location = New Point(10, 10),
            .ForeColor = Color.White,
            .BackColor = Color.DarkSlateGray,
            .AutoSize = True}
        pnlTitle.Controls.Add(lblSystemDate)
#End If

        btnMinimize = New Button With {
            .Text = "＿",
            .Font = New Font("游ゴシック", 12, FontStyle.Regular),
            .Size = New Size(30, 30),
            .ForeColor = Color.Black,
            .BackColor = GHRUN_Main.COLOR_BUTTON_DEFAULT,
            .Location = New Point(10, 5),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left
        }
        AddHandler btnMinimize.Click,
            Sub(sender, e) mainForm.WindowState = FormWindowState.Minimized
        pnlTitle.Controls.Add(btnMinimize)

        btnMaximize = New Button With {
            .Text = "□",
            .Font = New Font("游ゴシック", 12, FontStyle.Regular),
            .Size = New Size(30, 30),
            .ForeColor = Color.Black,
            .BackColor = GHRUN_Main.COLOR_BUTTON_DEFAULT,
            .Location = New Point(10, 5),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left}
        AddHandler btnMaximize.Click,
            Sub(sender, e)
                mainForm.WindowState = FormWindowState.Normal
                mainForm.Location = ScreenUtil.ScreenWorkAreaLocation(mainForm)
                mainForm.Size = ScreenUtil.ScreenWorkAreaSize(mainForm)
            End Sub
        pnlTitle.Controls.Add(btnMaximize)

        btnAppClose = New Button With {
            .Text = "閉じる",
            .Font = New Font("游ゴシック", 12, FontStyle.Regular),
            .Size = New Size(80, 30),
            .ForeColor = Color.Black,
            .BackColor = GHRUN_Main.COLOR_BUTTON_DEFAULT,
            .Location = New Point(10, 5),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left,
            .Name = "btnAppClose"
        }
        AddHandler btnAppClose.Click, Sub(sender, e) mainForm.Close()
        pnlTitle.Controls.Add(btnAppClose)

        mainForm.Controls.Add(pnlTitle)
    End Sub

    Public Sub TitleLayoutRefresh()
        If GHRUN_Main.IsImportComplete Then
            Return
        End If

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

    Private Sub toggleButton_Click(sender As Object, e As EventArgs)
        Dim pnlMenu = mainForm.SideMenuPanel
        If pnlMenu.Width > minWidth Then
            isCollapsing = False
            isExpanding = True
        Else
            isCollapsing = True
            isExpanding = False
        End If
        animationTimer.Start()
    End Sub

    Private Sub AnimationTimer_Tick(sender As Object, e As EventArgs)
        Dim pnlMenu = mainForm.SideMenuPanel
        If isCollapsing Then
            pnlMenu.Width += animationStep
            If pnlMenu.Width >= sidePanelWidth Then
                pnlMenu.Width = sidePanelWidth
                animationTimer.Stop()
                isExpanding = False
                btnToggle.Text = "Ξ"
            End If
        ElseIf isExpanding Then
            pnlMenu.Width -= animationStep
            If pnlMenu.Width <= minWidth Then
                pnlMenu.Width = minWidth
                animationTimer.Stop()
                isCollapsing = False
                btnToggle.Text = "Ξ"
            End If
        End If
    End Sub

    '    Private Sub AddMouseEventHandler()
    '        Dim pnlMenu = mainForm.SideMenuPanel

    '        AddHandler pnlMenu.MouseDown, AddressOf AllControls_MouseDown
    '        AddHandler pnlMenu.MouseMove, AddressOf AllControls_MouseMove

    '        AddHandler pnlTitle.MouseDown, AddressOf AllControls_MouseDown
    '        AddHandler pnlTitle.MouseMove, AddressOf AllControls_MouseMove

    '        AddHandler lblTitle.MouseDown, AddressOf AllControls_MouseDown
    '        AddHandler lblYM.MouseDown, AddressOf AllControls_MouseDown
    '        AddHandler lblLock.MouseDown, AddressOf AllControls_MouseDown
    '        AddHandler lblUser.MouseDown, AddressOf AllControls_MouseDown

    '#If DEBUG_DEV Then
    '        AddHandler lblSystemDate.MouseDown, AddressOf AllControls_MouseDown
    '#End If

    '        For Each ctrl As Control In mainForm.Controls
    '            If TypeOf ctrl Is System.Windows.Forms.MdiClient Then
    '                AddHandler ctrl.MouseDown, AddressOf AllControls_MouseDown
    '                AddHandler ctrl.MouseMove, AddressOf AllControls_MouseMove
    '                Exit For
    '            End If
    '        Next
    '    End Sub


    Private Sub AddMouseEventHandler()

        Dim ctrls() As Control = {
        pnlTitle,
        lblTitle,
        lblYM,
        lblLock,
        lblUser
    }

#If DEBUG_DEV Then
    ctrls = ctrls.Concat(New Control() {lblSystemDate}).ToArray()
#End If

        For Each c As Control In ctrls
            AddHandler c.MouseDown, AddressOf AllControls_MouseDown
            AddHandler c.MouseMove, AddressOf AllControls_MouseMove
            AddHandler c.MouseUp, AddressOf AllControls_MouseUp
        Next

        ' ★ MDI クライアントにもドラッグを付与
        For Each ctrl As Control In mainForm.Controls
            If TypeOf ctrl Is MdiClient Then
                AddHandler ctrl.MouseDown, AddressOf AllControls_MouseDown
                AddHandler ctrl.MouseMove, AddressOf AllControls_MouseMove
                AddHandler ctrl.MouseUp, AddressOf AllControls_MouseUp
            End If
        Next
    End Sub

    'Private Sub AllControls_MouseDown(sender As Object, e As MouseEventArgs)
    '    If e.Button = MouseButtons.Left Then
    '        Dim screenPoint As Point = CType(sender, Control).PointToScreen(e.Location)
    '        Dim formPoint As Point = mainForm.PointToClient(screenPoint)
    '        Dim resizeDir As UInteger = mainForm.GetHitTestResult(formPoint)
    '        GHRUN_Main.ReleaseCapture()
    '        GHRUN_Main.SendMessage(mainForm.Handle, GHRUN_Main.WM_NCLBUTTONDOWN, CType(resizeDir, IntPtr), IntPtr.Zero)
    '    End If
    'End Sub

    'Private Sub AllControls_MouseMove(sender As Object, e As MouseEventArgs)

    '    If e.Button = MouseButtons.Left Then Return

    '    Dim screenPoint As Point = CType(sender, Control).PointToScreen(e.Location)
    '    Dim formPoint As Point = mainForm.PointToClient(screenPoint)
    '    Dim resizeDir As UInteger = mainForm.GetHitTestResult(formPoint)

    '    Dim newCursor As Cursor = Cursors.Default

    '    Select Case resizeDir
    '        Case GHRUN_Main.HT_LEFT, GHRUN_Main.HT_RIGHT
    '            newCursor = Cursors.SizeWE
    '        Case GHRUN_Main.HT_TOP, GHRUN_Main.HT_BOTTOM
    '            newCursor = Cursors.SizeNS
    '        Case GHRUN_Main.HT_TOPLEFT, GHRUN_Main.HT_BOTTOMRIGHT
    '            newCursor = Cursors.SizeNWSE
    '        Case GHRUN_Main.HT_TOPRIGHT, GHRUN_Main.HT_BOTTOMLEFT
    '            newCursor = Cursors.SizeNESW
    '        Case Else
    '            newCursor = Cursors.Default
    '    End Select

    '    ' ★ カーソルが変わった時だけ変更する（ちらつき防止）
    '    If lastCursor IsNot newCursor Then
    '        mainForm.Cursor = newCursor
    '        lastCursor = newCursor
    '    End If
    'End Sub
    Private resizing As Boolean = False
    Private lastCursor As Cursor = Cursors.Default
    Private Sub AllControls_MouseDown(sender As Object, e As MouseEventArgs)

        If e.Button <> MouseButtons.Left Then Return

        resizing = True

        Dim screenPoint As Point = CType(sender, Control).PointToScreen(e.Location)
        Dim formPoint As Point = mainForm.PointToClient(screenPoint)
        Dim resizeDir As UInteger = mainForm.GetHitTestResult(formPoint)

        GHRUN_Main.ReleaseCapture()
        GHRUN_Main.SendMessage(mainForm.Handle, GHRUN_Main.WM_NCLBUTTONDOWN, CType(resizeDir, IntPtr), IntPtr.Zero)
    End Sub

    Private Sub AllControls_MouseMove(sender As Object, e As MouseEventArgs)

        If resizing Then
            ' ★ リサイズ中はカーソル固定（ちらつき防止）
            mainForm.Cursor = lastCursor
            Return
        End If

        Dim screenPoint As Point = CType(sender, Control).PointToScreen(e.Location)
        Dim formPoint As Point = mainForm.PointToClient(screenPoint)
        Dim resizeDir As UInteger = mainForm.GetHitTestResult(formPoint)

        Dim newCursor As Cursor = Cursors.Default

        Select Case resizeDir
            Case GHRUN_Main.HT_LEFT, GHRUN_Main.HT_RIGHT
                newCursor = Cursors.SizeWE
            Case GHRUN_Main.HT_TOP, GHRUN_Main.HT_BOTTOM
                newCursor = Cursors.SizeNS
            Case GHRUN_Main.HT_TOPLEFT, GHRUN_Main.HT_BOTTOMRIGHT
                newCursor = Cursors.SizeNWSE
            Case GHRUN_Main.HT_TOPRIGHT, GHRUN_Main.HT_BOTTOMLEFT
                newCursor = Cursors.SizeNESW
        End Select

        If lastCursor IsNot newCursor Then
            mainForm.Cursor = newCursor
            lastCursor = newCursor
        End If
    End Sub

    Private Sub AllControls_MouseUp(sender As Object, e As MouseEventArgs)
        resizing = False
    End Sub

    Private Sub MaximizeWindow(sender As Object, e As EventArgs)
        If mainForm.WindowState = FormWindowState.Maximized Then
            mainForm.WindowState = FormWindowState.Normal
            mainForm.Location = ScreenUtil.ScreenWorkAreaLocation(mainForm)
            mainForm.Size = ScreenUtil.ScreenWorkAreaSize(mainForm)
        Else
            mainForm.WindowState = FormWindowState.Maximized
        End If
    End Sub

    Private Sub TitleBar_DoubleClick(sender As Object, e As EventArgs)
        MaximizeWindow(sender, e)
    End Sub


End Class