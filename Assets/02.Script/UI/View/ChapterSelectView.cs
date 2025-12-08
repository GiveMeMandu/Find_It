using System.Collections.Generic;
using UnityEngine;
using UnityWeld.Binding;
using Sirenix.OdinInspector;
using Manager;
using UI;
using I2.Loc;

namespace UnityWeld
{
    [System.Serializable]
    public class StageInfo
    {
        [LabelText("스테이지 씬 이름")]
        [Tooltip("이 스테이지의 유니티 씬 이름")]
        public string sceneName;
        
        [LabelText("해금 조건 씬 이름들")]
        [Tooltip("이 스테이지를 해금하기 위해 클리어해야 하는 씬 이름들. 비어있으면 이전 스테이지 클리어 필요")]
        public List<string> unlockSceneNames = new List<string>();
    }

    [System.Serializable]
    public class ChapterInfo
    {
        [BoxGroup("챕터 정보")]
        [LabelText("챕터 인덱스")]
        public int chapterIndex;
        
        [BoxGroup("챕터 정보")]
        [LabelText("챕터 이름")]
        [TermsPopup("SceneName/")]
        public string chapterName;
        
        [BoxGroup("챕터 정보")]
        [LabelText("챕터 설명")]
        [TermsPopup("SceneDescription/")]
        [TextArea(3, 5)]
        public string chapterDescription;
        
        [BoxGroup("챕터 정보")]
        [LabelText("챕터 썸네일")]
        public Sprite chapterThumbnail;
        
        [BoxGroup("스테이지 목록")]
        [LabelText("스테이지 정보들")]
        [Tooltip("각 스테이지의 정보를 입력하세요")]
        public List<StageInfo> stages = new List<StageInfo>();
        
        [Button("스테이지 추가")]
        public void AddStage()
        {
            stages.Add(new StageInfo());
        }
    }

    [Binding]
    public class ChapterSelectView : ViewModel
    {
        [SerializeField] 
        [LabelText("챕터 목록")]
        private List<ChapterInfo> chapters = new List<ChapterInfo>();

        private int _currentChapterIndex = 0;
        private int _currentStageIndex = 0;

        private string _currentChapterName;
        private string _currentChapterDescription;
        private Sprite _currentChapterThumbnail;
        private string _currentStageLabel;
        private string _lockDescString;
        private bool _isPrevButtonActive = false;
        private bool _isNextButtonActive = false;
        private bool _isCurrentChapterLocked = false;
        private float _currentChapterProgress = 0f;

        [Binding]
        public string CurrentChapterName
        {
            get => _currentChapterName;
            set
            {
                _currentChapterName = value;
                OnPropertyChanged(nameof(CurrentChapterName));
            }
        }

        [Binding]
        public string CurrentChapterDescription
        {
            get => _currentChapterDescription;
            set
            {
                _currentChapterDescription = value;
                OnPropertyChanged(nameof(CurrentChapterDescription));
            }
        }

        [Binding]
        public Sprite CurrentChapterThumbnail
        {
            get => _currentChapterThumbnail;
            set
            {
                _currentChapterThumbnail = value;
                OnPropertyChanged(nameof(CurrentChapterThumbnail));
            }
        }

        [Binding]
        public string CurrentStageLabel
        {
            get => _currentStageLabel;
            set
            {
                _currentStageLabel = value;
                OnPropertyChanged(nameof(CurrentStageLabel));
            }
        }

        [Binding]
        public string LockDescString
        {
            get => _lockDescString;
            set
            {
                _lockDescString = value;
                OnPropertyChanged(nameof(LockDescString));
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

        [Binding]
        public bool IsCurrentChapterLocked
        {
            get => _isCurrentChapterLocked;
            set
            {
                _isCurrentChapterLocked = value;
                OnPropertyChanged(nameof(IsCurrentChapterLocked));
            }
        }

        [Binding]
        public float CurrentChapterProgress
        {
            get => _currentChapterProgress;
            set
            {
                _currentChapterProgress = Mathf.Clamp01(value);
                OnPropertyChanged(nameof(CurrentChapterProgress));
            }
        }

        /// <summary>
        /// 현재 선택된 챕터 정보
        /// </summary>
        public ChapterInfo CurrentChapter
        {
            get
            {
                if (_currentChapterIndex >= 0 && _currentChapterIndex < chapters.Count)
                    return chapters[_currentChapterIndex];
                return null;
            }
        }

        /// <summary>
        /// 현재 선택된 스테이지의 씬 이름
        /// </summary>
        public string CurrentStageSceneName
        {
            get
            {
                var chapter = CurrentChapter;
                if (chapter != null && _currentStageIndex >= 0 && _currentStageIndex < chapter.stages.Count)
                    return chapter.stages[_currentStageIndex].sceneName;
                return null;
            }
        }

        private void Start()
        {
            InitializeChapter();
        }

        private void InitializeChapter()
        {
            if (chapters.Count > 0)
            {
                _currentChapterIndex = 0;
                _currentStageIndex = 0;
                
                // 첫 스테이지 해금 확인
                string firstStageScene = GetFirstStageSceneName();
                if (!string.IsNullOrEmpty(firstStageScene))
                {
                    Global.UserDataManager.EnsureFirstStageUnlocked(firstStageScene);
                }
                
                UpdateChapterUI();
            }
        }

        /// <summary>
        /// 다음 챕터로 이동
        /// </summary>
        [Binding]
        public void NextChapter()
        {
            if (_currentChapterIndex < chapters.Count - 1)
            {
                _currentChapterIndex++;
                _currentStageIndex = 0; // 챕터 변경 시 첫 번째 스테이지로 초기화
                UpdateChapterUI();
                RefreshStageGroup();
            }
        }

        /// <summary>
        /// 이전 챕터로 이동
        /// </summary>
        [Binding]
        public void PrevChapter()
        {
            if (_currentChapterIndex > 0)
            {
                _currentChapterIndex--;
                _currentStageIndex = 0; // 챕터 변경 시 첫 번째 스테이지로 초기화
                UpdateChapterUI();
                RefreshStageGroup();
            }
        }

        /// <summary>
        /// 특정 스테이지를 선택합니다
        /// </summary>
        /// <param name="stageIndex">선택할 스테이지 인덱스</param>
        public void SelectStage(int stageIndex)
        {
            var chapter = CurrentChapter;
            if (chapter != null && stageIndex >= 0 && stageIndex < chapter.stages.Count)
            {
                _currentStageIndex = stageIndex;
                UpdateStageLabel();
            }
        }

        /// <summary>
        /// 게임 시작 버튼 클릭 시 호출
        /// </summary>
        [Binding]
        public void OnClickStartButton()
        {
            if (IsStageLockedByIndex(_currentStageIndex))
            {
                Debug.LogWarning("이 스테이지는 잠겨있습니다.");
                return;
            }
            
            string sceneName = CurrentStageSceneName;
            if (!string.IsNullOrEmpty(sceneName))
            {
                Debug.Log($"Loading scene: {sceneName}");
                LoadingSceneManager.LoadSceneByName(sceneName);
            }
            else
            {
                Debug.LogWarning("선택된 스테이지의 씬 이름이 유효하지 않습니다.");
            }
        }

        /// <summary>
        /// 챕터 UI 업데이트
        /// </summary>
        private void UpdateChapterUI()
        {
            var chapter = CurrentChapter;
            if (chapter != null)
            {
                CurrentChapterName = Loc.Get(chapter.chapterName);
                CurrentChapterDescription = chapter.chapterDescription;
                CurrentChapterThumbnail = chapter.chapterThumbnail;
            }

            UpdateButtonStates();
            UpdateStageLabel();
            UpdateChapterLockState();
            UpdateChapterProgress();
        }

        /// <summary>
        /// 이전/다음 버튼 활성화 상태 업데이트
        /// </summary>
        private void UpdateButtonStates()
        {
            IsPrevButtonActive = _currentChapterIndex > 0;
            IsNextButtonActive = _currentChapterIndex < chapters.Count - 1;
        }

        /// <summary>
        /// 현재 선택된 챕터/스테이지를 표시할 문자열을 갱신합니다. (예: "Stage 1-1")
        /// </summary>
        private void UpdateStageLabel()
        {
            var chapter = CurrentChapter;
            if (chapter != null && GetCurrentChapterStageCount() > 0)
            {
                CurrentStageLabel = string.Format("Stage {0}-{1}", _currentChapterIndex + 1, _currentStageIndex + 1);
                LockDescString = string.Format("Stage {0} 클리어를 해야합니다", _currentChapterIndex + 1);
            }
            else
            {
                CurrentStageLabel = string.Empty;
                LockDescString = string.Empty;
            }
        }

        /// <summary>
        /// 스테이지 그룹 새로고침 (ChapterSelectGroupView에서 호출할 이벤트)
        /// </summary>
        private void RefreshStageGroup()
        {
            // ChapterSelectGroupView가 이 이벤트를 구독하여 스테이지 목록을 갱신하도록 함
            var groupView = GetComponentInChildren<ChapterSelectGroupView>();
            if (groupView != null)
            {
                groupView.RefreshStages();
            }
        }

        /// <summary>
        /// 현재 챕터의 스테이지 개수 반환
        /// </summary>
        public int GetCurrentChapterStageCount()
        {
            var chapter = CurrentChapter;
            return chapter != null ? chapter.stages.Count : 0;
        }

        /// <summary>
        /// 현재 챕터의 특정 인덱스에 해당하는 씬 이름 반환
        /// </summary>
        public string GetStageSceneName(int stageIndex)
        {
            var chapter = CurrentChapter;
            if (chapter != null && stageIndex >= 0 && stageIndex < chapter.stages.Count)
                return chapter.stages[stageIndex].sceneName;
            return null;
        }
        
        /// <summary>
        /// 첫 번째 챕터의 첫 번째 스테이지 씬 이름을 반환합니다.
        /// </summary>
        public string GetFirstStageSceneName()
        {
            if (chapters.Count > 0 && chapters[0].stages.Count > 0)
                return chapters[0].stages[0].sceneName;
            return null;
        }
        
        /// <summary>
        /// 특정 스테이지가 잠겨있는지 확인
        /// </summary>
        /// <param name="stageIndex">확인할 스테이지 인덱스</param>
        /// <returns>잠금 상태 여부</returns>
        public bool IsStageLockedByIndex(int stageIndex)
        {
            var chapter = CurrentChapter;
            if (chapter == null || stageIndex < 0 || stageIndex >= chapter.stages.Count)
                return true;
            
            var stage = chapter.stages[stageIndex];
            return IsStageLockedInternal(stage, stageIndex);
        }
        
        /// <summary>
        /// StageInfo를 받아 잠금 상태 확인 (내부용)
        /// </summary>
        private bool IsStageLockedInternal(StageInfo stage, int stageIndex)
        {
            var chapter = CurrentChapter;
            if (chapter == null) return true;
            
            // 첫 번째 챕터의 첫 번째 스테이지만 항상 해금
            if (_currentChapterIndex == 0 && stageIndex == 0)
                return false;
            
            // unlockSceneNames가 지정되어 있는 경우
            if (stage.unlockSceneNames != null && stage.unlockSceneNames.Count > 0)
            {
                // 모든 unlockSceneNames의 스테이지가 클리어되어야 함
                foreach (var unlockSceneName in stage.unlockSceneNames)
                {
                    if (!Global.UserDataManager.IsStageClear(unlockSceneName))
                        return true; // 하나라도 클리어 안됐으면 잠금
                }
                return false; // 모두 클리어됐으면 해금
            }
            
            // unlockSceneNames가 없는 경우: 이전 스테이지 클리어 여부 확인
            if (stageIndex > 0 && stageIndex < chapter.stages.Count)
            {
                var prevStage = chapter.stages[stageIndex - 1];
                return !Global.UserDataManager.IsStageClear(prevStage.sceneName);
            }

            // stageIndex == 0인 경우: 이전 스테이지가 없으므로
            // 현재(해당 챕터의 0번째) 스테이지가 이미 클리어됐는지 확인합니다.
            // 클리어되어 있지 않으면 잠금(true), 클리어되어 있으면 해금(false).
            if (stageIndex == 0)
            {
                return !Global.UserDataManager.IsStageClear(stage.sceneName);
            }

            return false;
        }

        /// <summary>
        /// 현재 챕터의 잠금 상태를 업데이트합니다
        /// </summary>
        private void UpdateChapterLockState()
        {
            var chapter = CurrentChapter;
            if (chapter == null)
            {
                IsCurrentChapterLocked = true;
                return;
            }

            // 첫 번째 챕터는 항상 해금
            if (_currentChapterIndex == 0)
            {
                IsCurrentChapterLocked = false;
                return;
            }

            // 이전 챕터의 모든 스테이지가 클리어되어야 현재 챕터 해금
            if (_currentChapterIndex > 0 && _currentChapterIndex < chapters.Count)
            {
                var prevChapter = chapters[_currentChapterIndex - 1];
                foreach (var stage in prevChapter.stages)
                {
                    if (!Global.UserDataManager.IsStageClear(stage.sceneName))
                    {
                        IsCurrentChapterLocked = true;
                        return;
                    }
                }
            }

            IsCurrentChapterLocked = false;
        }

        /// <summary>
        /// 현재 챕터의 진행도를 업데이트합니다 (0~1 범위)
        /// </summary>
        private void UpdateChapterProgress()
        {
            var chapter = CurrentChapter;
            if (chapter == null || chapter.stages.Count == 0)
            {
                CurrentChapterProgress = 0f;
                return;
            }

            int clearedCount = 0;
            foreach (var stage in chapter.stages)
            {
                if (Global.UserDataManager.IsStageClear(stage.sceneName))
                {
                    clearedCount++;
                }
            }

            CurrentChapterProgress = 1f - ((float)clearedCount / chapter.stages.Count);
        }

        [Button("챕터 자동 세팅")]
        public void AutoSetChapters()
        {
#if UNITY_EDITOR
            chapters.Clear();

            // Path to Assets/01.Scene
            var scenesRoot = System.IO.Path.Combine(Application.dataPath, "01.Scene");
            if (!System.IO.Directory.Exists(scenesRoot))
            {
                Debug.LogWarning($"AutoSetChapters: directory not found: {scenesRoot}");
                return;
            }

            // Find immediate subdirectories that start with "Stage" (Stage1, Stage2...)
            var dirs = System.IO.Directory.GetDirectories(scenesRoot);
            var chapterList = new List<ChapterInfo>();

            foreach (var dir in dirs)
            {
                var folderName = System.IO.Path.GetFileName(dir);
                if (string.IsNullOrEmpty(folderName))
                    continue;

                if (!folderName.StartsWith("Stage", System.StringComparison.OrdinalIgnoreCase))
                    continue;

                // Collect scene files in this folder. Support both .scene and Unity .unity just in case.
                var sceneFiles = new List<string>();
                try
                {
                    sceneFiles.AddRange(System.IO.Directory.GetFiles(dir, "*.scene", System.IO.SearchOption.TopDirectoryOnly));
                    sceneFiles.AddRange(System.IO.Directory.GetFiles(dir, "*.unity", System.IO.SearchOption.TopDirectoryOnly));
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"AutoSetChapters: could not enumerate files in {dir}: {ex.Message}");
                }

                if (sceneFiles.Count == 0)
                    continue; // skip empty stage folders

                var chapter = new ChapterInfo();

                // Try to parse numeric index from "StageN". If fails, use current count.
                var idx = 0;
                if (folderName.Length > 5)
                {
                    var numPart = folderName.Substring(5);
                    if (!int.TryParse(numPart, out idx))
                        idx = chapterList.Count;
                }
                else
                {
                    idx = chapterList.Count;
                }

                chapter.chapterIndex = idx;
                chapter.chapterName = $"SceneName/{folderName}";
                chapter.chapterDescription = string.Empty;
                chapter.stages = new List<StageInfo>();

                // Try to load thumbnail sprite named IMG_Stage{idx} from SelectStage folder
                Sprite thumb = null;
                try
                {
                    var expectedName = $"IMG_Stage{idx}";
                    var expectedPath = $"Assets/03.NormalResource/Sprite/UI/SelectStage/{expectedName}.png";
                    thumb = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(expectedPath);
                    if (thumb == null)
                    {
                        var guids = UnityEditor.AssetDatabase.FindAssets(expectedName);
                        foreach (var g in guids)
                        {
                            var p = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                            if (p.EndsWith(".png") && p.Contains("SelectStage"))
                            {
                                thumb = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(p);
                                if (thumb != null) break;
                            }
                        }
                    }
                }
                catch (System.Exception)
                {
                    thumb = null;
                }

                chapter.chapterThumbnail = thumb;

                foreach (var file in sceneFiles)
                {
                    var name = System.IO.Path.GetFileNameWithoutExtension(file);
                    if (!string.IsNullOrEmpty(name))
                    {
                        bool alreadyExists = false;
                        foreach (var s in chapter.stages)
                        {
                            if (s.sceneName == name)
                            {
                                alreadyExists = true;
                                break;
                            }
                        }
                        if (!alreadyExists)
                        {
                            chapter.stages.Add(new StageInfo { sceneName = name });
                        }
                    }
                }

                chapterList.Add(chapter);
            }

            // Sort chapters by chapterIndex to keep Stage1, Stage2 order (fallback to name order)
            chapterList.Sort((a, b) => a.chapterIndex.CompareTo(b.chapterIndex));

            chapters.AddRange(chapterList);

            // Mark object dirty so Unity serializes changes in editor
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();

            // Update UI values if running in editor play mode or inspecting
            if (chapters.Count > 0)
            {
                _currentChapterIndex = 0;
                _currentStageIndex = 0;
                UpdateChapterUI();
                RefreshStageGroup();
            }
#else
            Debug.LogWarning("AutoSetChapters can only be run in the Unity Editor.");
#endif
        }
    }
}
