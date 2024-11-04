using System.Collections;
using System.Collections.Generic;
using Manager;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SO
{
    [CreateAssetMenu(fileName ="Reward", menuName = "보상/일반 보상")]
    public class RewardSO : SerializedScriptableObject
    {
        [BoxGroup("보상 설정")] [LabelText("보상 재화 종류")] [EnumToggleButtons]
        public MoneyType moneyType;
        [BoxGroup("보상 설정")] [LabelText("보상 방법")] [EnumToggleButtons]
        public RewardType rewardType;
        
        
        [BoxGroup("보상 설정")] [LabelText("보상 수량")] [ShowIf("rewardType", RewardType.NumValue)]
        public int amount;
        [BoxGroup("보상 설정")] [LabelText("보상 배수")] [ShowIf("rewardType", RewardType.Multiple)]
        public int multipleAmount;
        [BoxGroup("보상 설정")] [LabelText("시간 보상 단위(초)")] [ShowIf("rewardType", RewardType.TimeProfitReward)]
        public int timeReward;
    }
}