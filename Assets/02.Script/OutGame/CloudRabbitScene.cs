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
    public class CloudRabbitScene : SceneBase
    {
        private CloudRabbitPage cloudRabbitPage = null;
        private PageSlideEffect pageSlideEffect = null;
        public bool CanPlay = false;
        protected override void Start()

        {
            base.Start();
            cloudRabbitPage = Global.UIManager.OpenPage<CloudRabbitPage>();
            if (cloudRabbitPage != null)
            {
                pageSlideEffect = cloudRabbitPage.GetComponent<PageSlideEffect>();
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