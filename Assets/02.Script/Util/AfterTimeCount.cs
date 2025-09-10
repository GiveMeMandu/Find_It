using NaughtyAttributes;
using UnityEngine;

public class AfterTimeCount : MonoBehaviour
{
    [Label("시간 지나고 호출할 이벤트")]
    public UnityEngine.Events.UnityEvent AfterTimeEvent;
    [Label("시간 (초)")]
    public float TimeCount = 0f;
    [Label("OnEnable 시작 여부")]
    public bool IsStartOnEnable = false;
    private float currentTime = 0f;
    private bool isTiming = false;
    void OnEnable()
    {
        if (IsStartOnEnable)
        {
            StartTiming();
        }
    }
    void Update()
    {
        if (isTiming)
        {
            currentTime += Time.deltaTime;
            if (currentTime >= TimeCount)
            {
                isTiming = false;
                AfterTimeEvent?.Invoke();
            }
        }
    }
    public void StartTiming()
    {
        currentTime = 0f;
        isTiming = true;
    }
}
