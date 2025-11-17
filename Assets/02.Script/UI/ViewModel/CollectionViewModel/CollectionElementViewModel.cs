using System.Collections;
using System.Collections.Generic;
using UnityWeld.Binding;
using UnityWeld;
using UnityEngine.UI;
using UnityEngine;
using SO;
using I2.Loc;
using Manager;
namespace UI
{
    [Binding]
    public class CollectionElementViewModel : ViewModel
    {
        public CollectionScrollViewModel CollectionScrollView;
        private CollectionSO _collection;
        protected override void Awake() {
            base.Awake();
        }

        [Binding]
        public void OnClickShowCollectionInfo()
        {
            CollectionScrollView.OnClickShowCollectionInfo(_collection);
        }

        public void Init(
            CollectionScrollViewModel collectionScrollView,
            CollectionSO collection
        )
        {
            CollectionScrollView = collectionScrollView;
            _collection = collection;
            Image = collection.collectionImage;
            Name = LocalizationManager.GetTranslation(collection.collectionName);
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
                OnPropertyChanged(nameof(Count));
            }
        }
    }
}