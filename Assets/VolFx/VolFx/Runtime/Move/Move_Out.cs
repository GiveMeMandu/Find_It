using System;
using UnityEngine;
using UnityEngine.Events;

namespace VolFx
{
    [AddComponentMenu("VolFx/Transitions/Move_Out")]
    public class Move_Out : MonoBehaviour
    {
        [Tooltip("If enabled, this component allows the VolFx transition to continue\n" +
                 "once at least one active Move_Out is present in the scene.")]
        public bool _activeRelease = true;

        [Space]
        [Tooltip("Called when the Move transition enters a waiting (paused) state\n" +
                 "and this Move_Out is active.")]
        public UnityEvent _onWait;

        [Tooltip("Called when the Move transition playback is fully completed.")]
        public UnityEvent _onComplete;

        public bool ActiveRelease
        {
            get => _activeRelease;
            set => _activeRelease = value;
        }

        // =======================================================================
        private void OnEnable()
        {
            Move.s_Release.Add(this);
        }

        private void OnDisable()
        {
            Move.s_Release.Remove(this);
        }
    }
}