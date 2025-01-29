using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Manager;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using UI.Page;
using UI.Effect;
using System;
using Data;
using UnityWeld;

namespace OutGame
{
    public class PuzzleScene : SceneBase
    {
        public static float Speed = 1;
        private PuzzlePage puzzlePage = null;
        private PageSlideEffect pageSlideEffect = null;
        [SerializeField] private MapSelectView mapSelectView;
        public bool CanPlay = false;
        protected override void Start()

        {
            base.Start();
            puzzlePage = Global.UIManager.OpenPage<PuzzlePage>();
            if(mapSelectView == null) mapSelectView = FindObjectOfType<MapSelectView>();
            if (puzzlePage != null)
            {
                pageSlideEffect = puzzlePage.GetComponent<PageSlideEffect>();
            }


            CanPlay = true;
        }


        public void OnClickStartButton(int stageIndex = 0)
        {
            if(CanPlay)
                LoadingSceneManager.LoadScene(stageIndex + 4);
        }
    }
}