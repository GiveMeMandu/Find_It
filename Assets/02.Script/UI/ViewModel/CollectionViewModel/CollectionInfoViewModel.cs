using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using SO;
using Data;
using I2.Loc;
using UnityEngine;
using UnityWeld;
using UnityWeld.Binding;


namespace UI
{
    [Binding]
    public class CollectionInfoViewModel : ViewModel
    {
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
        [Binding]
        public void Hide()
        {
            transform.gameObject.SetActive(false);
        }
        public void Show(CollectionSO collection)
        {
            CollectionName = LocalizationManager.GetTranslation(collection.collectionName);
            CollectionCount = Global.CollectionManager.GetCollectionCount(collection).ToString();
            CollectionScene = SceneHelper.GetFormattedStageName(collection.scene);
        }
    }
}
