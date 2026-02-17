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
using Sirenix.OdinInspector;
using UnityWeld.Binding;

namespace OutGame
{
    public class MainMenu : SceneBase
    {
        private MainMenuPage mainMenuPage = null;
        private PageSlideEffect pageSlideEffect = null;
        [SerializeField] private MapSelectView mapSelectView;
        public bool CanPlay = false;
        private bool isMapButtonClicked = false;
        private bool isHomeClicked = false;
        private MoveEffect moveEffect;
        protected override void Start()
        {
            base.Start();
            mainMenuPage = Global.UIManager.OpenPage<MainMenuPage>();
            if(mapSelectView == null) mapSelectView = FindAnyObjectByType<MapSelectView>();
            if (mainMenuPage != null)
            {
                pageSlideEffect = mainMenuPage.GetComponent<PageSlideEffect>();
                moveEffect = mainMenuPage.GetComponent<MoveEffect>();
            }
            CanPlay = true;
            
            // 인게임 스테이지에서 Start 씬으로 전환된 경우 리뷰 페이지 열기
            if (LoadingSceneManager.shouldOpenReviewPage)
            {
                Global.UIManager.OpenPage<InGameReviewPage>();
                LoadingSceneManager.shouldOpenReviewPage = false; // 플래그 리셋
            }
        }

        public void OnClickMapButton()
        {
            if(isMapButtonClicked) return;
            isMapButtonClicked = true;
            mapSelectView.Refresh();
            Camera.main.transform.DOLocalMoveX(-19.86f, 1f).SetEase(Ease.OutQuint);
            // pageSlideEffect.SlideOut(true, 0.8f);
        }

        public void OnClickMainMenuButton()
        {
            isMapButtonClicked = false;
            Camera.main.transform.DOLocalMoveX(0f, 1f).SetEase(Ease.OutQuint);
            // pageSlideEffect.SlideIn(true, 0.8f);
        }

        public void OnClickStartButton(int stageIndex = 0)
        {
            // Debug.Log($"[MainMenu] OnClickStartButton called with stageIndex: {stageIndex}");
            // if(CanPlay)
            //     LoadingSceneManager.LoadScene(stageIndex);
            // isMapButtonClicked = false;
            // isHomeClicked = false;
            // Camera.main.transform.DOLocalMoveX(0f, 1f).SetEase(Ease.OutQuint);
        }

        [Button("테스트용: 홈버튼")]
        public void OnClickHomeButton()
        {
            if(isHomeClicked) return;
            isHomeClicked = true;
            Camera.main.transform.DOLocalMoveX(19.86f, 1f).SetEase(Ease.OutQuint);
            moveEffect.PlayVFX();
        }
    }
}