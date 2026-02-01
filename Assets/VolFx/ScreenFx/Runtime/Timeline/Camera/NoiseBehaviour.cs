using System;
using UnityEngine;
using UnityEngine.Playables;

#if !CINEMACHINE_CRINGE
using Cinemachine;
#else
using Unity.Cinemachine;
#endif

//  ScreenFx © NullTale - https://x.com/NullTale
namespace ScreenFx
{
    [Serializable]
    public class NoiseBehaviour : PlayableBehaviour
    {
        [Tooltip("Noise frequency multiplier")]
        public float _freq = 1;
        [Tooltip("Noise position multiplier")]
        public float _move = 1;
        [Tooltip("Noise torque multiplier")]
        public float _torque = 1;
        public Optional<NoiseSettings> _noise = new Optional<NoiseSettings>(false);
        
        private ScreenFx.NoiseHandle _handle;
        
        private ScreenFx.FloatHandle _freqHandle   = new ScreenFx.FloatHandle();
        private ScreenFx.FloatHandle _amplHandle   = new ScreenFx.FloatHandle();
        private ScreenFx.FloatHandle _torqueHandle = new ScreenFx.FloatHandle();
        private bool _isOpen;
        
        // =======================================================================
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (_isOpen)
                return;
            
            _isOpen = true;
            
            _handle = ScreenFx.GetNoiseHandle(_noise.GetValueOrDefault());
            _handle._freq.Add(_freqHandle);
            _handle._ampl.Add(_amplHandle);
            _handle._torque.Add(_torqueHandle);
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (_isOpen == false)
                return;
            
            _freqHandle._value   = _freq * info.weight;
            _amplHandle._value   = _move * info.weight;
            _torqueHandle._value = _torque * info.weight;
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (_isOpen == false)
                return;
            
            _isOpen = false;
            
            _handle._freq.Remove(_freqHandle);
            _handle._ampl.Remove(_amplHandle);
            _handle._torque.Remove(_torqueHandle);
        }
    }
}