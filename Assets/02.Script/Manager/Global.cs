using System;
using OutGame;
using UnityEngine;

namespace Manager
{
    /// <summary>
    /// 전역 접근을 위한 최상위 객체입니다.
    /// </summary>
    /// <remarks>
    /// Global.UIManager와 같은 방식으로 매니저 객체에 접근할 수 있습니다.
    /// </remarks>
    public class Global : MMSingleton<Global>
    {
        public EventHandler OnApplicationPauseEvt;
        public static SceneBase CurrentScene { get; set; }
        public static UIManager UIManager { get; private set; }
        public static UserDataManager UserDataManager { get; set; }
        public static SoundManager SoundManager { get; set; }
        // public static GoogleMobileAdsManager GoogleMobileAdsManager { get; set; }
        public static GoldManager GoldManager { get; set; }
        public static CashManager CashManager { get; set; }
        // public static GameStateManager GameStateManager { get; set; }
        // public static InputManager InputManager { get; set; }
        // public static OptionManager OptionManager { get; private set; }
        // public static GameDataManager GameDataManager { get; set; }
        // public static SceneBase CurrentScene { get; private set; }
        // public static LocalizationManager LocalizationManager { get; private set; }
        // public static NewDialogueManager DialogueManager { get; private set; }

        //* SO 데이터 관련 매니저
        public static DailyCheckManager DailyCheckManager { get; set; }
        // public static QuestManager QuestManager { get; set; }
        public static RewardManager RewardManager { get; set; }

        protected override void Awake()
        {
            base.Awake();
            Application.targetFrameRate = 60;
            Input.multiTouchEnabled = false;

            UserDataManager = new();
            UserDataManager.Load();

            LoadManagerPrefabs();

            GoldManager = new();
            GoldManager.Initial();

            CashManager = new();
            CashManager.Initial();
        }

        private void OnApplicationPause(bool pauseStatus) => OnApplicationPauseEvt?.Invoke(this, EventArgs.Empty);
        private void OnApplicationQuit() => OnApplicationPauseEvt?.Invoke(this, EventArgs.Empty);
        private void LoadManagerPrefabs()
        {
            string prefixManager = "Prefabs/Manager/";
            if (UIManager == null)
            {
                UIManager = Instantiate(Resources.Load<UIManager>(prefixManager + nameof(UIManager)), transform);
                UIManager.name = nameof(UIManager);
            }
            if (SoundManager == null)
            {
                SoundManager = Instantiate(Resources.Load<SoundManager>(prefixManager + nameof(SoundManager)), transform);
                SoundManager.name = nameof(SoundManager);
            }
            // if (GoogleMobileAdsManager == null)
            // {
            //     GoogleMobileAdsManager = Instantiate(Resources.Load<GoogleMobileAdsManager>(prefixManager + nameof(GoogleMobileAdsManager)), transform);
            //     GoogleMobileAdsManager.name = nameof(GoogleMobileAdsManager);
            // }
            if (DailyCheckManager == null)
            {
                DailyCheckManager = Instantiate(Resources.Load<DailyCheckManager>(prefixManager + nameof(DailyCheckManager)), transform);
                DailyCheckManager.name = nameof(DailyCheckManager);
            }
            if (RewardManager == null)
            {
                RewardManager = Instantiate(Resources.Load<RewardManager>(prefixManager + nameof(RewardManager)), transform);
                RewardManager.name = nameof(RewardManager);
            }
        }
    }
}
