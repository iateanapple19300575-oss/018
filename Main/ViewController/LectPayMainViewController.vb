Imports App.Common.Data.Model


''' <summary>
''' LECTPAYメイン画面 ViewController
''' </summary>
Public Class LectPayMainViewController

    ''' <summary>
    ''' 対象年月度ロックサービス
    ''' </summary>
    Private _service As New YearMonthLockService

    ''' <summary>
    ''' 対象年月の強制ロック解除をします。
    ''' </summary>
    ''' <param name="model"></param>
    ''' <returns></returns>
    Public Function UnLockedTargetYearMonth(ByVal model As YearMonthLockModel) As YearMonthLockModel
        Dim result As New YearMonthLockModel

        Try
            ' 対象年月度(NUL)をロックしていないので解除処理はしない。
            If DateUtil.IsNullOrEmpty(AppState.TargetYearMonth) Then
                result.Success = True
                Return result
            End If

            ' 自分の使用していた対象年月度をロック解除
            model.NowYearMonth = AppState.TargetYearMonth
            model.UnlokYearMonth = DateUtil.DateToFormatString(AppState.TargetYearMonth, "yyyyMM")
            result = _service.UnLockedTargetYearMonth(model)

        Catch ex As AppException
            result.SubStatus = False
            result.Exception = ex
            result.BusinessMessage = MessageIdConst.MSGID_E2011

        End Try

        Return result
    End Function

    ''' <summary>
    ''' 対象年月度ロック状態監視
    ''' </summary>
    ''' <returns></returns>
    Public Function TargetYearMonthLockCheck() As YearMonthLockModel
        Dim result As New YearMonthLockModel
        Dim model As New YearMonthLockModel

        If DateUtil.IsNullOrEmpty(AppState.TargetYearMonth) Then
            result.Success = True
            result.SubStatus = 0
            Return result
        End If

        model.NowYearMonth = AppState.TargetYearMonth
        result = _service.GetByYearMonth(model)
        With result.ResponseData
            If .LockPc = AppState.CurrentPcName AndAlso
               .LockUser = AppState.CurrentUserName AndAlso
               .LockStatus = "L" Then
                result.SubStatus = 0
            Else
                'result.SubStatus = 1001
            End If
        End With
        result.SubStatus = 0

        Return result
    End Function

End Class
