using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityWeld.Binding;

namespace UnityWeld
{
    [Binding]
    public class GroupView : ViewModel
    {
        private bool _isActive;

        [Binding]
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }
        private int _count = 0;
        public int Count => _count;
        [SerializeField] private ViewModel baseViewModel;

        [SerializeField] private Transform scrollParent;
        private List<ViewModel> _pool = new();

        protected override void Awake()
        {
            base.Awake();
            if (scrollParent == null && baseViewModel != null) scrollParent = baseViewModel.transform.parent;
            if(baseViewModel != null) baseViewModel.gameObject.SetActive(false);
        }

        public void PrepareViewModels(int count)
        {
            if (_pool.Count < count)
            {
                for (int i = _pool.Count; i < count; i++)
                {
                    var viewModel = Instantiate(baseViewModel, scrollParent);
                    _pool.Add(viewModel);
                }
            }
            for (int i = 0; i < count; i++)
            {
                _pool[i].gameObject.SetActive(true);
            }
            for(int i = count; i < _pool.Count; i++)
            {
                _pool[i].gameObject.SetActive(false);
            }
            _count = count;
        }
        
        public ViewModel GetViewModel(int index)
        {
            if(index < 0 || index >= Count)
                throw new IndexOutOfRangeException();
            return _pool[index];
        }
        public List<ViewModel> GetViewModels()
        {
            return _pool.GetRange(0, Count);
        }
    }
}
