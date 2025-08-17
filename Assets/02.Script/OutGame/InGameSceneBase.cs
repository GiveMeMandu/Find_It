using System.Collections;
using System.Collections.Generic;
using DeskCat.FindIt.Scripts.Core.Main.System;
using Manager;
using UI.Page;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace OutGame
{
    public class InGameSceneBase : SceneBase
    {
        public bool isTimeChallenge = false;
        public LevelManager _levelManager;
        
        protected override void Start()
        {
            base.Start();
            if(isTimeChallenge) {
                Global.UIManager.OpenPage<TimeChallengeMainPage>();
            } else {
                Global.UIManager.OpenPage<InGameMainPage>();
            }
            if(_levelManager == null) {
                _levelManager = FindAnyObjectByType<LevelManager>();
                if(_levelManager != null)
                    _levelManager.OnEndEvent.Add(ClearStageTask);
            } else {
                _levelManager.OnEndEvent.Add(ClearStageTask);
            }
        }

        protected virtual void StartStageBase()
        {
            if(_levelManager != null) {
                _levelManager.gameObject.SetActive(true);
            }
            Global.UIManager.OpenPage<InGameIntroPage>();
        }

        protected virtual async UniTask ClearStageTask()
        {
            var outroPage = Global.UIManager.OpenPage<InGameOutroPage>();
            await outroPage.WaitForClose();
        }

        public virtual void SkipIntro()
        {
            StartStageBase();
        }

    }
}
