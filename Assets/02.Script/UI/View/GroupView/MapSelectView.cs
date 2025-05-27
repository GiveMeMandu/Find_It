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
using UI.Effect;
using System.Linq;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace UnityWeld
{
    [System.Serializable]
    public class SceneInfo
    {
        [BoxGroup("스테이지 정보")] public int stageIndex;
        [BoxGroup("스테이지 정보")] public SceneName sceneName;
        [BoxGroup("스테이지 정보")] public Sprite sceneThumbnail;
        [BoxGroup("스테이지 정보")] public Sprite ChanllengeIcon;
        [BoxGroup("스테이지 정보")] public int ChanllengeCount;
        [BoxGroup("스테이지 정보")]
        [TermsPopup("SceneName/")]
        public string sceneNameStringKey;
        [BoxGroup("스테이지 정보")]
        [TermsPopup("SceneDescription/")]
        public string sceneDescriptionKey;
    }
    [Binding]
    public class MapSelectView : ViewModel
    {
        public List<ClickToMoveEffect> clickToMoveEffects = new List<ClickToMoveEffect>();
        public List<Button> clickToChangeStageBtns = new List<Button>();
        [SerializeField] private List<SceneInfo> sceneInfos = new List<SceneInfo>();
        
        [BoxGroup("버튼 이미지 설정")]
        [LabelText("선택된 버튼 이미지")]
        [SerializeField] private Sprite selectedButtonSprite;
        
        [BoxGroup("버튼 이미지 설정")]
        [LabelText("선택되지 않은 버튼 이미지")]
        [SerializeField] private Sprite unselectedButtonSprite;

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


        private int _currentStageIndex = 0;
        private SceneName _currentSceneName;

        /// <summary>
        /// 현재 선택된 스테이지 인덱스
        /// </summary>
        public int CurrentStageIndex => _currentStageIndex;

        /// <summary>
        /// 현재 선택된 씬 이름
        /// </summary>
        public SceneName CurrentSceneName => _currentSceneName;

        /// <summary>
        /// 현재 선택된 스테이지 정보
        /// </summary>
        public SceneInfo CurrentStageInfo => sceneInfos[_currentStageIndex];

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
            clickToMoveEffects = GetComponentsInChildren<ClickToMoveEffect>().ToList();
            clickToChangeStageBtns = clickToMoveEffects.Select(x => x.GetComponent<Button>()).ToList();
            InitialStageGroup();
            SetupStageButtons();
        }
        private void InitialStageGroup()
        {
            if (sceneInfos.Count > 0)
            {
                _currentSceneName = sceneInfos[0].sceneName;
                SetCurMap(0);
                
                // 게임 시작 시 첫 번째 스테이지에 이펙트 재생
                PlayInitialStageEffect();
            }
        }
        
        /// <summary>
        /// 게임 시작 시 첫 번째 스테이지에 이펙트를 재생합니다
        /// </summary>
        private void PlayInitialStageEffect()
        {
            // 현재 씬의 첫 번째 스테이지 인덱스 찾기 (버튼 인덱스 0번에 해당하는 실제 스테이지 인덱스)
            var currentSceneStageIndices = GetCurrentSceneStageIndices();
            
            if (currentSceneStageIndices.Count > 0 && clickToMoveEffects != null)
            {
                int firstStageIndex = currentSceneStageIndices[0]; // 현재 씬의 첫 번째 스테이지
                
                // 해당 인덱스가 유효한 범위인지 확인 후 이펙트 실행
                if (firstStageIndex >= 0 && firstStageIndex < clickToMoveEffects.Count)
                {
                    // 복귀하지 않고 바로 이펙트 재생하도록 PlayVFXForce 사용
                    PlayInitialStageEffectAsync(firstStageIndex).Forget();
                }
            }
        }
        private async UniTaskVoid PlayInitialStageEffectAsync(int firstStageIndex)
        {
            await UniTask.WaitForSeconds(0.5f);
            clickToMoveEffects[firstStageIndex].PlayVFXForce();
        }
        
        /// <summary>
        /// 씬 변경 후 딜레이를 두고 첫 번째 스테이지 이펙트를 재생합니다
        /// </summary>
        private async UniTaskVoid PlayInitialStageEffectDelayed()
        {
            // 이펙트 리셋 후 약간의 딜레이
            await UniTask.WaitForSeconds(0.1f);
            PlayInitialStageEffect();
        }
        [Binding]
        public void NextStageGroup()
        {
            NextStage();
        }
        [Binding]
        public void PrevStageGroup()
        {
            PrevStage();
        }
        [Binding]
        public void OnClickStartButton()
        {
            var main = Global.CurrentScene as OutGame.MainMenu;
            
            // ChallengeSelectView가 있는지 확인하고 챌린지가 선택되었는지 확인
            var challengeSelectView = FindAnyObjectByType<ChallengeSelectView>();
            if (challengeSelectView != null && challengeSelectView.SelectedChallengeType != ChallengeType.None)
            {
                // 챌린지가 선택된 경우 ChallengeSelectView에서 처리
                challengeSelectView.ExecuteSelectedChallenge();
            }
            else
            {
                // 일반 게임 시작 - 현재 선택된 씬과 버튼 인덱스로 계산
                int targetSceneIndex = CalculateSceneIndex(_currentSceneName, GetCurrentButtonIndex());
                Debug.Log($"[MapSelectView] Normal game start - Scene: {_currentSceneName}, ButtonIndex: {GetCurrentButtonIndex()}, TargetScene: {targetSceneIndex}");
                main.OnClickStartButton(targetSceneIndex);
            }
        }
        
        /// <summary>
        /// 현재 선택된 씬의 버튼 인덱스를 계산합니다 (0, 1, 2...)
        /// </summary>
        /// <returns>현재 씬 내에서의 버튼 인덱스</returns>
        private int GetCurrentButtonIndex()
        {
            var currentSceneStageIndices = GetCurrentSceneStageIndices();
            
            for (int i = 0; i < currentSceneStageIndices.Count; i++)
            {
                if (currentSceneStageIndices[i] == _currentStageIndex)
                {
                    return i;
                }
            }
            
            return 0; // 기본값
        }
        
        /// <summary>
        /// 씬 이름과 버튼 인덱스를 기반으로 실제 씬 인덱스를 계산합니다
        /// </summary>
        /// <param name="sceneName">선택된 씬 이름</param>
        /// <param name="buttonIndex">버튼 인덱스 (0, 1, 2)</param>
        /// <returns>Build Settings의 실제 씬 인덱스</returns>
        private int CalculateSceneIndex(SceneName sceneName, int buttonIndex)
        {
            switch (sceneName)
            {
                case SceneName.Stage1_1:
                case SceneName.Stage1_2:
                case SceneName.Stage1_3:
                    // Stage1 시리즈: 4, 5, 6
                    return 4 + buttonIndex;
                    
                case SceneName.Stage2_1:
                case SceneName.Stage2_2:
                case SceneName.Stage2_3:
                    // Stage2 시리즈: 7, 8, 9
                    return 7 + buttonIndex;
                    
                default:
                    Debug.LogWarning($"Unknown scene name: {sceneName}");
                    return 4; // 기본값으로 Stage1_1
            }
        }
        public void Refresh()
        {
            SetCurMap(_currentStageIndex);
        }

        /// <summary>
        /// 다음 씬으로 이동 (sceneName 단위)
        /// </summary>
        [Binding]
        public void NextStage()
        {
            // 현재 SceneName의 다음 SceneName으로 이동
            var uniqueSceneNames = GetUniqueSceneNames();
            int currentIndex = uniqueSceneNames.IndexOf(_currentSceneName);

            if (currentIndex < uniqueSceneNames.Count - 1)
            {
                SceneName nextSceneName = uniqueSceneNames[currentIndex + 1];
                SelectStageBySceneName(nextSceneName);
            }
        }

        /// <summary>
        /// 이전 씬으로 이동 (sceneName 단위)
        /// </summary>
        [Binding]
        public void PrevStage()
        {
            // 현재 SceneName의 이전 SceneName으로 이동
            var uniqueSceneNames = GetUniqueSceneNames();
            int currentIndex = uniqueSceneNames.IndexOf(_currentSceneName);

            if (currentIndex > 0)
            {
                SceneName prevSceneName = uniqueSceneNames[currentIndex - 1];
                SelectStageBySceneName(prevSceneName);
            }
        }

        /// <summary>
        /// 특정 스테이지를 선택합니다
        /// </summary>
        /// <param name="stageIndex">선택할 스테이지 인덱스</param>
        public void SelectStage(int stageIndex)
        {
            // 같은 스테이지를 다시 클릭해도 이펙트는 재생되도록 수정
            bool isSameStage = (stageIndex == _currentStageIndex);
            
            if (!isSameStage)
            {
                // 현재 스테이지 인덱스가 유효한 범위인지 확인 후 이펙트 실행
                if (clickToMoveEffects != null && _currentStageIndex >= 0 && _currentStageIndex < clickToMoveEffects.Count)
                {
                    clickToMoveEffects[_currentStageIndex].SetIsResetOnNext();
                    clickToMoveEffects[_currentStageIndex].PlayVFX();
                }
                
                SetCurMap(stageIndex);
            }

            // 새로운 스테이지 인덱스가 유효한 범위인지 확인 후 이펙트 실행 (같은 스테이지여도 실행)
            if (clickToMoveEffects != null && stageIndex >= 0 && stageIndex < clickToMoveEffects.Count)
            {
                // 강제로 이펙트 재생 (이미 재생 중이어도 실행)
                clickToMoveEffects[stageIndex].PlayVFXForce();
            }
        }

        /// <summary>
        /// 특정 씬 이름으로 스테이지를 선택합니다
        /// </summary>
        /// <param name="sceneName">선택할 씬 이름</param>
        public void SelectStageBySceneName(SceneName sceneName)
        {
            // 씬 변경 시 모든 이펙트 리셋
            ResetAllEffects();
            
            // 해당 sceneName의 첫 번째 스테이지를 찾아서 선택
            for (int i = 0; i < sceneInfos.Count; i++)
            {
                if (sceneInfos[i].sceneName == sceneName)
                {
                    // 씬 변경 시에는 이전 스테이지 이펙트 처리 없이 바로 맵만 설정
                    SetCurMap(i);
                    
                    // 새로운 씬의 첫 번째 스테이지에 이펙트 재생 (약간의 딜레이 후)
                    PlayInitialStageEffectDelayed().Forget();
                    break;
                }
            }
        }
        
        /// <summary>
        /// 모든 클릭 이펙트를 초기 상태로 리셋합니다
        /// </summary>
        private void ResetAllEffects()
        {
            if (clickToMoveEffects == null) return;
            
            foreach (var effect in clickToMoveEffects)
            {
                if (effect != null)
                {
                    effect.ResetToInitialState();
                }
            }
        }

        /// <summary>
        /// 고유한 씬 이름들의 리스트를 가져옵니다
        /// </summary>
        /// <returns>고유한 씬 이름들의 리스트</returns>
        private List<SceneName> GetUniqueSceneNames()
        {
            var uniqueScenes = new List<SceneName>();
            foreach (var sceneInfo in sceneInfos)
            {
                if (!uniqueScenes.Contains(sceneInfo.sceneName))
                {
                    uniqueScenes.Add(sceneInfo.sceneName);
                }
            }
            return uniqueScenes;
        }

        /// <summary>
        /// 스테이지 인덱스로 특정 스테이지 정보를 가져옵니다
        /// </summary>
        /// <param name="stageIndex">스테이지 인덱스</param>
        /// <returns>해당 스테이지 정보, 유효하지 않은 인덱스인 경우 null</returns>
        public SceneInfo GetStageInfo(int stageIndex)
        {
            if (IsValidStageIndex(stageIndex))
                return sceneInfos[stageIndex];
            return null;
        }

        private void SetCurMap(int stageIndex)
        {
            if (!IsValidStageIndex(stageIndex)) return;

            _currentStageIndex = stageIndex;
            var currentStage = sceneInfos[stageIndex];
            _currentSceneName = currentStage.sceneName;

            CurMapThumbnail = currentStage.sceneThumbnail;
            CurSceneNameString = string.Format("{0:D2}. {1}", currentStage.stageIndex + 1, LocalizationManager.GetTranslation(currentStage.sceneNameStringKey));
            CurSceneDescription = LocalizationManager.GetTranslation(currentStage.sceneDescriptionKey);
            CurChallengeCount = string.Format("x {0}", currentStage.ChanllengeCount);
            CurChallengeIcon = currentStage.ChanllengeIcon;

            // 버튼 상태 업데이트
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            // SceneName 기준으로 버튼 상태 업데이트
            var uniqueSceneNames = GetUniqueSceneNames();
            int currentIndex = uniqueSceneNames.IndexOf(_currentSceneName);
            
            IsPrevButtonActive = currentIndex > 0;
            IsNextButtonActive = currentIndex < uniqueSceneNames.Count - 1;
            
            // 현재 씬에 해당하는 스테이지 수에 따라 버튼 활성화/비활성화
            UpdateStageButtons();
            
            // 버튼 이미지 업데이트 (스테이지 변경 시)
            UpdateButtonImages();
        }
        
        /// <summary>
        /// 현재 선택된 씬 이름에 해당하는 스테이지 수를 가져옵니다
        /// </summary>
        /// <returns>현재 씬의 스테이지 수</returns>
        private int GetCurrentSceneStageCount()
        {
            int count = 0;
            foreach (var sceneInfo in sceneInfos)
            {
                if (sceneInfo.sceneName == _currentSceneName)
                {
                    count++;
                }
            }
            return count;
        }
        
        /// <summary>
        /// 현재 씬의 스테이지 수에 따라 clickToChangeStageBtns의 활성화 상태를 업데이트합니다
        /// </summary>
        private void UpdateStageButtons()
        {
            if (clickToChangeStageBtns == null) return;
            
            int currentSceneStageCount = GetCurrentSceneStageCount();
            
            for (int i = 0; i < clickToChangeStageBtns.Count; i++)
            {
                if (clickToChangeStageBtns[i] != null)
                {
                    bool shouldBeActive = i < currentSceneStageCount;
                    
                    // 버튼 활성화/비활성화
                    if(!shouldBeActive)
                        clickToMoveEffects[i].StopVFX();
                    clickToChangeStageBtns[i].gameObject.SetActive(shouldBeActive);
                    if(!shouldBeActive)
                    {
                        if(clickToChangeStageBtns[i].gameObject.activeSelf)
                        {
                            // 비활성화된 버튼은 초기 위치로 이동하고 복귀된 상태로 설정
                            if (!shouldBeActive)
                            {
                                // 해당 이펙트도 복귀된 상태로 설정 (DOTween으로 0초만에 즉시 이동)
                                if (i < clickToMoveEffects.Count && clickToMoveEffects[i] != null)
                                {
                                    clickToMoveEffects[i].ResetVFXImmediate();
                                }
                            }
                        }
                    }
                }
            }
            
            // 버튼 이벤트 다시 설정 (이미지 업데이트는 UpdateButtonStates에서 처리)
            SetupStageButtonsOnly();
        }
        
        /// <summary>
        /// 현재 선택된 씬에 해당하는 스테이지 인덱스들을 가져옵니다
        /// </summary>
        /// <returns>현재 씬의 스테이지 인덱스 리스트</returns>
        public List<int> GetCurrentSceneStageIndices()
        {
            var indices = new List<int>();
            for (int i = 0; i < sceneInfos.Count; i++)
            {
                if (sceneInfos[i].sceneName == _currentSceneName)
                {
                    indices.Add(i);
                }
            }
            return indices;
        }
        
        /// <summary>
        /// 스테이지 버튼들의 클릭 이벤트를 설정합니다
        /// </summary>
        private void SetupStageButtons()
        {
            SetupStageButtonsOnly();
            UpdateButtonImages();
        }
        
        /// <summary>
        /// 스테이지 버튼들의 클릭 이벤트만 설정합니다 (이미지 업데이트 제외)
        /// </summary>
        private void SetupStageButtonsOnly()
        {
            if (clickToChangeStageBtns == null) return;
            
            var currentSceneStageIndices = GetCurrentSceneStageIndices();
            
            for (int i = 0; i < clickToChangeStageBtns.Count; i++)
            {
                if (clickToChangeStageBtns[i] != null && i < currentSceneStageIndices.Count)
                {
                    // 기존 이벤트 제거
                    clickToChangeStageBtns[i].onClick.RemoveAllListeners();
                    
                    // 현재 씬의 해당 스테이지 인덱스로 SelectStage 호출하도록 설정
                    int stageIndex = currentSceneStageIndices[i];
                    clickToChangeStageBtns[i].onClick.AddListener(() => SelectStage(stageIndex));
                }
            }
        }
        
        /// <summary>
        /// 현재 선택된 스테이지에 따라 버튼 이미지를 업데이트합니다
        /// </summary>
        private void UpdateButtonImages()
        {
            if (clickToChangeStageBtns == null || selectedButtonSprite == null || unselectedButtonSprite == null) return;
            
            var currentSceneStageIndices = GetCurrentSceneStageIndices();
            int selectedButtonIndex = GetSelectedButtonIndex();
            
            for (int i = 0; i < clickToChangeStageBtns.Count; i++)
            {
                if (clickToChangeStageBtns[i] != null && i < currentSceneStageIndices.Count)
                {
                    var buttonImage = clickToChangeStageBtns[i].GetComponent<UnityEngine.UI.Image>();
                    if (buttonImage != null)
                    {
                        // 현재 선택된 버튼인지 확인
                        bool isSelected = (i == selectedButtonIndex);
                        buttonImage.sprite = isSelected ? selectedButtonSprite : unselectedButtonSprite;
                    }
                }
            }
        }
        
        /// <summary>
        /// 현재 선택된 스테이지의 버튼 인덱스를 가져옵니다
        /// </summary>
        /// <returns>선택된 버튼 인덱스, 찾지 못하면 -1</returns>
        private int GetSelectedButtonIndex()
        {
            var currentSceneStageIndices = GetCurrentSceneStageIndices();
            
            for (int i = 0; i < currentSceneStageIndices.Count; i++)
            {
                if (currentSceneStageIndices[i] == _currentStageIndex)
                {
                    return i;
                }
            }
            
            return -1; // 찾지 못한 경우
        }

        private bool IsValidStageIndex(int index)
        {
            int totalStages = sceneInfos.Count;
            bool isLastStage = index >= totalStages;
            bool isFirstStage = index < 0;
            return !isLastStage && !isFirstStage;
        }
    }
}
