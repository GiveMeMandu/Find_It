using System;
using UnityEngine;
using UnityEngine.Events;

namespace VolFx
{
    [AddComponentMenu("")]
    public class OnEnableCall : MonoBehaviour
    {
        public UnityEvent _onInvoke;
        
        // =======================================================================
        private void OnEnable()
        {
            _onInvoke.Invoke();
        }
    }
}