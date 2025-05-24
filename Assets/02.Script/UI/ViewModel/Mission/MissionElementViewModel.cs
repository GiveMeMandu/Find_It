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
        private Color _missionItemColor;
        [Binding]
        public Color MissionItemColor
        {
            get => _missionItemColor;
            set
            {
                _missionItemColor = value;
                OnPropertyChanged(nameof(MissionItemColor));
            }
        }
        private float _missionItemColorAlpha;
        [Binding]
        public float MissionItemColorAlpha
        {
            get => _missionItemColorAlpha;
            set
            {
                _missionItemColorAlpha = value;
                OnPropertyChanged(nameof(MissionItemColorAlpha));
            }
        }
        private Color _missionItemColorWhite;
        [Binding]
        public Color MissionItemColorWhite
        {
            get => _missionItemColorWhite;
            set
            {
                _missionItemColorWhite = value;
                OnPropertyChanged(nameof(MissionItemColorWhite));
            }
        }
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
                MissionItemColor = new Color(39f / 255f, 156f / 255f, 123f / 255f, 0.6f);
                MissionItemColorAlpha = 0.6f;
                MissionItemColorWhite = new Color(1f, 1f, 1f, 0.6f);
            }
        }

        public void Initialize(ItemSetData itemSetData)
        {
            _currentSetName = itemSetData.SetName;
            MissionName = string.Format("{0}", itemSetData.SetName);
            MissionNameDivider = string.Format("<alpha=#00>{0}", itemSetData.SetName);
            MissionItemColor = new Color(39f / 255f, 156f / 255f, 123f / 255f, 1f);
            MissionItemColorAlpha = 1f;
            MissionItemColorWhite = new Color(1f, 1f, 1f, 1f);

            if (IsComplete)
            {
                MissionItemColor = new Color(39f / 255f, 156f / 255f, 123f / 255f, 0.6f);
                MissionItemColorAlpha = 0.6f;
                MissionItemColorWhite = new Color(1f, 1f, 1f, 0.6f);
            }


            if (ItemSetManager.Instance != null)
            {
                ItemSetManager.Instance.OnSetCompleted += OnSetCompleted;
            }

            GetComponentInChildren<MissionItemGorupViewModel>().Initialize(itemSetData);
        }
    }
}