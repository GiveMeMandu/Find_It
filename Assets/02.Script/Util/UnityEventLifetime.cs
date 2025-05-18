using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class UnityEventLifetime : MonoBehaviour
{
    public enum EventTiming
    {
        OnEnable,
        Awake,
        Start
    }
    [LabelText("이벤트 타이밍 (언제 실행시킬거임)")]
    [SerializeField] private EventTiming eventTiming = EventTiming.Start;
    [LabelText("이벤트 실행")]
    [SerializeField] private UnityEvent onEvent = new UnityEvent();

    private bool isEventExecuted = false;

    private void OnEnable()
    {
        if (eventTiming == EventTiming.OnEnable && !isEventExecuted)
        {
            onEvent?.Invoke();
            isEventExecuted = true;
        }
    }

    private void Awake()
    {
        if (eventTiming == EventTiming.Awake && !isEventExecuted)
        {
            onEvent?.Invoke();
            isEventExecuted = true;
        }
    }

    private void Start()
    {
        if (eventTiming == EventTiming.Start && !isEventExecuted)
        {
            onEvent?.Invoke();
            isEventExecuted = true;
        }
    }
}
