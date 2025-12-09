using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityWeld.Binding;
using UnityWeld;
using Unity.VisualScripting;
using Manager;
using DeskCat.FindIt.Scripts.Core.Main.System;
using System.Collections.Generic;

namespace UI
{
    [Binding]
    public class ChapterSelectGroupView : GroupView
    {
        private ChapterSelectView _parentChapterView;
        private List<MapSelectElementView> _stageElements = new List<MapSelectElementView>();
        private int _currentSelectedIndex = -1;

        private void Start() 
        {
            _parentChapterView = GetComponentInParent<ChapterSelectView>();
            if (_parentChapterView == null)
            {
                Debug.LogError("ChapterSelectView를 찾을 수 없습니다!");
                return;
            }

            RefreshStages();
        }

        /// <summary>
        /// 현재 챕터의 스테이지 목록을 갱신합니다
        /// </summary>
        public void RefreshStages()
        {
            if (_parentChapterView == null) return;

            int stageCount = _parentChapterView.GetCurrentChapterStageCount();
            
            if (stageCount == 0)
            {
                Debug.LogWarning("현재 챕터에 스테이지가 없습니다.");
                return;
            }

            // 기존 뷰모델 초기화
            PrepareViewModels(stageCount);
            _stageElements.Clear();

            // 각 스테이지 초기화
            int firstUnlockedIndex = -1;
            for (int i = 0; i < GetViewModels().Count; i++)
            {
                var stageElement = GetViewModels()[i] as MapSelectElementView;
                if (stageElement != null)
                {
                    string sceneName = _parentChapterView.GetStageSceneName(i);
                    bool isLocked = _parentChapterView.IsStageLockedByIndex(i);
                    // Debug.Log($"Initializing stage {i}: SceneName={sceneName}, IsLocked={isLocked}");
                    stageElement.Initialize(i, sceneName, _parentChapterView, isLocked);
                    _stageElements.Add(stageElement);
                    
                    // 해금된 첫 번째 스테이지 찾기
                    if (firstUnlockedIndex == -1 && !isLocked)
                    {
                        firstUnlockedIndex = i;
                    }
                }
            }

            // 해금된 첫 번째 스테이지 자동 선택
            if (firstUnlockedIndex >= 0)
            {
                SelectStage(firstUnlockedIndex);
            }
        }

        /// <summary>
        /// 특정 스테이지를 선택합니다
        /// </summary>
        /// <param name="stageIndex">선택할 스테이지 인덱스</param>
        public void SelectStage(int stageIndex)
        {
            if (stageIndex < 0 || stageIndex >= _stageElements.Count)
                return;
            
            var targetElement = _stageElements[stageIndex];
            // 잠긴 스테이지는 선택하지 않음
            if (targetElement.IsLocked)
                return;

            // 이전 선택 해제
            if (_currentSelectedIndex >= 0 && _currentSelectedIndex < _stageElements.Count)
            {
                _stageElements[_currentSelectedIndex].Deselect();
            }

            // 새로운 스테이지 선택 (잠금 체크를 건너뛰고 직접 선택)
            _currentSelectedIndex = stageIndex;
            if (_parentChapterView != null)
            {
                _parentChapterView.SelectStage(stageIndex);
                targetElement.IsSelected = true;
            }
        }

        /// <summary>
        /// 현재 선택된 스테이지 인덱스 반환
        /// </summary>
        public int GetCurrentSelectedIndex()
        {
            return _currentSelectedIndex;
        }

        /// <summary>
        /// 현재 선택된 스테이지의 씬 이름 반환
        /// </summary>
        public string GetCurrentSelectedSceneName()
        {
            if (_currentSelectedIndex >= 0 && _currentSelectedIndex < _stageElements.Count)
            {
                return _stageElements[_currentSelectedIndex].GetSceneName();
            }
            return null;
        }
    }
    
    /// <summary>
    /// 이전 MapSelectGroupView는 더 이상 사용하지 않음 (호환성을 위해 남겨둠)
    /// </summary>
    [System.Obsolete("Use ChapterSelectGroupView instead")]
    [Binding]
    public class MapSelectGroupView : GroupView
    {
        private void Start() 
        {
            var itemSetDataList = ItemSetManager.Instance.GetItemSetDataList();
            if(itemSetDataList.Count == 0)
            {
                return;
            }
            PrepareViewModels(itemSetDataList.Count);

            for (int i = 0; i < GetViewModels().Count; i++)
            {
                var missionElementViewModel = GetViewModels()[i] as MissionElementViewModel;
                missionElementViewModel.Initialize(itemSetDataList[i]);
            }
        }
    }
}