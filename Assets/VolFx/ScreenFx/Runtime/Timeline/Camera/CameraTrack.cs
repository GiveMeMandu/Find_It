using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

//  ScreenFx © NullTale - https://x.com/NullTale
namespace ScreenFx
{
    [TrackColor(0.8f, 0.2f, 0.3f)]
    [TrackClipType(typeof(MoveAsset))]
    [TrackClipType(typeof(NoiseAsset))]
    public class CameraTrack : TrackAsset
    {
        [Tooltip("Track offset multiplier")]
        public float _offsetMul = 1f;
        [Tooltip("Use offset in absolute world coordinates")]
        public bool  _offsetAbs;
        [Tooltip("Track ortho multiplier")]
        public float _orthoMul  = 1f;
        [Tooltip("Track fov multiplier")]
        public float _fovMul    = 1f;
        [Tooltip("Track roll multiplier")]
        public float _rollMul  = 1f;
        
        // =======================================================================
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var mixerTrack = ScriptPlayable<CameraMixerBehaviour>.Create(graph, inputCount);
            var beh = mixerTrack.GetBehaviour();
            beh._offsetAbs = _offsetAbs;
            beh._offsetMul = _offsetMul;
            beh._orthoMul = _orthoMul;
            beh._fovMul = _fovMul;
            beh._rollMul = _rollMul;
            return mixerTrack;
        }
    }
}