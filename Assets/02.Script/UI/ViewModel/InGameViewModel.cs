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


        private LevelManager _levelManager;
        private void OnEnable() {
            _levelManager = FindFirstObjectByType<LevelManager>();
            _levelManager.OnFoundObj += OnFoundObj;
            _levelManager.OnFoundObjCountChanged += OnFoundObjCountChanged;
        }

        private void OnFoundObj(object sender, HiddenObj e)
        {
            FoundObjCountText = string.Format("{0}/{1}", _levelManager.GetTotalHiddenObjCount() - _levelManager.GetLeftHiddenObjCount(), _levelManager.GetTotalHiddenObjCount());
            FoundObjCountFillAmount = (_levelManager.GetTotalHiddenObjCount() - _levelManager.GetLeftHiddenObjCount()) / (float)_levelManager.GetTotalHiddenObjCount();
        }

        private void OnFoundObjCountChanged(object sender, EventArgs e)
        {
            FoundObjCountText = string.Format("{0}/{1}", _levelManager.GetTotalHiddenObjCount() - _levelManager.GetLeftHiddenObjCount(), _levelManager.GetTotalHiddenObjCount());
            FoundObjCountFillAmount = (_levelManager.GetTotalHiddenObjCount() - _levelManager.GetLeftHiddenObjCount()) / (float)_levelManager.GetTotalHiddenObjCount();
        }

        [Binding]
        public void ClosePage()
        {
        }
    }
}