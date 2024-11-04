using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;
using UnityWeld.Binding;

// 현재 퀘스트가 뭔지 가장 높은걸 보여줌
namespace UI
{
    [Binding]
    public class CurQuestViewModel : BaseViewModel
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
            Global.GoldManager.OnGoldValueChanged += OnGoldValueChange;
            Gold = Global.GoldManager.GetGoldText();

        }
        private void OnDisable() {
            Global.GoldManager.OnGoldValueChanged -= OnGoldValueChange;
        }

        private void OnGoldValueChange(object sender, string e) => Gold = e;
    }
}