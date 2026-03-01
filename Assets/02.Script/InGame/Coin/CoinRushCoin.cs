using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// 코인러쉬 모드에서 사용되는 개별 코인 클래스
/// </summary>
public class CoinRushCoin : MonoBehaviour
{
    [Header("연속 클릭 옵션")]
    [Tooltip("활성화 시 코인은 첫 클릭 후 정상적으로 사라지며, 사라지는 애니메이션 동안 추가 클릭마다 보상을 지급합니다.")]
    public bool allowContinuousClick = false;

    private int value;
    private float lifetime;
    public UnityEvent onFound = new UnityEvent();

    private float spawnTime;
    private System.Action _continuousCollectCallback;
    // 첫 클릭은 HiddenObj의 TargetClickAction에서 처리하므로, 이후 클릭부터 연속 콜백 호출
    private bool _firstClickHandled = false;
    

    /// <summary>
    /// 연속 클릭 모드에서 클릭마다 호출할 콜백을 등록합니다.
    /// </summary>
    public void SetContinuousCollectCallback(System.Action callback)
    {
        _continuousCollectCallback = callback;
    }

    public void Initialize(int coinValue, float coinLifetime, System.Action collectCallback)
    {
        value = coinValue;
        lifetime = coinLifetime;
        if (collectCallback != null)
        {
            onFound.AddListener(new UnityAction(collectCallback));
        }
        spawnTime = Time.time;

        // 클릭 가능하도록 콜라이더 추가
        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<CircleCollider2D>();
        }
    }

    private void Update()
    {
        // 수명이 0 이하면 무제한 (수명 체크 안 함)
        if (lifetime <= 0f)
            return;
            
        // 수명 체크
        if (Time.time - spawnTime >= lifetime)
        {
            DestroyCoin();
        }

        // 깜빡이는 효과 (수명이 얼마 남지 않았을 때)
        if (Time.time - spawnTime >= lifetime * 0.7f)
        {
            float blinkRate = 5f;
            bool visible = Mathf.Sin(Time.time * blinkRate) > 0;
            GetComponent<Renderer>().enabled = visible;
        }
    }

    private void OnMouseDown()
    {
        if (allowContinuousClick)
        {
            // 첫 클릭은 HiddenObj의 TargetClickAction에서 보상 처리됨 → 여기서는 건너뜀
            // 이후 클릭(사라지는 애니메이션 중)부터 연속 보상 지급
            if (!_firstClickHandled)
            {
                _firstClickHandled = true;
            }
            else
            {
                _continuousCollectCallback?.Invoke();
            }
        }
        else
        {
            CollectCoin();
        }
    }

    public void CollectCoin()
    {
        onFound.Invoke();
    }

    private void DestroyCoin()
    {
        Destroy(gameObject);
    }

}