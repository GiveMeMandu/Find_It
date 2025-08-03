using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace SnowRabbit.Stage
{
    public class StageData : MonoBehaviour
    {
        public List<Trigger> triggers = new List<Trigger>();
        [Button("트리거 추가")]
        public void AddTrigger()
        {
            triggers.Add(new Trigger());
        }
    }
}