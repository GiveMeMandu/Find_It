using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

#if !CINEMACHINE_CRINGE
using Cinemachine;
#else
using Unity.Cinemachine;
#endif

//  ScreenFx © NullTale - https://x.com/NullTale
namespace ScreenFx
{
    [AddComponentMenu("")]
    [SaveDuringPlay]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class ScreenFx : CinemachineExtension
    {
        public static List<Vec3Handle>  s_Offset    = new List<Vec3Handle>();
        public static List<Vec3Handle>  s_OffsetAbs = new List<Vec3Handle>();
        public static NoiseHandle       s_NoiseMain = new NoiseHandle();
        
        public static List<FloatHandle> s_Fov   = new List<FloatHandle>();
        public static List<FloatHandle> s_Dutch = new List<FloatHandle>();
        public static Dictionary<NoiseSettings, NoiseHandle> s_Noise = new Dictionary<NoiseSettings, NoiseHandle>();
        
        private static ScreenFxPool s_Instance;
        public static  ScreenFxPool Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    #pragma warning disable CS0618
                    s_Instance = FindObjectOfType<ScreenFxPool>();
                    if (s_Instance == null)
                    {
                        var go = new GameObject("ScreenFx");
                        s_Instance = go.AddComponent<ScreenFxPool>();
                    }
                }

                return s_Instance;
            }
        }

        public static NoiseHandle GetNoiseHandle(NoiseSettings settings)
        {
            if (settings == null)
                return s_NoiseMain;
            
            if (s_Noise.TryGetValue(settings, out var handle))
                return handle;
            
            handle = new NoiseHandle() { _settings = settings };
            s_Noise.Add(settings, handle);
            return handle;
        }
        
        [Tooltip("Default noise if noise parameter for a track asset is not set")]
        public NoiseSettings _noiseDefault;
        
        public DeltaTime _time;
        
        // =======================================================================
        public class ValueHandle<T>
        {
            public T _value;
        }

        public enum DeltaTime
        {
            Cinemachine,
            Time,
            RealTime
        }
        
        public class FloatHandle : ValueHandle<float>
        {
        }
        
        public class Vec3Handle : ValueHandle<Vector3>
        {
        }
        
        public class NoiseHandle
        {
            public NoiseSettings     _settings;
            public List<FloatHandle> _freq   = new List<FloatHandle>();
            public List<FloatHandle> _ampl   = new List<FloatHandle>();
            public List<FloatHandle> _torque = new List<FloatHandle>();
            public float             _time;
        }

        // =======================================================================
        protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            if (stage != CinemachineCore.Stage.Body)
                return;
            
            state.PositionCorrection += state.RawOrientation * s_OffsetAbs.Aggregate(Vector3.zero, (a, b) => a + b._value);
            state.PositionCorrection += s_Offset.Aggregate(Vector3.zero, (a, b) => a + b._value);
            
            ref var lenseSettings = ref state.Lens;
            var sum = s_Fov.Aggregate(0f, (n, handle) => n + handle._value);
            if (state.Lens.Orthographic)
            {
                lenseSettings.OrthographicSize += sum;
            }
            else
            {
                lenseSettings.FieldOfView += sum;
            }
            
            deltaTime = _time switch {
                DeltaTime.Cinemachine => deltaTime,
                DeltaTime.Time        => Time.deltaTime,
                DeltaTime.RealTime    => Time.unscaledDeltaTime,
                _                     => throw new ArgumentOutOfRangeException()
            };
                
            
            if (Application.isPlaying == false)
            {
                deltaTime = Time.deltaTime;
            }
            else
            {
                if (deltaTime <= 0)
                    return;
            }
            
            lenseSettings.Dutch += s_Dutch.Aggregate(0f, (n, handle) => n + handle._value);
            
#if UNITY_EDITOR
            if (_noiseDefault == null)
            {
                _noiseDefault = UnityEditor.AssetDatabase.LoadAssetAtPath<NoiseSettings>("Packages/com.unity.cinemachine/Presets/Noise/6D Shake.asset");
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
            
            _noiseCorrection(ref state, _noiseDefault, s_NoiseMain);
            foreach (var handle in s_Noise.Values)
                _noiseCorrection(ref state, handle._settings, handle);

            // -----------------------------------------------------------------------
            void _noiseCorrection(ref CameraState state, NoiseSettings noise, NoiseHandle handle)
            {
                noise.GetSignal(handle._time, out var pos, out var rot);
                handle._time += deltaTime * handle._freq.MaxOrDefault(n => n._value) * 0.1f;
                
                var posImpact = pos * handle._ampl.MaxOrDefault(n => n._value);
                state.PositionCorrection += posImpact;
    
                var rotImpact = Quaternion.SlerpUnclamped(Quaternion.identity, rot, handle._torque.MaxOrDefault(n => n._value));
                state.OrientationCorrection *= rotImpact;
            }
        }
        
        public static void ScreenFlash(Color color, AnimationCurve alpha, int sortingOreder = 10000, RenderMode mode = RenderMode.ScreenSpaceCamera)
        {
            Instance.StartCoroutine(_screenFlash());

            // =======================================================================
            IEnumerator _screenFlash()
            {
                using var handle = new ScreenOverlay(sortingOreder, mode, 0, 1f, "static_call_screen");
                var duration     = alpha.keys[1].time - alpha.keys[0].time;
                var endTime      = Time.time + duration;
                var alphaInitial = color.a;
                var colorInitial = color;

                colorInitial.a = alphaInitial *  alpha.keys[0].value;
                handle.Color = colorInitial;
                handle.Canvas.gameObject.SetActive(true);
                
                while (endTime >= Time.time)
                {
                    yield return null;
                    colorInitial.a = alphaInitial * alpha.Evaluate(duration - (endTime - Time.time));
                    handle.Color   = colorInitial;
                }
            }
        }
        
        public static void VolumeShot(VolumeProfile vol, AnimationCurve weight, float priority = 10000f, int layer = 0)
        {
            Instance.StartCoroutine(_volumeShot());

            // =======================================================================
            IEnumerator _volumeShot()
            {
                using var handle = new VolumeAsset.VolHandle(vol, priority, "static_call_vol", layer);
                var startTime    = Time.time;
                var duration     = weight.keys[1].time - weight.keys[0].time;
                var endTime      = Time.time + duration;
                
                while (endTime >= Time.time)
                {
                    yield return null;
                    handle.Weight = weight.Evaluate(duration - (endTime - Time.time));
                }
            }
        }
        
        public static void CameraFov(float impact, AnimationCurve lerp, float duration)
        {
            ScreenFx.Instance.StartCoroutine(_ortho());

            // -----------------------------------------------------------------------
            IEnumerator _ortho()
            {
                var startTime = Time.time;
                var endTime   = Time.time + duration;
                var ortho     = new ScreenFx.FloatHandle();
                
                ScreenFx.s_Fov.Add(ortho);
                while (endTime > Time.time)
                {
                    ortho._value = lerp.Evaluate((startTime - Time.time) / duration) * impact;
                    yield return null;
                }
                ScreenFx.s_Fov.Remove(ortho);
            }
        }
    }
}