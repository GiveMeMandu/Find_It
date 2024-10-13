using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class AutoTaskControl : MonoBehaviour
{
    protected CancellationTokenSource destroyCancellation = new CancellationTokenSource(); //삭제시 취소처리
    protected CancellationTokenSource disableCancellation = new CancellationTokenSource(); //비활성화시 취소처리
    protected virtual void OnEnable()
    {
        if (destroyCancellation != null)
        {
            destroyCancellation.Dispose();
        }
        destroyCancellation = new CancellationTokenSource();
    }
    protected virtual void OnDisable()
    {
        if (!disableCancellation.IsCancellationRequested)
        {
            disableCancellation.Cancel();
        }
    }
}
