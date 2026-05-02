using System.Collections.Generic;
using System.Linq;
using Manager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityWeld;
using UnityWeld.Binding;
using echo17.EndlessBook.Demo02;

namespace UI.Page
{
    [Binding]
    public class BookPage : PageViewModel
    {
        public TabGroup tabGroup;
        private HorizontalLayoutGroup _tabButtonLayoutGroup;

        public Sprite albumIcon;
        public Sprite cameraIcon;
        public Sprite stickerIcon;

        [Header("Page Numbers")]
        public int homePageNumber = 2;
        public int albumPageNumber = 1;
        public int stickerBookPageNumber = 3;
        public int stickerPageNumber = 5;

        private Demo02 _bookController;

        void OnEnable()
        {
            Init();
        }
        public override void Init(params object[] parameters)
        {
            _bookController = Object.FindAnyObjectByType<Demo02>();

            _tabButtonLayoutGroup = tabGroup.GetComponent<HorizontalLayoutGroup>();
            // 초기화 시 첫 번째 탭이 선택되도록 설정
            if (tabGroup != null)
            {
                tabGroup.Clear();
                tabGroup.AddTab("UI/Common/Album", () => OnClickAlbumViewButton(), albumIcon);
                tabGroup.AddTab("UI/Common/Sticker", () => OnClickStickerViewButton(), stickerIcon);
                tabGroup.AddTab("UI/Hideout", () => OnClickHomeViewButton(), albumIcon);
            }
            if (_tabButtonLayoutGroup != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_tabButtonLayoutGroup.GetComponent<RectTransform>());
            }
            
            // 시작 시 탭 선택 로직을 코루틴으로 실행하여 북 컨트롤러가 준비된 후 넘어가도록 함
            StartCoroutine(InitialSelectTab(2));
        }

        private System.Collections.IEnumerator InitialSelectTab(int index)
        {
            // 한 프레임 대기하여 BookController 등이 완전히 초기화되기를 기다림
            yield return null;
            if (tabGroup != null)
            {
                tabGroup.SelectTab(index);
            }
        }

        void Start()
        {
            _tabButtonLayoutGroup.enabled = false;
        }

        private void SafeTurnToPage(int pageNumber)
        {
            if (_bookController == null || _bookController.book == null) return;

            // 이미 넘기고 있는 중이면 무시 (버튼 연타 방지)
            if (_bookController.book.IsTurningPages || _bookController.book.IsChangingState) return;

            // 이미 해당 페이지가 펼쳐져 있다면 무시
            if (_bookController.book.CurrentState == echo17.EndlessBook.EndlessBook.StateEnum.OpenMiddle)
            {
                if (_bookController.book.CurrentLeftPageNumber == pageNumber || _bookController.book.CurrentRightPageNumber == pageNumber)
                {
                    return;
                }
            }

            _bookController.TurnToPage(pageNumber);
        }

        [Binding]
        public override void ClosePage()
        {
            Global.UIManager.ClosePage(this);
        }


        [Binding]
        public void OnClickStickerViewButton()
        {
            SafeTurnToPage(stickerPageNumber);
        }

        [Binding]
        public void OnClickHomeViewButton()
        {
            SafeTurnToPage(homePageNumber);
        }
        [Binding]
        public void OnClickAlbumViewButton()
        {
            SafeTurnToPage(albumPageNumber);
        }

        [Binding]
        public void OnClickStickerBookViewButton()
        {
            SafeTurnToPage(stickerBookPageNumber);
        }
    }
}