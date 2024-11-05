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
using System;
using Data;
using UnityWeld;

namespace OutGame
{
    public class MainMenu : SceneBase
    {
        public static float Speed = 1;
        private MainMenuPage mainMenuPage = null;
        private PageSlideEffect pageSlideEffect = null;
        [SerializeField] private MapSelectView mapSelectView;
        public bool CanPlay = false;
        protected override void Start()
        {
            base.Start();
            mainMenuPage = Global.UIManager.OpenPage<MainMenuPage>();
            if(mapSelectView == null) mapSelectView = FindObjectOfType<MapSelectView>();
            if (mainMenuPage != null)
            {
                pageSlideEffect = mainMenuPage.GetComponent<PageSlideEffect>();
            }
            CanPlay = true;
        }

        public void OnClickMapButton()
        {
            mapSelectView.Refresh();
            Camera.main.transform.DOLocalMoveX(-19.86f, 1f).SetEase(Ease.OutQuint);
            pageSlideEffect.SlideOut(true, 0.8f);
        }

        public void OnClickMainMenuButton()
        {
            Camera.main.transform.DOLocalMoveX(0f, 1f).SetEase(Ease.OutQuint);
            pageSlideEffect.SlideIn(true, 0.8f);
        }

        public void OnClickStartButton(int stageIndex = 0)
        {
            if(CanPlay)
                LoadingSceneManager.LoadScene(stageIndex + 4);
        }
    }
}