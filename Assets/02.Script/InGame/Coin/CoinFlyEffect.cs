using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using Manager;

/// <summary>
/// 코인을 찾았을 때 월드 좌표에서 UI(IngameCoinLayer) 방향으로 
/// DOTween을 사용해 끌어당기는 비주얼 이펙트를 생성합니다.
/// 날아가는 이펙트 오브젝트는 Canvas 위의 UI(Image)로 생성됩니다.
/// CoinRushModeManager에 부착하여 사용합니다.
/// </summary>
public class CoinFlyEffect : MonoBehaviour
{
    [Header("Canvas 설정")]
    [LabelText("이펙트가 생성될 Canvas")]
    [Tooltip("UI 이펙트가 배치될 Canvas. 비워두면 타겟 UI의 Canvas를 자동으로 사용합니다.")]
    public Canvas effectCanvas;

    [Header("이펙트 프리팹")]
    [LabelText("코인 이펙트 프리팹 (UI)")]
    [Tooltip("날아갈 코인 UI 프리팹 (Image 컴포넌트 포함). 없으면 찾은 코인의 스프라이트로 Image를 자동 생성합니다.")]
    public GameObject coinEffectPrefab;

    [Header("이펙트 설정")]
    [LabelText("이동 시간 (초)")]
    public float flyDuration = 0.6f;

    [LabelText("시작 크기 배율")]
    public float startScale = 1.0f;

    [LabelText("끝 크기 배율")]
    public float endScale = 0.3f;

    [LabelText("이펙트 UI 크기")]
    [Tooltip("자동 생성되는 코인 UI의 크기 (픽셀)")]
    public Vector2 coinUISize = new Vector2(80f, 80f);

    [LabelText("이동 Ease")]
    public Ease moveEase = Ease.InBack;

    [LabelText("크기 Ease")]
    public Ease scaleEase = Ease.InCubic;

    [Header("흩어짐 효과")]
    [LabelText("흩어짐 사용")]
    [Tooltip("코인이 먼저 살짝 튀어나간 뒤 UI로 이동합니다.")]
    public bool useScatter = true;

    [ShowIf("useScatter")]
    [LabelText("흩어짐 반경 (픽셀)")]
    public float scatterRadius = 150f;

    [ShowIf("useScatter")]
    [LabelText("흩어짐 시간 (초)")]
    public float scatterDuration = 0.15f;

    [ShowIf("useScatter")]
    [LabelText("흩어짐 Ease")]
    public Ease scatterEase = Ease.OutCubic;

    [Header("복수 코인 연출")]
    [LabelText("이펙트 코인 개수")]
    [Tooltip("한 번에 날아가는 코인 이펙트 수")]
    [Range(1, 10)]
    public int coinCount = 3;

    [LabelText("코인 간 딜레이 (초)")]
    public float coinDelay = 0.05f;

    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    /// <summary>
    /// 코인 획득 시 날아가는 UI 이펙트를 재생합니다.
    /// </summary>
    /// <param name="worldPosition">코인의 월드 좌표</param>
    /// <param name="targetUI">목표 UI RectTransform (활성화된 IngameCoinLayer)</param>
    /// <param name="sourceSprite">원본 코인 스프라이트 (프리팹이 없을 때 Image에 사용)</param>
    public void PlayFlyEffect(Vector3 worldPosition, RectTransform targetUI, Sprite sourceSprite = null)
    {
        if (targetUI == null) return;
        if (_mainCamera == null) _mainCamera = Camera.main;
        if (_mainCamera == null) return;

        // 사용할 Canvas 결정
        Canvas canvas = effectCanvas;
        if (canvas == null)
        {
            canvas = Global.UIManager.uiCanvas;
        }
        if (canvas == null)
        {
            Debug.LogWarning("[CoinFlyEffect] Canvas를 찾을 수 없습니다.");
            return;
        }

        // 루트 Canvas 가져오기 (좌표 변환에 사용)
        Canvas rootCanvas = canvas.rootCanvas;

        for (int i = 0; i < coinCount; i++)
        {
            float delay = i * coinDelay;
            CreateAndFlyCoinUI(worldPosition, targetUI, sourceSprite, delay, rootCanvas);
        }
    }

    private void CreateAndFlyCoinUI(Vector3 worldPosition, RectTransform targetUI, Sprite sourceSprite, float delay, Canvas rootCanvas)
    {
        RectTransform canvasRect = rootCanvas.GetComponent<RectTransform>();

        // 코인 월드 좌표 → Canvas 내 로컬 좌표 변환
        Vector2 startAnchoredPos = WorldToCanvasPosition(worldPosition, canvasRect, rootCanvas);

        // 타겟 UI의 Canvas 내 로컬 좌표
        Vector2 targetAnchoredPos = GetTargetCanvasPosition(targetUI, canvasRect, rootCanvas);

        // 이펙트용 UI 오브젝트 생성
        GameObject flyObj;
        RectTransform flyRect;

        if (coinEffectPrefab != null)
        {
            flyObj = Instantiate(coinEffectPrefab, canvasRect);
            flyRect = flyObj.GetComponent<RectTransform>();
            if (flyRect == null)
            {
                flyRect = flyObj.AddComponent<RectTransform>();
            }
        }
        else
        {
            // 프리팹이 없으면 Image 기반으로 자동 생성
            flyObj = new GameObject("CoinFlyEffect", typeof(RectTransform), typeof(Image));
            flyRect = flyObj.GetComponent<RectTransform>();
            flyObj.transform.SetParent(canvasRect, false);

            var image = flyObj.GetComponent<Image>();
            if (sourceSprite != null)
            {
                image.sprite = sourceSprite;
            }
            image.raycastTarget = false; // 클릭 방해 방지

            // UI 크기 설정
            flyRect.sizeDelta = coinUISize;
        }

        // 앵커를 중앙으로 설정
        flyRect.anchorMin = new Vector2(0.5f, 0.5f);
        flyRect.anchorMax = new Vector2(0.5f, 0.5f);
        flyRect.pivot = new Vector2(0.5f, 0.5f);

        // 시작 위치 설정
        flyRect.anchoredPosition = startAnchoredPos;
        flyRect.localScale = Vector3.one * startScale;

        // DOTween Sequence
        Sequence seq = DOTween.Sequence();

        if (useScatter)
        {
            // 1단계: 흩어짐 - 랜덤 방향으로 살짝 튀어나감 (Canvas 좌표 기준)
            Vector2 randomDir = Random.insideUnitCircle.normalized * scatterRadius;
            Vector2 scatterPos = startAnchoredPos + randomDir;

            seq.AppendInterval(delay);
            seq.Append(flyRect.DOAnchorPos(scatterPos, scatterDuration).SetEase(scatterEase));

            // 2단계: 타겟 UI로 이동
            seq.Append(flyRect.DOAnchorPos(targetAnchoredPos, flyDuration).SetEase(moveEase));
            seq.Join(flyRect.DOScale(endScale, flyDuration).SetEase(scaleEase));
        }
        else
        {
            seq.AppendInterval(delay);
            seq.Append(flyRect.DOAnchorPos(targetAnchoredPos, flyDuration).SetEase(moveEase));
            seq.Join(flyRect.DOScale(endScale, flyDuration).SetEase(scaleEase));
        }

        // 이펙트 완료 후 오브젝트 제거
        seq.OnComplete(() =>
        {
            if (flyObj != null)
            {
                Destroy(flyObj);
            }
        });

        seq.OnKill(() =>
        {
            if (flyObj != null)
            {
                Destroy(flyObj);
            }
        });

        seq.SetUpdate(true); // TimeScale 영향 안 받도록
        seq.Play();
    }

    /// <summary>
    /// 월드 좌표를 Canvas의 anchoredPosition으로 변환합니다.
    /// </summary>
    private Vector2 WorldToCanvasPosition(Vector3 worldPosition, RectTransform canvasRect, Canvas canvas)
    {
        // 월드 좌표 → 스크린 좌표
        Vector2 screenPoint = _mainCamera.WorldToScreenPoint(worldPosition);

        // 스크린 좌표 → Canvas 로컬 좌표
        Camera uiCamera = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, uiCamera, out Vector2 localPoint);

        return localPoint;
    }

    /// <summary>
    /// 타겟 UI의 Canvas 내 anchoredPosition을 계산합니다.
    /// 타겟이 같은 Canvas에 있으면 직접 변환, 다르면 스크린 좌표 경유.
    /// </summary>
    private Vector2 GetTargetCanvasPosition(RectTransform targetUI, RectTransform canvasRect, Canvas canvas)
    {
        // 타겟의 월드 좌표 → 스크린 좌표
        Canvas targetCanvas = targetUI.GetComponentInParent<Canvas>();
        Camera targetCamera = null;
        if (targetCanvas != null && targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            targetCamera = targetCanvas.worldCamera;
        }

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(targetCamera, targetUI.position);

        // 스크린 좌표 → 이펙트 Canvas 로컬 좌표
        Camera uiCamera = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, uiCamera, out Vector2 localPoint);

        return localPoint;
    }
}
