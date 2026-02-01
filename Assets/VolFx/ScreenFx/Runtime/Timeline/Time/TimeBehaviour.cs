using System;
using UnityEngine;
using UnityEngine.Playables;

//  ScreenFx © NullTale - https://x.com/NullTale
namespace ScreenFx
{
    [Serializable]
    public class TimeBehaviour : PlayableBehaviour
    {
        [Range(0, 3)] [Tooltip("Time scale multiplier")]
        public float Mul = 1;

        [NonSerialized]
        public TimeAsset.TimeHandle Handle;

        // =======================================================================
        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            Handle._mul = 1;
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            Handle._mul = Mathf.LerpUnclamped(1f, Mul, info.weight);
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            Handle.Dispose();
            Handle = null;
        }
    }
}