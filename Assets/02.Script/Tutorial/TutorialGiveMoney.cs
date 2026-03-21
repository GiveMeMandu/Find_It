using System;
using System.Numerics;
using Manager;
using UnityEngine;
namespace InGame.Tutorial
{
    public class TutorialGiveMoney : TutorialBase
    {
        [Header("지급할 돈")]
        public string targetGiveMoney = "1";

        private bool canExecute = false;
        public override void Enter()
        {
            if(Global.CoinManager.GetCoinValue() >= Global.CoinManager.GetCoinUnitValue(targetGiveMoney))
            {
                canExecute = true;
            }else
            {
                Global.CoinManager.AddCoin(Global.CoinManager.GetCoinUnitValue(targetGiveMoney), false, 20);
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

        }
    }
}