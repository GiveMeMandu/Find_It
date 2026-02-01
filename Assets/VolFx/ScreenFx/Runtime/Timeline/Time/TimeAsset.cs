using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Timeline;

//  ScreenFx © NullTale - https://x.com/NullTale
namespace ScreenFx
{
    [Serializable]
    public class TimeAsset : PlayableAsset, ITimelineClipAsset
    {
        public ClipCaps clipCaps => ClipCaps.Blending;
        
        [InplaceField(nameof(TimeBehaviour.Mul))]
        public TimeBehaviour m_Template;
        [NonSerialized]
        public TimeTrack     m_Track;
        [NonSerialized]
        public TimelineClip  m_Clip;

        // =======================================================================
        public class TimeHandle : IDisposable
        {
            public int   _order;
            public float _mul;

            // =======================================================================
            public TimeHandle(int order, float mul, string name)
            {
                _order = order;
                _mul   = mul;
                
                ScreenFx.Instance.AddTimeHandle(this);
            }

            public void Dispose()
            {
                ScreenFx.Instance.RemoveTimeHandle(this);
            }
        }
        
        // =======================================================================
        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            var playable  = ScriptPlayable<TimeBehaviour>.Create(graph, m_Template);
            var beh = playable.GetBehaviour();
            beh.Handle = new TimeHandle(m_Track.m_Order, beh.Mul, m_Clip.displayName);

            return playable;
        }
    }
}