using System;
using System.Collections.Generic;
using I2.Loc;
using OutGame;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Manager
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

    /// <summary>
    /// 챕터/스테이지 데이터와 로직을 관리하는 매니저입니다.
    /// Global.StageManager로 접근할 수 있습니다.
    /// </summary>
    public class StageManager : MonoBehaviour
    {
        [SerializeField]
        [LabelText("챕터 목록")]
        private List<ChapterInfo> chapters = new List<ChapterInfo>();

        private int _currentChapterIndex = 0;
        private int _currentStageIndex = 0;
        private bool _isCurrentChapterLocked = false;
        private float _currentChapterProgress = 0f;

        /// <summary>
        /// 챕터 또는 스테이지 상태가 변경될 때 발생합니다.
        /// </summary>
        public event Action OnChapterDataChanged;

        /// <summary>
        /// 현재 활성화된 씬의 SceneBase
        /// </summary>
        public SceneBase CurrentScene { get; set; }

        #region Properties

        public int CurrentChapterIndex => _currentChapterIndex;
        public int CurrentStageIndex => _currentStageIndex;
        public int ChapterCount => chapters.Count;
        public bool IsCurrentChapterLocked => _isCurrentChapterLocked;
        public float CurrentChapterProgress => _currentChapterProgress;
        public bool IsPrevAvailable => _currentChapterIndex > 0;
        public bool IsNextAvailable => _currentChapterIndex < chapters.Count - 1;

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

        #endregion

        #region Public Methods

        /// <summary>
        /// 초기화합니다. Global에서 로드 후 호출됩니다.
        /// </summary>
        public void Initialize()
        {
            if (chapters.Count > 0)
            {
                _currentChapterIndex = 0;
                _currentStageIndex = 0;
                UpdateChapterState();
            }
        }

        /// <summary>
        /// 다음 챕터로 이동
        /// </summary>
        public void NextChapter()
        {
            if (_currentChapterIndex < chapters.Count - 1)
            {
                _currentChapterIndex++;
                _currentStageIndex = 0;
                UpdateChapterState();
                OnChapterDataChanged?.Invoke();
            }
        }

        /// <summary>
        /// 이전 챕터로 이동
        /// </summary>
        public void PrevChapter()
        {
            if (_currentChapterIndex > 0)
            {
                _currentChapterIndex--;
                _currentStageIndex = 0;
                UpdateChapterState();
                OnChapterDataChanged?.Invoke();
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
        /// 모든 챕터의 모든 스테이지 씬 이름을 반환합니다.
        /// </summary>
        public List<string> GetAllStageSceneNames()
        {
            List<string> allScenes = new List<string>();
            foreach (var chapter in chapters)
            {
                foreach (var stage in chapter.stages)
                {
                    if (!string.IsNullOrEmpty(stage.sceneName))
                    {
                        allScenes.Add(stage.sceneName);
                    }
                }
            }
            return allScenes;
        }

        /// <summary>
        /// 특정 챕터 인덱스의 챕터 이름(로컬라이즈 키)을 반환합니다.
        /// </summary>
        /// <param name="chapterIndex">챕터 인덱스</param>
        /// <returns>챕터 이름 (localization key), 없으면 null</returns>
        public string GetChapterName(int chapterIndex)
        {
            if (chapterIndex >= 0 && chapterIndex < chapters.Count)
                return chapters[chapterIndex].chapterName;
            return null;
        }

        /// <summary>
        /// 특정 챕터 인덱스의 ChapterInfo를 반환합니다.
        /// </summary>
        /// <param name="chapterIndex">챕터 인덱스</param>
        /// <returns>ChapterInfo, 없으면 null</returns>
        public ChapterInfo GetChapterInfo(int chapterIndex)
        {
            if (chapterIndex >= 0 && chapterIndex < chapters.Count)
                return chapters[chapterIndex];
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
        /// 현재 스테이지 레이블을 반환합니다. (예: "Stage 1-1")
        /// </summary>
        public string GetStageLabel()
        {
            var chapter = CurrentChapter;
            if (chapter != null && GetCurrentChapterStageCount() > 0)
                return string.Format("Stage {0}-{1}", _currentChapterIndex + 1, _currentStageIndex + 1);
            return string.Empty;
        }

        /// <summary>
        /// 잠금 설명 문자열을 반환합니다.
        /// </summary>
        public string GetLockDescription()
        {
            var chapter = CurrentChapter;
            if (chapter != null && GetCurrentChapterStageCount() > 0)
                return string.Format("Stage {0} 클리어를 해야합니다", _currentChapterIndex + 1);
            return string.Empty;
        }

        #endregion

        #region Internal Logic

        /// <summary>
        /// 챕터 상태를 갱신합니다 (잠금 상태, 진행도 등)
        /// </summary>
        private void UpdateChapterState()
        {
            UpdateChapterLockState();
            UpdateChapterProgress();
        }

        /// <summary>
        /// StageInfo를 받아 잠금 상태 확인 (내부용)
        /// </summary>
        private bool IsStageLockedInternal(StageInfo stage, int stageIndex)
        {
            var chapter = CurrentChapter;
            if (chapter == null) return true;

            // 이미 클리어한 스테이지는 무조건 해금
            if (Global.UserDataManager.IsStageClear(stage.sceneName))
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
            if (stageIndex > 0 && stageIndex <= chapter.stages.Count)
            {
                var prevStage = chapter.stages[stageIndex - 1];
                return !Global.UserDataManager.IsStageClear(prevStage.sceneName);
            }
            if (stageIndex == 0 && _currentChapterIndex > 0)
            {
                return _isCurrentChapterLocked;
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
                _isCurrentChapterLocked = true;
                return;
            }

            // 첫 번째 챕터는 항상 해금
            if (_currentChapterIndex == 0)
            {
                _isCurrentChapterLocked = false;
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
                        _isCurrentChapterLocked = true;
                        return;
                    }
                }
            }

            _isCurrentChapterLocked = false;
        }

        /// <summary>
        /// 현재 챕터의 진행도를 업데이트합니다 (0~1 범위)
        /// </summary>
        private void UpdateChapterProgress()
        {
            var chapter = CurrentChapter;
            if (chapter == null || chapter.stages.Count == 0)
            {
                _currentChapterProgress = 0f;
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

            _currentChapterProgress = 1f - ((float)clearedCount / chapter.stages.Count);
        }

        #endregion

        #region Editor

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

                // Collect scene files in this folder
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
                    continue;

                var chapter = new ChapterInfo();

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

                // Try to load thumbnail sprite
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

            // Sort chapters by chapterIndex
            chapterList.Sort((a, b) => a.chapterIndex.CompareTo(b.chapterIndex));

            chapters.AddRange(chapterList);

            // Mark object dirty so Unity serializes changes
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();

            if (chapters.Count > 0)
            {
                _currentChapterIndex = 0;
                _currentStageIndex = 0;
                Debug.Log($"AutoSetChapters: {chapters.Count}개의 챕터를 자동 설정했습니다.");
            }
#else
            Debug.LogWarning("AutoSetChapters can only be run in the Unity Editor.");
#endif
        }

        #endregion
    }
}
