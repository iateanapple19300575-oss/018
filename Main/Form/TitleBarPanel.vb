Imports System.Windows.Forms
Imports System.Drawing
Imports LecturePay.Common.Infrastructure

Public Class TitleBarPanel
    Inherits Panel

    Public Event ToggleMenu()
    Public Event MinimizeClicked()
    Public Event MaximizeClicked()
    Public Event CloseClicked()

    Private lblTitle As Label
    Private lblYM As Label
    Private lblLock As Label
    Private lblUser As Label
    Private lblSystemDate As Label

    Private btnToggle As Button
    Private btnMinimize As Button
    Private btnMaximize As Button
    Private btnClose As Button

    Private _isInitializing As Boolean = True

    Public Sub New()
        Me.Height = 40
        Me.Dock = DockStyle.Top
        Me.BackColor = Color.DarkSlateGray

        BuildUI()

        _isInitializing = False
        RefreshLayout() ' ← 初期化完了後に呼ぶ
    End Sub

    Private Sub BuildUI()

        ' ハンバーガー
        btnToggle = New Button With {
            .Text = "Ξ",
            .Size = New Size(30, 30),
            .Location = New Point(10, 5)
        }
        AddHandler btnToggle.Click, Sub() RaiseEvent ToggleMenu()
        Me.Controls.Add(btnToggle)

        ' アプリ名
        lblTitle = New Label With {
            .Text = AppCommon.ApplicationName,
            .ForeColor = Color.White,
            .Location = New Point(50, 10),
            .AutoSize = True
        }
        Me.Controls.Add(lblTitle)

        ' 年月度
        lblYM = New Label With {
            .Text = AppCommon.GetTargetYearMonth(),
            .ForeColor = Color.White,
            .Location = New Point(200, 10),
            .AutoSize = True
        }
        Me.Controls.Add(lblYM)

        ' ロック状態
        lblLock = New Label With {
            .Text = AppCommon.GetLockStatus(),
            .ForeColor = Color.White,
            .Location = New Point(350, 10),
            .AutoSize = True
        }
        Me.Controls.Add(lblLock)

        ' ユーザー名
        lblUser = New Label With {
            .Text = AppCommon.GetUser(),
            .ForeColor = Color.White,
            .Location = New Point(500, 10),
            .AutoSize = True
        }
        Me.Controls.Add(lblUser)

#If DEBUG_DEV Then
            lblSystemDate = New Label With {
                .Text = "(デバッグ用)日付：" & DateTime.Now.ToString(),
                .ForeColor = Color.White,
                .Location = New Point(700, 10),
                .AutoSize = True
            }
            Me.Controls.Add(lblSystemDate)
#End If

        ' 最小化
        btnMinimize = New Button With {.Text = "＿", .Size = New Size(30, 30), .Location = New Point(900, 5)}
        AddHandler btnMinimize.Click, Sub() RaiseEvent MinimizeClicked()
        Me.Controls.Add(btnMinimize)

        ' 最大化
        btnMaximize = New Button With {.Text = "□", .Size = New Size(30, 30), .Location = New Point(940, 5)}
        AddHandler btnMaximize.Click, Sub() RaiseEvent MaximizeClicked()
        Me.Controls.Add(btnMaximize)

        ' 閉じる
        btnClose = New Button With {.Text = "閉じる", .Size = New Size(80, 30), .Location = New Point(980, 5)}
        AddHandler btnClose.Click, Sub() RaiseEvent CloseClicked()
        Me.Controls.Add(btnClose)

    End Sub

    Public Sub RefreshLayout()
        Dim w = Me.Width
        Dim margin = 10

        ' 左側：ハンバーガー + タイトル
        btnToggle.Location = New Point(margin, 5)
        lblTitle.Location = New Point(btnToggle.Right + margin, 10)

        ' 右側：閉じる・最大化・最小化
        btnClose.Location = New Point(w - margin - btnClose.Width, 5)
        btnMaximize.Location = New Point(btnClose.Left - btnMaximize.Width, 5)
        btnMinimize.Location = New Point(btnMaximize.Left - btnMinimize.Width, 5)

        ' 中央〜右寄せ：年月度・ロック・ユーザー・日付
        Dim rightBase = btnMinimize.Left - margin

        lblUser.Location = New Point(rightBase - lblUser.Width, 10)
        rightBase = lblUser.Left - margin

        lblLock.Location = New Point(rightBase - lblLock.Width, 10)
        rightBase = lblLock.Left - margin

        lblYM.Location = New Point(rightBase - lblYM.Width, 10)
#If DEBUG_DEV Then
            rightBase = lblYM.Left - margin
            lblSystemDate.Location = New Point(rightBase - lblSystemDate.Width, 10)
#End If
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)

        If _isInitializing Then
            Return
        End If
        RefreshLayout()
    End Sub
End Class

