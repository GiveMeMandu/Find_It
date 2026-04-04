using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using SO;
using I2.Loc;
using UnityEngine;
using UnityWeld;
using UnityWeld.Binding;
using UnityEngine.UI;
using UI.Page;

namespace UI
{
    [Binding]
    public class CollectionInfoViewModel : ViewModel
    {
        public Image collectSelectedImage;
        private CollectionSO _currentCollection;
        private CollectionPage _collectionPage;
        private bool _isBatchMode;
        [Binding]
        public bool IsBatchMode
        {
            get => _isBatchMode;
            set
            {
                _isBatchMode = value;
                OnPropertyChanged(nameof(IsBatchMode));
            }
        }

        [Binding]
        public void ToggleBatchMode()
        {
            IsBatchMode = !IsBatchMode;
        }

        private bool _isPlaced;
        [Binding]
        public bool IsPlaced
        {
            get => _isPlaced;
            set
            {
                _isPlaced = value;
                OnPropertyChanged(nameof(IsPlaced));
            }
        }

        [Binding]
        public void OnClickPlaceSticker()
        {
            if (_currentCollection != null)
            {
                var diaryViewModel = FindFirstObjectByType<CollectionDiaryViewModel>(UnityEngine.FindObjectsInactive.Include);
                
                // 일괄 선택 모드가 아닐 때만 탭 이동
                if (!_isBatchMode && _collectionPage != null)
                {
                    _collectionPage.tabGroup.SelectTab(0);
                }

                if (diaryViewModel != null)
                {
                    diaryViewModel.PlaceNewSticker(_currentCollection);
                    // Refresh count text
                    CollectionCount = Global.CollectionManager.GetCollectionCount(_currentCollection).ToString();
                    UpdateIsPlacedStatus();
                }
                else
                {
                    Debug.LogWarning("CollectionDiaryViewModel not found in scene!");
                }
            }
        }

        public void UpdateIsPlacedStatus()
        {
            if (_currentCollection == null) return;
            var placedStickers = Global.UserDataManager.GetAllPlacedStickers();
            bool found = false;
            if (placedStickers != null)
            {
                foreach (var data in placedStickers)
                {
                    if (data.collectionKey == _currentCollection.name)
                    {
                        found = true;
                        break;
                    }
                }
            }
            IsPlaced = found;
        }

        private string _collectionName;
        [Binding]
        public string CollectionName
        {
            get => _collectionName;
            set
            {
                _collectionName = value;
                OnPropertyChanged(nameof(CollectionName));
            }
        }

        private string _collectionCount;
        [Binding]
        public string CollectionCount
        {
            get => _collectionCount;
            set
            {
                _collectionCount = value;
                OnPropertyChanged(nameof(CollectionCount));
            }
        }

        private string _collectionScene;
        [Binding]
        public string CollectionScene
        {
            get => _collectionScene;
            set
            {
                _collectionScene = value;
                OnPropertyChanged(nameof(CollectionScene));
            }
        }
        private Sprite _collectionImage;
        [Binding]
        public Sprite CollectionImage
        {
            get => _collectionImage;
            set
            {
                _collectionImage = value;
                OnPropertyChanged(nameof(CollectionImage));
            }
        }

        private Vector2 _collectionImageSize;
        [Binding]
        public Vector2 CollectionImageSize
        {
            get => _collectionImageSize;
            set
            {
                _collectionImageSize = value;
                OnPropertyChanged(nameof(CollectionImageSize));
            }
        }
        
        [Binding]
        public void Hide()
        {
            transform.gameObject.SetActive(false);
        }
        public void Show(CollectionSO collection)
        {
            _collectionPage = FindFirstObjectByType<CollectionPage>(UnityEngine.FindObjectsInactive.Include);
            _currentCollection = collection;
            CollectionName = LocalizationManager.GetTranslation(collection.collectionName);
            CollectionCount = Global.CollectionManager.GetCollectionCount(collection).ToString();
            CollectionImage = collection.collectionImage;
            if (collectSelectedImage != null && CollectionImage != null)
            {

                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(collectSelectedImage.rectTransform);

                var spriteRect = CollectionImage.rect;
                float spriteW = spriteRect.width;
                float spriteH = spriteRect.height;

                float containerW = collectSelectedImage.rectTransform.rect.width;
                float containerH = collectSelectedImage.rectTransform.rect.height;

                if (spriteW > 0 && spriteH > 0 && containerW > 0 && containerH > 0)
                {
                    float scale = Mathf.Min(containerW / spriteW, containerH / spriteH);
                    CollectionImageSize = new Vector2(spriteW * scale, spriteH * scale);
                }
            }
            // 챕터 인덱스를 이용하여 StageManager에서 챕터 이름 가져오기
            var stageManager = Global.StageManager;
            if (stageManager != null && collection.chapterIndex >= 0 && collection.chapterIndex < stageManager.ChapterCount)
            {
                string chapterName = stageManager.GetChapterName(collection.chapterIndex);
                CollectionScene = !string.IsNullOrEmpty(chapterName) 
                    ? LocalizationManager.GetTranslation(chapterName) 
                    : $"Chapter {collection.chapterIndex + 1}";
            }
            else
            {
                CollectionScene = $"Chapter {collection.chapterIndex + 1}";
            }
        }
    }
}
