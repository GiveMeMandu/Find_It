using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityWeld.Binding;
using UnityWeld;
using Unity.VisualScripting;
using Manager;
using DeskCat.FindIt.Scripts.Core.Main.System;

namespace UI
{
    [Binding]
    public class MissionElementViewModel : ViewModel
    {
        private string _missionName;
        private string _currentSetName;

        [Binding]
        public string MissionName
        {
            get => _missionName;
            set
            {
                _missionName = value;
                OnPropertyChanged(nameof(MissionName));
            }
        }

        private string _missionNameDivider;

        [Binding]
        public string MissionNameDivider
        {
            get => _missionNameDivider;
            set
            {
                _missionNameDivider = value;
                OnPropertyChanged(nameof(MissionNameDivider));
            }
        }

        private bool _isComplete;

        [Binding]
        public bool IsComplete
        {
            get => _isComplete;
            set
            {
                _isComplete = value;
                OnPropertyChanged(nameof(IsComplete));
            }
        }

        private void OnDestroy()
        {
            if (ItemSetManager.Instance != null)
            {
                ItemSetManager.Instance.OnSetCompleted -= OnSetCompleted;
            }
        }

        private void OnSetCompleted(string setName)
        {
            if (setName == _currentSetName)
            {
                IsComplete = true;
            }
        }

        public void Initialize(ItemSetData itemSetData)
        {
            _currentSetName = itemSetData.SetName;
            MissionName = string.Format("{0}", itemSetData.SetName);
            MissionNameDivider = string.Format("<alpha=#00>{0}", itemSetData.SetName);

            if (ItemSetManager.Instance != null)
            {
                ItemSetManager.Instance.OnSetCompleted += OnSetCompleted;
            }

            GetComponentInChildren<MissionItemGorupViewModel>().Initialize(itemSetData);
        }
    }
}