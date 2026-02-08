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
        public static GoogleMobileAdsManager GoogleMobileAdsManager { get; set; }
        public static CoinManager CoinManager { get; set; }
        public static CashManager CashManager { get; set; }
        public static SpinTicketManager SpinTicketManager { get; set; }
        public static ItemManager ItemManager { get; set; }
        public static MainMenuSelectedManager MainMenuSelectedManager { get; set; }
        // public static GameStateManager GameStateManager { get; set; }
        public static InputManager InputManager { get; set; }
        // public static OptionManager OptionManager { get; private set; }
        // public static GameDataManager GameDataManager { get; set; }
        // public static SceneBase CurrentScene { get; private set; }
        // public static LocalizationManager LocalizationManager { get; private set; }
        // public static NewDialogueManager DialogueManager { get; private set; }
        public static CollectionManager CollectionManager { get; set; }

        //* SO 데이터 관련 매니저
        public static DailyCheckManager DailyCheckManager { get; set; }
        public static QuestManager QuestManager { get; set; }
        public static RewardManager RewardManager { get; set; }
        public static int StageTimer { get; set; } = 600;
        public static UIEffectManager UIEffectManager { get; set; }

        protected override void Awake()
        {
            base.Awake();
            Application.targetFrameRate = 60;
            Input.multiTouchEnabled = false;

            UserDataManager = new();
            UserDataManager.Load();

            LoadManagerPrefabs();

            CoinManager = new();
            CoinManager.Initial();

            // CashManager = new();
            // CashManager.Initial();

            // SpinTicketManager = new();
            // SpinTicketManager.Initial();

            // ItemManager는 이제 LoadManagerPrefabs에서 로드됨
            // ItemManager = new();
            // ItemManager.Initial();
        }

        private void OnApplicationPause(bool pauseStatus) => OnApplicationPauseEvt?.Invoke(this, EventArgs.Empty);
        private void OnApplicationQuit()
        {
            OnApplicationPauseEvt?.Invoke(this, EventArgs.Empty);
            if (ItemManager != null)
            {
                ItemManager.Dispose();
            }
        }
        private void LoadManagerPrefabs()
        {
            string prefixManager = "Prefabs/Manager/";
            
            // UIManager 로드
            try
            {
                if (UIManager == null)
                {
                    UIManager = Instantiate(Resources.Load<UIManager>(prefixManager + nameof(UIManager)), transform);
                    UIManager.name = nameof(UIManager);
                    Debug.Log("UIManager loaded successfully");
                }
                if (UIEffectManager == null)
                {
                    UIEffectManager = UIManager.GetComponent<UIEffectManager>();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load UIManager: {e.Message}");
            }
            
            // SoundManager 로드
            try
            {
                if (SoundManager == null)
                {
                    SoundManager = Instantiate(Resources.Load<SoundManager>(prefixManager + nameof(SoundManager)), transform);
                    SoundManager.name = nameof(SoundManager);
                    Debug.Log("SoundManager loaded successfully");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load SoundManager: {e.Message}");
            }
            
            // DailyCheckManager 로드
            try
            {
                if (DailyCheckManager == null)
                {
                    DailyCheckManager = Instantiate(Resources.Load<DailyCheckManager>(prefixManager + nameof(DailyCheckManager)), transform);
                    DailyCheckManager.name = nameof(DailyCheckManager);
                    Debug.Log("DailyCheckManager loaded successfully");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load DailyCheckManager: {e.Message}");
            }
            
            // RewardManager 로드
            try
            {
                if (RewardManager == null)
                {
                    RewardManager = Instantiate(Resources.Load<RewardManager>(prefixManager + nameof(RewardManager)), transform);
                    RewardManager.name = nameof(RewardManager);
                    Debug.Log("RewardManager loaded successfully");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load RewardManager: {e.Message}");
            }
            
            // InputManager 로드
            try
            {
                if (InputManager == null)
                {
                    InputManager = Instantiate(Resources.Load<InputManager>(prefixManager + nameof(InputManager)), transform);
                    InputManager.name = nameof(InputManager);
                    Debug.Log("InputManager loaded successfully");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load InputManager: {e.Message}");
            }
            
            // QuestManager 로드
            try
            {
                if (QuestManager == null)
                {
                    QuestManager = Instantiate(Resources.Load<QuestManager>(prefixManager + nameof(QuestManager)), transform);
                    QuestManager.name = nameof(QuestManager);
                    Debug.Log("QuestManager loaded successfully");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load QuestManager: {e.Message}");
            }
            
            // GoogleMobileAdsManager 로드 (모바일에서 문제가 될 수 있음)
            try
            {
                if (GoogleMobileAdsManager == null)
                {
                    GoogleMobileAdsManager = Instantiate(Resources.Load<GoogleMobileAdsManager>(prefixManager + nameof(GoogleMobileAdsManager)), transform);
                    GoogleMobileAdsManager.name = nameof(GoogleMobileAdsManager);
                    Debug.Log("GoogleMobileAdsManager loaded successfully");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load GoogleMobileAdsManager: {e.Message}");
            }
            
            // ItemManager 로드
            try
            {
                if (ItemManager == null)
                {
                    ItemManager = Instantiate(Resources.Load<ItemManager>(prefixManager + nameof(ItemManager)), transform);
                    ItemManager.name = nameof(ItemManager);
                    ItemManager.Initial();
                    Debug.Log("ItemManager loaded successfully");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load ItemManager: {e.Message}");
            }
            
            // MainMenuSelectedManager 로드
            try
            {
                if (MainMenuSelectedManager == null)
                {
                    MainMenuSelectedManager = Instantiate(Resources.Load<MainMenuSelectedManager>(prefixManager + nameof(MainMenuSelectedManager)), transform);
                    MainMenuSelectedManager.name = nameof(MainMenuSelectedManager);
                    Debug.Log("MainMenuSelectedManager loaded successfully");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load MainMenuSelectedManager: {e.Message}");
            }

            if (CollectionManager == null)
            {
                CollectionManager = Instantiate(Resources.Load<CollectionManager>(prefixManager + nameof(CollectionManager)), transform);
                CollectionManager.name = nameof(CollectionManager);
                Debug.Log("CollectionManager loaded successfully");
            }
            
            Debug.Log("LoadManagerPrefabs completed");
        }
    }
}
