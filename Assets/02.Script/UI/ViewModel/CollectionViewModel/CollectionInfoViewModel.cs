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
        [Binding]
        public void OnClickPlaceSticker()
        {
            if (_currentCollection != null)
            {
                var diaryViewModel = FindFirstObjectByType<CollectionDiaryViewModel>(UnityEngine.FindObjectsInactive.Include);
                if(_collectionPage != null)
                {
                    _collectionPage.tabGroup.SelectTab(0);
                }
                if (diaryViewModel != null)
                {
                    diaryViewModel.PlaceNewSticker(_currentCollection);
                    // Refresh count text
                    CollectionCount = Global.CollectionManager.GetCollectionCount(_currentCollection).ToString();
                }
                else
                {
                    Debug.LogWarning("CollectionDiaryViewModel not found in scene!");
                }
            }
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
