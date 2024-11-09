using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Manager;
using UnityEngine;
using UnityWeld.Binding;

namespace UI
{
    [Binding]
    public class GameEndViewModel : BaseViewModel
    {
        [Binding]
        public void OnClickHomeBtn()
        {
            LoadingSceneManager.LoadScene((int)SceneName.Start);
        }
        [Binding]
        public void OnClickRetryBtn()
        {
            LoadingSceneManager.LoadScene((int)Global.CurrentScene.SceneName);
        }
    }
}