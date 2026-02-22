using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;

public class MouseUIController : MonoBehaviour
{
    public static MouseUIController Instance => FindAnyObjectByType<MouseUIController>();
    public static Vector2 Position => Instance.MousePosition;
    
    public enum EMouseState
    {
        Idle,
        Click
    }

    [Header("Mouse UI")]
    [Tooltip("마우스 커서 UI 루트 오브젝트")]
    public RectTransform MouseCursorUI;
    
    [Tooltip("Idle 상태 스프라이트")]
    public Sprite IdleSprite;
    
    [Tooltip("Click 상태 스프라이트")]
    public Sprite ClickSprite;

    [Header("Fade Settings")]
    [Tooltip("페이드 인/아웃 지속 시간")]
    public float FadeDuration = 0.3f;
    
    [Tooltip("클릭 애니메이션 지속 시간")]
    public float ClickAnimationDuration = 0.1f;


    private EMouseState _currentState = EMouseState.Idle;
    private Image _cursorImage;
    private CanvasGroup _canvasGroup;
    public Vector2 MousePosition = new Vector2();

    public void Init()
    {
        // 마우스 위치 초기화
        MousePosition = Mouse.current?.position.value ?? Vector2.zero;
        
        // UI 컴포넌트 초기화
        if (MouseCursorUI != null)
        {
            _cursorImage = MouseCursorUI.GetComponent<Image>();
            if (_cursorImage == null)
            {
                _cursorImage = MouseCursorUI.GetComponentInChildren<Image>();
            }
            
            _canvasGroup = MouseCursorUI.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = MouseCursorUI.gameObject.AddComponent<CanvasGroup>();
            }
            
            // 초기 상태 설정
            SetState(EMouseState.Idle);
            _canvasGroup.alpha = 0f; // 시작은 투명하게
        }
    }

    private void Update()
    {
        UpdateMousePosition();
        UpdateMouseInput();
    }

    // 마우스 위치 업데이트
    private void UpdateMousePosition()
    {
        // 컴퓨터 마우스 커서 숨기기
        Cursor.visible = false;
        
        // 마우스 위치 가져오기
        MousePosition = Mouse.current?.position.value ?? Input.mousePosition;
        
        // UI 위치 업데이트
        if (MouseCursorUI != null)
        {
            MouseCursorUI.position = MousePosition;
        }
    }

    // 마우스 입력 처리
    private void UpdateMouseInput()
    {
        if (Mouse.current == null) return;
        
        // 클릭 감지
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            OnMouseDown();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            OnMouseUp();
        }
    }

    // 마우스 다운 이벤트
    private void OnMouseDown()
    {
        SetState(EMouseState.Click);
        
        // 클릭 애니메이션 (스케일 효과)
        if (MouseCursorUI != null)
        {
            MouseCursorUI.DOKill();
            MouseCursorUI.DOScale(0.9f, ClickAnimationDuration)
                .SetEase(Ease.OutQuad);
        }
    }

    // 마우스 업 이벤트
    private void OnMouseUp()
    {
        SetState(EMouseState.Idle);
        
        // 원래 크기로 복귀
        if (MouseCursorUI != null)
        {
            MouseCursorUI.DOKill();
            MouseCursorUI.DOScale(1f, ClickAnimationDuration)
                .SetEase(Ease.OutQuad);
        }
    }

    // 상태 변경
    private void SetState(EMouseState newState)
    {
        if (_currentState == newState) return;
        
        _currentState = newState;
        
        // 스프라이트 변경
        if (_cursorImage != null)
        {
            switch (_currentState)
            {
                case EMouseState.Idle:
                    if (IdleSprite != null)
                        _cursorImage.sprite = IdleSprite;
                    break;
                case EMouseState.Click:
                    if (ClickSprite != null)
                        _cursorImage.sprite = ClickSprite;
                    break;
            }
        }
    }

    #region 페이드 효과

    // 페이드 인
    public void FadeIn()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.DOKill();
            _canvasGroup.DOFade(1f, FadeDuration).SetEase(Ease.OutQuad);
        }
    }

    // 페이드 아웃
    public void FadeOut()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.DOKill();
            _canvasGroup.DOFade(0f, FadeDuration).SetEase(Ease.InQuad);
        }
    }

    // 즉시 표시
    public void ShowImmediate()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.DOKill();
            _canvasGroup.alpha = 1f;
        }
    }

    // 즉시 숨기기
    public void HideImmediate()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.DOKill();
            _canvasGroup.alpha = 0f;
        }
    }

    #endregion

    #region 마우스 포인터 디자인 변경

    // 마우스 포인터 스프라이트 변경
    public void ChangeMouseSprite(Sprite sprite)
    {
        if (_cursorImage != null && sprite != null)
        {
            _cursorImage.sprite = sprite;
        }
    }

    // Idle 스프라이트 변경
    public void ChangeIdleSprite(Sprite sprite)
    {
        IdleSprite = sprite;
        if (_currentState == EMouseState.Idle && _cursorImage != null)
        {
            _cursorImage.sprite = sprite;
        }
    }

    // Click 스프라이트 변경
    public void ChangeClickSprite(Sprite sprite)
    {
        ClickSprite = sprite;
        if (_currentState == EMouseState.Click && _cursorImage != null)
        {
            _cursorImage.sprite = sprite;
        }
    }

    #endregion
}