using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
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

        [LabelText("모델 생성 부모")]
        [SerializeField] private Transform modelParent;
        private List<ViewModel> _pool = new();

        private bool IsInitial = false;
        protected override void Awake()
        {
            base.Awake();
            if (baseViewModel == null) return;
            if (modelParent == null) modelParent = transform;
            baseViewModel.gameObject.SetActive(false);
            IsInitial = true;
        }

        public void PrepareViewModels(int count)
        {
            if(!IsInitial) Awake();
            if (_pool.Count < count)
            {
                for (int i = _pool.Count; i < count; i++)
                {
                    var viewModel = Instantiate(baseViewModel, modelParent);

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
