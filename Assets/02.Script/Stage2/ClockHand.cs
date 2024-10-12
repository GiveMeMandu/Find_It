using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace InGame
{
    public class ClockHand : FoundObj
    {
        [LabelText("시계 물건 찾았을 때")]
        public UnityEvent OnFoundClockThing;
        private Clock _clock; 
        private void Awake() {
            _clock = FindObjectOfType<Clock>();
        }
        public virtual void GotClock()
        {
            OnFoundClockThing.Invoke();
        }
    }
}