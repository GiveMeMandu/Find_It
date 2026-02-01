using UnityEngine;
using UnityEngine.Events;

namespace VolFx
{
    [AddComponentMenu("VolFx/Transitions/Move_In")]
    public class Move_In : MonoBehaviour
    {
        [Tooltip("If enabled, this component prevents the VolFx transition from continuing\n" +
                 "until it becomes inactive or is removed from the scene.")]
        public bool _activeLock = true;

        [Space]
        [Tooltip("Called when the Move transition starts playback.")]
        public UnityEvent _onStart;

        [Tooltip("Called when the Move transition enters a waiting (paused) state\n" +
                 "due to active locks or release conditions.")]
        public UnityEvent _onWait;

        public bool ActiveLock
        {
            get => _activeLock;
            set => _activeLock = value;
        }

        // =======================================================================
        private void OnEnable()
        {
            Move.s_locks.Add(this);
        }

        private void OnDisable()
        {
            Move.s_locks.Remove(this);
        }
    }
}