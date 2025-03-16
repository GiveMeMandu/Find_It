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
using System.Text.RegularExpressions;
using System.Linq;

namespace UnityWeld
{
    [Binding]
    public class PuzzleLevelSelectView : GroupView
    {
        public static PuzzleLevelSelectView Instance { get; private set; }
        
        private Dictionary<SceneName, List<PuzzleData>> _stageGroups = new Dictionary<SceneName, List<PuzzleData>>();
        private SceneName _currentSceneName;
        private int _currentStageIndex;
        
        private string _stageTitle;
        
        [Binding]
        public string StageTitle
        {
            get => _stageTitle;
            set
            {
                _stageTitle = value;
                OnPropertyChanged(nameof(StageTitle));
            }
        }
        
        private Sprite _stageImage;
        
        [Binding]
        public Sprite StageImage
        {
            get => _stageImage;
            set
            {
                _stageImage = value;
                OnPropertyChanged(nameof(StageImage));
            }
        }
        
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
        
        private bool _isPrevButtonActive = false;
        
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
        
        private bool _isNextButtonActive = false;
        
        [Binding]
        public void NextSubStage()
        {
            // 현재 SceneName 내에서 다음 스테이지로 이동
            if (_currentStageIndex < _stageGroups[_currentSceneName].Count - 1)
            {
                SelectStage(_currentSceneName, _currentStageIndex + 1);
            }
        }
        
        [Binding]
        public void PrevSubStage()
        {
            // 현재 SceneName 내에서 이전 스테이지로 이동
            if (_currentStageIndex > 0)
            {
                SelectStage(_currentSceneName, _currentStageIndex - 1);
            }
        }
        
        // 스테이지 내 이동 버튼 활성화 상태
        private bool _isPrevSubStageActive = false;
        private bool _isNextSubStageActive = false;
        
        [Binding]
        public bool IsPrevSubStageActive
        {
            get => _isPrevSubStageActive;
            set
            {
                _isPrevSubStageActive = value;
                OnPropertyChanged(nameof(IsPrevSubStageActive));
            }
        }
        
        [Binding]
        public bool IsNextSubStageActive
        {
            get => _isNextSubStageActive;
            set
            {
                _isNextSubStageActive = value;
                OnPropertyChanged(nameof(IsNextSubStageActive));
            }
        }
        
        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }
        
        private void OnEnable()
        {
            InitializeStageGroups();
            SetupStageButtons();
        }
        
        private void InitializeStageGroups()
        {
            _stageGroups.Clear();
            
            // PuzzleGameManager에서 모든 PuzzleData 가져오기
            PuzzleData[] allPuzzleData = PuzzleGameManager.Instance.PuzzleDataList;
            // SceneName별로 그룹화
            foreach (PuzzleData puzzleData in allPuzzleData)
            {
                if (!_stageGroups.ContainsKey(puzzleData.sceneName))
                {
                    _stageGroups[puzzleData.sceneName] = new List<PuzzleData>();
                }
                
                _stageGroups[puzzleData.sceneName].Add(puzzleData);
            }
            
            // 각 그룹 내에서 stageIndex 기준으로 정렬
            foreach (var sceneName in _stageGroups.Keys.ToList())
            {
                _stageGroups[sceneName] = _stageGroups[sceneName].OrderBy(p => p.stageIndex).ToList();
            }
        }
        
        private void SetupStageButtons()
        {
            // 스테이지 그룹 수에 맞게 버튼 생성
            int stageGroupCount = _stageGroups.Count;
            PrepareViewModels(stageGroupCount);
            
            // 각 버튼에 스테이지 그룹 정보 설정
            int index = 0;
            foreach (var stageGroup in _stageGroups)
            {
                SceneName sceneName = stageGroup.Key;
                List<PuzzleData> stages = stageGroup.Value;
                
                if (stages.Count > 0)
                {
                    PuzzleData representativeStage = stages[0]; // 대표 스테이지로 첫 번째 스테이지 사용
                    
                    var viewModel = GetViewModel(index);
                    PuzzleStageSelectViewModel stageViewModel = viewModel.GetComponent<PuzzleStageSelectViewModel>();
                    if (stageViewModel != null)
                    {
                        string stageName = string.Format("{0}", (int)sceneName - 3);
                        stageViewModel.Initialize(stageName, 0, sceneName, representativeStage.puzzleImage);
                    }
                    
                    index++;
                }
            }
            
            // 첫 번째 스테이지 그룹 선택
            if (_stageGroups.Count > 0)
            {
                SelectStage(_stageGroups.Keys.First(), 0);
            }
        }
        
        public void SelectStage(SceneName sceneName, int stageIndex)
        {
            if (!_stageGroups.ContainsKey(sceneName) || 
                stageIndex < 0 || 
                stageIndex >= _stageGroups[sceneName].Count)
            {
                return;
            }
            
            _currentSceneName = sceneName;
            _currentStageIndex = stageIndex;
            
            PuzzleData selectedStage = _stageGroups[sceneName][stageIndex];
            
            // UI 업데이트
            int sceneNumber = (int)sceneName;
            int totalStagesInGroup = _stageGroups[sceneName].Count;
            
            // 스테이지 제목 형식: "Stage 1-2: 퍼즐 이름 (2/5)"
            StageTitle = $"Stage {sceneNumber}-{stageIndex + 1}: {selectedStage.puzzleName} ({stageIndex + 1}/{totalStagesInGroup})";
            StageImage = selectedStage.puzzleImage;
            
            // 버튼 상태 업데이트
            UpdateButtonStates();
        }
        
        [Binding]
        public void NextStage()
        {
            // 현재 SceneName의 다음 SceneName으로 이동
            List<SceneName> sceneNames = _stageGroups.Keys.ToList();
            int currentIndex = sceneNames.IndexOf(_currentSceneName);
            
            if (currentIndex < sceneNames.Count - 1)
            {
                // 다음 SceneName으로 이동
                SceneName nextSceneName = sceneNames[currentIndex + 1];
                SelectStage(nextSceneName, 0); // 다음 SceneName의 첫 번째 스테이지 선택
            }
        }
        
        [Binding]
        public void PrevStage()
        {
            // 현재 SceneName의 이전 SceneName으로 이동
            List<SceneName> sceneNames = _stageGroups.Keys.ToList();
            int currentIndex = sceneNames.IndexOf(_currentSceneName);
            
            if (currentIndex > 0)
            {
                // 이전 SceneName으로 이동
                SceneName prevSceneName = sceneNames[currentIndex - 1];
                SelectStage(prevSceneName, 0); // 이전 SceneName의 첫 번째 스테이지 선택
            }
        }
        
        [Binding]
        public void OnClickStartButton()
        {
            // 선택된 스테이지 시작
            PuzzleGameManager.Instance.InitializePuzzle(_stageGroups[_currentSceneName][_currentStageIndex].stageIndex);
        }
        
        private void UpdateButtonStates()
        {
            // SceneName 기준으로 버튼 상태 업데이트
            List<SceneName> sceneNames = _stageGroups.Keys.ToList();
            int currentIndex = sceneNames.IndexOf(_currentSceneName);
            
            IsPrevButtonActive = currentIndex > 0;
            IsNextButtonActive = currentIndex < sceneNames.Count - 1;
            
            // 스테이지 내 이동 버튼 상태 업데이트
            IsPrevSubStageActive = _currentStageIndex > 0;
            IsNextSubStageActive = _currentStageIndex < _stageGroups[_currentSceneName].Count - 1;
        }
    }
}
