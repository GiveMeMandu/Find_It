using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace DeskCat.FindIt.Scripts.Core.Main.System
{
    public class DialogToast : MonoBehaviour
    {
        public Text dialogText;
        public Transform OriginalTransform;
        public Vector2 AnchorPosition;
        private RectTransform rectTransform;
        private bool startChecking = false;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (!gameObject.activeSelf) return;
            
            // 마우스 클릭이나 터치 입력이 있을 때 체크 시작
            if (!startChecking)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame || 
                    (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame))
                {
                    startChecking = true;
                }
                return;
            }

            // 현재 마우스/터치 위치 가져오기
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.isInProgress)
            {
                mousePosition = Touchscreen.current.primaryTouch.position.ReadValue();
            }
            // 스크린 좌표를 RectTransform 좌표로 변환
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                mousePosition,
                null,
                out Vector2 localPoint);

            // RectTransform 영역을 벗어났는지 확인
            if (!rectTransform.rect.Contains(localPoint))
            {
                gameObject.SetActive(false);
            }
        }

        public void Initialize(string dialog, Transform objTransform)
        {
            startChecking = false;
            dialogText.text = dialog;
            transform.SetParent(objTransform);
            gameObject.SetActive(false);
            GetComponent<RectTransform>().anchoredPosition = AnchorPosition;
            gameObject.SetActive(true);
            transform.gameObject.GetComponent<Animator>().enabled = true;
            transform.SetParent(OriginalTransform);
        }
        
        public void DisableAnimator() {
            transform.gameObject.GetComponent<Animator>().enabled = false;
        }
    }
}