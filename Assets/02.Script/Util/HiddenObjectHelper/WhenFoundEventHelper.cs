using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class WhenFoundEventHelper : MonoBehaviour
{
    [Label("찾았을 때 이벤트")]
    public UnityEvent onFoundEvent;
}
