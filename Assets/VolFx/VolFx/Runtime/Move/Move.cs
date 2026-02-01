using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [AddComponentMenu("VolFx/Transitions/Move")]
    [DefaultExecutionOrder(-10000)]
    [RequireComponent(typeof(PlayableDirector))]
    public class Move : MonoBehaviour, INotificationReceiver
    {
        internal static HashSet<Move_In>  s_locks   = new HashSet<Move_In>();
        internal static HashSet<Move_Out> s_Release = new HashSet<Move_Out>();

        [Tooltip("Waits until all active Move_In components release their locks before continuing playback")]
        public bool _waitInLocks  = true;
        [Tooltip("Waits until at least one Move_Out component becomes active before continuing playback")]
        public bool _waitOutRelease;
        
        [Tooltip("Timeline signal where playback pauses until all locks or releases are resolved")]
        public Optional<SignalAsset> _waitSignal;
        
        [Space]
        [Tooltip("Invoked when the transition playback starts")]
        public UnityEvent _onStart;
        [Tooltip("Invoked when the transition enters a paused or waiting state")]
        public UnityEvent _onWait;
        [Tooltip("Invoked when the transition playback is fully completed")]
        public UnityEvent _onComplete;
        
        private PlayableDirector _director;
        private Coroutine        _waitCoroutine;
        private SignalEmitter    _pauseSignal;
        
        // =======================================================================
        private void OnEnable()
        {
            _director = GetComponent<PlayableDirector>();
            _director.extrapolationMode = DirectorWrapMode.None;
            _director.playOnAwake = false;
            _director.Stop();
            
            _waitCoroutine = null;
            
            // _director.stopped += _ => Destroy(gameObject);
        }

 #if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
 #endif
        private static void _init()
        {
            s_locks   = new HashSet<Move_In>();
            s_Release = new HashSet<Move_Out>();
        }
        
        public void Play()
        {
            if (_waitCoroutine != null)
            {
                StopCoroutine(_waitCoroutine);
                _waitCoroutine = null;
            }
            
            _director.Play();
            _waitCoroutine = StartCoroutine(_play());
        }

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is SignalEmitter pauseSignal 
                && (_waitSignal.Enabled == false || _waitSignal.Value == pauseSignal.asset))
            {
                pauseSignal.emitOnce = true;
                _pauseSignal = pauseSignal;
            }
        }

        IEnumerator _play()
        {
            var root      = _director.playableGraph.GetRootPlayable(0);
            var initSpeed = root.GetSpeed();
            _pauseSignal = null;
            
            _callStart();
            
            // wait pause signal (pause signal can be not presented)
            while (_pauseSignal == null && _director.state == PlayState.Playing)
                yield return null;
            
            if (_pauseSignal != null)
                _onWait.Invoke();
            
            // pause
            var wait = (_hasLocks() || _hasRelease() == false) && _pauseSignal != null;
            if (wait)
            {
                _callWait();

                // wait in emitter time
                root.SetSpeed(0d);
                _director.time = _pauseSignal.time;
                _director.Evaluate();
            }

            while (wait)
            {
                wait = _hasLocks() || _hasRelease() == false;
                yield return null;
            }

            // restore
            _director.playableGraph.GetRootPlayable(0).SetSpeed(initSpeed);

            // wait complete
            while (_director.state == PlayState.Playing)
                yield return null;

            _callComplete();
        }
    
        private void _callStart()
        {
            _onStart.Invoke();
            
            foreach (var moveIn in s_locks)
                moveIn._onStart.Invoke();
        }
        
        private void _callWait()
        {
            foreach (var moveIn in s_locks)
                moveIn._onWait.Invoke();
            foreach (var moveOut in s_Release)
                moveOut._onWait.Invoke();
        }

        private void _callComplete()
        {
            _onComplete.Invoke();
            
            foreach (var moveOut in s_Release)
                moveOut._onComplete.Invoke();
        }
        
        private bool _hasLocks()
        {
            if (_waitInLocks == false)
                return false;
            
            return s_locks.Any(n => n.ActiveLock);
        }
        
        private bool _hasRelease()
        {
            if (_waitOutRelease == false)
                return true;
            
            return s_Release.Any(n => n.ActiveRelease);
        }
    }
}