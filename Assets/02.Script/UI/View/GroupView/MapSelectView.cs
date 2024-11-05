using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using UI;
using UnityWeld.Binding;
using Manager;
using Data;

namespace UnityWeld
{
    [System.Serializable]
    public class SceneInfo
    {
        public SceneName sceneName;
        public Sprite sceneThumbnail;
        public string sceneNameString;
        public string sceneDescription;
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

        private void SetCurMap(int pageIndex)
        {
            if(!IsValidPageIndex(pageIndex)) return;
            
            curPageIndex = pageIndex;
            CurMapThumbnail = sceneInfos[pageIndex].sceneThumbnail;
            
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
