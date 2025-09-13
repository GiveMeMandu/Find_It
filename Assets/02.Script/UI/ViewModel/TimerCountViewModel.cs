using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityWeld.Binding;
using UnityWeld;
using UnityEngine.Events;

namespace UI
{
    [Binding]
    public class TimerCountViewModel : ViewModel
    {
        public UnityEvent OnTimerEnd = new UnityEvent();
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
            CurrentTimeText = FormatTime((int)CurrentTime);
            
            while (CurrentTime > 0)
            {
                try
                {
                    await UniTask.Delay(1000, cancellationToken: cancellationToken);
                    CurrentTime--;
                    CurrentTimeText = FormatTime((int)CurrentTime);
                }
                catch (System.OperationCanceledException)
                {
                    return;
                }
            }

            OnTimerComplete?.Invoke();
            OnTimerEnd.Invoke();
        }

        private string FormatTime(int totalSeconds)
        {
            if (totalSeconds < 0) totalSeconds = 0;
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;

            if (hours > 0)
            {
                return string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds);
            }

            return string.Format("{0:D2}:{1:D2}", minutes, seconds);
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}