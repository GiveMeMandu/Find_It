using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;
using UnityWeld;
using UnityWeld.Binding;
using UnityEngine.UI;

namespace UI
{
    [Binding]
    public class PhotoInfoViewModel : ViewModel
    {
        public Image collectSelectedImage;
        
        private string _photoName;
        [Binding]
        public string PhotoName
        {
            get => _photoName;
            set
            {
                _photoName = value;
                OnPropertyChanged(nameof(PhotoName));
            }
        }

        private Sprite _photoImage;
        [Binding]
        public Sprite PhotoImage
        {
            get => _photoImage;
            set
            {
                _photoImage = value;
                OnPropertyChanged(nameof(PhotoImage));
            }
        }

        private Vector2 _photoImageSize;
        [Binding]
        public Vector2 PhotoImageSize
        {
            get => _photoImageSize;
            set
            {
                _photoImageSize = value;
                OnPropertyChanged(nameof(PhotoImageSize));
            }
        }
        
        [Binding]
        public void Hide()
        {
            transform.gameObject.SetActive(false);
        }
        
        private string _stickerButtonText;
        [Binding]
        public string StickerButtonText
        {
            get => _stickerButtonText;
            set
            {
                _stickerButtonText = value;
                OnPropertyChanged(nameof(StickerButtonText));
            }
        }

        private bool _isAllPlaced;
        [Binding]
        public bool IsAllPlaced
        {
            get => _isAllPlaced;
            set
            {
                _isAllPlaced = value;
                OnPropertyChanged(nameof(IsAllPlaced));
            }
        }
        
        
        public void Show(Sprite photoSprite, string photoName)
        {
            PhotoName = photoName;
            PhotoImage = photoSprite;
            
            UpdatePlacedStatus();

            if (collectSelectedImage != null && PhotoImage != null)
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(collectSelectedImage.rectTransform);

                var spriteRect = PhotoImage.rect;
                float spriteW = spriteRect.width;
                float spriteH = spriteRect.height;

                float containerW = collectSelectedImage.rectTransform.rect.width;
                float containerH = collectSelectedImage.rectTransform.rect.height;

                if (spriteW > 0 && spriteH > 0 && containerW > 0 && containerH > 0)
                {
                    float scale = Mathf.Min(containerW / spriteW, containerH / spriteH);
                    PhotoImageSize = new Vector2(spriteW * scale, spriteH * scale);
                }
            }
        }

        public void UpdatePlacedStatus()
        {
            if (string.IsNullOrEmpty(PhotoName)) return;

            var placedStickers = Global.UserDataManager.GetAllPlacedStickers();
            bool isPlaced = false;

            if (placedStickers != null)
            {
                foreach (var data in placedStickers)
                {
                    if (data.collectionKey == PhotoName)
                    {
                        isPlaced = true;
                        break;
                    }
                }
            }

            IsAllPlaced = isPlaced;

            if (IsAllPlaced)
            {
                StickerButtonText = I2.Loc.LocalizationManager.GetTranslation("UI/Sticker/Already_Used");
            }
            else
            {
                StickerButtonText = I2.Loc.LocalizationManager.GetTranslation("UI/Sticker/Use_Sticker");
            }
        }

        [Binding]
        public void OnClickPlacePhoto()
        {
            if (string.IsNullOrEmpty(PhotoName) || IsAllPlaced)
            {
                return;
            }

            var diaryViewModel = FindFirstObjectByType<CollectionDiaryViewModel>(UnityEngine.FindObjectsInactive.Include);
            var collectionPage = FindFirstObjectByType<UI.Page.CollectionPage>(UnityEngine.FindObjectsInactive.Include);

            if (collectionPage != null)
            {
                collectionPage.tabGroup.SelectTab(0);
            }

            if (diaryViewModel != null)
            {
                diaryViewModel.PlaceNewPhotoSticker(PhotoName, PhotoImage);
                UpdatePlacedStatus();
            }
            else
            {
                Debug.LogWarning("CollectionDiaryViewModel not found in scene!");
            }
        }
    }
}