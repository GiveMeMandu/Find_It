using System;
using DG.Tweening;
using OutGame;
using Pooling;
using UnityEngine;

namespace Manager
{
    /// <summary>
    /// ?�역 ?�근???�한 최상??객체?�니??
    /// </summary>
    /// <remarks>
    /// Global.UIManager?� 같�? 방식?�로 매니?� 객체???�근?????�습?�다.
    /// </remarks>
    public class Global : MMSingleton<Global>
    {
        public EventHandler OnApplicationPauseEvt;
        public static StageManager StageManager { get; set; }
        public static SceneBase CurrentScene
        {
            get => StageManager?.CurrentScene;
            set { if (StageManager != null) StageManager.CurrentScene = value; }
        }
        public static UIManager UIManager { get; private set; }
        public static UserDataManager UserDataManager { get; set; }
        public static SoundManager SoundManager { get; set; }
        public static GoogleMobileAdsManager GoogleMobileAdsManager { get; set; }
        public static CoinManager CoinManager { get; set; }
        public static CashManager CashManager { get; set; }
        public static SpinTicketManager SpinTicketManager { get; set; }
        public static ItemManager ItemManager { get; set; }
        public static MainMenuSelectedManager MainMenuSelectedManager { get; set; }
        public static OptionManager OptionManager { get; set; }
        // public static GameStateManager GameStateManager { get; set; }
        public static InputManager InputManager { get; set; }
        // public static OptionManager OptionManager { get; private set; }
        // public static GameDataManager GameDataManager { get; set; }
        // public static SceneBase CurrentScene { get; private set; }
        // public static LocalizationManager LocalizationManager { get; private set; }
        // public static NewDialogueManager DialogueManager { get; private set; }
        public static CollectionManager CollectionManager { get; set; }

        //* SO ?�이??관??매니?�
        public static DailyCheckManager DailyCheckManager { get; set; }
        public static QuestManager QuestManager { get; set; }
        public static RewardManager RewardManager { get; set; }
        public static int StageTimer { get; set; } = 600;
        public static UIEffectManager UIEffectManager { get; set; }
        public static PoolManager PoolManager { get; set; }
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

            OptionManager = new OptionManager();
            OptionManager.Init();

            // CashManager = new();
            // CashManager.Initial();

            // SpinTicketManager = new();
            // SpinTicketManager.Initial();

            // ItemManager???�제 LoadManagerPrefabs?�서 로드??
            // ItemManager = new();
            // ItemManager.Initial();

        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeDOTween()
        {
            // DOTween 초기??�?캐퍼?�티 ?�장 (IndexOutOfRangeException 방�?)
            // Awake ?�계보다 먼�? ?�행?�어 기본 Capacity(200, 50)�?초기?�되??것을 막습?�다.
            DOTween.SetTweensCapacity(8000, 2000);
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

            // StageManager 로드
            try
            {
                if (StageManager == null)
                {
                    StageManager = Instantiate(Resources.Load<StageManager>(prefixManager + nameof(StageManager)), transform);
                    StageManager.name = nameof(StageManager);
                    StageManager.Initialize();
                    Debug.Log("StageManager loaded successfully");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load StageManager: {e.Message}");
            }

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

            // GoogleMobileAdsManager 로드 (모바?�에??문제가 ?????�음)
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

            // PoolManager 로드
            try
            {
                if (PoolManager == null)
                {
                    PoolManager = Instantiate(Resources.Load<PoolManager>(prefixManager + nameof(PoolManager)), transform);
                    PoolManager.name = nameof(PoolManager);
                    Debug.Log("PoolManager loaded successfully");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load PoolManager: {e.Message}");
            }

            Debug.Log("LoadManagerPrefabs completed");
        }
    }
}
