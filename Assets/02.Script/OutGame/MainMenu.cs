using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Manager;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using UI.Page;
using UI.Effect;

namespace OutGame
{
    public class MainMenu : SceneBase
    {
        public static float Speed = 1;
        private MainMenuPage mainMenuPage = null;
        private PageSlideEffect pageSlideEffect = null;
        protected override void Start()
        {
            base.Start();
            mainMenuPage = Global.UIManager.OpenPage<MainMenuPage>();
            if (mainMenuPage != null)
            {
                mainMenuPage.CanPlay = true;
                pageSlideEffect = mainMenuPage.GetComponent<PageSlideEffect>();
            }
        }

        public void OnClickMapButton()
        {
            Camera.main.transform.DOLocalMoveX(-19.86f, 1f).SetEase(Ease.OutQuint);
            pageSlideEffect.SlideOut();
        }

    }
}