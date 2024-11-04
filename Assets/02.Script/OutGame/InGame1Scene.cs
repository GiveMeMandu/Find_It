using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using UI.Page;
using UnityEngine;

namespace OutGame
{
    /// <summary>
    /// 게임 데이터외 여러가지 게임 시작시 세팅을 도와줍니다.
    /// </summary>
    public class InGame1Scene : SceneBase
    {
        [SerializeField] private float timeSpeed = 1;
        protected override void Start()
        {
            base.Start();
            Global.UIManager.OpenPage<InGameMainPage>();
            Time.timeScale = 1 * timeSpeed;

            // Global.GoogleMobileAdsManager.OnAdEnd += OnAdEnd;
        }

        private void OnAdEnd(object sender, EventArgs e)
        {
            Time.timeScale = 1 * timeSpeed;
        }
    }
}