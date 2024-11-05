using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using UI;
using UnityWeld.Binding;
using Manager;
using Data;
using I2.Loc;

namespace UnityWeld
{
    [System.Serializable]
    public class SceneInfo
    {
        [BoxGroup("스테이지 정보")] public SceneName sceneName;
        [BoxGroup("스테이지 정보")] public Sprite sceneThumbnail;
        [BoxGroup("스테이지 정보")] public Sprite ChanllengeIcon;
        [BoxGroup("스테이지 정보")] public int ChanllengeCount;
        [BoxGroup("스테이지 정보")] [TermsPopup("SceneName/")] 
        public string sceneNameStringKey;
        [BoxGroup("스테이지 정보")] [TermsPopup("SceneDescription/")]
        public string sceneDescriptionKey;
    }
    [Binding]
    public class MapSelectView : ViewModel
    {
        [SerializeField] private List<SceneInfo> sceneInfos = new List<SceneInfo>();

        private Sprite _curMapThumbnail;

        [Binding]
        public Sprite CurMapThumbnail
        {
            get => _curMapThumbnail;
            set
            {
                _curMapThumbnail = value;
                OnPropertyChanged(nameof(CurMapThumbnail));
            }
        }
        private string _curSceneNameString;

        [Binding]
        public string CurSceneNameString    
        {
            get => _curSceneNameString;
            set
            {
                _curSceneNameString = value;
                OnPropertyChanged(nameof(CurSceneNameString));
            }
        }
        private string _curSceneDescription;

        [Binding]
        public string CurSceneDescription
        {
            get => _curSceneDescription;
            set
            {
                _curSceneDescription = value;
                OnPropertyChanged(nameof(CurSceneDescription));
            }
        }
        private string _curChallengeCount;

        [Binding]
        public string CurChallengeCount
        {
            get => _curChallengeCount;
            set
            {
                _curChallengeCount = value;
                OnPropertyChanged(nameof(CurChallengeCount));
            }
        }
        private Sprite _curChallengeIcon;

        [Binding]
        public Sprite CurChallengeIcon
        {
            get => _curChallengeIcon;
            set
            {
                _curChallengeIcon = value;
                OnPropertyChanged(nameof(CurChallengeIcon));
            }
        }


        private int curPageIndex = 1;
        private bool _isPrevButtonActive = false;
        private bool _isNextButtonActive = false;

        [Binding]
        public bool IsPrevButtonActive
        {
            get => _isPrevButtonActive;
            set
            {
                _isPrevButtonActive = value;
                OnPropertyChanged(nameof(IsPrevButtonActive));
            }
        }

        [Binding]
        public bool IsNextButtonActive
        {
            get => _isNextButtonActive;
            set
            {
                _isNextButtonActive = value;
                OnPropertyChanged(nameof(IsNextButtonActive));
            }
        }
        private void OnEnable()
        {
            InitialTrainingGroup();
        }
        private void InitialTrainingGroup()
        {
            SetCurMap(0);
        }
        [Binding]
        public void NextTrainingGroup()
        {
            SetCurMap(curPageIndex + 1);
        }
        [Binding]
        public void PrevTrainingGroup()
        {
            SetCurMap(curPageIndex - 1);
        }
        [Binding]
        public void OnClickStartButton()
        {
            var main = Global.CurrentScene as OutGame.MainMenu;
            main.OnClickStartButton(curPageIndex);
        }
        public void Refresh()
        {
            SetCurMap(curPageIndex);
        }

        private void SetCurMap(int pageIndex)
        {
            if(!IsValidPageIndex(pageIndex)) return;
            
            curPageIndex = pageIndex;
            CurMapThumbnail = sceneInfos[pageIndex].sceneThumbnail;
            CurSceneNameString = string.Format("{0:D2}. {1}", pageIndex+1, LocalizationManager.GetTranslation(sceneInfos[pageIndex].sceneNameStringKey));
            CurSceneDescription = LocalizationManager.GetTranslation(sceneInfos[pageIndex].sceneDescriptionKey);
            CurChallengeCount = string.Format("x {0}", sceneInfos[pageIndex].ChanllengeCount);
            CurChallengeIcon = sceneInfos[pageIndex].ChanllengeIcon;

            // 버튼 상태 업데이트
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            IsPrevButtonActive = curPageIndex > 0;
            IsNextButtonActive = curPageIndex < sceneInfos.Count - 1;
        }

        private bool IsValidPageIndex(int index)
        {
            int totalPages = sceneInfos.Count;
            bool isLastPage = index >= totalPages;
            bool isFirstPage = index < 0;
            return !isLastPage && !isFirstPage;            
        }
    }
}
