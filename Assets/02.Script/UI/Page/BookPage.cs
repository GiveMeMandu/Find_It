using System.Collections.Generic;
using System.Linq;
using Manager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityWeld;
using UnityWeld.Binding;
using echo17.EndlessBook.Demo02;
using Cysharp.Threading.Tasks;

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
        private int? _queuedPageNumber = null;
        private Coroutine _queueCoroutine = null;

        void OnEnable()
        {
            Init();
            DisableLayoutRebuilder().Forget();
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
            tabGroup.SelectTab(2); // 첫 번째 탭 선택 (초기화 시에는 기능 미실행)
        }
        public async UniTaskVoid DisableLayoutRebuilder()
        {
            await UniTask.DelayFrame(1); // 다음 프레임까지 대기하여 레이아웃 계산이 완료되도록 함
            if (_tabButtonLayoutGroup != null)
            {
                _tabButtonLayoutGroup.enabled = false;
            }
        }

        private void SafeTurnToPage(int pageNumber)
        {
            if (_bookController == null || _bookController.book == null) return;

            // 이미 해당 페이지가 펼쳐져 있거나 넘기는 중인 목표가 같다면 무시
            if (_queuedPageNumber == pageNumber) return;
            if (_bookController.book.CurrentState == echo17.EndlessBook.EndlessBook.StateEnum.OpenMiddle)
            {
                if (_bookController.book.CurrentLeftPageNumber == pageNumber || _bookController.book.CurrentRightPageNumber == pageNumber)
                {
                    return;
                }
            }

            // 현재 넘기는 중이거나 상태 변경 중이면 예약
            if (_bookController.book.IsTurningPages || _bookController.book.IsChangingState)
            {
                _queuedPageNumber = pageNumber;
                if (_queueCoroutine == null)
                {
                    _queueCoroutine = StartCoroutine(ProcessQueueCoroutine());
                }
                return;
            }

            // 즉시 실행 가능한 경우
            _queuedPageNumber = null;
            _bookController.TurnToPage(pageNumber);
        }

        private System.Collections.IEnumerator ProcessQueueCoroutine()
        {
            while (_queuedPageNumber.HasValue)
            {
                // 책이 동작을 멈출 때까지 대기
                while (_bookController.book.IsTurningPages || _bookController.book.IsChangingState)
                {
                    yield return null;
                }

                // 예약된 페이지가 있다면 실행
                if (_queuedPageNumber.HasValue)
                {
                    int targetPage = _queuedPageNumber.Value;
                    _queuedPageNumber = null; // 실행 전 비움 (연속 예약 대비)
                    
                    // 이미 목표 페이지에 도달해있는지 다시 확인
                    if (!(_bookController.book.CurrentLeftPageNumber == targetPage || _bookController.book.CurrentRightPageNumber == targetPage))
                    {
                        _bookController.TurnToPage(targetPage);
                        // 넘기기 시작했으므로 다음 루프에서 다시 대기
                        yield return null; 
                    }
                }
            }
            _queueCoroutine = null;
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