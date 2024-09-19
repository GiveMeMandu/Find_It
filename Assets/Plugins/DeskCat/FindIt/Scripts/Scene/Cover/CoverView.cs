using System;
using DeskCat.FindIt.Scripts.Core.Model;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DeskCat.FindIt.Scripts.Scene.Cover
{
    public class CoverView : MonoBehaviour
    {
        public GameObject Canvas;
        public Button IntroBtn;
        public Button PlayBtn;
        public Button SettingBtn;

        public GameObject SettingPanel;

        private void Start()
        {
            IntroBtn.onClick.AddListener(IntroBtnFunction);
            PlayBtn.onClick.AddListener(PlayBtnFunction);
            SettingBtn.onClick.AddListener(SettingBtnFunction);
            GlobalSetting.InitializeSetting();
            if (PlayerPrefs.GetInt("IsTutorial") == 1)
            {
                Canvas.gameObject.SetActive(false);
                SceneManager.LoadScene("3_Stage1");
            }
            else {
                Canvas.gameObject.SetActive(true);
            }
        }

        private void PlayBtnFunction()
        {
            SceneManager.LoadScene("2_Select");
        }
        private void IntroBtnFunction()
        {
            PlayerPrefs.SetInt("IsTutorial", 1);
            SceneManager.LoadScene("3_Stage1");
        }

        private void SettingBtnFunction()
        {
            SettingPanel.SetActive(true);
        }
            
        
    }
}