Imports System.Drawing
Imports System.Windows.Forms

Public Class SideMenuController

    Private ReadOnly mainForm As GHRUN_Main
    Private ReadOnly pnlMenu As Panel

    ' メインメニューのボタン
    Public ReadOnly menuButtons As New List(Of Button)

    ' サブメニューのボタン
    Private ReadOnly subMenuButtons As New List(Of Button)

    ' 展開中のメニューインデックス
    Private currentExpandedIndex As Integer = -1

    ' EventInfo：フォーム型を保持する（共通化）
    Private Class EventInfo
        Property Name As String
        Property Text As String
        Property FormType As Type
    End Class

    Public Sub New(form As GHRUN_Main, menuPanel As Panel)
        Me.mainForm = form
        Me.pnlMenu = menuPanel
    End Sub

    '===========================================================
    ' メインメニュー初期化
    '===========================================================
    Public Sub InitializeMenu()

        AddMainButton("年月度設定", AddressOf ShowMonthSettingSubmenu)
        AddMainButton("勤怠データ管理", AddressOf ShowAttendanceDataManagementSubMenu)
        AddMainButton("勤怠マスタ管理", AddressOf ShowAttendanceMasterManagementSubMenu)
        AddMainButton("出講管理情報出力", AddressOf ShowAttendanceDataOutputSubMenu)
        AddMainButton("設定", AddressOf ShowSettingSubMenu)

        LayoutMenu()
    End Sub

    '===========================================================
    ' メインメニューボタン追加
    '===========================================================
    Private Sub AddMainButton(text As String, clickHandler As EventHandler)

        Dim btn As New Button With {
            .Text = text,
            .Width = pnlMenu.Width - 40,
            .Left = 20,
            .Height = 35,
            .BackColor = GHRUN_Main.COLOR_BUTTON_DEFAULT,
            .Font = New Font("游ゴシック", 12, FontStyle.Regular),
            .Tag = "NoLog"
        }

        AddHandler btn.Click, clickHandler

        menuButtons.Add(btn)
        pnlMenu.Controls.Add(btn)
    End Sub

    '===========================================================
    ' メニューのレイアウト
    '===========================================================
    Public Sub LayoutMenu()

        Dim y As Integer = 10

        For i As Integer = 0 To menuButtons.Count - 1

            menuButtons(i).Top = y
            y += menuButtons(i).Height + 15

            If i = currentExpandedIndex Then

                y -= 10

                For Each subBtn In subMenuButtons
                    subBtn.Top = y
                    pnlMenu.Controls.Add(subBtn)
                    y += subBtn.Height + 3
                Next

                y += 20
            End If
        Next
    End Sub

    '===========================================================
    ' サブメニュー表示（共通化後）
    '===========================================================
    Private Sub ShowSubMenu(index As Integer, eventInfo As List(Of EventInfo))

        ' サブメニュー非表示
        If index < 0 Then
            ClearSubMenu()
            LayoutMenu()

            For Each ctrl As Button In menuButtons
                ctrl.BackColor = GHRUN_Main.COLOR_BUTTON_DEFAULT
            Next

            Return
        End If

        ' 既に展開中のメニューを閉じる
        If currentExpandedIndex >= 0 Then
            menuButtons(currentExpandedIndex).BackColor = GHRUN_Main.COLOR_BUTTON_DEFAULT
        End If

        ' 同じメニューを押した → 閉じる
        If index = currentExpandedIndex Then
            ClearSubMenu()
            LayoutMenu()
            Return
        End If

        ' 新しいメニューを展開
        ClearSubMenu()
        currentExpandedIndex = index
        menuButtons(currentExpandedIndex).BackColor = GHRUN_Main.COLOR_BUTTON_ENABLE

        For Each info In eventInfo

            Dim btn As New Button With {
                .Name = info.Name,
                .Text = info.Text,
                .Width = pnlMenu.Width - 40,
                .Left = 35,
                .Height = 30,
                .BackColor = GHRUN_Main.COLOR_BUTTON_ENABLE,
                .Font = New Font("游ゴシック", 12, FontStyle.Regular),
                .Tag = "NoLog"
            }

            ' ★ 共通化されたフォーム起動
            AddHandler btn.Click,
                Sub(sender, e)
                    mainForm.OpenChildFormDynamic(info.FormType, sender, e)
                End Sub

            subMenuButtons.Add(btn)
        Next

        LayoutMenu()
    End Sub

    '===========================================================
    ' 年月度設定メニュー
    '===========================================================
    Private Sub ShowMonthSettingSubmenu(sender As Object, e As EventArgs)

        Dim eventInfo As New List(Of EventInfo) From {
            New EventInfo With {
                .Name = "btnMenuItemSelectMonth",
                .Text = "年月度設定",
                .FormType = GetType(frmTargetYearMonth)
            }
        }

        ContextMenu_Click_logger(sender, e)
        ShowSubMenu(0, eventInfo)
    End Sub

    '===========================================================
    ' 勤怠データ管理メニュー
    '===========================================================
    Private Sub ShowAttendanceDataManagementSubMenu(sender As Object, e As EventArgs)

        Dim eventInfo As New List(Of EventInfo) From {
            New EventInfo With {.Name = "btnAttendanceDataImport", .Text = "勤怠データ取込", .FormType = GetType(FrmLectureImport)},
            New EventInfo With {.Name = "btnAllowanceDeductionDataImport", .Text = "手当・控除データ取込", .FormType = GetType(FrmAllowanceDeductionImport)},
            New EventInfo With {.Name = "btnTravelExpensesMgmt", .Text = "交通費管理", .FormType = GetType(FrmTransportExpenses)},
            New EventInfo With {.Name = "btnCheckAttendanceData", .Text = "勤怠データ確認", .FormType = GetType(FrmCheckAttendanceData)},
            New EventInfo With {.Name = "btnClosingProcess", .Text = "締め処理", .FormType = GetType(FormMonthlyClosing)},
            New EventInfo With {.Name = "btnAttendanceDataOutput", .Text = "勤怠データ出力", .FormType = GetType(FrmAttendanceDataOutput)}
        }

        ContextMenu_Click_logger(sender, e)
        ShowSubMenu(1, eventInfo)
    End Sub

    '===========================================================
    ' 勤怠マスタ管理メニュー
    '===========================================================
    Private Sub ShowAttendanceMasterManagementSubMenu(sender As Object, e As EventArgs)

        Dim eventInfo As New List(Of EventInfo) From {
            New EventInfo With {.Name = "btnTimeTableDataImport", .Text = "時間割設定データ取込", .FormType = GetType(FrmImportTimetableData)},
            New EventInfo With {.Name = "btnLectureFeeDataImport", .Text = "出講料設定データ取込", .FormType = GetType(FrmImportLectureFeeData)}
        }

        ContextMenu_Click_logger(sender, e)
        ShowSubMenu(2, eventInfo)
    End Sub

    '===========================================================
    ' 出講管理情報出力メニュー
    '===========================================================
    Private Sub ShowAttendanceDataOutputSubMenu(sender As Object, e As EventArgs)

        Dim eventInfo As New List(Of EventInfo) From {
            New EventInfo With {.Name = "btnLectureInformationOutput", .Text = "出講管理情報出力", .FormType = GetType(FrmAttendanceDataOutput)}
        }

        ContextMenu_Click_logger(sender, e)
        ShowSubMenu(3, eventInfo)
    End Sub

    '===========================================================
    ' 設定メニュー
    '===========================================================
    Private Sub ShowSettingSubMenu(sender As Object, e As EventArgs)

        Dim eventInfo As New List(Of EventInfo) From {
            New EventInfo With {.Name = "btnLockHistory", .Text = "年月ロック状況確認", .FormType = GetType(FrmLockHistory)},
            New EventInfo With {.Name = "btnImportHistory", .Text = "取込履歴", .FormType = GetType(FrmImportHistory)},
            New EventInfo With {.Name = "btnOperationHistory", .Text = "操作履歴", .FormType = GetType(FrmOperationHistory)}
        }

        ContextMenu_Click_logger(sender, e)
        ShowSubMenu(4, eventInfo)
    End Sub

    '===========================================================
    ' サブメニュークリア
    '===========================================================
    Private Sub ClearSubMenu()

        For Each btn In subMenuButtons
            pnlMenu.Controls.Remove(btn)
        Next

        subMenuButtons.Clear()
        currentExpandedIndex = -1
    End Sub

End Class
