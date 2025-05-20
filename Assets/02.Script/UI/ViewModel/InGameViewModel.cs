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
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnFoundObj += OnFoundObj;
                LevelManager.Instance.OnFoundObjCountChanged += OnFoundObjCountChanged;
                
                // 현재 상태로 UI 강제 업데이트
                UpdateFoundObjCountDisplay();
            }
        }

        private void OnDisable()
        {
            LevelManager.Instance.OnFoundObj -= OnFoundObj;
            LevelManager.Instance.OnFoundObjCountChanged -= OnFoundObjCountChanged;
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

        [Binding]
        public void ClosePage()
        {
        }
    }
}