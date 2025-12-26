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
        private bool _isSubscribed = false;
        
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



        private void Start()
        {
            // Start에서 바로 구독하지 않고 LateStart로 연기
            StartCoroutine(LateStart());
        }

        private System.Collections.IEnumerator LateStart()
        {
            // 한 프레임 대기하여 LevelManager 초기화 보장
            yield return null;
            
            if (LevelManager.Instance != null && !_isSubscribed)
            {
                // Subscribe only to the count-changed event to avoid duplicate updates
                LevelManager.Instance.OnFoundObjCountChanged += OnFoundObjCountChanged;
                _isSubscribed = true;

                // 현재 상태로 UI 강제 업데이트
                UpdateFoundObjCountDisplay();
            }
        }

        private void OnDisable()
        {
            if (LevelManager.Instance != null && _isSubscribed)
            {
                LevelManager.Instance.OnFoundObjCountChanged -= OnFoundObjCountChanged;
                _isSubscribed = false;
            }
        }

        private void OnFoundObj(object sender, HiddenObj e)
        {
            // Keep for compatibility but delegate to centralized update to avoid inconsistencies
            UpdateFoundObjCountDisplay();
        }

        private void OnFoundObjCountChanged(object sender, EventArgs e)
        {
            UpdateFoundObjCountDisplay();
        }

        private void UpdateFoundObjCountDisplay()
        {
            if (LevelManager.Instance != null && LevelManager.Instance.TargetObjDic != null)
            {
                int total = LevelManager.Instance.GetTotalHiddenObjCount();
                int found = total - LevelManager.Instance.GetLeftHiddenObjCount();

                FoundObjCountText = string.Format("{0}/{1}", found, total);
                FoundObjCountFillAmount = total == 0 ? 0f : (found) / (float)total;
            }
            else
            {
                // TargetObjDic이 아직 초기화되지 않은 경우 기본값 표시
                FoundObjCountText = "0/0";
                FoundObjCountFillAmount = 0f;
            }
        }



        [Binding]
        public void ClosePage()
        {
        }
    }
}