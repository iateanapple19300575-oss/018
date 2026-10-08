<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class GHRUN_Main
    Inherits BaseForm
    Implements IMDIParent

    'フォームがコンポーネントの一覧をクリーンアップするために dispose をオーバーライドします。
    <System.Diagnostics.DebuggerNonUserCode()> _
	Protected Overrides Sub Dispose(ByVal disposing As Boolean)
		Try
			If disposing AndAlso components IsNot Nothing Then
				components.Dispose()
			End If
		Finally
			MyBase.Dispose(disposing)
		End Try
	End Sub

	'Windows フォーム デザイナーで必要です。
	Private components As System.ComponentModel.IContainer

	'メモ: 以下のプロシージャは Windows フォーム デザイナーで必要です。
	'Windows フォーム デザイナーを使用して変更できます。  
	'コード エディターを使って変更しないでください。
	<System.Diagnostics.DebuggerStepThrough()> _
	Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.animationTimer = New System.Windows.Forms.Timer(Me.components)
        Me.LockMonitoringTimer = New System.Windows.Forms.Timer(Me.components)
        Me.SuspendLayout()
        '
        'animationTimer
        '
        Me.animationTimer.Interval = 10
        '
        'LockMonitoringTimer
        '
        Me.LockMonitoringTimer.Interval = 60000
        '
        'GHRUN_Main
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1484, 861)
        Me.ControlBox = False
        Me.IsMdiContainer = True
        Me.Name = "GHRUN_Main"
        Me.Text = "出講料管理"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents animationTimer As Timer
    Friend WithEvents LockMonitoringTimer As Timer
End Class
