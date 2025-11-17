using System.Collections.Generic;
using System.Linq;
using Manager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityWeld;
using UnityWeld.Binding;

namespace UI.Page
{
    [Binding]
    public class CollectionPage : PageViewModel
    {
        [Binding]
        public override void ClosePage()
        {
            Global.UIManager.ClosePage(this);
        }
        public List<ScrollView> scrollViews = new List<ScrollView>();
        private void OnEnable()
        {
            var s = GetComponentsInChildren<ScrollView>();
            scrollViews = s.ToList();
            OnClickToyMenuButton();
        }
        [Binding]
        public void OnClickToyMenuButton()
        {
            foreach (var scroll in scrollViews)
            {
                if (scroll as CollectionScrollViewModel)
                {
                    scroll.gameObject.SetActive(true);
                }
                else scroll.gameObject.SetActive(false);
            }
        }
    }
}