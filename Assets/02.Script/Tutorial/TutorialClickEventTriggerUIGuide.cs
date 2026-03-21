using InGame;
using Manager;
using UI;
using UI.Page;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace InGame.Tutorial
{
    public class TutorialClickEventTriggerUIGuide : TutorialBase
    {
        [SerializeField] private Button _targetButton; // 터치할 UI 버튼
        private InGameMainPage inGameMainPage;
        private bool isTouched = false;
        [SerializeField] private Vector2 offset = new Vector2(0, 0);
        private GuidePage guidePage;
        public bool isFinger = false;
        public bool isLayoutElement = false;
        public void Init(Button targetButton = null, Vector2 offset = default, bool isFinger = false, bool isLayoutElement = false)
        {
            if (targetButton != null)
                _targetButton = targetButton;
            this.offset = offset;
            this.isFinger = isFinger;
            this.isLayoutElement = isLayoutElement;
            Enter();
        }

        public override void Enter()
        {
            if (Global.UIManager.GetPages<InGameMainPage>().Count > 0)
            {
                inGameMainPage = Global.UIManager.GetPages<InGameMainPage>()[0];

                if (_targetButton != null)
                {
                    // Event Trigger 컴포넌트 가져오기
                    EventTrigger eventTrigger = _targetButton.GetComponent<EventTrigger>();
                    if (eventTrigger != null)
                    {
                        // PointerClick 이벤트에 대한 새 엔트리 생성
                        EventTrigger.Entry entry = new EventTrigger.Entry();
                        entry.eventID = EventTriggerType.PointerClick;
                        entry.callback.AddListener((data) => { OnButtonClicked(); });
                        eventTrigger.triggers.Add(entry);
                    }

                    // 가이드 UI 표시
                    guidePage = Global.UIManager.OpenPage<GuidePage>();
                    var guideViewModel = guidePage.GetComponentInChildren<GuideViewModel>();

                    RectTransform buttonRect = _targetButton.GetComponent<RectTransform>();
                    guideViewModel.SetTargetGuide(buttonRect, offset, isFinger, isLayoutElement);
                }
            }
        }

        public override void Execute(TutorialController controller)
        {
            if (isTouched)
            {
                controller.SetNextTutorial();
                if (_targetButton != null)
                {
                    // Event Trigger 제거
                    EventTrigger eventTrigger = _targetButton.GetComponent<EventTrigger>();
                    if (eventTrigger != null)
                    {
                        eventTrigger.triggers.Clear();
                    }
                }
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