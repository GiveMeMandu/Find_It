using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;
using UnityWeld;
using UnityWeld.Binding;

namespace UI
{
    [Binding]
    public class QuestViewModel : ScrollView
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

        // private Quest _curQuest;
        // private void OnEnable() {
        //     Global.QuestManager.QuestEvents.onQuestStateChange += QuestStateChange;
        // }
        
        // private void OnDisable()
        // {
        //     Global.QuestManager.QuestEvents.onQuestStateChange -= QuestStateChange;
        // }

        // private void QuestStateChange(Quest quest)
        // {
        //     var viewModels = GetViewModels();

        //     foreach(var questView in viewModels) {
        //         if(questView is QuestScrollElementViewModel)
        //         {
        //             var q = questView as QuestScrollElementViewModel;
        //             if(q.CheckQuestSame(quest))
        //                 q.Initialize(quest);
        //         }
        //     }
        // }
    }
}