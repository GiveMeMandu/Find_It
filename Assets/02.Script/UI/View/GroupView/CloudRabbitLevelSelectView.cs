using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;
using UnityWeld.Binding;
using Data;
using I2.Loc;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UI.Page;
using Manager;
using static CloudRabbitGameManager;
using Cysharp.Threading.Tasks;

namespace UnityWeld
{
    [Binding]
    public class CloudRabbitLevelSelectView : GroupView
    {
        public static CloudRabbitLevelSelectView Instance { get; private set; }
        
        [SerializedDictionary]
        public SerializedDictionary<SceneName, List<CloudRabbitData>> _stageGroups = new SerializedDictionary<SceneName, List<CloudRabbitGameManager.CloudRabbitData>>();
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
            
            // CloudRabbitGameManager에서 모든 CloudRabbitData 가져오기
            var allCloudRabbitData = CloudRabbitGameManager.Instance.CloudRabbitDataList;
            // SceneName별로 그룹화
            foreach (CloudRabbitGameManager.CloudRabbitData cloudRabbitData in allCloudRabbitData)
            {
                if (!_stageGroups.ContainsKey(cloudRabbitData.sceneName))
                {
                    _stageGroups[cloudRabbitData.sceneName] = new List<CloudRabbitGameManager.CloudRabbitData>();
                }
                
                // GameManager의 CloudRabbitData를 직접 추가
                _stageGroups[cloudRabbitData.sceneName].Add(cloudRabbitData);
            }
            
            // 각 그룹 내에서 stageIndex 기준으로 정렬
            foreach (var sceneName in _stageGroups.Keys.ToList())
            {
                _stageGroups[sceneName] = _stageGroups[sceneName].OrderBy(p => p.stageIndex).ToList();
            }
        }
        
        private void SetupStageButtons()
        {
            if (_stageGroups.Count > 0)
            {
                SelectStage(_stageGroups.Keys.First(), 0);
                RefreshStageButtons(_stageGroups.Keys.First());
            }
        }
        
        private void RefreshStageButtons(SceneName sceneName)
        {
            if (!_stageGroups.ContainsKey(sceneName))
                return;

            List<CloudRabbitGameManager.CloudRabbitData> stages = _stageGroups[sceneName];
            
            // 현재 선택된 스테이지의 모든 인덱스에 맞는 버튼 생성
            PrepareViewModels(stages.Count);
            
            for (int i = 0; i < stages.Count; i++)
            {
                var viewModel = GetViewModel(i);
                PuzzleStageSelectViewModel stageViewModel = viewModel.GetComponent<PuzzleStageSelectViewModel>();
                if (stageViewModel != null)
                {
                    CloudRabbitGameManager.CloudRabbitData stageData = stages[i];
                    string stageName = $"{(int)sceneName - 3}-{stageData.stageIndex + 1}";
                    // 마지막 매개변수를 null로 전달하고 나중에 별도로 이미지 설정 방법을 구현
                    stageViewModel.Initialize(stageName, stageData.stageIndex, sceneName, null);
                }
            }
        }
        
        public void SelectStage(SceneName sceneName, int stageIndex)
        {
            if (!_stageGroups.ContainsKey(sceneName))
                return;
                
            var stages = _stageGroups[sceneName];
            var selectedStage = stages.FirstOrDefault(s => s.stageIndex == stageIndex + 1);
            
            if (selectedStage == null)
                return;
                
            _currentSceneName = sceneName;
            _currentStageIndex = stageIndex;
            
            // UI 업데이트
            int sceneNumber = (int)sceneName;
            
            // 스테이지 제목 형식
            StageTitle = string.IsNullOrEmpty(selectedStage.stageName) 
                ? $"{LocalizationManager.GetTranslation("SceneName/" + sceneName)}"
                : $"{sceneNumber}-{selectedStage.stageIndex}: {selectedStage.stageName}";
            
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
                SelectStage(nextSceneName, 0);
                RefreshStageButtons(nextSceneName);
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
                SelectStage(prevSceneName, 0);
                RefreshStageButtons(prevSceneName);
            }
        }
        
        public void StartStage(SceneName sceneName, int stageIndex)
        {
            if (!_stageGroups.ContainsKey(sceneName))
                return;
                
            var stages = _stageGroups[sceneName];
            var selectedStage = stages.FirstOrDefault(s => s.stageIndex == stageIndex);
            
            if (selectedStage == null)
                return;
            
            // 구름 토끼 게임 시작
            CloudRabbitGameManager.Instance.InitializeGame(sceneName, selectedStage.stageIndex).Forget();
        }
        
        [Binding]
        public void OnClickStartButton()
        {
            // 현재 선택된 스테이지 시작
            StartStage(_currentSceneName, _currentStageIndex);
        }
        
        private void UpdateButtonStates()
        {
            // SceneName 기준으로 버튼 상태 업데이트
            List<SceneName> sceneNames = _stageGroups.Keys.ToList();
            int currentIndex = sceneNames.IndexOf(_currentSceneName);
            
            IsPrevButtonActive = currentIndex > 0;
            IsNextButtonActive = currentIndex < sceneNames.Count - 1;
            
            // 스테이지 내 이동 버튼 상태 업데이트
            var currentStages = _stageGroups[_currentSceneName];
            var currentStageData = currentStages.FirstOrDefault(s => s.stageIndex == _currentStageIndex + 1);
            if (currentStageData != null)
            {
                int currentStageListIndex = currentStages.IndexOf(currentStageData);
                IsPrevSubStageActive = currentStageListIndex > 0;
                IsNextSubStageActive = currentStageListIndex < currentStages.Count - 1;
            }
        }
    }
}
