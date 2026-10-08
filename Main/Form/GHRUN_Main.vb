'===========================
' メインフォーム
'===========================
Imports System.ComponentModel
Imports App.Common.Data.Model
Imports MasterImport
Imports Microsoft.Win32
Imports PunchDataImport

Public Class GHRUN_Main
    Inherits BaseForm
    Implements IMDIParent

    Public Declare Auto Function SendMessage Lib "user32.dll" (ByVal hWnd As IntPtr, ByVal Msg As UInteger, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
    Public Declare Auto Function ReleaseCapture Lib "user32.dll" () As Boolean

    Private _controller As New LectPayMainViewController

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
    Public Const TITLE_SEPARATE As Integer = 40
    Public Const TITLE_PANEL_MENU_CTRL_LEFT As Integer = TITLE_MARGIN
    Public Const TITLE_PANEL_APPL_NAME_LEFT As Integer = TITLE_MARGIN + TITLE_MENU_CTRL_WIDTH
    Public Const TITLE_PANEL_YEAR_MONTH_LEFT As Integer = TITLE_PANEL_APPL_NAME_LEFT + TITLE_MARGIN + TITLE_APPL_NAME_WIDTH
    Public Const TITLE_PANEL_LOCK_STATUS_LEFT As Integer = TITLE_PANEL_YEAR_MONTH_LEFT + TITLE_MARGIN + TITLE_YEAR_MONTH_WIDTH
    Public Const TITLE_PANEL_SYSTEM_DATE_WIDTH As Integer = 300
    Public Const TITLE_PANEL_USER_NAME_RLEFT As Integer = TITLE_USER_NAME_WIDTH + TITLE_MARGIN + TITLE_PANEL_MIN_RLEFT
    Public Const TITLE_PANEL_MIN_RLEFT As Integer = TITLE_PANEL_MAX_RLEFT + TITLE_MIN_WIDTH
    Public Const TITLE_PANEL_MAX_RLEFT As Integer = TITLE_MAX_WIDTH + TITLE_PANEL_CLOSE_RLEFT
    Public Const TITLE_PANEL_CLOSE_RLEFT As Integer = TITLE_MARGIN + TITLE_CLOSE_WIDTH

    Public Const TITLE_MENU_CTRL_WIDTH As Integer = 30
    Public Const TITLE_APPL_NAME_WIDTH As Integer = 120
    Public Const TITLE_YEAR_MONTH_WIDTH As Integer = 260
    Public Const TITLE_LOCK_STATUS_WIDTH As Integer = 150
    Public Const TITLE_USER_NAME_WIDTH As Integer = 200
    Public Const TITLE_MIN_WIDTH As Integer = 30
    Public Const TITLE_MAX_WIDTH As Integer = 30
    Public Const TITLE_CLOSE_WIDTH As Integer = 80

    Public Shared COLOR_TITLE_BAR As Color = Color.DarkSlateGray
    Public Shared COLOR_BUTTON_ENABLE As Color = Color.LightGoldenrodYellow
    Public Shared COLOR_BUTTON_DEFAULT As Color = Color.LightGray

    Private Shared _initialized As Boolean = False

    Private pnlMenu As Panel
    'Public menuButtons As New List(Of Button)

    Private currentUser As String = Environment.UserName
    Private currentYM As String

    'Private LockMonitoringTimer As New Timer()

    Private titleController As TitleBarController
    Private menuController As SideMenuController
    Private mdiController As MdiController

    Public Shared IsImportComplete As Boolean = False

    Public Sub New()
        InitializeComponent()

        Me.IsMdiContainer = True
        Me.FormBorderStyle = FormBorderStyle.None
        Me.BackColor = Color.Black
        Me.MinimumSize = New Size(960, 540)
        Me.Size = ScreenUtil.ScreenWorkAreaSize(Me)

        'LockMonitoringTimer.Interval = 60000

        AddHandler SystemEvents.DisplaySettingsChanged, AddressOf OnDisplaySettingsChanged
        AddHandler SystemEvents.UserPreferenceChanged, AddressOf OnUserPreferenceChanged

        InitializeWindow()
    End Sub

    Private Sub InitializeWindow()
        pnlMenu = New Panel With {
            .Dock = DockStyle.Left,
            .Width = SIDE_PANEL_WIDTH,
            .AutoScroll = True,
            .BackColor = Color.FromArgb(&HFFCAE6CA)
        }
        Me.Controls.Add(pnlMenu)

        titleController = New TitleBarController(Me)
        titleController.InitializeTitleBar()

        menuController = New SideMenuController(Me, pnlMenu)
        menuController.InitializeMenu()

        mdiController = New MdiController(Me)
    End Sub

    Private Sub LectPay_Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
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

    Private Sub GHRUN_Main_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        Dim result As New YearMonthLockModel

        LockMonitoringTimer.Stop()

        Dim model As New YearMonthLockModel
        result = _controller.UnLockedTargetYearMonth(model)
        If Not result.Success Then
            LectPayAppMsgBox.ShowMessage(result.BusinessMessage)
        End If
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        RemoveHandler StateDispatcher.UiStateChanged, AddressOf OnUiStateChanged
        MyBase.OnFormClosed(e)
    End Sub

    Protected Overridable Sub OnMdiChildActivated(sender As Object, e As EventArgs)
        mdiController.OnMdiChildActivated()
    End Sub

    Private Sub GHRUN_Main_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        WindowResize()
    End Sub

    Private Sub WindowResize()
        If Not IsImportComplete Then
            Return
        End If
        titleController.TitleLayoutRefresh()
    End Sub

    Private Sub OnDisplaySettingsChanged(sender As Object, e As EventArgs)
        WindowResize()
    End Sub

    Private Sub OnUserPreferenceChanged(sender As Object, e As UserPreferenceChangedEventArgs)
        If e.Category = UserPreferenceCategory.Window Then
            WindowResize()
        End If
    End Sub

    Private Sub GHRUN_Main_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        RemoveHandler SystemEvents.DisplaySettingsChanged, AddressOf OnDisplaySettingsChanged
        RemoveHandler SystemEvents.UserPreferenceChanged, AddressOf OnUserPreferenceChanged
    End Sub

    Protected Overrides Sub WndProc(ByRef m As Message)
        Const WM_NCCALCSIZE As Integer = &H83

        If m.Msg = WM_NCCALCSIZE Then
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

    Public Sub frmTargetYearMonth_FormClosed(sender As Object, e As FormClosedEventArgs)
        SideMenuEnable()
    End Sub

    Public Overrides Sub OnUiStateChanged(key As String, value As Object)
        StateUpdateHelper.UpdateState(Me, key, value)
    End Sub

    Public Sub OnStateChanged(key As String, value As Object)
        StateUpdateHelper.UpdateState(Me, key, value)
    End Sub

    Private Sub OpenChildForm(Of T As {Form, New})(sender As Object, e As EventArgs)
        If mdiController.ShowChildForm(Of T)() Then
            Return
        End If

        ContextMenuItem_Click_logger(sender, e)
        mdiController.CloseMDIChildScreen(New T)
    End Sub

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

    Public Sub MenuItemSelectMonth_Click(sender As Object, e As EventArgs)
        OpenChildForm(Of frmTargetYearMonth)(sender, e)
    End Sub

    Public Sub AttendanceDataImport_Click(sender As Object, e As EventArgs)
        OpenChildForm(Of FrmLectureImport)(sender, e)
    End Sub

    Public Sub AllowanceDeductionDataImport_Click(sender As Object, e As EventArgs)
        OpenChildForm(Of FrmAllowanceDeductionImport)(sender, e)
    End Sub

    Public Sub TransportationExpenseManagement_Click(sender As Object, e As EventArgs)
        OpenChildForm(Of FrmTransportExpenses)(sender, e)
    End Sub

    Public Sub CheckAttendanceData_Click(sender As Object, e As EventArgs)
        OpenChildForm(Of FrmCheckAttendanceData)(sender, e)
    End Sub

    Public Sub ClosingProcess_Click(sender As Object, e As EventArgs)
        OpenChildForm(Of FormMonthlyClosing)(sender, e)
    End Sub

    Public Sub ConfirmationProcess_Click(sender As Object, e As EventArgs)
        OpenChildForm(Of FrmAttendanceDataOutput)(sender, e)
    End Sub

    Public Sub TimeTableDataImport_Click(sender As Object, e As EventArgs)
        OpenChildForm(Of FrmImportTimetableData)(sender, e)
    End Sub

    Public Sub LectureFeeDataImport_Click(sender As Object, e As EventArgs)
        OpenChildForm(Of FrmImportLectureFeeData)(sender, e)
    End Sub

    'Public Sub LectureInformationOutput_Click(sender As Object, e As EventArgs)
    '    MessageBox.Show("未実装")
    'OpenChildForm(Of FrmXxxxxxxxx)(sender, e)
    'End Sub

    Public Sub Preferences_Click(sender As Object, e As EventArgs)
        OpenChildForm(Of FrmSystemEnvSettings)(sender, e)
    End Sub

    Public Sub LockHistory_Click(sender As Object, e As EventArgs)
        OpenChildForm(Of FrmLockHistory)(sender, e)
    End Sub

    Public Sub ImportHistory_Click(sender As Object, e As EventArgs)
        OpenChildForm(Of FrmImportHistory)(sender, e)
    End Sub

    Public Sub OperationHistory_Click(sender As Object, e As EventArgs)
        OpenChildForm(Of FrmOperationHistory)(sender, e)
    End Sub

    Public Sub DisplayInitialize()
        AppState.DebugSystemDate = SystemDateForDebugging()

        Me.Location = New Point(0, 0)

        titleController.TitleLayoutRefresh()
        menuController.LayoutMenu()
        SideMenuEnable()

        IsImportComplete = False
    End Sub

    Public Sub SideMenuEnable()
        Dim buttons = menuController.menuButtons

        If DateUtil.IsNullOrEmpty(AppState.TargetYearMonth) Then
            buttons(1).Enabled = False
            buttons(2).Enabled = False
            buttons(3).Enabled = False
            'buttons(4).Enabled = False
        Else
            buttons(0).Enabled = True
            buttons(1).Enabled = True
            buttons(2).Enabled = True
            buttons(3).Enabled = True
            'buttons(4).Enabled = True
        End If
    End Sub

    Public Function GetHitTestResult(p As Point) As UInteger

        Const BORDER As Integer = 14
        Const CORNER As Integer = 18

        Dim left As Boolean = p.X >= 0 AndAlso p.X <= BORDER
        Dim right As Boolean = p.X >= Me.Width - BORDER AndAlso p.X <= Me.Width
        Dim top As Boolean = p.Y >= 0 AndAlso p.Y <= BORDER
        Dim bottom As Boolean = p.Y >= Me.Height - BORDER AndAlso p.Y <= Me.Height

        ' ★ 角の判定を優先（幅広め）
        If p.X <= CORNER AndAlso p.Y <= CORNER Then Return HT_TOPLEFT
        If p.X >= Me.Width - CORNER AndAlso p.Y <= CORNER Then Return HT_TOPRIGHT
        If p.X <= CORNER AndAlso p.Y >= Me.Height - CORNER Then Return HT_BOTTOMLEFT
        If p.X >= Me.Width - CORNER AndAlso p.Y >= Me.Height - CORNER Then Return HT_BOTTOMRIGHT

        ' ★ 境界線判定（矩形領域）
        If left Then Return HT_LEFT
        If right Then Return HT_RIGHT
        If top Then Return HT_TOP
        If bottom Then Return HT_BOTTOM

        Return HT_CAPTION
    End Function

    Private Sub LockMonitoringTimer_Tick(sender As Object, e As EventArgs) Handles LockMonitoringTimer.Tick
        Dim lockInfo As YearMonthLockModel = _controller.TargetYearMonthLockCheck()
        If Not lockInfo.Success Then
            Return
        End If

        If lockInfo.SubStatus = 0 Then
            Return
        End If

        MessageBox.Show("他ユーザーに強制解除されました。年月度設定からやり直してください。")

        AppState.TargetYearMonth = Date.MinValue
        AppState.LockStatus = False
        mdiController.CloseMDIChildAll()
        RestartLoad()
        menuController.LayoutMenu()
    End Sub

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

    Public ReadOnly Property SideMenuPanel As Panel Implements IMDIParent.SideMenuPanel
        Get
            Return pnlMenu
        End Get
    End Property

    Protected Overrides Sub OnPaint(e As PaintEventArgs)

        MyBase.OnPaint(e)

        Dim g As Graphics = e.Graphics
        Dim borderColor As Color = Color.FromArgb(80, 80, 80) ' 薄いグレー
        Dim resizeColor As Color = Color.FromArgb(120, 120, 120)

        Dim pen As New Pen(borderColor, 2)

        ' ★ 通常時の境界線
        g.DrawLine(pen, 0, 0, Me.Width, 0) ' 上
        g.DrawLine(pen, 0, Me.Height - 1, Me.Width, Me.Height - 1) ' 下
        g.DrawLine(pen, 0, 0, 0, Me.Height) ' 左
        g.DrawLine(pen, Me.Width - 1, 0, Me.Width - 1, Me.Height) ' 右

        ' ★ 角を少し強調
        Dim cornerPen As New Pen(resizeColor, 3)
        g.DrawRectangle(cornerPen, 0, 0, 20, 20)
        g.DrawRectangle(cornerPen, Me.Width - 20, 0, 20, 20)
        g.DrawRectangle(cornerPen, 0, Me.Height - 20, 20, 20)
        g.DrawRectangle(cornerPen, Me.Width - 20, Me.Height - 20, 20, 20)
    End Sub
End Class