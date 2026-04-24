using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using OptionPageNamespace;
using Manager;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityWeld.Binding;

namespace UI
{
    [UnityWeld.Binding.Binding]
    public partial class OptionPage : PageViewModel
    {
        public override bool BlockEscape => false;

        [Header("References")]
        public OptionGroup optionGroupPrefab;
        public RectTransform contentRoot;
        public TabGroup tabGroup;

        [Label("플레이탭 그룹")]
        public GameObject PlayTabContent;
        [Label("아지트에 있을 때 플레이탭 그룹")]
        public GameObject AgitPlayTabContent;

        public SoundManager soundManager => Global.SoundManager;

        public enum TabType
        {
            Play,
            Options,
            Controls,
        }

        protected override void Awake()
        {
            base.Awake();
            if (optionGroupPrefab != null)
                optionGroupPrefab.gameObject.SetActive(false);
        }

        public override void Init(params object[] parameters)
        {
            InitializeTabs();
            Time.timeScale = 0;

            Global.InputManager.DisableGameInputOnly();


            // 첫 번째 탭 선택 (Play)
            tabGroup.SelectTab(0);
        }
        public override void OnClose()
        {

            Time.timeScale = 1;
            base.OnClose();
            Global.InputManager.EnableGameInputOnly();

        }

        private void InitializeTabs()
        {
            tabGroup.Clear();
            tabGroup.AddTab("UI/Menu/Play", () => OpenTab(TabType.Play));
            tabGroup.AddTab("UI/Menu/Options", () => OpenTab(TabType.Options));
            tabGroup.AddTab("UI/Menu/Key_Settings", () => OpenTab(TabType.Controls));
        }

        public void OpenTab(TabType tab)
        {
            // contentRoot 에 있는 active한 Child 들을 모두 제거
            foreach (Transform child in contentRoot)
            {
                if (child.gameObject.activeSelf)
                {
                    Destroy(child.gameObject);
                }
            }

            // Ensure PlayTabContent is only active when Play tab is open
            if (PlayTabContent != null)
                PlayTabContent.SetActive(false);
            if (AgitPlayTabContent != null)
                AgitPlayTabContent.SetActive(false);

            switch (tab)
            {
                case TabType.Play:
                    if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == (int)Data.SceneNum.START)
                    {
                        if (AgitPlayTabContent != null)
                            AgitPlayTabContent.SetActive(true);
                        if (PlayTabContent != null)
                            PlayTabContent.SetActive(false);
                    }
                    else
                    {
                        if (AgitPlayTabContent != null)
                            AgitPlayTabContent.SetActive(false);
                        // Play 탭은 동적 그룹 생성을 진행하지 않고 바인딩된 메서드만 사용하도록 비워둡니다.
                        if (PlayTabContent != null)
                            PlayTabContent.SetActive(true);
                    }
                    break;
                case TabType.Options:
                    CreateOptionsGroup();
                    break;
                case TabType.Controls:
                    CreateControlsOptionGroup();
                    break;
            }
        }

        private void CreateOptionsGroup()
        {
            // 기존 Play 탭에 있던 언어/스크린 설정 및 그래픽, 사운드 설정을 모두 Options 탭으로 통합합니다.
            CreateLanguageOption();
            CreateScreenOption();
            CreateGraphicsOptionGroup();
            CreateSoundOptionGroup();
        }

        [UnityWeld.Binding.Binding]
        public void GoToStartScene()
        {
            Global.UIManager.ClosePage(this);
            LoadingSceneManager.LoadScene(Data.SceneNum.START);
        }

        [UnityWeld.Binding.Binding]
        public void RestartCurrentScene()
        {
            Global.UIManager.ClosePage(this);

            // StageManager에서 관리하는 현재 스테이지의 씬 이름으로 재시작
            if (Global.StageManager != null && !string.IsNullOrEmpty(Global.StageManager.CurrentStageSceneName))
            {
                LoadingSceneManager.LoadSceneByName(Global.StageManager.CurrentStageSceneName);
            }
            else
            {
                // StageManager 정보가 없을 경우 폴백 (현재 활성화된 씬 재로드)
                LoadingSceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }
        }
        [Binding]
        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
        }


        private OptionGroup CreateOptionGroup(string labelTerm)
        {
            var optionGroupObj = Instantiate(optionGroupPrefab, contentRoot).gameObject;
            optionGroupObj.SetActive(true);
            var optionGroup = optionGroupObj.GetComponent<OptionGroup>();
            optionGroup.labelText.SetTerm(labelTerm);
            return optionGroup;
        }

        private void CreateSoundOptionGroup()
        {
            var soundGroup = CreateOptionGroup("UI/Option/Sound");

            // 배경음 (BGM)
            var bgmSlider = soundGroup.CreateOptionSlider();
            bgmSlider.Init("UI/Option/BGM", (int)soundManager.BGMVolume, (value) =>
            {
                soundManager.BGMVolume = value;
            });

            // 효과음 (SFX)
            var sfxSlider = soundGroup.CreateOptionSlider();
            sfxSlider.Init("UI/Option/SFX", (int)soundManager.SFXVolume, (value) =>
            {
                soundManager.SFXVolume = value;
            });
        }
    }
}
