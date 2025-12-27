using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// 코인러쉬 모드에서 사용되는 개별 코인 클래스
/// </summary>
public class CoinRushCoin : MonoBehaviour
{
    private int value;
    private float lifetime;
    public UnityEvent onFound = new UnityEvent();

    private float spawnTime;

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
        CollectCoin();
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