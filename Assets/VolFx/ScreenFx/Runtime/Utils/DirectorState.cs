using System;
using UnityEngine;
using UnityEngine.Playables;

//  ScreenFx © NullTale - https://x.com/NullTale
namespace ScreenFx
{
    [DefaultExecutionOrder(-10)]
    [RequireComponent(typeof(PlayableDirector))]
    [AddComponentMenu("VolFx/DirectorState")]
    public class DirectorState : MonoBehaviour
    {
        [Tooltip("Speed relative")]
        public double m_Speed     = 1d;
        [Tooltip("Up speed multiplier")]
        public double m_UpSpeed   = 1d;
        [Tooltip("Down speed multiplier")]
        public double m_DownSpeed = 1d;
        [Tooltip("Normalized Up move interpolation")]
        public float  m_UpLerp;
        [Tooltip("Normalized Down move interpolation")]
        public float  m_DownLerp;
        
        [Tooltip("Director time remap")]
        public Optional<AnimationCurve> m_Interpolation;

        private PlayableDirector m_Director;

        private double m_Time;
        private double m_Duration;

        [Tooltip("Use unscaled time")]
        public bool m_UnScaledTime = true;

        [SerializeField] [Range(0, 1)]
        private double m_DesiredTime;

        public double DesiredTime
        {
            get => m_DesiredTime;
            set
            {
                var val = Math.Clamp(value, 0, 1);
                if (val == m_DesiredTime)
                    return;

                m_DesiredTime = val;
                enabled       = true;
            }
        }
        
        public bool IsOn
        {
            get => m_DesiredTime == 1d;
            set => SetTime(value);
        }
        
        public bool IsComplete => enabled == false;

        public PlayableDirector Director => m_Director;

        // =======================================================================
        public void SetTimeImmediate(float normalizedTime)
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false && m_Director == null)
                m_Director = GetComponent<PlayableDirector>();
#endif
            
            DesiredTime = normalizedTime;
            
            m_Time          = m_DesiredTime * m_Duration;
            m_Director.time = m_Interpolation.Enabled ? m_Interpolation.Value.Evaluate((float)(m_Time / m_Duration)) * m_Duration : m_Time;
            m_Director.Evaluate();
        }
        
        public void SetTime(float normalizedTime)
        {
            DesiredTime = normalizedTime;
        }

        public void SetTime(bool state)
        {
            DesiredTime = state ? 1d : 0d;
        }
        
        private void Awake()
        {
            m_Director = GetComponent<PlayableDirector>();
            m_Director.Stop();

            m_Duration                   = m_Director.duration;
            m_Time                       = m_Duration * DesiredTime;
            m_Director.timeUpdateMode    = DirectorUpdateMode.Manual;
            m_Director.extrapolationMode = DirectorWrapMode.Hold;
        }

        private void OnEnable()
        {
            m_Director.time = m_Time;
            m_Director.Evaluate();
        }

        private void OnValidate()
        {
            if (m_Director == null)
                return;

            if (m_Director.duration * DesiredTime != m_Director.time)
                enabled = true;
        }

        private void Update()
        {
            var desiredTime = m_Director.duration * DesiredTime;

            if (m_Time == desiredTime)
            {
                enabled = false;
                return;
            }

            var deltaTime = m_UnScaledTime ? UnityEngine.Time.unscaledDeltaTime : UnityEngine.Time.deltaTime;
            if (desiredTime > m_Director.time)
            {
                m_Time += m_UpSpeed * m_Speed * deltaTime;
                m_Time =  Mathf.Lerp((float)m_Time, (float)desiredTime, m_UpLerp * deltaTime);
                
                if (m_Time > desiredTime)
                    m_Time = desiredTime;
            }
            else
            {
                m_Time -= m_DownSpeed * m_Speed * deltaTime;
                m_Time =  Mathf.Lerp((float)m_Time, (float)desiredTime, m_DownLerp * deltaTime);
                
                if (m_Time < desiredTime)
                    m_Time = desiredTime;
            }

            m_Director.time = m_Interpolation.Enabled ? m_Interpolation.Value.Evaluate((float)(m_Time / m_Duration)) * m_Duration : m_Time;
            m_Director.Evaluate();
        }

        public void On()
        {
            SetTime(true);
        }

        public void Off()
        {
            SetTime(false);
        }
    }
}