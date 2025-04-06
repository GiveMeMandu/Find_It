using UnityEngine;
using DG.Tweening;
using Manager;
using UI.Page;
using UI.Effect;
using UnityWeld;
using System;
using Cysharp.Threading.Tasks;

namespace OutGame
{
    public class CreditScene : SceneBase
    {
        [SerializeField] private MapSelectView mapSelectView;
        [SerializeField] private Cart cart;
        [SerializeField] private Transform IntroBackground1;
        [SerializeField] private Transform IntroBackground2;
        [SerializeField] private Transform logo;
        [SerializeField] private float background1Width = 100.0f;
        [SerializeField] private float background2Width = 100.0f;
        [SerializeField] private float scrollSpeed = 2.0f;

        protected override void Start()
        {
            base.Start();
            WaitForCartStop().Forget();
            BackgroundMove().Forget();
        }

        private async UniTask WaitForCartStop()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2));
            cart.StopWheel();
            await UniTask.Delay(TimeSpan.FromSeconds(5));
            logo.gameObject.SetActive(false);
        }

        private async UniTask BackgroundMove()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(3));
            cart.StartWheel();
            
            Vector3 bg1InitialPos = IntroBackground1.position;
            Vector3 bg2InitialPos = IntroBackground2.position;
            
            // 배경 이미지 너비 사용 (인스펙터에서 설정 가능)
            float bg1Width = background1Width;
            float bg2Width = background2Width;
            
            // 초기 위치 조정 코드 제거 - Unity Editor에서 직접 배치한 위치 유지
            
            Debug.Log($"배경1 위치: {bg1InitialPos}, 배경2 위치: {bg2InitialPos}, 너비1: {bg1Width}, 너비2: {bg2Width}");
            
            while (true)
            {
                // 배경 이동
                IntroBackground1.Translate(Vector3.left * scrollSpeed * Time.deltaTime);
                IntroBackground2.Translate(Vector3.left * scrollSpeed * Time.deltaTime);
                
                // 첫 번째 배경이 화면 밖으로 완전히 벗어나면 (화면 왼쪽 경계 기준)
                if (IntroBackground1.position.x + bg1Width < Camera.main.transform.position.x - (Camera.main.orthographicSize * Camera.main.aspect))
                {
                    // 두 번째 배경 뒤로 이동
                    float newXPos = IntroBackground2.position.x + bg2Width;
                    IntroBackground1.position = new Vector3(newXPos, bg1InitialPos.y, bg1InitialPos.z);
                    Debug.Log($"배경1 재배치: {IntroBackground1.position}, 기준점: {Camera.main.transform.position.x - (Camera.main.orthographicSize * Camera.main.aspect)}");
                }
                
                // 두 번째 배경이 화면 밖으로 완전히 벗어나면 (화면 왼쪽 경계 기준)
                if (IntroBackground2.position.x + bg2Width < Camera.main.transform.position.x - (Camera.main.orthographicSize * Camera.main.aspect))
                {
                    // 첫 번째 배경 뒤로 이동
                    float newXPos = IntroBackground1.position.x + bg1Width;
                    IntroBackground2.position = new Vector3(newXPos, bg2InitialPos.y, bg2InitialPos.z);
                    Debug.Log($"배경2 재배치: {IntroBackground2.position}, 기준점: {Camera.main.transform.position.x - (Camera.main.orthographicSize * Camera.main.aspect)}");
                }
                
                await UniTask.Yield();
            }
        }

        public void OnClickMapButton()
        {
            mapSelectView.Refresh();
            Camera.main.transform.DOLocalMoveX(-19.86f, 1f).SetEase(Ease.OutQuint);
        }

        public void OnClickMainMenuButton()
        {
            Camera.main.transform.DOLocalMoveX(0f, 1f).SetEase(Ease.OutQuint);
        }

        public void OnClickStartButton(int stageIndex = 0)
        {
                LoadingSceneManager.LoadScene(stageIndex + 4);
        }
    }
}