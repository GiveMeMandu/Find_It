using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Timeline;

//  ScreenFx © NullTale - https://x.com/NullTale
namespace ScreenFx
{
    [Serializable]
    public class VolumeAsset : PlayableAsset, ITimelineClipAsset
    {
        public ClipCaps clipCaps => ClipCaps.Blending;
        
        [InplaceField(nameof(VolumeBehaviour.Weight))]
        public VolumeBehaviour m_Template;
        [Tooltip("Volume settings")]
        public VolumeProfile   m_Volume;
        [NonSerialized]
        public VolumeTrack     m_Track;
        [NonSerialized]
        public TimelineClip    m_Clip;

        // =======================================================================
        public class VolHandle : IDisposable
        {
            private Volume _volume;
            
            public float Weight
            {
                set
                {
#if UNITY_EDITOR
                    if (_volume == null || _volume.gameObject == null)
                        return;
#endif
                    _volume.weight = value;
                    _volume.gameObject.SetActive(_volume.weight > 0f);
                }
            }

            // =======================================================================
            public VolHandle(VolumeProfile profile, float priority, string name, int layer)
            {
                var go = new GameObject($"Vol_{name}");
                go.layer = layer;
                go.transform.SetParent(ScreenFx.Instance.transform);
                go.hideFlags     = HideFlags.DontSave;
                _volume          = go.AddComponent<Volume>();
                _volume.profile  = profile;
                _volume.priority = priority;
                _volume.weight   = 0f;
            }

            public void Dispose()
            {
#if UNITY_EDITOR
                if (_volume == null || _volume.gameObject == null)
                    return;
                
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(_volume.gameObject);
                else
                    UnityEngine.Object.DestroyImmediate(_volume.gameObject);
#else
                UnityEngine.Object.Destroy(_volume.gameObject);
#endif
            }
        }
        
        // =======================================================================
        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            if (m_Volume == null)
                return Playable.Null;

            var playable  = ScriptPlayable<VolumeBehaviour>.Create(graph, m_Template);
            var behaviour = playable.GetBehaviour();
            behaviour.Handle = new VolHandle(m_Volume, m_Track.m_Priority, m_Clip.displayName, m_Track.m_Layer);

            return playable;
        }
    }

}