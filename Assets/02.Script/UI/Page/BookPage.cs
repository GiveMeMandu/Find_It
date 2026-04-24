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
    public class BookPage : PageViewModel
    {
        public List<GameObject> Views = new List<GameObject>();
        public TabGroup tabGroup;
        private HorizontalLayoutGroup _tabButtonLayoutGroup;

        public Sprite albumIcon;
        public Sprite cameraIcon;
        public Sprite stickerIcon;

        public override void Init(params object[] parameters)
        {
            _tabButtonLayoutGroup = tabGroup.GetComponent<HorizontalLayoutGroup>();
            // 초기화 시 첫 번째 탭이 선택되도록 설정
            if (tabGroup != null)
            {
                tabGroup.Clear();

                tabGroup.AddTab("UI/Common/Album", () => OnClickAlbumViewButton(), albumIcon);
                tabGroup.AddTab("UI/Common/Camera", () => OnClickStickerBookViewButton(), cameraIcon);
                tabGroup.AddTab("UI/Common/Sticker", () => OnClickStickerViewButton(), stickerIcon);
            }
            if (_tabButtonLayoutGroup != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_tabButtonLayoutGroup.GetComponent<RectTransform>());
                _tabButtonLayoutGroup.enabled = false;
            }
            tabGroup.SelectTab(2); // 첫 번째 탭 선택
        }
        [Binding]
        public override void ClosePage()
        {
            Global.UIManager.ClosePage(this);
        }


        [Binding]
        public void OnClickStickerViewButton()
        {
            foreach (var scroll in Views)
            {
                if (scroll.TryGetComponent(out CollectionScrollViewModel collectionScrollViewModel))
                {
                    scroll.gameObject.SetActive(true);
                }
                else scroll.gameObject.SetActive(false);
            }
        }

        [Binding]
        public void OnClickAlbumViewButton()
        {
            // TODO: 카메라 관련 뷰 모델이 추가되면 여기에 활성화 로직 추가
            foreach (var scroll in Views)
            {
                if (scroll.TryGetComponent(out CollectionDiaryViewModel collectionDiaryViewModel))
                {
                    scroll.gameObject.SetActive(true);
                }
                else scroll.gameObject.SetActive(false);
            }
        }

        [Binding]
        public void OnClickStickerBookViewButton()
        {
            foreach (var scroll in Views)
            {
                if (scroll.TryGetComponent(out UnityWeld.CollectionBookScrollViewModel collectionBookScrollViewModel))
                {
                    scroll.gameObject.SetActive(true);
                }
                else scroll.gameObject.SetActive(false);
            }
        }
    }
}