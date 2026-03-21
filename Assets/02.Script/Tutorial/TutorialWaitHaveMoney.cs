using System;
using System.Numerics;
using Manager;
using UnityEngine;
namespace InGame.Tutorial
{
    public class TutorialWaitHaveMoney : TutorialBase
    {
        [Header("목표 돈")]
        public string targetHaveMoney = "1";

        private bool canExecute = false;
        public override void Enter()
        {
            if(Global.CoinManager.GetCoinValue() >= Global.CoinManager.GetCoinUnitValue(targetHaveMoney))
            {
                canExecute = true;
            }
            
            Global.CoinManager.OnCoinValueChanged += OnCoinValueChanged;
        }

        private void OnCoinValueChanged(object sender, string e)
        {
            if(Global.CoinManager.GetCoinUnitValue(e) >= Global.CoinManager.GetCoinUnitValue(targetHaveMoney))
            {
                canExecute = true;
            }
        }

        public override void Execute(TutorialController controller)
        {
            if (!canExecute) return;
            controller.SetNextTutorial();
        }

        public override void Exit()
        {
            Global.CoinManager.OnCoinValueChanged -= OnCoinValueChanged;
        }
    }
}