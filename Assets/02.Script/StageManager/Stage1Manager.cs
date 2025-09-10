using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DeskCat.FindIt.Scripts.Core.Main.System;
using Manager;
using OutGame;
using Sirenix.OdinInspector;
using UI.Page;
using UnityEngine;
using UnityEngine.Playables;

namespace InGame
{

    public class Stage1Manager : InGameSceneBase
    {
        [LabelText("인트로")]
        [SerializeField] private PlayableDirector _introDirector;
        [LabelText("아웃트로")]
        [SerializeField] private PlayableDirector _outroDirector;

        protected override void Start()
        {
            base.Start();
            if (_introDirector != null) _introDirector.enabled = false;
            StartStage();
        }
        public override void SkipIntro()
        {
            if (Global.UIManager.GetCurrentPage() is InGameMainPage currentPage)
            {
                // 인트로가 할당되지 않았으면 스킵 불가
                if (_introDirector == null)
                {
                    currentPage.ShowSkipButton = false;
                    base.SkipIntro();
                    return;
                }

                // 타임라인 완전 초기화 후 마지막으로 이동
                _introDirector.Stop();
                _introDirector.time = 0;
                _introDirector.Evaluate();
                _introDirector.playableGraph.Evaluate(0f);

                // 타임라인 재생 후 즉시 마지막으로 이동
                _introDirector.enabled = true;
                _introDirector.Play();
                _introDirector.time = _introDirector.duration;
                _introDirector.Evaluate();
                _introDirector.Stop();
                _introDirector.enabled = false;

                currentPage.ShowSkipButton = false;
            }
            base.SkipIntro();
        }

        public void StartStage()
        {
            bool isIntro = PlayerPrefs.GetInt("IsIntro1") == 1;
            if (isIntro && _introDirector != null)
            {
                PlayerPrefs.SetInt("IsIntro1", 0);
                PlayerPrefs.Save();
                _levelManager.gameObject.SetActive(false);
                _introDirector.initialTime = 0;
                _introDirector.enabled = true;

                // StartStageBase();
                // 스킵 버튼 2초 후 활성화
                if (Global.UIManager.GetCurrentPage() is InGameMainPage currentPage)
                {
                    ShowSkipButtonDelayed(currentPage).Forget();
                }
            }
            else
            {
                StartStageBase();
            }
        }
        protected override async UniTask ClearStageTask()
        {
            await base.ClearStageTask();

            // 아웃트로를 LevelManager의 OnEndEvent에 등록
            if (_outroDirector != null)
            {
                _outroDirector.initialTime = 0;
                _outroDirector.enabled = true;
                _outroDirector.Play();
                await UniTask.WaitUntil(() => _outroDirector.state != PlayState.Playing);
            }

            // PlayerPrefs.Save();
        }

        private async UniTaskVoid ShowSkipButtonDelayed(InGameMainPage page)
        {
            await UniTask.Delay(5000); // 2초 대기
            page.ShowSkipButton = true;
        }
    }

}
