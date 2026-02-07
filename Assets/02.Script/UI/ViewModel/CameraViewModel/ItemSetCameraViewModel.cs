using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using SO;
using Data;
using I2.Loc;
using UnityEngine;
using UnityWeld;
using UnityWeld.Binding;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DeskCat.FindIt.Scripts.Core.Main.System;
using UI.Page;
using TMPro;


namespace UI
{
    [Binding]
    public class ItemSetCameraViewModel : ViewModel
    {
        private string _itemSetName;
        [Binding]
        public string ItemSetName
        {
            get => _itemSetName;
            set
            {
                _itemSetName = value;
                itemSetNameText.text = _itemSetName;
                OnPropertyChanged(nameof(ItemSetName));
            }
        }

        private bool _isFocusIconActive;
        [Binding]
        public bool IsFocusIconActive
        {
            get => _isFocusIconActive;
            set
            {
                _isFocusIconActive = value;
                OnPropertyChanged(nameof(IsFocusIconActive));
            }
        }

        [Header("Camera Effect Components")]
        [SerializeField] private Image whiteoutImage; // 화이트아웃용 Image
        [SerializeField] private Image photoImage; // 촬영된 사진이 표시될 Image
        [SerializeField] private Image photoFrameImage; // 촬영된 사진이 표시될 Image

        [SerializeField] private RectTransform captureArea; // 캡처 영역 (선택사항)
        [SerializeField] private GameObject focusIcon; // 포커스 아이콘 (선택사항)
        [SerializeField] private TextMeshProUGUI itemSetNameText;

        private GameObject _targetObject;

        private float _originalCameraSize; // 연출 전 카메라 크기 저장
        
        /// <summary>
        /// 카메라 연출 시작
        /// </summary>
        /// <param name="targetObject">카메라가 이동할 대상 오브젝트</param>
        /// <param name="zoomSize">확대할 Orthographic Size (0 이하이면 줌 안 함)</param>
        /// <param name="onComplete">연출 완료 콜백</param>
        public async UniTask PlayCameraEffect(GameObject targetObject, float zoomSize = 0f, Action onComplete = null)
        {
            _targetObject = targetObject;
            IsFocusIconActive = false;
            
            // 터치 입력 차단 (카메라 연출 동안 터치 방지)
            if (Global.InputManager != null)
            {
                Global.InputManager.DisableGameInputOnly();
            }
            
            // 카메라 연출 중 플래그 설정
            if (ItemSetManager.Instance != null)
            {
                ItemSetManager.Instance.IsPlayingCameraEffect = true;
            }

            // 1. 카메라 이동 & 줌 & 블러 시작
            if (_targetObject != null && Util.CameraSetting.CameraView2D.Instance != null)
            {
                // 원래 카메라 크기 저장 (복원용)
                _originalCameraSize = Util.CameraSetting.CameraView2D.Instance.CurrentOrthographicSize;
                
                // 블러 시작 (흐리게)
                if (Global.UIEffectManager != null && Global.UIEffectManager.BlurController != null)
                {
                    Global.UIEffectManager.BlurController.TurnOnBlur(.1f); // 강한 블러
                }

                // 카메라 이동 + 줌 (1초) - zoomSize가 유효하면 이동과 줌을 동시에
                if (zoomSize > 0f)
                {
                    await Util.CameraSetting.CameraView2D.Instance.MoveCameraAndZoomAsync(
                        _targetObject.transform.position, zoomSize, 1f);
                }
                else
                {
                    await Util.CameraSetting.CameraView2D.Instance.MoveCameraToPositionAsync(
                        _targetObject.transform.position, 1f);
                }
            }
            
            // LevelManager UI 숨기기
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.HideUI();
            }

            // InGameMainPage 버튼들 숨기기
            var inGamePages = Global.UIManager.GetPages<InGameMainPage>();
            foreach (var page in inGamePages)
            {
                page.HideAllGroup();
            }

            // 2. 블러 유지한 채로 1초 대기
            await UniTask.Delay(1000);

            // 3. 초점 맞추기 (블러 1 -> 0) & 초점 아이콘 활성화
            IsFocusIconActive = true;
            if (focusIcon != null)
            {
                focusIcon.SetActive(true);
            }
            
            if (Global.UIEffectManager != null && Global.UIEffectManager.BlurController != null)
            {
                // 1초 동안 블러 해제 (초점 맞추는 연출)
                Global.UIEffectManager.BlurController.BlurFadeOut(.75f).Forget();
                await UniTask.Delay(1000);
            }
            else
            {
                await UniTask.Delay(1000);
            }

            // 4. 화이트아웃 효과 (흰색 60% 불투명도)
            if (whiteoutImage != null)
            {
                whiteoutImage.gameObject.SetActive(true);
                Color whiteColor = Color.white;
                whiteColor.a = 0f;
                whiteoutImage.color = whiteColor;

                // 0.3초 동안 60% 불투명도로 페이드인
                float duration = 0.3f;
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float alpha = Mathf.Lerp(0f, 0.6f, elapsed / duration);
                    whiteColor.a = alpha;
                    whiteoutImage.color = whiteColor;
                    await UniTask.Yield();
                }
                whiteColor.a = 0f;
                whiteoutImage.color = whiteColor;
            }
            if(focusIcon != null)
            {
                focusIcon.SetActive(false);
            }

            // 5. 사진 찍기
            await CaptureAndSaveScreenshot();

            // 6. 화이트아웃 해제
            if (whiteoutImage != null)
            {
                Color whiteColor = whiteoutImage.color;
                float duration = 0.3f;
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float alpha = Mathf.Lerp(0.6f, 0f, elapsed / duration);
                    whiteColor.a = alpha;
                    whiteoutImage.color = whiteColor;
                    await UniTask.Yield();
                }
                whiteoutImage.gameObject.SetActive(false);
            }

            // 7. 2초 대기
            await UniTask.Delay(2000);

            // 카메라 줌 복원 (원래 크기로 되돌리기)
            if (Util.CameraSetting.CameraView2D.Instance != null && _originalCameraSize > 0f)
            {
                await Util.CameraSetting.CameraView2D.Instance.ZoomCameraToSizeAsync(_originalCameraSize, 0.5f);
            }
            
            // 완료 콜백 호출
            onComplete?.Invoke();

            // LevelManager UI 복원
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.ShowUI();
            }

            // InGameMainPage 버튼들 복원
            var restorePages = Global.UIManager.GetPages<InGameMainPage>();
            foreach (var page in restorePages)
            {
                page.ShowAllGroup();
            }
            
            // 카메라 연출 중 플래그 해제
            if (ItemSetManager.Instance != null)
            {
                ItemSetManager.Instance.IsPlayingCameraEffect = false;
            }
            
            // 터치 입력 복원
            if (Global.InputManager != null)
            {
                Global.InputManager.EnableGameInputOnly();
            }
        }

        private async UniTask CaptureAndSaveScreenshot()
        {
            // 캡처 영역이 지정된 경우 해당 영역만 캡처, 아니면 전체 화면 캡처
            Texture2D screenshot = null;
            
            if (captureArea != null)
            {
                // 지정된 영역만 캡처
                screenshot = await CaptureRectArea(captureArea);
            }
            else
            {
                // 전체 화면 캡처
                await UniTask.WaitForEndOfFrame();
                screenshot = ScreenCapture.CaptureScreenshotAsTexture();
            }

            if (screenshot != null)
            {
                // 파일로 저장
                string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string filename = $"Mission_{ItemSetName}_{timestamp}.png";
                byte[] bytes = screenshot.EncodeToPNG();
                System.IO.File.WriteAllBytes(filename, bytes);
                
                Debug.Log($"[Screenshot] Saved to {filename}");

                // 사진 이미지 객체에 sprite로 할당
                if (photoImage != null)
                {
                    Sprite photoSprite = Sprite.Create(
                        screenshot,
                        new Rect(0, 0, screenshot.width, screenshot.height),
                        new Vector2(0.5f, 0.5f)
                    );
                    photoImage.sprite = photoSprite;
                    photoFrameImage.gameObject.SetActive(true);
                }

                // 카메라 셔터 사운드 재생
                if (Global.SoundManager != null)
                {
                    // Global.SoundManager.PlaySFX(SFXEnum.CameraShutter); // 예시
                }
            }

            await UniTask.Yield();
        }

        private async UniTask<Texture2D> CaptureRectArea(RectTransform rectTransform)
        {
            await UniTask.WaitForEndOfFrame();

            // RectTransform의 화면 좌표 계산
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            
            // 화면 좌표로 변환
            Vector2 min = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(null, corners[2]);

            // 좌표 보정 (화면 크기 내로 제한)
            int x = Mathf.Clamp((int)min.x, 0, Screen.width);
            int y = Mathf.Clamp((int)min.y, 0, Screen.height);
            int width = Mathf.Clamp((int)(max.x - min.x), 1, Screen.width - x);
            int height = Mathf.Clamp((int)(max.y - min.y), 1, Screen.height - y);

            // 전체 화면 캡처
            Texture2D fullScreenshot = ScreenCapture.CaptureScreenshotAsTexture();
            
            // 지정된 영역만 추출
            Texture2D croppedTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
            Color[] pixels = fullScreenshot.GetPixels(x, y, width, height);
            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();

            // 전체 스크린샷 텍스처 정리
            UnityEngine.Object.Destroy(fullScreenshot);

            return croppedTexture;
        }
    }
}
