using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

//  ScreenFx © NullTale - https://x.com/NullTale
namespace ScreenFx
{
    [TrackColor(0.5979568f, 0.3724881f, 0.922f)]
    [TrackClipType(typeof(TimeAsset))]
    public class TimeTrack : TrackAsset
    {
        [Tooltip("Time scale multiplication order")]
        public int          m_Order;
        
        // =======================================================================
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            foreach (var clip in this.GetClips())
            {
                var time = clip.asset as TimeAsset;
                if (time == null)
                    continue;
                
                time.m_Track = this;
                time.m_Clip  = clip;
            }

            return base.CreateTrackMixer(graph, go, inputCount);
        }
    }
}