using System;
using System.Collections.Generic;
using System.Linq;
using UnityWeld;
using UnityWeld.Binding;

namespace UI.Page
{
    [Binding]
    public class QuestPage : PageViewModel
    {
        private List<QuestViewModel> questViewModels = new List<QuestViewModel>();

        private void OnEnable()
        {
            var s = GetComponentsInChildren<QuestViewModel>();
            questViewModels = s.ToList();
            OnClickDailyQuestButton();
        }
        [Binding]
        public void OnClickDailyQuestButton()
        {
            foreach (var q in questViewModels)
            {
                if (q as DailyQuestScrollView)
                {
                    q.gameObject.SetActive(true);
                }
                else q.gameObject.SetActive(false);
            }
        }
    }
}