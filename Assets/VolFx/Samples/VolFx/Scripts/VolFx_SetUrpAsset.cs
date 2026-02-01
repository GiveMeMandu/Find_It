using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VolFx
{
    [AddComponentMenu("")]
    public class VolFx_SetUrpAsset : MonoBehaviour
    {
        public UniversalRenderPipelineAsset _urp;
        public UnityEvent _onStart;
        
        // =======================================================================
        private void Start()
        {
            Debug.Log($"Urp Asset {_urp.name} was set as a main");
            QualitySettings.renderPipeline = _urp;
            _onStart.Invoke();
        }
    }
}
