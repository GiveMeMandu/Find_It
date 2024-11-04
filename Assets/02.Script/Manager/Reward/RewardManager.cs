using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using SO;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Manager
{
    public enum RewardType
    {
        [LabelText("수량")]
        NumValue, //* 그냥 숫자 그대로 주기
        [LabelText("재화의 배수")]
        Multiple, //* 현재 돈의 배수에 따라 주기
        [LabelText("시간에 따른 수익 보상")]
        TimeProfitReward, //* 시간에 따라 수익 발생하는 정도로 보상
    }

    public partial class RewardManager : MonoBehaviour
    {
        protected void Start()
        {
            if (store == null) { ResetReward(); return; }
            //* 하루이상 접속 안하면 첫날부터 
            // if (Mathf.Abs(GetTimeSpanFromNow(store.lastUTCTime).Days) > 1) ResetReward();
        }

        public string CalculateRewardByType(RewardSO rewardSO)
        {
            string rewardValue = "0";

            switch (rewardSO.rewardType)
            {
                case RewardType.NumValue:
                    rewardValue = CalculateNumValueReward(rewardSO);
                    break;
                case RewardType.Multiple:
                    rewardValue = CalculateMultipleReward(rewardSO);
                    break;
                case RewardType.TimeProfitReward:
                    rewardValue = CalculateTimeProfitReward(rewardSO);
                    break;
                default:
                    Debug.LogError("Invalid Reward Type");
                    break;
            }

            return rewardValue;
        }

        public void GiveRewardByType(RewardSO rewardSO)
        {
            string rewardValue = CalculateRewardByType(rewardSO);
            BigInteger rewardAmount = Global.GoldManager.GetGoldUnitValue(rewardValue);
            AddReward(rewardSO.moneyType, rewardAmount);
        }

        private string CalculateNumValueReward(RewardSO rewardSO)
        {
            int value = rewardSO.amount;
            return value.ToString();
        }

        private string CalculateMultipleReward(RewardSO rewardSO)
        {
            int value = rewardSO.multipleAmount;
            BigInteger rewardValue = Global.GoldManager.Gold * value;
            return rewardValue.ToString();
        }

        private string CalculateTimeProfitReward(RewardSO rewardSO)
        {
            return Global.GoldManager.GetGoldEPS(rewardSO.timeReward);
        }

        private void AddReward(MoneyType moneyType, BigInteger value)
        {
            if (moneyType == MoneyType.Gold)
            {
                Global.GoldManager.AddGold(value);
            }
            else if (moneyType == MoneyType.Cash)
            {
                Global.CashManager.AddCash(value);
            }
        }
    }
}
