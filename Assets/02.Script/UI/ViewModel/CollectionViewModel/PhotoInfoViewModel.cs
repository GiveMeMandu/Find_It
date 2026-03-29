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
        
        public void Show(Sprite photoSprite, string photoName)
        {
            PhotoName = photoName;
            PhotoImage = photoSprite;
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
    }
}