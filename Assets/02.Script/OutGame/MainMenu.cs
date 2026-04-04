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
        public static bool IsFirstMainMenuLoad = true;
        private MainMenuPage mainMenuPage = null;
        private PageSlideEffect pageSlideEffect = null;
        [SerializeField] private MapSelectView mapSelectView;
        public bool CanPlay = false;
        private bool isMapButtonClicked = false;
        private bool isHomeClicked = false;
        // 홈으로 한 번 이동하면 이후 클릭을 무시하기 위한 플래그
        private bool hasGoneHome = false;
        private MoveEffect moveEffect;
        private CameraRootPanning cameraRootPanning;
        private bool isCameraMoving = false;
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
            
            // CameraRootPanning 컴포넌트 찾기
            cameraRootPanning = FindAnyObjectByType<CameraRootPanning>();

            if (!IsFirstMainMenuLoad)
            {
                isHomeClicked = true;
                hasGoneHome = true;
                isMapButtonClicked = false;
                Camera.main.transform.localPosition = new Vector3(19.86f, Camera.main.transform.localPosition.y, Camera.main.transform.localPosition.z);
                moveEffect.MoveToTargetInstantly(); // 이동 효과 즉시 타겟으로 세팅
            }
            IsFirstMainMenuLoad = false;
            
            CanPlay = true;
            Global.UIManager.mouseUIController.FadeIn();
            // 인게임 스테이지에서 Start 씬으로 전환된 경우 리뷰 페이지 열기
            if (LoadingSceneManager.shouldOpenReviewPage)
            {
                Global.UIManager.OpenPage<InGameReviewPage>();
                LoadingSceneManager.shouldOpenReviewPage = false; // 플래그 리셋
            }
        }

        public void OnClickMapButton()
        {
            if(isMapButtonClicked || isCameraMoving) return;
            isMapButtonClicked = true;
            isHomeClicked = false;
            isCameraMoving = true;
            mapSelectView.Refresh();
            
            // 카메라 패닝 비활성화
            if (cameraRootPanning != null)
                cameraRootPanning.DisablePanningForCameraMove();
            
            Camera.main.transform.DOLocalMoveX(-19.86f, 1f)
                .SetEase(Ease.OutQuint)
                .OnComplete(() => 
                {
                    isCameraMoving = false;
                    // 카메라 이동 완료 후 패닝 재활성화 및 baseline 업데이트
                    if (cameraRootPanning != null)
                        cameraRootPanning.EnablePanningAfterCameraMove();
                });
            // pageSlideEffect.SlideOut(true, 0.8f);
        }

        public void OnClickMainMenuButton()
        {
            if(isCameraMoving) return;
            isMapButtonClicked = false;
            isHomeClicked = false;
            isCameraMoving = true;
            
            // 카메라 패닝 비활성화
            if (cameraRootPanning != null)
                cameraRootPanning.DisablePanningForCameraMove();
            
            Camera.main.transform.DOLocalMoveX(0f, 1f)
                .SetEase(Ease.OutQuint)
                .OnComplete(() => 
                {
                    isCameraMoving = false;
                    // 카메라 이동 완료 후 패닝 재활성화 및 baseline 업데이트
                    if (cameraRootPanning != null)
                        cameraRootPanning.EnablePanningAfterCameraMove();
                });
            moveEffect.PlayVFX();
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
            if (hasGoneHome) return;
            if(isCameraMoving) return;

            isHomeClicked = !isHomeClicked;
            isMapButtonClicked = false;
            isCameraMoving = true;

            if (cameraRootPanning != null)
                cameraRootPanning.DisablePanningForCameraMove();

            float targetX = isHomeClicked ? 19.86f : 0f;
            Camera.main.transform.DOKill();
            Camera.main.transform.DOLocalMoveX(targetX, 1f)
                .SetEase(isHomeClicked ? Ease.OutCubic : Ease.OutQuint)
                .OnComplete(() =>
                {
                    isCameraMoving = false;
                    if (cameraRootPanning != null)
                        cameraRootPanning.EnablePanningAfterCameraMove();
                    // 홈으로 이동한 경우, 이후 재호출 차단
                    if (isHomeClicked)
                        hasGoneHome = true;
                });
            moveEffect.PlayVFX();
        }
    }
}