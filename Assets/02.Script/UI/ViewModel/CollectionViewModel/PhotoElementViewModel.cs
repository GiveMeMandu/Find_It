using System.Collections;
using System.Collections.Generic;
using UnityWeld.Binding;
using UnityWeld;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    [Binding]
    public class PhotoElementViewModel : ViewModel
    {
        public UnityWeld.CollectionBookScrollViewModel CollectionBookScrollView;
        public Image IconImage;
        public UnityEvent OnSelected = new UnityEvent();
        public GameObject selectedObj;
        
        private Sprite _photoSprite;
        private string _photoName;

        protected override void Awake() 
        {
            base.Awake();
        }

        [Binding]
        public void OnClickShowPhotoInfo()
        {
            OnSelected.Invoke();
            CollectionBookScrollView.SelectElement(this);
            CollectionBookScrollView.OnClickShowPhotoInfo(_photoSprite, _photoName);
        }

        public void Init(
            UnityWeld.CollectionBookScrollViewModel scrollView, 
            Sprite sprite, 
            string photoName
        )
        {
            if (selectedObj != null) selectedObj.SetActive(false);
            CollectionBookScrollView = scrollView;
            _photoSprite = sprite;
            _photoName = photoName;
            
            Image = sprite;
            Name = photoName;

            if (IconImage != null && Image != null)
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(IconImage.rectTransform);

                var spriteRect = Image.rect;
                float spriteW = spriteRect.width;
                float spriteH = spriteRect.height;

                float containerW = IconImage.rectTransform.rect.width;
                float containerH = IconImage.rectTransform.rect.height;

                if (spriteW > 0 && spriteH > 0 && containerW > 0 && containerH > 0)
                {
                    float scale = Mathf.Min(containerW / spriteW, containerH / spriteH);
                    ImageSize = new Vector2(spriteW * scale, spriteH * scale);
                }
            }
        }

        private Sprite _image;
        [Binding]
        public Sprite Image
        {
            get => _image;
            set
            {
                _image = value;
                OnPropertyChanged(nameof(Image));
            }
        }

        private Vector2 _imageSize;
        [Binding]
        public Vector2 ImageSize
        {
            get => _imageSize;
            set
            {
                _imageSize = value;
                OnPropertyChanged(nameof(ImageSize));
            }
        }

        private string _name;
        [Binding]
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }
}