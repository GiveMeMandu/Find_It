using System.Collections.Generic;
using System.Linq;
using Manager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityWeld;
using UnityWeld.Binding;

namespace UI.Page
{
    [Binding]
    public class CollectionPage : PageViewModel
    {
        public List<ScrollView> scrollViews = new List<ScrollView>();
        public List<CollectionPageTabMenuViewModel> tabViewModels = new List<CollectionPageTabMenuViewModel>();
        public TabGroup tabGroup;
        private HorizontalLayoutGroup _tabButtonLayoutGroup;

        public override void Init(params object[] parameters)
        {
            _tabButtonLayoutGroup = tabGroup.GetComponent<HorizontalLayoutGroup>();
            // 초기화 시 첫 번째 탭이 선택되도록 설정
            if (tabGroup != null)
            {
                tabGroup.Clear();

                tabGroup.AddTab("UI/Common/Sticker", () => OnClickStickerViewButton());
                tabGroup.AddTab("UI/Common/Camera", () => OnClickStickerBookViewButton());
                tabGroup.AddTab("UI/Common/Album", () => OnClickAlbumViewButton());
            }
            if (_tabButtonLayoutGroup != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_tabButtonLayoutGroup.GetComponent<RectTransform>());
                _tabButtonLayoutGroup.enabled = false;
            }
            tabGroup.SelectTab(0); // 첫 번째 탭 선택
        }
        [Binding]
        public override void ClosePage()
        {
            Global.UIManager.ClosePage(this);
        }
        private void OnEnable()
        {
            var s = GetComponentsInChildren<ScrollView>(true);
            scrollViews = s.ToList();

            var tabs = GetComponentsInChildren<CollectionPageTabMenuViewModel>(true);
            tabViewModels = tabs.ToList();

        }

        private void UpdateTabActiveState(CollectionTabType activeTabType)
        {
            foreach (var tab in tabViewModels)
            {
                if (tab != null)
                {
                    tab.IsActive = (tab.TabType == activeTabType);
                }
            }
        }

        [Binding]
        public void OnClickStickerViewButton()
        {
            UpdateTabActiveState(CollectionTabType.Sticker);
            foreach (var scroll in scrollViews)
            {
                if (scroll is CollectionScrollViewModel)
                {
                    scroll.gameObject.SetActive(true);
                }
                else scroll.gameObject.SetActive(false);
            }
        }

        [Binding]
        public void OnClickAlbumViewButton()
        {
            UpdateTabActiveState(CollectionTabType.Camera);
            // TODO: 카메라 관련 뷰 모델이 추가되면 여기에 활성화 로직 추가
        }

        [Binding]
        public void OnClickStickerBookViewButton()
        {
            UpdateTabActiveState(CollectionTabType.StickerBook);
            foreach (var scroll in scrollViews)
            {
                if (scroll is UnityWeld.CollectionBookScrollViewModel)
                {
                    scroll.gameObject.SetActive(true);
                }
                else scroll.gameObject.SetActive(false);
            }
        }
    }
}