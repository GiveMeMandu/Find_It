using System.Collections.Generic;
using UnityEngine;
using UnityWeld.Binding;
using Sirenix.OdinInspector;
using Manager;
using UI;
using I2.Loc;

namespace UnityWeld
{
    /// <summary>
    /// 챕터 선택 화면의 UI 바인딩을 담당하는 뷰입니다.
    /// 실제 챕터/스테이지 데이터와 로직은 StageManager에 위임합니다.
    /// </summary>
    [Binding]
    public class ChapterSelectView : ViewModel
    {
        private string _currentChapterName;
        private string _currentChapterDescription;
        private Sprite _currentChapterThumbnail;
        private string _currentStageLabel;
        private string _lockDescString;
        private bool _isPrevButtonActive = false;
        private bool _isNextButtonActive = false;
        private bool _isCurrentChapterLocked = false;
        private float _currentChapterProgress = 0f;

        private StageManager StageManager => Global.StageManager;

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

        private void Start()
        {
            if (StageManager != null)
            {
                StageManager.OnChapterDataChanged += UpdateChapterUI;
                UpdateChapterUI();
            }
            else
            {
                Debug.LogError("StageManager를 찾을 수 없습니다!");
            }
        }

        private void OnDestroy()
        {
            if (StageManager != null)
            {
                StageManager.OnChapterDataChanged -= UpdateChapterUI;
            }
        }

        /// <summary>
        /// 다음 챕터로 이동
        /// </summary>
        [Binding]
        public void NextChapter()
        {
            StageManager?.NextChapter();
        }

        /// <summary>
        /// 이전 챕터로 이동
        /// </summary>
        [Binding]
        public void PrevChapter()
        {
            StageManager?.PrevChapter();
        }

        /// <summary>
        /// 게임 시작 버튼 클릭 시 호출
        /// </summary>
        [Binding]
        public void OnClickStartButton()
        {
            if (StageManager == null) return;

            if (StageManager.IsStageLockedByIndex(StageManager.CurrentStageIndex))
            {
                Debug.LogWarning("이 스테이지는 잠겨있습니다.");
                return;
            }

            string sceneName = StageManager.CurrentStageSceneName;
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
        /// StageManager의 상태를 기반으로 챕터 UI를 업데이트합니다
        /// </summary>
        private void UpdateChapterUI()
        {
            if (StageManager == null) return;

            var chapter = StageManager.CurrentChapter;
            if (chapter != null)
            {
                CurrentChapterName = Loc.Get(chapter.chapterName);
                CurrentChapterDescription = chapter.chapterDescription;
                CurrentChapterThumbnail = chapter.chapterThumbnail;
            }

            IsPrevButtonActive = StageManager.IsPrevAvailable;
            IsNextButtonActive = StageManager.IsNextAvailable;
            CurrentStageLabel = StageManager.GetStageLabel();
            LockDescString = StageManager.GetLockDescription();
            IsCurrentChapterLocked = StageManager.IsCurrentChapterLocked;
            CurrentChapterProgress = StageManager.CurrentChapterProgress;

            RefreshStageGroup();
        }

        /// <summary>
        /// 스테이지 그룹 새로고침
        /// </summary>
        private void RefreshStageGroup()
        {
            var groupView = GetComponentInChildren<ChapterSelectGroupView>();
            if (groupView != null)
            {
                groupView.RefreshStages();
            }
        }
    }
}
