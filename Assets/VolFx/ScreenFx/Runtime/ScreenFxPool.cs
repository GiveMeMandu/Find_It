using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//  ScreenFx © NullTale - https://x.com/NullTale
namespace ScreenFx
{
    public class ScreenFxPool : MonoBehaviour
    {
        private static TimeHandleComparer s_TimeHandleComparer = new TimeHandleComparer();
        
        private List<TimeAsset.TimeHandle> _timeHandles = new List<TimeAsset.TimeHandle>();
        
        // =======================================================================
        public class TimeHandleComparer : IComparer<TimeAsset.TimeHandle>
        {
            public int Compare(TimeAsset.TimeHandle x, TimeAsset.TimeHandle y)
            {
                return x._order - y._order;
            }
        }
        
        // =======================================================================
        internal void AddTimeHandle(TimeAsset.TimeHandle handle)
        {
            _timeHandles.Add(handle);
            _timeHandles.Sort(s_TimeHandleComparer);
        }
        
        internal void RemoveTimeHandle(TimeAsset.TimeHandle handle)
        {
            _timeHandles.Remove(handle);
            
            // restore initial handle because we don't wont to to update our stuff 
            if (_timeHandles.Count == 0)
                Time.timeScale = 1f;
        }

        private IEnumerator Start()
        {
            if (transform.parent != null)
            {
                transform.SetParent(null);
                yield return null;
            }
            
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            // do not override time scale if we don't have handles
            if (_timeHandles.Count == 0)
                return;
            
            var timeScale = 1f;
            foreach (var timeHandle in _timeHandles)
                timeScale *= timeHandle._mul;
            
            Time.timeScale = timeScale;
        }
    }
}