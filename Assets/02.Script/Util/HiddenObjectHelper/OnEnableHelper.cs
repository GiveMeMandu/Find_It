using UnityEngine;
using UnityEngine.Events;

public class OnEnableHelper : MonoBehaviour
{
    public UnityEvent onEnableEvent;

    public void OnEnable()
    {
        onEnableEvent.Invoke();
    }
}
