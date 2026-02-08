using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;
using UnityWeld.Binding;

namespace UI
{
    [Binding]
    public class GoldViewModel : BaseViewModel
    {
        private string _gold;
        [Binding]
        public string Gold
        {
            get => _gold;
            set{
                _gold = value;
                OnPropertyChanged(nameof(Gold));
            }
        }
        private void OnEnable() {
            Global.CoinManager.OnCoinValueChanged += OnGoldValueChange;
            Gold = Global.CoinManager.GetCoinText();

        }
        private void OnDisable() {
            Global.CoinManager.OnCoinValueChanged -= OnGoldValueChange;
        }

        private void OnGoldValueChange(object sender, string e) => Gold = e;
    }
}