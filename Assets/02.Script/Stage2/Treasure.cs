using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame
{
    public class Treasure : MonoBehaviour
    {
        [SerializeField] private Stage2Manager stage2Manager;

        public void OnDayChange()
        {
            stage2Manager.StartStage();
        }
    }
}