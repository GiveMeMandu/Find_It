using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityWeld.Binding;
using UnityWeld;

namespace UI
{
    [Binding]
    public class TimerCountViewModel : ViewModel
    {
        private float _currentTime;

        [Binding]
        public float CurrentTime
        {
            get => _currentTime;
            set
            {
                _currentTime = value;
                OnPropertyChanged(nameof(CurrentTime));
            }
        }
        private string _currentTimeText;

        [Binding]
        public string CurrentTimeText
        {
            get => _currentTimeText;
            set
            {
                _currentTimeText = value;
                OnPropertyChanged(nameof(CurrentTimeText));
            }
        }

        private CancellationTokenSource _cancellationTokenSource;
        public event System.Action OnTimerComplete;

        public void Initialize(float seconds)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            StartTimer(seconds, _cancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid StartTimer(float seconds, CancellationToken cancellationToken)
        {
            CurrentTime = seconds;
            CurrentTimeText = string.Format("{0}", CurrentTime);
            
            while (CurrentTime > 0)
            {
                try
                {
                    await UniTask.Delay(1000, cancellationToken: cancellationToken);
                    CurrentTime--;
                    CurrentTimeText = string.Format("{0}", CurrentTime);
                }
                catch (System.OperationCanceledException)
                {
                    return;
                }
            }

            OnTimerComplete?.Invoke();
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}