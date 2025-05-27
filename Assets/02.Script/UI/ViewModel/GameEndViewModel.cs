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
            LoadingSceneManager.LoadScene(SceneNum.START);
        }
        [Binding]
        public void OnClickRetryBtn()
        {
            if (Global.CurrentScene != null)
            {
                LoadingSceneManager.LoadScene((int)Global.CurrentScene.SceneName);
            }
            else
            {
                // CurrentScene이 null인 경우 현재 씬 재로드
                LoadingSceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}