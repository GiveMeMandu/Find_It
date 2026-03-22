using Manager;
using UI;
using UnityEngine;

namespace InGame.Tutorial
{
    public class TutorialSetInput : TutorialBase
    {
        public bool isDisableInput = true; // 입력 비활성화 여부 (true면 비활성화, false면 활성화)
        public bool disableGameInputOnly = true; // 게임 입력만 비활성화할지 여부
        public bool activeAfterExit = false; // 종료 후 입력 활성화 여부

        private bool canExecute = false;

        public override void Enter()
        {
            canExecute = true;
            
            if (isDisableInput)
            {
                if (disableGameInputOnly)
                    Global.InputManager.DisableGameInputOnly();
                else
                    Global.InputManager.DisableAllInput();
            }
            else
            {
                if (disableGameInputOnly)
                    Global.InputManager.EnableGameInputOnly();
                else
                    Global.InputManager.EnableAllInput();
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
            if (activeAfterExit)
            {
                if (disableGameInputOnly)
                    Global.InputManager.EnableGameInputOnly();
                else
                    Global.InputManager.EnableAllInput();
            }
        }
    }
}
