using InGame;
using Manager;
using UI;
using UI.Page;
using UnityEngine;

namespace InGame.Tutorial
{
    public class TutorialClickGuide : TutorialBase
    {
        [Header("클릭 대상")]
        [SerializeField] private LeanClickEvent _targetTouchable;
        [SerializeField] private SpriteRenderer _targetGuideSprite;
        [SerializeField] private Sprite _customMaskSprite; // 커스텀 마스크 이미지
        public bool isInputDisableMoveInput = true; // 클릭 가이드 실행 시 입력 비활성화 여부

        [Header("스킵가능한건지")]
        [SerializeField] private bool _isSkipable = false;
        private InGameMainPage inGameMainPage;
        private bool isTouched = false;
        private float timer = 0f;
        private const float SKIP_TIME = 3f;

        public override void Enter()
        {
            timer = 0f;
            if (isInputDisableMoveInput)
            {
                Global.InputManager.DisableGameInputOnly();
            }

            if (_targetTouchable != null)
            {
                _targetTouchable.isTutorialTarget = true;
            }

            if (Global.UIManager.GetPages<InGameMainPage>().Count > 0)
            {
                LeanClickEvent.OnGlobalClickSuccess += OnGlobalClickSuccess;
                inGameMainPage = Global.UIManager.GetPages<InGameMainPage>()[0];
                var guideViewModel = Global.UIManager.OpenPage<GuidePage>().GetComponentInChildren<GuideViewModel>();
                guideViewModel.SetTargetGuide(_targetGuideSprite, _customMaskSprite);
            }
        }

        public override void Execute(TutorialController controller)
        {
            if(isTouched)
            {
                controller.SetNextTutorial();
                return;
            }

            if(_isSkipable)
            {
                timer += Time.deltaTime;
                if(timer >= SKIP_TIME)
                {
                    controller.SetNextTutorial();
                }
            }
        }

        public override void Exit()
        {
            if (_targetTouchable != null)
            {
                _targetTouchable.isTutorialTarget = false;
            }

            LeanClickEvent.OnGlobalClickSuccess -= OnGlobalClickSuccess;
            Global.UIManager.ClosePage();
            if (isInputDisableMoveInput)
            {
                Global.InputManager.EnableGameInputOnly();
            }
        }

        private void OnDestroy()
        {
            LeanClickEvent.OnGlobalClickSuccess -= OnGlobalClickSuccess;
        }

        private void OnGlobalClickSuccess(GameObject clickedObject, Vector2 screenPos)
        {
            if (_targetTouchable != null && clickedObject == _targetTouchable.gameObject)
            {
                isTouched = true;
            }
        }
    }
}