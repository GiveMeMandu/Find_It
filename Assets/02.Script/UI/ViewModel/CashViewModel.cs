using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;
using UnityWeld.Binding;

namespace UI
{
    [Binding]
    public class CashViewModel : BaseViewModel
    {
        private string _cash;
        [Binding]
        public string Cash
        {
            get => _cash;
            set{
                _cash = value;
                OnPropertyChanged(nameof(Cash));
            }
        }
        private void OnEnable() {
            Global.CashManager.OnCashValueChanged += OnCashValueChange;
            Cash = Global.CashManager.GetCashText();
        }
        private void OnDisable() {
            Global.CashManager.OnCashValueChanged -= OnCashValueChange;
        }

        private void OnCashValueChange(object sender, string e)
        {
            Cash = Global.CashManager.GetCashText();
        }
    }
}