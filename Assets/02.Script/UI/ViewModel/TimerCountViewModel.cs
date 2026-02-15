using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityWeld.Binding;
using UnityWeld;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace UI
{
    [Binding]
    public class TimerCountViewModel : ViewModel
    {
        public UnityEvent OnTimerEnd = new UnityEvent();
        
        [Header("Image Swap Settings")]
        [Tooltip("스프라이트를 교체할 대상 이미지 컴포넌트")]
        public Image targetImage;
        
        [Tooltip("남은 시간 비율(0~1)에 따라 적용할 스프라이트 설정.\n예: 0.5로 설정 시 진행률이 0.5 이하가 되면 해당 스프라이트 적용")]
        public List<TimerSpriteData> spriteSettings;

        [System.Serializable]
        public struct TimerSpriteData
        {
            [Range(0f, 1f)]
            public float progressThreshold; // 이 값 이하일 때 적용
            public Sprite sprite;
        }

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

        private float _timerProgress = 1f;

        /// <summary>
        /// 타이머 진행률 (1 → 0). Slider Value 바인딩용.
        /// </summary>
        [Binding]
        public float TimerProgress
        {
            get => _timerProgress;
            set
            {
                _timerProgress = value;
                OnPropertyChanged(nameof(TimerProgress));
            }
        }

        private float _maxTime;
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
            _maxTime = seconds;
            float remaining = seconds;

            CurrentTime = seconds;
            TimerProgress = 1f;
            CurrentTimeText = FormatTime((int)CurrentTime);
            
            // 초기 상태 이미지 업데이트
            UpdateTimerSprite(TimerProgress);
            
            while (remaining > 0)
            {
                try
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                    remaining -= Time.deltaTime;
                    if (remaining < 0) remaining = 0;

                    // 매 프레임 게이지 갱신
                    TimerProgress = _maxTime > 0 ? remaining / _maxTime : 0f;
                    UpdateTimerSprite(TimerProgress);

                    // 초 단위 텍스트 갱신 (CeilToInt: 0.1초 남았어도 1초로 표시하다가 0이 되면 0초)
                    // 기존 로직(Delay 1초 후 -1)과 유사하게 맞추려면 CeilToInt 사용
                    int displayTime = Mathf.CeilToInt(remaining);
                    if ((int)CurrentTime != displayTime)
                    {
                        CurrentTime = displayTime;
                        CurrentTimeText = FormatTime(displayTime);
                    }
                }
                catch (System.OperationCanceledException)
                {
                    return;
                }
            }

            // 정확히 0으로 맞춤
            CurrentTime = 0;
            TimerProgress = 0f;
            CurrentTimeText = FormatTime(0);
            UpdateTimerSprite(0f);

            OnTimerComplete?.Invoke();
            OnTimerEnd.Invoke();
        }

        private void UpdateTimerSprite(float progress)
        {
            if (targetImage == null || spriteSettings == null || spriteSettings.Count == 0) return;

            // 조건(threshold보다 작거나 같음)을 만족하는 설정 중 가장 작은 threshold를 가진 항목을 찾습니다.
            // 예: 0.3, 0.6이 있을 때 progress가 0.2면 0.3이 선택됨.
            // 리스트를 threshold 오름차순으로 정렬해두는 것이 좋습니다.
            // 여기서는 매번 정렬하지 않고 순회하며 찾습니다.
            
            TimerSpriteData? targetData = null;
            
            // progress보다 큰 threshold 중 가장 작은 값을 찾거나 (구간별 이미지)
            // 혹은 progress 이하인 threshold 중 가장 큰 값을 찾습니다. (게이지가 줄어들 때 변화)
            
            // 요구사항: "특정 수치에 해당하는" -> 보통 시간이 줄어들면서 30% 남았을 때 빨간색 등으로 변경
            // 따라서 "현재 progress보다 크거나 같은 threshold 중 가장 작은 값" (즉, 현재 구간) 
            // 또는 "설정된 threshold 이하로 떨어졌을 때" 적용.
            
            // 방식 결정: threshold를 '이 값 이하로 떨어지면 적용'으로 해석하면 자연스럽습니다.
            // 하지만 보통은 단계별로 변해야 하므로, "현재 진행률을 포함하는 구간"의 스프라이트를 찾습니다.
            
            // 예시: 
            // 1.0 ~ 0.6 : Normal Sprite (Threshold 1.0)
            // 0.6 ~ 0.3 : Warning Sprite (Threshold 0.6)
            // 0.3 ~ 0.0 : Danger Sprite (Threshold 0.3)
            
            // 가장 근접한 상위 threshold를 찾습니다.
            var candidate = spriteSettings
                .Where(x => x.progressThreshold >= progress)
                .OrderBy(x => x.progressThreshold)
                .FirstOrDefault();

            // struct는 default일 때 threshold 0이므로 체크 주의 (값이 없으면 default 반환됨)
            // sprite가 null이 아니면 유효한 설정으로 간주
            if (candidate.sprite != null && targetImage.sprite != candidate.sprite)
            {
                targetImage.sprite = candidate.sprite;
            }
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