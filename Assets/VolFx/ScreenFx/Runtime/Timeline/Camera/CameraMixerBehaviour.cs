using UnityEngine;
using UnityEngine.Playables;

//  ScreenFx © NullTale - https://x.com/NullTale
namespace ScreenFx
{
    public class CameraMixerBehaviour : PlayableBehaviour
    {
        private ScreenFx.Vec3Handle  _offset = new ScreenFx.Vec3Handle();
        private ScreenFx.FloatHandle _ortho  = new ScreenFx.FloatHandle();
        private ScreenFx.FloatHandle _fov    = new ScreenFx.FloatHandle();
        private ScreenFx.FloatHandle _dutch  = new ScreenFx.FloatHandle();
        public  float                _offsetMul;
        public  float                _orthoMul;
        public  float                _fovMul;
        public  float                _rollMul;
        public  bool                 _offsetAbs;

        // =======================================================================
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (_offsetAbs) ScreenFx.s_OffsetAbs.Add(_offset);
            else            ScreenFx.s_Offset.Add(_offset);
            
            ScreenFx.s_Fov.Add(_fov);
            ScreenFx.s_Dutch.Add(_dutch);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (_offsetAbs) ScreenFx.s_OffsetAbs.Remove(_offset);
            else            ScreenFx.s_Offset.Remove(_offset);
            
            ScreenFx.s_Fov.Remove(_fov);
            ScreenFx.s_Dutch.Remove(_dutch);
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var inputCount = playable.GetInputCount();

            // calculate weights
            var offset = Vector3.zero;
            var ortho  = 0f;
            var fov    = 0f;
            var dutch  = 0f;
            
            for (var n = 0; n < inputCount; n++)
            {
                // get clips data
                var iw = playable.GetInputWeight(n);
                var ip = playable.GetInput(n);
                var it = ip.GetPlayableType();
                if (it == typeof(CameraBehaviour))
                {
                    var beh = ((ScriptPlayable<CameraBehaviour>)ip).GetBehaviour();

                    offset += beh._offset * beh._weight * iw;
                    fov    += beh._fov * beh._weight * iw;
                    dutch  += beh._roll * beh._weight * iw;
                }
            }

            _offset._value = offset * _offsetMul;
            _ortho._value  = ortho * _orthoMul;
            _fov._value    = fov * _fovMul;
            _dutch._value  = dutch * _rollMul;
        }
    }
}