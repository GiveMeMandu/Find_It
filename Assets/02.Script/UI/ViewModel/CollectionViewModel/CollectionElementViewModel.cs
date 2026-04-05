using System.Collections;
using System.Collections.Generic;
using UnityWeld.Binding;
using UnityWeld;
using UnityEngine.UI;
using UnityEngine;
using SO;
using I2.Loc;
using Manager;
using UnityEngine.Events;
namespace UI
{
    [Binding]
    public class CollectionElementViewModel : ViewModel
    {
        public CollectionScrollViewModel CollectionScrollView;
        public Image IconImage;
        public UnityEvent OnSelected = new UnityEvent();
        public GameObject selectedObj;
        private CollectionSO _collection;
        protected override void Awake() {
            base.Awake();
        }

        [Binding]
        public void OnClickShowCollectionInfo()
        {
            OnSelected.Invoke();
            CollectionScrollView.SelectElement(this);
            CollectionScrollView.OnClickShowCollectionInfo(_collection);
        }

        public void Init(
            CollectionScrollViewModel collectionScrollView,
            CollectionSO collection
        )
        {
            if (selectedObj != null) selectedObj.SetActive(false);
            CollectionScrollView = collectionScrollView;
            _collection = collection;
            Image = collection.collectionImage;
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

            string nameTerm = string.IsNullOrEmpty(collection.collectionName) ? $"Collection/Name/{collection.name}" : collection.collectionName;
            string translatedName = LocalizationManager.GetTranslation(nameTerm);
            Name = string.IsNullOrEmpty(translatedName) ? collection.name : translatedName;
            
            Count = Global.CollectionManager.GetCollectionCount(collection);
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

        private bool isLocked;
        [Binding]
        public bool IsLocked
        {
            get => isLocked;
            set
            {
                isLocked = value;
                OnPropertyChanged(nameof(IsLocked));
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

        private string _description;
        [Binding]
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        private int _count;
        [Binding]
        public int Count
        {
            get => _count;
            set
            {
                _count = value;
                if (_count > 0)
                {
                    IsLocked = false;
                }
                else
                {
                    IsLocked = true;
                }
                OnPropertyChanged(nameof(Count));
            }
        }
    }
}