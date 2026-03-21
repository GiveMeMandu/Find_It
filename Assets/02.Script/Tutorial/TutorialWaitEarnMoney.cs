using System;
using System.Numerics;
using Manager;
using UnityEngine;
namespace InGame.Tutorial
{
    public class TutorialWaitEarnMoney : TutorialBase
    {
        [Header("목표 수익")]
        public string targetEarnMoney = "1";

        private bool canExecute = false;

        private BigInteger prevGoldValue;
        private BigInteger goldCollected;
        public override void Enter()
        {
            prevGoldValue = Global.CoinManager.GetCoinValue();
            Global.CoinManager.OnCoinValueIncreased += OnGoldValueIncreased;
        }

        private void OnGoldValueIncreased(object sender, BigInteger e)
        {
            if (prevGoldValue < e)
            {
                goldCollected = e - prevGoldValue;
                if(goldCollected >= Global.CoinManager.GetCoinUnitValue(targetEarnMoney))
                {
                    canExecute = true;
                }
            }
        }

        public override void Execute(TutorialController controller)
        {
            if (!canExecute) return;
            controller.SetNextTutorial();
        }

        public override void Exit()
        {
            Global.CoinManager.OnCoinValueIncreased -= OnGoldValueIncreased;
        }
    }
}