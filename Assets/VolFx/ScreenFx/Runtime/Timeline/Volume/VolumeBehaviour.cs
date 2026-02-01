using System;
using UnityEngine;
using UnityEngine.Playables;

//  ScreenFx © NullTale - https://x.com/NullTale
namespace ScreenFx
{
    [Serializable]
    public class VolumeBehaviour : PlayableBehaviour
    {
        [Range(0, 1)] [Tooltip("Weight of the Volume Component")]
        public float Weight = 1;

        [NonSerialized]
        public VolumeAsset.VolHandle Handle;

        // =======================================================================
        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            Handle.Weight = 0;
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            Handle.Weight = Weight * info.weight;
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            Handle.Dispose();
            Handle = null;
        }
    }
}