using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class ChangeDayObject : MonoBehaviour
{
    private bool isFound = false;
    public bool IsFound => isFound;
    public EventHandler OnFound;
    
    public virtual void Found()
    {
        if (isFound) return;
        
        isFound = true;
        OnFound?.Invoke(this, EventArgs.Empty);
    }
}
