using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Threading;
using UnityEngine;
using UnityWeld;
using UnityWeld.Binding;

namespace UI
{
    [Binding]
    public class MiniGameProgressBarViewModel : ViewModel
    {

        private CancellationTokenSource _cts;
        private bool _isRunning;

        private float _progressValue = 0; //* 진행도
        [Binding]
        public float ProgressValue
        {
            get { return _progressValue; }
            set
            {
                _progressValue = value;
                OnPropertyChanged(nameof(ProgressValue));
            }
        }
        private string _progressValueText;
        [Binding]
        public string ProgressValueText
        {
            get { return _progressValueText; }
            set { _progressValueText = value; OnPropertyChanged(nameof(ProgressValueText)); }
        }

        public EventHandler OnProgressBarFilled; //* 진행바가 다 찼을 때
        public EventHandler<float> OnProgressBarChanged; //* 진행바의 수치가 바뀌었을 때
        public EventHandler OnProgressBarEmpty; // 타이머가 0이 되었을 때 이벤트 추가

        private float _timerDuration;

        /// <summary>
        /// 타이머를 설정하는 메서드
        /// </summary>
        /// <param name="duration">타이머 지속 시간(초)</param>
        public void SetupTimer(float duration)
        {
            _timerDuration = duration;
        }
        /// <summary>
        /// 타이머 시작하는 메서드
        /// </summary>
        /// <param name="duration">타이머 지속 시간(초)</param>
        public void StartTimer(float duration)
        {
            _timerDuration = duration;
            RunTimerAsync().Forget();
        }

        /// <summary>
        /// 실제 타이머 실행 로직
        /// </summary>
        private async UniTaskVoid RunTimerAsync()
        {
            if (_isRunning) return;

            _isRunning = true;
            _cts = new CancellationTokenSource();
            ProgressValue = 1f;
            int lastPlayedSecond = -1; // 마지막으로 재생된 초를 추적

            try
            {
                while (_progressValue > 0 && !_cts.Token.IsCancellationRequested)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: _cts.Token);
                    ProgressValue -= 0.1f / _timerDuration;
                    ProgressValue = Mathf.Clamp(ProgressValue, 0, 1);
                    OnProgressBarChanged?.Invoke(this, ProgressValue);

                    // 남은 시간 계산
                    float remainingTime = ProgressValue * _timerDuration;
                    int currentSecond = Mathf.CeilToInt(remainingTime);
                    ProgressValueText = $"{currentSecond}초";
                    // 3,2초일 때 카운트다운 효과음 재생
                    if (currentSecond <= 3 && currentSecond > 1 && currentSecond != lastPlayedSecond)
                    {
                        // Global.SoundManager.PlaySFXInstance(SFXEnum.CountDown, 1);
                        lastPlayedSecond = currentSecond;
                    }
                    // 1초일 때 다른 효과음 재생
                    else if (currentSecond == 1 && currentSecond != lastPlayedSecond)
                    {
                        // Global.SoundManager.PlaySFXInstance(SFXEnum.GameOver, 1); // 1초 남았을 때의 다른 효과음
                        lastPlayedSecond = currentSecond;
                    }

                    if (ProgressValue <= 0)
                    {
                        OnProgressBarEmpty?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            finally
            {
                _isRunning = false;
            }
        }

        /// <summary>
        /// 실행 중인 타이머를 중지하는 메서드
        /// </summary>
        public void StopTimer()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _isRunning = false;
        }

        private void OnDestroy()
        {
            StopTimer();
        }

    }
}