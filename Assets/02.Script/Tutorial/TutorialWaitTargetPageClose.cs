using Manager;
using UI;
using UnityEngine;

namespace InGame.Tutorial
{
    public class TutorialWaitTargetPageClose : TutorialBase
    {
        [Header("닫기를 기다릴 페이지 (클래스 이름 입력)")]
        [Tooltip("타겟 UI 클래스 이름 (예: InGameIntroPage)")]
        public string targetClassName = "InGameIntroPage";

        private bool canExecute = false;

        public override void Enter()
        {
            canExecute = false;
            if (Global.UIManager != null)
            {
                Global.UIManager.OnClosePage += OnPageClosed;
            }
        }

        private void OnPageClosed(object sender, PageViewModel closedPage)
        {
            if (closedPage == null || string.IsNullOrEmpty(targetClassName)) return;
            
            // 클래스 이름만 가지고 비교
            if (closedPage.GetType().Name == targetClassName)
            {
                canExecute = true;
            }
        }

        public override void Execute(TutorialController controller)
        {
            if (!canExecute) return;
            
            controller.SetNextTutorial();
            canExecute = false; // 중복 실행 방지
        }

        public override void Exit()
        {
            if (Global.UIManager != null)
            {
                Global.UIManager.OnClosePage -= OnPageClosed;
            }
        }
    }
}
