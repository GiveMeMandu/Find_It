using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class OnDisableHelper : MonoBehaviour
{
    public UnityEvent onDisbleEvent;
    public UnityEvent TestEvent;

    public void OnDisable()
    {
        onDisbleEvent.Invoke();
    }

    [Button("Test Event 테스트")]
    public void TestEventButton()
    {
        TestEvent.Invoke();
    }
}
