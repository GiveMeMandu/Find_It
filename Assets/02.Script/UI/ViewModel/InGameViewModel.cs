using System;
using DeskCat.FindIt.Scripts.Core.Main.System;
using Manager;
using UnityWeld.Binding;

namespace UI
{
    [Binding]
    public class InGameViewModel : BaseViewModel
    {
        private string _foundObjCountText;
        private float _foundObjCountFillAmount;
        
        // 아이템 관련 필드들
        private string _compassCountText;
        private string _stopwatchCountText;
        private string _hintCountText;
        private bool _isCompassActive;
        private bool _isStopwatchActive;
        private bool _isHintActive;
        
        [Binding]
        public string FoundObjCountText
        {
            get => _foundObjCountText;
            set
            {
                _foundObjCountText = value;
                OnPropertyChanged(nameof(FoundObjCountText));
            }
        }

        [Binding]
        public float FoundObjCountFillAmount
        {
            get => _foundObjCountFillAmount;
            set
            {
                _foundObjCountFillAmount = value;
                OnPropertyChanged(nameof(FoundObjCountFillAmount));
            }
        }

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
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnFoundObj += OnFoundObj;
                LevelManager.Instance.OnFoundObjCountChanged += OnFoundObjCountChanged;
                
                // 현재 상태로 UI 강제 업데이트
                UpdateFoundObjCountDisplay();
            }

            // 아이템 관련 이벤트 구독
            if (Global.ItemManager != null)
            {
                Global.ItemManager.OnItemCountChanged += OnItemCountChanged;
                UpdateAllItemCounts();
            }
        }

        private void OnDisable()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnFoundObj -= OnFoundObj;
                LevelManager.Instance.OnFoundObjCountChanged -= OnFoundObjCountChanged;
            }

            // 아이템 관련 이벤트 구독 해제
            if (Global.ItemManager != null)
            {
                Global.ItemManager.OnItemCountChanged -= OnItemCountChanged;
            }
        }

        private void OnFoundObj(object sender, HiddenObj e)
        {
            FoundObjCountText = string.Format("{0}/{1}", LevelManager.Instance.GetTotalHiddenObjCount() - LevelManager.Instance.GetLeftHiddenObjCount(), LevelManager.Instance.GetTotalHiddenObjCount());
            FoundObjCountFillAmount = (LevelManager.Instance.GetTotalHiddenObjCount() - LevelManager.Instance.GetLeftHiddenObjCount()) / (float)LevelManager.Instance.GetTotalHiddenObjCount();
        }

        private void OnFoundObjCountChanged(object sender, EventArgs e)
        {
            FoundObjCountText = string.Format("{0}/{1}", LevelManager.Instance.GetTotalHiddenObjCount() - LevelManager.Instance.GetLeftHiddenObjCount(), LevelManager.Instance.GetTotalHiddenObjCount());
            FoundObjCountFillAmount = (LevelManager.Instance.GetTotalHiddenObjCount() - LevelManager.Instance.GetLeftHiddenObjCount()) / (float)LevelManager.Instance.GetTotalHiddenObjCount();
        }

        private void UpdateFoundObjCountDisplay()
        {
            if (LevelManager.Instance != null)
            {
                FoundObjCountText = string.Format("{0}/{1}", 
                    LevelManager.Instance.GetTotalHiddenObjCount() - LevelManager.Instance.GetLeftHiddenObjCount(), 
                    LevelManager.Instance.GetTotalHiddenObjCount());
                FoundObjCountFillAmount = (LevelManager.Instance.GetTotalHiddenObjCount() - LevelManager.Instance.GetLeftHiddenObjCount()) 
                    / (float)LevelManager.Instance.GetTotalHiddenObjCount();
            }
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
            UnityEngine.Debug.Log("나침반 활성화: 숨겨진 오브젝트 위치 힌트 표시");
            
            // 30초 후 효과 해제
            Invoke(nameof(DeactivateCompass), 30f);
        }

        private void ActivateStopwatch()
        {
            IsStopwatchActive = true;
            // TODO: 초시계 효과 구현 (게임 시간 일시정지 또는 시간 추가)
            UnityEngine.Debug.Log("초시계 활성화: 시간 정지 또는 시간 추가");
            
            // 효과는 즉시 적용되고 비활성화
            Invoke(nameof(DeactivateStopwatch), 1f);
        }

        private void ActivateHint()
        {
            IsHintActive = true;
            // TODO: 힌트 효과 구현 (화면 확대 또는 오브젝트 강조)
            UnityEngine.Debug.Log("힌트 활성화: 화면 확대 또는 오브젝트 강조");
            
            // 20초 후 효과 해제
            Invoke(nameof(DeactivateHint), 20f);
        }

        private void DeactivateCompass()
        {
            IsCompassActive = false;
            UnityEngine.Debug.Log("나침반 효과 해제");
        }

        private void DeactivateStopwatch()
        {
            IsStopwatchActive = false;
            UnityEngine.Debug.Log("초시계 효과 해제");
        }

        private void DeactivateHint()
        {
            IsHintActive = false;
            UnityEngine.Debug.Log("힌트 효과 해제");
        }

        // 테스트용 아이템 추가 메서드들
        [Binding]
        public void AddCompass()
        {
            if (Global.ItemManager != null)
            {
                Global.ItemManager.AddItem(ItemType.Compass, 1);
                UnityEngine.Debug.Log("나침반 1개 추가됨");
            }
        }

        [Binding]
        public void AddStopwatch()
        {
            if (Global.ItemManager != null)
            {
                Global.ItemManager.AddItem(ItemType.Stopwatch, 1);
                UnityEngine.Debug.Log("초시계 1개 추가됨");
            }
        }

        [Binding]
        public void AddHint()
        {
            if (Global.ItemManager != null)
            {
                Global.ItemManager.AddItem(ItemType.Hint, 1);
                UnityEngine.Debug.Log("힌트 1개 추가됨");
            }
        }

        [Binding]
        public void ClosePage()
        {
        }
    }
}