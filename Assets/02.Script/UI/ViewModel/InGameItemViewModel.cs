using System;
using Manager;
using UnityEngine;
using UnityWeld.Binding;

namespace UI
{
    [Binding]
    public class InGameItemViewModel : BaseViewModel
    {
        // 아이템 관련 필드들
        private string _compassCountText;
        private string _stopwatchCountText;
        private string _hintCountText;
        private bool _isCompassActive;
        private bool _isStopwatchActive;
        private bool _isHintActive;
        
        // 아이템 수량 텍스트 프로퍼티들
        [Binding]
        public string CompassCountText
        {
            get => _compassCountText;
            set
            {
                _compassCountText = value;
                OnPropertyChanged(nameof(CompassCountText));
            }
        }

        [Binding]
        public string StopwatchCountText
        {
            get => _stopwatchCountText;
            set
            {
                _stopwatchCountText = value;
                OnPropertyChanged(nameof(StopwatchCountText));
            }
        }

        [Binding]
        public string HintCountText
        {
            get => _hintCountText;
            set
            {
                _hintCountText = value;
                OnPropertyChanged(nameof(HintCountText));
            }
        }

        // 아이템 활성화 상태 프로퍼티들
        [Binding]
        public bool IsCompassActive
        {
            get => _isCompassActive;
            set
            {
                _isCompassActive = value;
                OnPropertyChanged(nameof(IsCompassActive));
            }
        }

        [Binding]
        public bool IsStopwatchActive
        {
            get => _isStopwatchActive;
            set
            {
                _isStopwatchActive = value;
                OnPropertyChanged(nameof(IsStopwatchActive));
            }
        }

        [Binding]
        public bool IsHintActive
        {
            get => _isHintActive;
            set
            {
                _isHintActive = value;
                OnPropertyChanged(nameof(IsHintActive));
            }
        }

        private void Start()
        {
            // 아이템 관련 이벤트 구독
            if (Global.ItemManager != null)
            {
                Global.ItemManager.OnItemCountChanged += OnItemCountChanged;
                UpdateAllItemCounts();
            }
            
            // 초기 상태 설정
            InitializeItemStates();
        }

        private void OnDisable()
        {
            // 아이템 관련 이벤트 구독 해제
            if (Global.ItemManager != null)
            {
                Global.ItemManager.OnItemCountChanged -= OnItemCountChanged;
            }
        }
        
        private void InitializeItemStates()
        {
            // 모든 아이템 효과 비활성화
            IsCompassActive = false;
            IsStopwatchActive = false;
            IsHintActive = false;
        }

        // 아이템 관련 메서드들
        private void OnItemCountChanged(object sender, ItemType itemType)
        {
            UpdateItemCount(itemType);
        }

        private void UpdateItemCount(ItemType itemType)
        {
            if (Global.ItemManager == null) return;

            int count = Global.ItemManager.GetItemCount(itemType);
            
            switch (itemType)
            {
                case ItemType.Compass:
                    CompassCountText = count.ToString();
                    break;
                case ItemType.Stopwatch:
                    StopwatchCountText = count.ToString();
                    break;
                case ItemType.Hint:
                    HintCountText = count.ToString();
                    break;
            }
        }

        private void UpdateAllItemCounts()
        {
            UpdateItemCount(ItemType.Compass);
            UpdateItemCount(ItemType.Stopwatch);
            UpdateItemCount(ItemType.Hint);
        }

        // 아이템 사용 메서드들
        [Binding]
        public void UseCompass()
        {
            if (Global.ItemManager != null && Global.ItemManager.UseItem(ItemType.Compass))
            {
                ActivateCompass();
            }
        }

        [Binding]
        public void UseStopwatch()
        {
            if (Global.ItemManager != null && Global.ItemManager.UseItem(ItemType.Stopwatch))
            {
                ActivateStopwatch();
            }
        }

        [Binding]
        public void UseHint()
        {
            if (Global.ItemManager != null && Global.ItemManager.UseItem(ItemType.Hint))
            {
                ActivateHint();
            }
        }

        // 아이템 효과 활성화 메서드들
        private void ActivateCompass()
        {
            IsCompassActive = true;
            // TODO: 나침반 효과 구현 (숨겨진 오브젝트 위치 힌트 표시)
            Debug.Log("나침반 활성화: 숨겨진 오브젝트 위치 힌트 표시");
            
            // 30초 후 효과 해제
            Invoke(nameof(DeactivateCompass), 30f);
        }

        private void ActivateStopwatch()
        {
            IsStopwatchActive = true;
            Debug.Log("초시계 활성화: 시간 추가");
            
            // TimeChallengeViewModel에 시간 추가 효과 적용
            var timeChallengeViewModel = FindAnyObjectByType<TimeChallengeViewModel>();
            if (timeChallengeViewModel != null)
            {
                // 30초 시간 추가
                timeChallengeViewModel.AddTime(30f);
                Debug.Log("30초 시간 추가됨");
            }
            
            // 효과는 즉시 적용되고 비활성화
            Invoke(nameof(DeactivateStopwatch), 1f);
        }

        private void ActivateHint()
        {
            IsHintActive = true;
            // TODO: 힌트 효과 구현 (화면 확대 또는 오브젝트 강조)
            Debug.Log("힌트 활성화: 화면 확대 또는 오브젝트 강조");
            
            // 20초 후 효과 해제
            Invoke(nameof(DeactivateHint), 20f);
        }

        private void DeactivateCompass()
        {
            IsCompassActive = false;
            Debug.Log("나침반 효과 해제");
        }

        private void DeactivateStopwatch()
        {
            IsStopwatchActive = false;
            Debug.Log("초시계 효과 해제");
        }

        private void DeactivateHint()
        {
            IsHintActive = false;
            Debug.Log("힌트 효과 해제");
        }

        // 테스트용 아이템 추가 메서드들
        [Binding]
        public void AddCompass()
        {
            if (Global.ItemManager != null)
            {
                Global.ItemManager.AddItem(ItemType.Compass, 1);
                Debug.Log("나침반 1개 추가됨");
            }
        }

        [Binding]
        public void AddStopwatch()
        {
            if (Global.ItemManager != null)
            {
                Global.ItemManager.AddItem(ItemType.Stopwatch, 1);
                Debug.Log("초시계 1개 추가됨");
            }
        }

        [Binding]
        public void AddHint()
        {
            if (Global.ItemManager != null)
            {
                Global.ItemManager.AddItem(ItemType.Hint, 1);
                Debug.Log("힌트 1개 추가됨");
            }
        }
        
        // 아이템 사용 가능 여부 확인 메서드들
        [Binding]
        public bool CanUseCompass()
        {
            return Global.ItemManager != null && Global.ItemManager.GetItemCount(ItemType.Compass) > 0;
        }
        
        [Binding]
        public bool CanUseStopwatch()
        {
            return Global.ItemManager != null && Global.ItemManager.GetItemCount(ItemType.Stopwatch) > 0;
        }
        
        [Binding]
        public bool CanUseHint()
        {
            return Global.ItemManager != null && Global.ItemManager.GetItemCount(ItemType.Hint) > 0;
        }
    }
}
