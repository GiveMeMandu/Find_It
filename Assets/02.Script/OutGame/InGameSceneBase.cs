using System.Collections;
using System.Collections.Generic;
using DeskCat.FindIt.Scripts.Core.Main.System;
using Manager;
using UI.Page;
using UnityEngine;

namespace OutGame
{
    public class InGameSceneBase : SceneBase
    {
        public LevelManager _levelManager;
        protected override void Start()
        {
            base.Start();
            Global.UIManager.OpenPage<InGameMainPage>();
        }
    }
}
