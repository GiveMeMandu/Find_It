using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Manager;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityWeld.Binding;


namespace UnityWeld
{
    [Binding]
    public class DailyQuestScrollView : QuestViewModel
    {
        // private void OnEnable()
        // {
        //     InitAllDailyQuestViewModels();
        // }
        // public void InitAllDailyQuestViewModels()
        // {
        //     var allDailyQuest = Global.QuestManager.GetDailyQuests();

        //     PrepareViewModels(allDailyQuest.Count);
        //     var viewModels = GetViewModels();
        //     for(int i = 0; i < viewModels.Count; i++) {
        //         if(viewModels[i] as QuestScrollElementViewModel)
        //         {
        //             var view = viewModels[i] as QuestScrollElementViewModel;
                        
        //             view.Initialize(allDailyQuest[i]);
        //         }
        //     }
        // }
    }
}