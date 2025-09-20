using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class AutoTaskControl : MonoBehaviour
{
    protected CancellationTokenSource destroyCancellation = new CancellationTokenSource(); //삭제시 취소처리
    protected virtual void OnEnable()
    {
        ResetCancellationToken();
    }
    protected virtual void OnDisable()
    {
        if (destroyCancellation != null && !destroyCancellation.IsCancellationRequested)
        {
            try
            {
                destroyCancellation.Cancel();
            }
            catch (System.Exception ex)
            {
                // CancellationTokenSource가 이미 dispose되었거나 다른 문제가 발생한 경우
                Debug.LogWarning($"AutoTaskControl 취소 처리 중 예외 발생: {ex.Message}");
            }
        }
    }
    protected virtual void OnDestroy()
    {
        if (destroyCancellation != null)
        {
            try
            {
                if (!destroyCancellation.IsCancellationRequested)
                {
                    destroyCancellation.Cancel();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"AutoTaskControl OnDestroy 취소 처리 중 예외 발생: {ex.Message}");
            }
            finally
            {
                try
                {
                    destroyCancellation.Dispose();
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"AutoTaskControl OnDestroy dispose 중 예외 발생: {ex.Message}");
                }
            }
        }
    }
    protected void StopAllTask()
    {
        if (destroyCancellation != null && !destroyCancellation.IsCancellationRequested)
        {
            try
            {
                destroyCancellation.Cancel();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"AutoTaskControl StopAllTask 취소 처리 중 예외 발생: {ex.Message}");
            }
        }
    }
    protected void ResetCancellationToken()
    {
        if (destroyCancellation != null)
        {
            try
            {
                if (!destroyCancellation.IsCancellationRequested)
                {
                    destroyCancellation.Cancel();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"AutoTaskControl ResetCancellationToken 취소 처리 중 예외 발생: {ex.Message}");
            }
            finally
            {
                try
                {
                    destroyCancellation.Dispose();
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"AutoTaskControl ResetCancellationToken dispose 중 예외 발생: {ex.Message}");
                }
            }
        }
        destroyCancellation = new CancellationTokenSource();
    }
}
