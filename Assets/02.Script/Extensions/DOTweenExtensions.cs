using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public static class DOTweenExtensions
{
    /// <summary>
    /// Convert a DOTween Tween into a UniTask that completes when the tween completes (or faults when killed).
    /// </summary>
    public static UniTask<AsyncUnit> ToUniTask(this Tween tween, CancellationToken cancellationToken = default)
    {
        if (tween == null) return UniTask.FromResult(AsyncUnit.Default);

        var tcs = new UniTaskCompletionSource<AsyncUnit>();
        bool finished = false;

        tween.OnComplete(() =>
        {
            if (finished) return;
            finished = true;
            tcs.TrySetResult(AsyncUnit.Default);
        });

        tween.OnKill(() =>
        {
            if (finished) return;
            finished = true;
            tcs.TrySetException(new OperationCanceledException("Tween was killed"));
        });

        if (cancellationToken.CanBeCanceled)
        {
            cancellationToken.Register(() =>
            {
                if (finished) return;
                if (tween.IsActive()) tween.Kill();
            });
        }

        return tcs.Task;
    }
}
