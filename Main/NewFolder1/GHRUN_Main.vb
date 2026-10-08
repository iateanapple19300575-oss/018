Imports System.ComponentModel
Imports App.Common.Data.Model
Imports LecturePay.Common.Infrastructure
Imports MasterImport
Imports Microsoft.Win32
Imports PunchDataImport
Imports System.Drawing
Imports System.Windows.Forms

Public Class GHRUN_Main
    Inherits BaseForm
    Implements IMDIParent

    '===========================
    ' コントローラ
    '===========================
    Private titleController As TitleBarController
    Private menuController As SideMenuController
    Private mdiController As MdiController

    '===========================
    ' Designer が生成するコントロール
    '===========================
    Friend WithEvents animationTimer As Timer
    Friend WithEvents LockMonitoringTimer As Timer

    Friend WithEvents pnlTitle As Panel
    Friend WithEvents pnlMenu As Panel

    Friend WithEvents btnToggle As Button
    Friend WithEvents lblTitle As Label
    Friend WithEvents lblYM As Label
    Friend WithEvents lblLock As Label
    Friend WithEvents lblUser As Label
    Friend WithEvents lblSystemDate As Label
    Friend WithEvents btnMinimize As Button
    Friend WithEvents btnMaximize As Button
    Friend WithEvents btnAppClose As Button

    '===========================
    ' 定数（元コードから移植）
    '===========================
    Public Const BORDER_WIDTH As Integer = 10
    Public Const HT_LEFT As Integer = 10
    Public Const HT_RIGHT As Integer = 11
    Public Const HT_TOP As Integer = 12
    Public Const HT_TOPLEFT As Integer = 13
    Public Const HT_TOPRIGHT As Integer = 14
    Public Const HT_BOTTOM As Integer = 15
    Public Const HT_BOTTOMLEFT As Integer = 16
    Public Const HT_BOTTOMRIGHT As Integer = 17
    Public Const WM_NCHITTEST As Integer = &H84
    Public Const WM_NCLBUTTONDOWN As UInteger = &HA1
    Public Const HT_CAPTION As UInteger = &H2

    Public Const SIDE_PANEL_WIDTH As Integer = 240

    Public Const TITLE_MARGIN As Integer = 10
    Public Const TITLE_MENU_CTRL_WIDTH As Integer = 30
    Public Const TITLE_APPL_NAME_WIDTH As Integer = 120
    Public Const TITLE_YEAR_MONTH_WIDTH As Integer = 260
    Public Const TITLE_LOCK_STATUS_WIDTH As Integer = 150
    Public Const TITLE_USER_NAME_WIDTH As Integer = 200
    Public Const TITLE_SYSTEM_DATE_WIDTH As Integer = 300
    Public Const TITLE_MIN_WIDTH As Integer = 30
    Public Const TITLE_MAX_WIDTH As Integer = 30
    Public Const TITLE_CLOSE_WIDTH As Integer = 80

    Public Const TITLE_PANEL_MENU_CTRL_LEFT As Integer = TITLE_MARGIN
    Public Const TITLE_PANEL_APPL_NAME_LEFT As Integer = TITLE_MARGIN + TITLE_MENU_CTRL_WIDTH
    Public Const TITLE_PANEL_YEAR_MONTH_LEFT As Integer = TITLE_PANEL_APPL_NAME_LEFT + TITLE_MARGIN + TITLE_APPL_NAME_WIDTH
    Public Const TITLE_PANEL_LOCK_STATUS_LEFT As Integer = TITLE_PANEL_YEAR_MONTH_LEFT + TITLE_MARGIN + TITLE_YEAR_MONTH_WIDTH
    Public Const TITLE_PANEL_SYSTEM_DATE_RLEFT As Integer = TITLE_SYSTEM_DATE_WIDTH + TITLE_MARGIN + TITLE_USER_NAME_WIDTH + TITLE_MARGIN + TITLE_MIN_WIDTH + TITLE_MAX_WIDTH + TITLE_CLOSE_WIDTH
    Public Const TITLE_PANEL_USER_NAME_RLEFT As Integer = TITLE_USER_NAME_WIDTH + TITLE_MARGIN + TITLE_MIN_WIDTH + TITLE_MAX_WIDTH + TITLE_CLOSE_WIDTH
    Public Const TITLE_PANEL_MIN_RLEFT As Integer = TITLE_MIN_WIDTH + TITLE_MAX_WIDTH + TITLE_CLOSE_WIDTH
    Public Const TITLE_PANEL_MAX_RLEFT As Integer = TITLE_MAX_WIDTH + TITLE_CLOSE_WIDTH
    Public Const TITLE_PANEL_CLOSE_RLEFT As Integer = TITLE_CLOSE_WIDTH + TITLE_MARGIN

    Public Shared COLOR_TITLE_BAR As Color = Color.DarkSlateGray
    Public Shared COLOR_BUTTON_ENABLE As Color = Color.LightGoldenrodYellow
    Public Shared COLOR_BUTTON_DEFAULT As Color = Color.LightGray

    Private Shared _initialized As Boolean = False

    Public Shared IsImportComplete As Boolean = False

    '===========================
    ' Win32 API（Shared なし）
    '===========================
    Public Declare Auto Function SendMessage Lib "user32.dll" (
        ByVal hWnd As IntPtr,
        ByVal Msg As UInteger,
        ByVal wParam As IntPtr,
        ByVal lParam As IntPtr
    ) As IntPtr

    Public Declare Auto Function ReleaseCapture Lib "user32.dll" () As Boolean

    '===========================
    ' コンストラクタ
    '===========================
    Public Sub New()
        MyBase.New("出講料計算メイン", False)
        InitializeComponent()

        Me.IsMdiContainer = True
        Me.FormBorderStyle = FormBorderStyle.None
        Me.BackColor = Color.Black
        Me.MinimumSize = New Size(960, 540)
        Me.Size = ScreenUtil.ScreenWorkAreaSize(Me)

        ' Designer の Timer を使用
        animationTimer.Interval = 10
        LockMonitoringTimer.Interval = 60000

        AddHandler SystemEvents.DisplaySettingsChanged, AddressOf OnDisplaySettingsChanged
        AddHandler SystemEvents.UserPreferenceChanged, AddressOf OnUserPreferenceChanged

        InitializeWindow()
    End Sub

    '===========================
    ' 初期化
    '===========================
    Private Sub InitializeWindow()

        ' ★ Designer が生成した pnlMenu を使用
        pnlMenu.Width = SIDE_PANEL_WIDTH
        pnlMenu.Dock = DockStyle.Left
        pnlMenu.AutoScroll = True
        pnlMenu.BackColor = Color.FromArgb(&HFFCAE6CA)

        ' コントローラ生成
        titleController = New TitleBarController(Me)
        menuController = New SideMenuController(Me, pnlMenu)
        mdiController = New MdiController(Me)

        ' タイトルバー初期化
        titleController.InitializeTitleBar()

        ' メニュー初期化
        menuController.InitializeMenu()
    End Sub

    '===========================
    ' Load
    '===========================
    Private Sub GHRUN_Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        AddHandler Me.MdiChildActivate, AddressOf OnMdiChildActivated

        If Not _initialized Then
            StateDispatcher.InitializeForUIThread()
            StateDispatcher.Attach()
            _initialized = True
        End If

        AppState.MDIMainForm = Me

        RestartLoad()
    End Sub

    Private Sub RestartLoad()
        DisplayInitialize()
        titleController.TitleLayoutRefresh()

        LockMonitoringTimer.Stop()
        LockMonitoringTimer.Start()
    End Sub

    '===========================
    ' 共通化されたフォーム起動
    '===========================
    Public Sub OpenChildFormDynamic(formType As Type, sender As Object, e As EventArgs)

        ' 既に開いている？
        For Each child As Form In Me.MdiChildren
            If child.GetType() Is formType Then
                child.Activate()
                Return
            End If
        Next

        ' ログ
        ContextMenuItem_Click_logger(sender, e)

        ' 新規生成して表示
        Dim newForm As Form = CType(Activator.CreateInstance(formType), Form)
        mdiController.CloseMDIChildScreen(newForm)
    End Sub

    '===========================
    ' MDI 子フォームアクティブ化
    '===========================
    Protected Overridable Sub OnMdiChildActivated(sender As Object, e As EventArgs)
        mdiController.OnMdiChildActivated()
    End Sub

    '===========================
    ' Resize
    '===========================
    Private Sub GHRUN_Main_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        titleController.TitleLayoutRefresh()
    End Sub

    Private Sub OnDisplaySettingsChanged(sender As Object, e As EventArgs)
        titleController.TitleLayoutRefresh()
    End Sub

    Private Sub OnUserPreferenceChanged(sender As Object, e As UserPreferenceChangedEventArgs)
        If e.Category = UserPreferenceCategory.Window Then
            titleController.TitleLayoutRefresh()
        End If
    End Sub

    '===========================
    ' WndProc（境界線ドラッグ）
    '===========================
    Protected Overrides Sub WndProc(ByRef m As Message)

        Const WM_NCCALCSIZE As Integer = &H83

        If m.Msg = WM_NCCALCSIZE Then
            ' 何もしない
        ElseIf m.Msg = WM_NCHITTEST Then

            MyBase.WndProc(m)

            If m.Result.ToInt32() <> HT_CAPTION AndAlso m.Result.ToInt32() <> 0 Then
                Return
            End If

            Dim screenPoint As Point = New Point(m.LParam.ToInt32() And &HFFFF, m.LParam.ToInt32() >> 16)
            Dim clientPoint As Point = Me.PointToClient(screenPoint)
            Dim result As UInteger = GetHitTestResult(clientPoint)

            If result <> 0 Then
                m.Result = CType(result, IntPtr)
                Return
            End If

        Else
            MyBase.WndProc(m)
        End If

    End Sub

    '===========================
    ' HitTest（境界線判定）
    '===========================
    Public Function GetHitTestResult(p As Point) As UInteger

        Dim left As Boolean = p.X < BORDER_WIDTH
        Dim right As Boolean = p.X > Me.Width - BORDER_WIDTH
        Dim top As Boolean = p.Y < BORDER_WIDTH
        Dim bottom As Boolean = p.Y > Me.Height - BORDER_WIDTH

        Select Case True
            Case left And top : Return HT_TOPLEFT
            Case left And bottom : Return HT_BOTTOMLEFT
            Case right And top : Return HT_TOPRIGHT
            Case right And bottom : Return HT_BOTTOMRIGHT
            Case left : Return HT_LEFT
            Case right : Return HT_RIGHT
            Case top : Return HT_TOP
            Case bottom : Return HT_BOTTOM
            Case Else : Return HT_CAPTION
        End Select

    End Function

    '===========================
    ' ロック監視
    '===========================
    Private Sub LockMonitoringTimer_Tick(sender As Object, e As EventArgs) Handles LockMonitoringTimer.Tick

        Dim lockInfo As YearMonthLockModel = (New LectPayMainViewController).TargetYearMonthLockCheck()

        If Not lockInfo.Success Then Return
        If lockInfo.SubStatus = 0 Then Return

        MessageBox.Show("他ユーザーに強制解除されました。年月度設定からやり直してください。")

        AppState.TargetYearMonth = Date.MinValue
        AppState.LockStatus = False

        mdiController.CloseMDIChildAll()
        RestartLoad()
        menuController.LayoutMenu()
    End Sub

    '===========================
    ' 初期表示
    '===========================
    Public Sub DisplayInitialize()

        AppState.DebugSystemDate = SystemDateForDebugging()

        Me.Location = New Point(0, 0)

        lblYM.Text = AppCommon.GetTargetYearMonth()
        lblLock.Text = AppCommon.GetLockStatus()
        lblUser.Text = AppCommon.GetUser()

#If DEBUG_DEV Then
        lblSystemDate.Text = "(デバッグ用)日付：" & AppCommon.SystemDate().ToString("yyyy年MM月dd日")
#End If

        menuController.LayoutMenu()
        SideMenuEnable()

        IsImportComplete = False
    End Sub

    '===========================
    ' メニュー活性制御
    '===========================
    Public Sub SideMenuEnable()

        Dim buttons = menuController.menuButtons

        If DateUtil.IsNullOrEmpty(AppState.TargetYearMonth) Then
            buttons(1).Enabled = False
            buttons(2).Enabled = False
            buttons(3).Enabled = False
            buttons(4).Enabled = False
        Else
            buttons(0).Enabled = True
            buttons(1).Enabled = True
            buttons(2).Enabled = True
            buttons(3).Enabled = True
            buttons(4).Enabled = True
        End If

    End Sub

    '===========================
    ' デバッグ日付
    '===========================
    Private Function SystemDateForDebugging() As Date

        Dim result As Date = Now()

#If DEBUG Then
        If My.Application.CommandLineArgs.Count > 0 Then
            For Each arg As String In My.Application.CommandLineArgs
                Dim param() As String = arg.ToUpper().Split("=")
                If param(0) = "SYSTEM_DATE" Then
                    If Not Date.TryParse(param(1), result) Then
                        result = Now()
                    End If
                    Exit For
                End If
            Next
        End If
#End If

        Return result
    End Function

    '===========================
    ' IMDIParent
    '===========================
    Public ReadOnly Property SideMenuPanel As Panel Implements IMDIParent.SideMenuPanel
        Get
            Return pnlMenu
        End Get
    End Property

End Class
