using System;
using System.Collections;
using System.Collections.Generic;
using DeskCat.FindIt.Scripts.Core.Main.System;
using UnityEngine;

namespace InGame
{
    public class LevelManagerCount : MonoBehaviour
    {

        protected int curFoundCount = 0;
        protected int curFoundCountMax = 0;
        protected LevelManager levelManager;

        protected virtual void Awake() {
            
            levelManager = FindObjectOfType<LevelManager>();
            curFoundCountMax = levelManager.TargetObjs.Length;
        }
        
        protected virtual void OnEnable()
        {
            levelManager.OnFoundObj += OnFoundObj;
        }

        protected virtual void OnDisable()
        {
            levelManager.OnFoundObj -= OnFoundObj;
        }

        protected virtual void OnFoundObj(object sender, int e)
        {
            curFoundCount = curFoundCountMax - e;
        }
    }
}