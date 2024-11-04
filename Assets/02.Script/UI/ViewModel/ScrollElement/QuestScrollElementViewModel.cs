using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityWeld.Binding;
using SO;
using Manager;
using System;
// using I2.Loc;
namespace UI
{
    [Binding]
    public class QuestScrollElementViewModel : ScrollElementViewModel
    {
        // private Quest _quest;
        
        // public void Initialize(Quest quest)
        // {
        //     _quest = quest;
        //     UpdateValue();
        // }
        // public bool CheckQuestSame(Quest quest)
        // {
        //     if(_quest == quest)
        //     {
        //         return true;
        //     }
        //     return false;
        // }

        // private void UpdateValue()
        // {
        //     Name = LocalizationManager.GetTranslation(_quest.info.displayName);
        //     Info = LocalizationManager.GetTranslation(_quest.info.displayInfo);
        //     Status = _quest.GetFullStatusText();
            
        //     // rewards
        //     GoldReward = _quest.info.goldReward.ToString();
        //     CashReward = _quest.info.cashReward.ToString();
        //     //TODO 선행퀘 표기             
        //     // foreach (QuestInfoSO prerequisiteQuestInfo in quest.info.questPrerequisites)
        //     // {
        //     //     questRequirementsText.text += prerequisiteQuestInfo.displayName + "\n";
        //     // }
        // }
        [Binding]
        public void GetQuestReward()
        {
        }
        private string _name;

        [Binding]
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
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
        private string _status;

        [Binding]
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }
        private string _goldReward;

        [Binding]
        public string GoldReward
        {
            get => _goldReward;
            set
            {
                _goldReward = value;
                OnPropertyChanged(nameof(GoldReward));
            }
        }
        private string _cashReward;

        [Binding]
        public string CashReward
        {
            get => _cashReward;
            set
            {
                _cashReward = value;
                OnPropertyChanged(nameof(CashReward));
            }
        }
        private bool _isRewardAble;

        [Binding]
        public bool IsRewardAble
        {
            get => _isRewardAble;
            set
            {
                _isRewardAble = value;
                OnPropertyChanged(nameof(IsRewardAble));
            }
        }
    }
}