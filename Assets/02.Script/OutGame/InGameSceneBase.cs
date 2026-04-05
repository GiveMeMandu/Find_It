using System.Collections;
using System.Collections.Generic;
using DeskCat.FindIt.Scripts.Core.Main.System;
using Manager;
using UI.Page;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

namespace OutGame
{
    public class InGameSceneBase : SceneBase
    {
        public bool isTimeChallenge = false;
        public LevelManager _levelManager;
        [LabelText("인트로 UI 페이지 오픈 여부")]
        public bool isIntroPageOpen = true;
        
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
                {
                    _levelManager.OnEndEvent.Add(ClearStageTask);
                }
            } else {
                _levelManager.OnEndEvent.Add(ClearStageTask);
            }

            // 추가적인 StageManager 없이 InGameSceneBase 단독으로 사용될 때 자동 실행되도록 처리
            if (this.GetType() == typeof(InGameSceneBase))
            {
                StartStageBase();
            }
        }

        protected virtual void StartStageBase()
        {
            if(_levelManager != null) {
                _levelManager.gameObject.SetActive(true);
            }
            // if(isIntroPageOpen) {
                Global.UIManager.OpenPage<InGameIntroPage>();
            // }
        }

        protected virtual async UniTask ClearStageTask()
        {
            var outroPage = Global.UIManager.OpenPage<InGameOutroPage>();
            await outroPage.WaitForClose();
        }

        public virtual void SkipIntro()
        {
            StartStageBase();
            _levelManager.ShowUI();
        }

    }
}
