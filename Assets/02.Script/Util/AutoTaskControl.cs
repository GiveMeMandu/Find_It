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
        if (!destroyCancellation.IsCancellationRequested)
        {
            destroyCancellation.Cancel();
        }
    }
    protected virtual void OnDestroy()
    {
        if (destroyCancellation != null)
        {
            destroyCancellation.Cancel();
            destroyCancellation.Dispose();
        }
    }
    protected void StopAllTask()
    {
        if (!destroyCancellation.IsCancellationRequested)
        {
            destroyCancellation.Cancel();
        }
    }
    protected void ResetCancellationToken()
    {
        if (destroyCancellation != null)
        {
            if (!destroyCancellation.IsCancellationRequested)
            {
                destroyCancellation.Cancel();
            }
            destroyCancellation.Dispose();
        }
        destroyCancellation = new CancellationTokenSource();
    }
}
