using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Manager;
using UI;
using UnityEngine;
using UnityWeld.Binding;
using SO;

namespace UnityWeld
{
    [Binding]
    public class CollectionScrollViewModel : ScrollView
    {
        [SerializeField]
        private CollectionInfoViewModel _collectionInfoViewModel;

        public void OnClickShowCollectionInfo(CollectionSO collection)
        {
            _collectionInfoViewModel.transform.gameObject.SetActive(true);
            _collectionInfoViewModel.Show(collection);
        }

        private void OnEnable() {
            var managers = UnityEngine.Resources.FindObjectsOfTypeAll<Manager.CollectionManager>();
            var manager = (managers != null && managers.Length > 0) ? managers[0] : null;
            var allCollections = manager != null ? manager.GetAllCollections() : new List<CollectionSO>();
            PrepareViewModels(allCollections.Count);
            var viewModels = GetViewModels();
            for(int i = 0; i < viewModels.Count; i++)
            {
                var viewModel = viewModels[i] as CollectionElementViewModel;
                viewModel.Init(this, allCollections[i]);
            }
        }
    }
}
