using UI;
using UI.Page;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Manager;

namespace InGame.Tutorial
{
    public class TutorialClickUIGuide : TutorialBase
    {
        [SerializeField] private Button _targetButton; // 터치할 UI 버튼
        [SerializeField] private Sprite _customMaskSprite; // 커스텀 마스크 이미지
        private InGameMainPage inGameMainPage;
        private bool isTouched = false;
        [SerializeField] private Vector2 offset = new Vector2(0, 0);
        private GuidePage guidePage;
        public bool isFinger = false;
        public bool isLayoutElement = false;  // Layout Group 내부 요소인지 여부

        public void Init(Button targetButton = null, Vector2 offset = default, bool isFinger = false, bool isLayoutElement = false, Sprite customMaskSprite = null)
        {
            if (targetButton != null)
                _targetButton = targetButton;
            this.offset = offset;
            this.isFinger = isFinger;
            this.isLayoutElement = isLayoutElement;
            if (customMaskSprite != null)
                _customMaskSprite = customMaskSprite;
            Enter();
        }

        public override void Enter()
        {
            if (Global.UIManager.GetPages<InGameMainPage>().Count > 0)
            {
                inGameMainPage = Global.UIManager.GetPages<InGameMainPage>()[0];

                if (_targetButton != null)
                {
                    _targetButton.onClick.AddListener(OnButtonClicked);

                    // 가이드 UI 표시
                    guidePage = Global.UIManager.OpenPage<GuidePage>();
                    
                    // 레이아웃 업데이트를 위해 코루틴 시작
                    StartCoroutine(SetupGuideAfterLayout());
                }
            }
        }

        private IEnumerator SetupGuideAfterLayout()
        {
            // 한 프레임 대기하여 레이아웃 업데이트 허용
            yield return null;
            
            // 캔버스와 레이아웃 강제 업데이트
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_targetButton.GetComponent<RectTransform>());
            
            // 한 프레임 더 대기하여 모든 변경사항이 적용되도록 함
            yield return null;

            var guideViewModel = guidePage.GetComponentInChildren<GuideViewModel>();
            RectTransform buttonRect = _targetButton.GetComponent<RectTransform>();
            guideViewModel.SetTargetGuide(buttonRect, offset, isFinger, isLayoutElement, _customMaskSprite);
        }

        public override void Execute(TutorialController controller)
        {
            if (isTouched)
            {
                if (_targetButton != null)
                {
                    _targetButton.onClick.RemoveListener(OnButtonClicked);
                }
                controller.SetNextTutorial();
            }
        }

        public override void Exit()
        {
        }

        private void OnButtonClicked()
        {
            Global.UIManager.ClosePage(guidePage);
            isTouched = true;
        }
    }
}