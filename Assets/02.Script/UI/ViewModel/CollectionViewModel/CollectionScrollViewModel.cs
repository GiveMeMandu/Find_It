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

        private CollectionElementViewModel _selectedElement;

        public void SelectElement(CollectionElementViewModel element)
        {
            if (_selectedElement != null && _selectedElement.selectedObj != null)
            {
                _selectedElement.selectedObj.SetActive(false);
            }
            _selectedElement = element;
            if (_selectedElement != null && _selectedElement.selectedObj != null)
            {
                _selectedElement.selectedObj.SetActive(true);
            }
        }

        public void OnClickShowCollectionInfo(CollectionSO collection)
        {
            _collectionInfoViewModel.transform.gameObject.SetActive(true);
            _collectionInfoViewModel.Show(collection);
        }

        private void OnEnable() {
            var allCollections = Global.CollectionManager != null ? Global.CollectionManager.GetAllCollections() : new List<CollectionSO>();
            var ownedCollections = allCollections.FindAll(c => Global.CollectionManager.GetCollectionCount(c) > 0);
            
            PrepareViewModels(allCollections.Count);
            var viewModels = GetViewModels();
            for(int i = 0; i < viewModels.Count; i++)
            {
                var viewModel = viewModels[i] as CollectionElementViewModel;
                viewModel.Init(this, allCollections[i]);
                if(i == 0)
                {
                    SelectElement(viewModel);
                    OnClickShowCollectionInfo(allCollections[i]);
                }
            }

            if (allCollections.Count == 0)
            {
                if (_collectionInfoViewModel != null && _collectionInfoViewModel.transform != null)
                {
                    _collectionInfoViewModel.transform.gameObject.SetActive(false);
                }
            }
        }
    }
}
