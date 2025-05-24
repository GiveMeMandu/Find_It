using System;
using DeskCat.FindIt.Scripts.Core.Main.System;
using Manager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityWeld.Binding;
using Cysharp.Threading.Tasks;

namespace UI.Page
{
    [Binding]
    public class InGameMainPage : PageViewModel
    {
        private bool _showSkipButton;

        [Binding]
        public bool ShowSkipButton
        {
            get => _showSkipButton;
            set
            {
                _showSkipButton = value;
                OnPropertyChanged(nameof(ShowSkipButton));
            }
        }
        private bool _showMissionButton;
        [Binding]
        public bool ShowMissionButton
        {
            get => _showMissionButton;
            set
            {
                _showMissionButton = value;
                OnPropertyChanged(nameof(ShowMissionButton));
            }
        }

        private bool _showFoundObjToolTip;

        [Binding]
        public bool ShowFoundObjToolTip
        {
            get => _showFoundObjToolTip;
            set
            {
                _showFoundObjToolTip = value;
                OnPropertyChanged(nameof(ShowFoundObjToolTip));
            }
        }

        private Vector3 _tooltipLocalScale = Vector3.one;

        [Binding]
        public Vector3 TooltipLocalScale
        {
            get => _tooltipLocalScale;
            set
            {
                _tooltipLocalScale = value;
                TooltipTextLocalScale = value;
                OnPropertyChanged(nameof(TooltipLocalScale));
            }
        }
        private Vector3 _tooltipTextLocalScale = Vector3.one;

        [Binding]
        public Vector3 TooltipTextLocalScale
        {
            get => _tooltipTextLocalScale;
            set
            {
                _tooltipTextLocalScale = value;
                OnPropertyChanged(nameof(TooltipTextLocalScale));
            }
        }
        private Vector2 _foundObjToolTipPos;

        [Binding]
        public Vector2 FoundObjToolTipPos
        {
            get => _foundObjToolTipPos;
            set
            {
                _foundObjToolTipPos = value;
                OnPropertyChanged(nameof(FoundObjToolTipPos));
            }
        }

        private HiddenObj _currentFoundObj;
        private Vector2 _tooltipSize = new Vector2(235.2f, 135f);

        private void OnEnable()
        {
            ShowFoundObjToolTip = false;
            ShowSkipButton = false;
            ShowMissionButton = false;
            _currentFoundObj = null;
        }

        private void Start()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnFoundObj += OnFoundObj;
            }
            
            if (ItemSetManager.Instance != null)
            {
                ItemSetManager.Instance.OnSetCompleted += OnSetCompleted;
                // 이미 완료된 미션이 있다면 버튼 활성화
                if (ItemSetManager.Instance.FoundSetsCount > 0)
                {
                    ShowMissionButton = true;
                }
            }
        }

        private void OnDestroy()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnFoundObj -= OnFoundObj;
            }
            
            if (ItemSetManager.Instance != null)
            {
                ItemSetManager.Instance.OnSetCompleted -= OnSetCompleted;
            }
        }

        private void OnSetCompleted(string setName)
        {
            ShowMissionButton = true;
        }

        private void Update()
        {
            if (ShowFoundObjToolTip && _currentFoundObj != null)
            {
                UpdateToolTipPosition(_currentFoundObj);
            }
        }

        private void OnFoundObj(object sender, HiddenObj e)
        {
            ShowFoundObjAsync(e).Forget();
        }

        private async UniTask ShowFoundObjAsync(HiddenObj e)
        {
            _currentFoundObj = e;
            UpdateToolTipPosition(e);

            ShowFoundObjToolTip = true;
            await UniTask.Delay(TimeSpan.FromSeconds(0.6f));
            ShowFoundObjToolTip = false;
            _currentFoundObj = null;
        }

        private void UpdateToolTipPosition(HiddenObj obj)
        {
            var camera = Camera.main;
            Vector3 objPosition;
            SpriteRenderer spriteRenderer = null;

            if (obj.BgAnimationTransform != null && obj.BgAnimationSpriteRenderer != null)
            {
                // BgAnimationLerp의 현재 위치 사용
                if (obj.BgAnimationLerp != null)
                {
                    objPosition = obj.BgAnimationSpriteRenderer.transform.position;
                    // BgAnimationSpriteRenderer의 스프라이트 크기 고려
                    if (obj.BgAnimationSpriteRenderer.sprite != null)
                    {
                        float spriteHeight = obj.BgAnimationSpriteRenderer.sprite.bounds.size.y;

                        // 현재 적용된 스케일 또는 목표 스케일 사용
                        spriteHeight *= obj.BgAnimationLerp.ToScale.y;
                        objPosition.y += spriteHeight * 0.5f;  // 스프라이트의 상단 위치로 조정
                    }
                }
                else
                {
                    // 기존 fallback 코드 유지
                    Bounds bounds = obj.BgAnimationLerp.InitialBounds;
                    objPosition = new Vector3(
                        bounds.center.x,
                        bounds.max.y,
                        bounds.center.z
                    );
                }
            }
            else if (obj.spriteRenderer != null)
            {
                spriteRenderer = obj.spriteRenderer;
                objPosition = obj.transform.position;
                float spriteHeight = spriteRenderer.sprite.bounds.size.y * obj.transform.localScale.y;
                objPosition.y += spriteHeight * 0.5f;  // 스프라이트의 상단 위치로 조정
            }
            else
            {
                objPosition = obj.transform.position;
            }
            Vector3 screenPos = camera.WorldToScreenPoint(objPosition);

            // 객체가 카메라 시야에서 벗어났는지 확인
            if (screenPos.x < 0 || screenPos.x > Screen.width ||
                screenPos.y < 0 || screenPos.y > Screen.height ||
                screenPos.z < 0)
            {
                ShowFoundObjToolTip = false;
                return;
            }

            // Canvas의 ScaleFactor를 가져옵니다
            var canvas = GetComponentInParent<Canvas>();
            float scaleFactor = canvas.scaleFactor;

            // 스크린의 중앙을 (0,0)으로 하는 좌표계로 변환 (scaleFactor 적용)
            Vector2 localPoint = new Vector2(
                (screenPos.x - Screen.width * 0.5f) / scaleFactor,
                (screenPos.y - Screen.height * 0.5f) / scaleFactor
            );

            // 툴팁 크기도 scaleFactor로 나누어 조정
            Vector2 tooltipHalfSize = _tooltipSize * 0.5f;

            // 화면 경계 계산 (scaleFactor 적용)
            Vector2 screenHalfSize = new Vector2(
                (Screen.width * 0.5f) / scaleFactor,
                (Screen.height * 0.5f) / scaleFactor
            );
            Vector2 maxBound = screenHalfSize - tooltipHalfSize;
            Vector2 minBound = -maxBound;

            // 기본 스케일 설정
            TooltipLocalScale = Vector3.one;

            // 화면 위로 넘어갈 경우
            if (localPoint.y + tooltipHalfSize.y > maxBound.y)
            {
                // 위치를 오브젝트 아래로 이동
                if (obj.BgAnimationTransform != null && obj.BgAnimationSpriteRenderer != null)
                {
                    // 배경 이미지의 하단 위치로 이동
                    objPosition = new Vector3(
                        obj.BgAnimationLerp.InitialBounds.center.x,
                        obj.BgAnimationLerp.InitialBounds.min.y,
                        obj.BgAnimationLerp.InitialBounds.center.z
                    );
                }
                else if (spriteRenderer != null)
                {
                    Bounds bounds = spriteRenderer.bounds;
                    objPosition = new Vector3(
                        bounds.center.x,
                        bounds.min.y,
                        bounds.center.z
                    );
                }

                screenPos = camera.WorldToScreenPoint(objPosition);
                screenPos -= new Vector3(0, _tooltipSize.y * scaleFactor, 0);
                localPoint = new Vector2(
                    (screenPos.x - Screen.width * 0.5f) / scaleFactor,
                    (screenPos.y - Screen.height * 0.5f) / scaleFactor
                );

                // 스케일 y축 반전
                TooltipLocalScale = new Vector3(1, -1, 1);
            }

            // 툴팁이 화면 밖으로 나가지 않도록 위치 조정
            localPoint.x = Mathf.Clamp(localPoint.x, minBound.x, maxBound.x);
            localPoint.y = Mathf.Clamp(localPoint.y, minBound.y, maxBound.y);

            FoundObjToolTipPos = localPoint;
        }

        [Binding]
        public void OnClickOptionButton()
        {
            Global.UIManager.OpenPage<OptionPage>();
        }
        [Binding]
        public void OnClickAdButton()
        {
            // Global.GoogleMobileAdsManager.ShowRewardedAd();
        }


        [Binding]
        public void OnClickSkipButton()
        {
            var inGameSceneBase = FindFirstObjectByType<OutGame.InGameSceneBase>();
            inGameSceneBase.SkipIntro();
        }


        [Binding]
        public void OnClickRotateUIButton()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.ToggleScrollView();
            }
        }
    }
}