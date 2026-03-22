using Manager;
using UnityEngine;
using UnityEngine.UI;
using UnityWeld;
using UnityWeld.Binding;
using System.Collections;

namespace UI
{
    [Binding]
    public class GuideViewModel : ViewModel
    {
        [SerializeField] private RectTransform _maskArea;  // 마스크 영역
        [SerializeField] private RectTransform _darkPanel; // 어두운 배경 패널
        [SerializeField] private RectTransform _fingerImage; // 손가락 이미지
        [SerializeField] private float _animationDuration = 0.5f; // 애니메이션 시간
        [SerializeField] private float _fingerOffsetY = 100f; // Y축 오프셋 (위아래 거리)
        [SerializeField] private float _fingerOffsetX = 0f;   // X축 오프셋 (좌우 거리)
        [SerializeField] private float _fingerBobAmount = 20f; // 위아래 움직임 크기
        [SerializeField] private float _fingerBobSpeed = 2f; // 위아래 움직임 속도
        
        private Vector2 _targetSize;
        private Vector2 _targetPosition;
        private Vector2 _fingerBasePosition;
        private bool _isUITarget;
        private bool _isFinger;
        private Canvas _canvas;
        private Image _maskImage;
        private Sprite _defaultMaskSprite;

        protected override void Awake()
        {
            base.Awake();
            _canvas = Global.UIManager.uiCanvas;
            if (_canvas == null)
            {
                Debug.LogError("GuideViewModel requires a Canvas parent!");
            }
            
            _maskImage = _maskArea.GetComponent<Image>();
            if (_maskImage != null)
            {
                _defaultMaskSprite = _maskImage.sprite;
            }
        }

        // UI 요소용 가이드 설정
        public void SetTargetGuide(RectTransform target, Vector2 offset = default, bool isFinger = true, bool isLayoutElement = false, Sprite customMaskSprite = null)
        {
            _isUITarget = true;
            _isFinger = isFinger;

            if (_maskImage != null)
            {
                _maskImage.sprite = customMaskSprite != null ? customMaskSprite : _defaultMaskSprite;
            }

            // 캔버스와 카메라 설정 가져오기
            RectTransform canvasRect = _canvas.GetComponent<RectTransform>();
            Camera cam = _canvas.worldCamera;

            // 타겟의 월드 위치 얻기
            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners);

            // 월드 코너를 스크린을 거쳐 캔버스 로컬 좌표로 변환해 정확한 렌더링 크기 계산
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                RectTransformUtility.WorldToScreenPoint(cam, corners[0]),
                cam,
                out Vector2 localBottomLeft
            );
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                RectTransformUtility.WorldToScreenPoint(cam, corners[2]),
                cam,
                out Vector2 localTopRight
            );

            _targetSize = new Vector2(
                Mathf.Abs(localTopRight.x - localBottomLeft.x),
                Mathf.Abs(localTopRight.y - localBottomLeft.y)
            );
            
            Debug.Log($"Target Size: {_targetSize}, Target Rect Size: {target.rect.size}, Mask Size: {_maskArea.sizeDelta}");

            // 타겟의 중심점 월드 위치 계산
            Vector3 worldCenter = (corners[0] + corners[2]) * 0.5f;

            // 월드 좌표를 스크린 좌표로 변환
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldCenter);
            
            // 스크린 좌표를 캔버스 로컬 좌표로 변환
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPoint,
                cam,
                out Vector2 localPoint
            );

            // 마스크 영역 설정
            _maskArea.anchorMin = new Vector2(0.5f, 0.5f);
            _maskArea.anchorMax = new Vector2(0.5f, 0.5f);
            _maskArea.pivot = new Vector2(0.5f, 0.5f);
            
            // 위치와 크기 설정
            _maskArea.sizeDelta = _targetSize;
            _maskArea.anchoredPosition = localPoint + offset;

            // 손가락 이미지 설정
            if(!isFinger)
            {
                _fingerImage.gameObject.SetActive(false);
            }
            else
            {
                _fingerImage.gameObject.SetActive(true);
                
                // 손가락 위치 설정
                _fingerImage.anchorMin = new Vector2(0.5f, 0.5f);
                _fingerImage.anchorMax = new Vector2(0.5f, 0.5f);
                _fingerImage.pivot = new Vector2(0.5f, 1f);
                
                // 손가락 위치 계산 (타겟의 하단 중앙)
                Vector2 bottomCenter = new Vector2(localPoint.x, localPoint.y - (_targetSize.y * 0.5f));
                _fingerBasePosition = bottomCenter + new Vector2(_fingerOffsetX, -_fingerOffsetY);
                _fingerImage.anchoredPosition = _fingerBasePosition;
            }

            StartCoroutine(AnimateMaskToTargetUI());
        }

        // Sprite 렌더러용 가이드 설정
        public void SetTargetGuide(SpriteRenderer target, Sprite customMaskSprite = null)
        {
            if (_maskImage != null)
            {
                _maskImage.sprite = customMaskSprite != null ? customMaskSprite : _defaultMaskSprite;
            }

            Camera mainCam = Camera.main;
            Canvas canvas = _maskArea.GetComponentInParent<Canvas>();
            
            // 스프라이트의 월드 크기를 스크린 좌표로 변환
            Vector2 spriteSize = target.bounds.size;
            
            // 스프라이트의 월드 위치를 스크린 좌표로 변환 후 Canvas 기준으로 변경
            Vector3 screenPos = mainCam.WorldToScreenPoint(target.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                screenPos,
                canvas.worldCamera,
                out Vector2 localPoint
            );
            
            // Canvas의 스케일을 고려한 크기 계산
            float canvasScale = canvas.scaleFactor;
            _targetSize = spriteSize * (Screen.height / mainCam.orthographicSize / 2f) / canvasScale;
            _targetPosition = (Vector2)canvas.GetComponent<RectTransform>().TransformPoint(localPoint);
            
            StartCoroutine(AnimateMaskToTarget());
        }

        private IEnumerator AnimateMaskToTarget()
        {
            float elapsedTime = 0f;
            Vector2 startSize = _maskArea.sizeDelta;
            Vector2 startPos = _maskArea.position;
            Vector2 fingerStartPos = _fingerImage.position;

            // 손가락 위치 계산 (타겟의 중앙 아래쪽)
            Canvas canvas = _maskArea.GetComponentInParent<Canvas>();
            Vector2 targetScreenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, _targetPosition);
            Vector2 fingerScreenPos = targetScreenPos + new Vector2(_fingerOffsetX, -(_targetSize.y/2 + _fingerOffsetY));
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                fingerScreenPos,
                canvas.worldCamera,
                out Vector2 localFingerPoint
            );
            _fingerBasePosition = canvas.GetComponent<RectTransform>().TransformPoint(localFingerPoint);
            
            while (elapsedTime < _animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / _animationDuration;
                
                _maskArea.sizeDelta = Vector2.Lerp(startSize, _targetSize, progress);
                _maskArea.position = Vector2.Lerp(startPos, _targetPosition, progress);
                _fingerImage.position = Vector2.Lerp(fingerStartPos, _fingerBasePosition, progress);
                
                yield return null;
            }

            _maskArea.sizeDelta = _targetSize;
            _maskArea.position = _targetPosition;
            _fingerImage.position = _fingerBasePosition;

            // 손가락 위아래 움직임 시작
            StartCoroutine(AnimateFinger());
        }

        private IEnumerator AnimateMaskToTargetUI()
        {
            float elapsedTime = 0f;
            Vector2 startSize = Vector2.zero;
            Vector2 fingerStartPos = _fingerImage.anchoredPosition;

            while (elapsedTime < _animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / _animationDuration;
                
                _maskArea.sizeDelta = Vector2.Lerp(startSize, _targetSize, progress);
                
                if (_isFinger)
                {
                    _fingerImage.anchoredPosition = Vector2.Lerp(fingerStartPos, _fingerBasePosition, progress);
                }
                
                yield return null;
            }

            _maskArea.sizeDelta = _targetSize;
            if (_isFinger)
            {
                _fingerImage.anchoredPosition = _fingerBasePosition;
                StartCoroutine(AnimateFinger());
            }
        }

        private IEnumerator AnimateFinger()
        {
            Vector2 basePos = _fingerImage.anchoredPosition;
            
            while (true)
            {
                float yOffset = Mathf.Sin(Time.time * _fingerBobSpeed) * _fingerBobAmount;
                _fingerImage.anchoredPosition = basePos + new Vector2(0, yOffset);
                yield return null;
            }
        }

        // 가이드 종료 시 호출
        public void HideGuide()
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
        }
    }
}