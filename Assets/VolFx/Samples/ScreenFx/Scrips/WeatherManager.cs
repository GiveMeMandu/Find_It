using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ScreenFx
{
    public class WeatherManager : MonoBehaviour
    {
        public DirectorState _sun;
        public DirectorState _clouds;
        public ParticleSystem _snow;
        public float          _snowMax;
        
        // =======================================================================
        public void Update()
        {
            var ws = VolumeManager.instance.stack.GetComponent<WeatherSettings>();
        
            // apply volume parameters, snow, sun, clouds
            var  em = _snow.emission;
            em.rateOverTimeMultiplier = ws._snow.value * _snowMax;
            
            _sun.SetTimeImmediate(ws._sun.value);
            
            _clouds.SetTimeImmediate(ws._clouds.value);
        }
    }
}