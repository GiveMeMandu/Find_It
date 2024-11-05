using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DeskCat.FindIt.Scripts.Core.Main.System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

namespace InGame
{

    public class Stage1Manager : LevelManagerCount, IStageManager
    {
        [SerializeField] private LevelManager _levelManager;
        [LabelText("인트로")]
        [SerializeField] private PlayableDirector _introDirector;
        [LabelText("아웃트로")]
        [SerializeField] private PlayableDirector _outroDirector;
        [SerializeField] private PlayableDirector _outroDirector2;
        [SerializeField] private PlayableDirector _outroDirector3;

        protected override void Start()
        {
            base.Start();
            _introDirector.enabled = false;
            StartStage();
            GameManager.SetResolution();
            levelManager.OnEndEvnt.Add(ClearStageTask);
        }

        public void StartStage()
        {
            bool isTutorial = PlayerPrefs.GetInt("IsTutorial") == 1;
            if (isTutorial)
            {
                PlayerPrefs.SetInt("IsTutorial", 0);
                PlayerPrefs.Save();
                _levelManager.gameObject.SetActive(false);
                _introDirector.initialTime = 0;
                _introDirector.enabled = true;
            }
            else _levelManager.gameObject.SetActive(true);
        }

        public void ClearStage()
        {

        }
        public async UniTask ClearStageTask()
        {
            _outroDirector.initialTime = 0;
            _outroDirector.enabled = true;
            _outroDirector.Play();
            _outroDirector2.initialTime = 0;
            _outroDirector2.enabled = true;
            _outroDirector2.Play();
            await UniTask.WaitUntil(() => _outroDirector.state != PlayState.Playing);
            await UniTask.WaitUntil(() => _outroDirector2.state != PlayState.Playing);
            await UniTask.WaitForSeconds(5.5f);
            _outroDirector3.initialTime = 0;
            _outroDirector3.enabled = true;
            _outroDirector3.Play();
            await UniTask.WaitUntil(() => _outroDirector3.state != PlayState.Playing);

            PlayerPrefs.Save();
        }
    }
}
