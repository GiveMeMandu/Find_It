using DeskCat.FindIt.Scripts.Core.Model;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DeskCat.FindIt.Scripts.Scene.Cover
{
    public class CoverSettingView : MonoBehaviour
    {
        public GameObject MainPanel;
        public Button CloseBtn;

        public Slider MusicSlider;
        public Slider SoundFxSlider;

        [Header("MultiLanguages")] public Text LanguageTextValue;
        public Button PrevBtn;
        public Button NextBtn;

        public Button BackToTitleBtn;
        public Button ResumeBtn;
        public string TitleSceneName = "Cover";

        private void Start()
        {
            CloseBtn.onClick.AddListener(CloseBtnFunction);

            MusicSlider.onValueChanged.AddListener(GlobalSetting.ChangeMusicVolume);
            SoundFxSlider.onValueChanged.AddListener(GlobalSetting.ChangeSoundVolume);
            PrevBtn.onClick.AddListener(PrevLanguage);
            NextBtn.onClick.AddListener(NextLanguage);

            if (BackToTitleBtn != null)
            {
                BackToTitleBtn.onClick.AddListener(BackToTitle);
            }

            if (ResumeBtn != null)
            {
                ResumeBtn.onClick.AddListener(Resume);
            }

            GlobalSetting.LanguageChangeAction += LanguageChange;

            InitializeValue();
        }

        public void OpenPauseSetting()
        {
            MainPanel.SetActive(true);
        }

        private void Resume()
        {
            MainPanel.SetActive(false);
        }

        private void BackToTitle()
        {
            SceneManager.LoadScene(TitleSceneName);
        }

        private void OnDestroy()
        {
            GlobalSetting.LanguageChangeAction -= LanguageChange;
            MusicSlider.onValueChanged.RemoveAllListeners();
            SoundFxSlider.onValueChanged.RemoveAllListeners();
            PrevBtn.onClick.RemoveAllListeners();
            NextBtn.onClick.RemoveAllListeners();
            CloseBtn.onClick.RemoveAllListeners();
        }

        private void InitializeValue()
        {
            MusicSlider.value = GlobalSetting.MusicVolume;
            SoundFxSlider.value = GlobalSetting.SoundVolume;
            LanguageTextValue.text = GlobalSetting.CurrentLanguage;
        }

        private void LanguageChange(string data)
        {
            LanguageTextValue.text = data;
        }

        private void NextLanguage()
        {
            GlobalSetting.NextLanguage();
        }

        private void PrevLanguage()
        {
            GlobalSetting.PrevLanguage();
        }

        private void CloseBtnFunction()
        {
            MainPanel.SetActive(false);
        }
    }
}