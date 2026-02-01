using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

//  ScreenFx © NullTale - https://x.com/NullTale
namespace ScreenFx
{
    [TrackColor(0.5979568f, 0.3724881f, 0.922f)]
    [TrackClipType(typeof(VolumeAsset))]
    public class VolumeTrack : TrackAsset
    {
        [Layer]
        [Tooltip("Layer of the Volume GameObject")]
        public int            m_Layer;
        [Tooltip("Volume component Priority")]
        public float          m_Priority;
        
        // =======================================================================
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            foreach (var clip in this.GetClips())
            {
                var vol = clip.asset as VolumeAsset;
                if (vol == null)
                    continue;

                vol.m_Track = this;
                vol.m_Clip  = clip;
            }

            return base.CreateTrackMixer(graph, go, inputCount);
        }
    }
}