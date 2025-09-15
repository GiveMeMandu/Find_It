using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class OnEnableHelper : MonoBehaviour
{
    public UnityEvent onEnableEvent;
    public UnityEvent TestEvent;

    public void OnEnable()
    {
        onEnableEvent.Invoke();
    }

    [Button("Test Event 테스트")]
    public void TestEventButton()
    {
        TestEvent.Invoke();
    }
}
