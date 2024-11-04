using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityWeld.Binding;

namespace UI
{
    [Binding]
    public class UpgradeViewModel : BaseViewModel
    {
        public event EventHandler OnDoUpgrade;
        [Binding]
        public void DoUpgrade()
        {
            OnDoUpgrade?.Invoke(this, EventArgs.Empty);
        }
        private int _level;
        [Binding]
        public int Level
        {
            get => _level;
            set
            {
                _level = value;
                OnPropertyChanged(nameof(Level));
            }
        }
        private string _info;
        [Binding]
        public string Info
        {
            get => _info;
            set
            {
                _info = value;
                OnPropertyChanged(nameof(Info));
            }
        }
        private string _cost;
        [Binding]
        public string Cost
        {
            get => _cost;
            set
            {
                _cost = value;
                OnPropertyChanged(nameof(Cost));
            }
        }
        private float _statValue;
        [Binding]
        public float StatValue
        {
            get => _statValue;
            set
            {
                _statValue = value;
                OnPropertyChanged(nameof(StatValue));
            }
        }

        private bool _isUpgradable;

        [Binding]
        public bool IsUpgradeable
        {
            get => _isUpgradable;
            set
            {
                _isUpgradable = value;
                OnPropertyChanged(nameof(IsUpgradeable));
            }
        }
        private Sprite _icon;

        [Binding]
        public Sprite Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                OnPropertyChanged(nameof(Icon));
            }
        }
    }
}