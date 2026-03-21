using UI;
using UI.Page;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using Manager;

namespace InGame.Tutorial
{
    public class TutorialInGamePage : TutorialBase
    {
        public bool isOnlyShowUIAll = false;
        [Header("튜토리얼 위해서 UI 숨기기")]
        public bool isOnlyHideUI = false;
        public bool isOnlyHideUI2 = false;
        [Header("튜토리얼 위해서 UI 숨기기")]
        public bool isNeedClickGuide = false;
        [Header("자동으로 다음 튜토리얼로")]
        public bool isAutoExecute = false;
        public int autoExecuteTime = 1000;
        private bool canExecute = false;
        private InGameMainPage inGameMainPage;

        private TutorialClickUIGuide tutorialClickUIGuide;

        public override void Enter()
        {
            if (isAutoExecute)
            {
                WaitCooltime(autoExecuteTime).Forget();
            }
            if (Global.UIManager.GetPages<InGameMainPage>().Count > 0)
            {
                inGameMainPage = Global.UIManager.GetPages<InGameMainPage>()[0];
                if (isOnlyShowUIAll)
                {
                    var tutorialVisibles = inGameMainPage.GetComponents<TutorialVisible>();
                    foreach (var tutorialVisible in tutorialVisibles)
                    {
                        tutorialVisible.SetVisible(true);
                    }
                    if (!isNeedClickGuide)
                        return;
                }
                if (isOnlyHideUI)
                {
                    var tutorialVisible = inGameMainPage.GetComponent<TutorialVisible>();
                    if (tutorialVisible != null)
                    {
                        tutorialVisible.Enter();
                    }
                    if (!isNeedClickGuide)
                        return;
                }
                if (isOnlyHideUI2)
                {
                    var tutorialVisible2 = inGameMainPage.GetComponents<TutorialVisible>()
                        .Skip(1).FirstOrDefault();
                    if (tutorialVisible2 != null)
                    {
                        tutorialVisible2.Enter();
                    }
                    if (!isNeedClickGuide)
                        return;
                }
                if (inGameMainPage.TryGetComponent(out tutorialClickUIGuide))
                {
                    tutorialClickUIGuide.isFinger = true;
                    tutorialClickUIGuide.Enter();
                }
            }
        }

        private async UniTaskVoid WaitCooltime(int cooltime)
        {
            await UniTask.Delay(cooltime); // 1초 대기
            canExecute = true;
        }

        public override void Execute(TutorialController controller)
        {
            if (tutorialClickUIGuide != null)
            {
                tutorialClickUIGuide.Execute(controller);
            }
            if (!canExecute) return;
            controller.SetNextTutorial();
        }

        public override void Exit()
        {

        }


    }
}