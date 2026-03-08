using UnityEngine;
using UnityEngine.Events;

public class ToggleActiveHelper : MonoBehaviour
{
    public UnityEvent OnToggleActive = new UnityEvent();
    public UnityEvent OnToggleDeactive = new UnityEvent();

    public void ToggleActive()
    {
        bool isActive = !gameObject.activeSelf;
        gameObject.SetActive(isActive);
        if (isActive)
        {
            OnToggleActive.Invoke();
        }
        else
        {
            OnToggleDeactive.Invoke();
        }
    }
}
