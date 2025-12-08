using UnityEngine;
using UnityWeld.Binding;
using UI;
using Sirenix.OdinInspector;

namespace UnityWeld
{
    [Binding]
    public class MapSelectElementView : ViewModel
    {
        [SerializeField]
        [LabelText("스테이지 인덱스")]
        [ReadOnly]
        private int stageIndex;

        [SerializeField]
        [LabelText("씬 이름")]
        [ReadOnly]
        private string sceneName;

        [SerializeField]
        [LabelText("스테이지 썸네일")]
        private Sprite stageThumbnail;

        [SerializeField]
        [LabelText("스테이지 이름 표시")]
        private string stageDisplayName;

        private bool _isSelected;
        private bool _isLocked;
        private ChapterSelectView _parentView;
        private ChapterSelectGroupView _groupView;

        private string _displayName;
        private Sprite _thumbnail;

        [Binding]
        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        [Binding]
        public Sprite Thumbnail
        {
            get => _thumbnail;
            set
            {
                _thumbnail = value;
                OnPropertyChanged(nameof(Thumbnail));
            }
        }

        [Binding]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
        
        [Binding]
        public bool IsLocked
        {
            get => _isLocked;
            set
            {
                _isLocked = value;
                OnPropertyChanged(nameof(IsLocked));
            }
        }

        /// <summary>
        /// 스테이지 정보를 초기화합니다
        /// </summary>
        /// <param name="index">스테이지 인덱스</param>
        /// <param name="scene">씬 이름</param>
        /// <param name="parentView">부모 ChapterSelectView</param>
        /// <param name="isLocked">잠금 상태</param>
        public void Initialize(int index, string scene, ChapterSelectView parentView, bool isLocked = false)
        {
            stageIndex = index;
            sceneName = scene;
            _parentView = parentView;
            _groupView = GetComponentInParent<ChapterSelectGroupView>();
            IsLocked = isLocked;

            DisplayName = string.IsNullOrEmpty(stageDisplayName) 
                ? $"{index + 1}" 
                : stageDisplayName;
            Thumbnail = stageThumbnail;
            IsSelected = false;
        }

        /// <summary>
        /// 스테이지 선택 시 호출
        /// </summary>
        [Binding]
        public void OnClickStage()
        {
            if (IsLocked)
            {
                Debug.LogWarning($"잠겨있는 스테이지입니다: {DisplayName}");
                return;
            }
            
            if (_groupView != null)
            {
                _groupView.SelectStage(stageIndex);
                Debug.Log($"Selected Stage: {DisplayName} (Scene: {sceneName})");
            }
            else if (_parentView != null)
            {
                _parentView.SelectStage(stageIndex);
                IsSelected = true;
                Debug.Log($"Selected Stage: {DisplayName} (Scene: {sceneName})");
            }
        }

        /// <summary>
        /// 선택 해제
        /// </summary>
        public void Deselect()
        {
            IsSelected = false;
        }

        /// <summary>
        /// 현재 씬 이름 반환
        /// </summary>
        public string GetSceneName()
        {
            return sceneName;
        }

        /// <summary>
        /// 스테이지 인덱스 반환
        /// </summary>
        public int GetStageIndex()
        {
            return stageIndex;
        }
    }
}
