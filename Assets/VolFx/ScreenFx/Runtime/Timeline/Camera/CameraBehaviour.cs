using System;
using UnityEngine;
using UnityEngine.Playables;

//  ScreenFx © NullTale - https://x.com/NullTale
namespace ScreenFx
{
    [Serializable]
    public class CameraBehaviour : PlayableBehaviour
    {
        [Range(0, 1)]
        public float   _weight = 1f;
        [Tooltip("Camera local offset")]
        public Vector3 _offset;
        [Tooltip("Ortho or Perspective adjustments")]
        public float   _fov;
        [Tooltip("Camera Z Roll (cm dutch)")]
        public float   _roll;
    }
}
