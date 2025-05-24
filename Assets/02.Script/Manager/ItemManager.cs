using System;
using UnityEngine;

namespace Manager
{
    public enum ItemType
    {
        Compass,        // 나침반
        Stopwatch,      // 초시계
        Hint            // 힌트
    }
    public class ItemData
    {
        public ItemType itemType;
        public int count;
    }

    public class ItemManager
    {
        public event EventHandler<ItemType> OnItemCountChanged;
        
        // 광고 시청 관련 상수
        private const int MAX_AD_VIEWS_PER_DAY = 10;
        private const string AD_VIEWS_PREFS_KEY = "ItemManager_RemainingAdViews";
        private const string LAST_AD_VIEWS_DATE_KEY = "ItemManager_LastAdViewsDate";
        
        private int _remainingAdViews = MAX_AD_VIEWS_PER_DAY;
        
        public void Initial()
        {
            LoadRemainingAdViews();
            Global.DailyCheckManager.SubscribeToDayChanged(OnDailyCheck);
        }

        public void Dispose()
        {
            SaveRemainingAdViews();
            if (Global.DailyCheckManager != null)
            {
                Global.DailyCheckManager.OnDayChanged -= OnDailyCheck;
            }
        }

        #region 아이템 수량 관리
        
        public int GetItemCount(ItemType itemType)
        {
            return Global.UserDataManager.GetItemCount(itemType);
        }

        public void AddItem(ItemType itemType, int count = 1)
        {
            Global.UserDataManager.AddItem(itemType, count);
            OnItemCountChanged?.Invoke(this, itemType);
        }

        public bool UseItem(ItemType itemType)
        {
            int currentCount = GetItemCount(itemType);
            
            if (currentCount <= 0)
            {
                // 아이템이 없으면 광고 시청 시도
                return TryWatchAdForItem(itemType);
            }

            bool success = Global.UserDataManager.UseItem(itemType, 1);
            if (success)
            {
                OnItemCountChanged?.Invoke(this, itemType);
            }
            return success;
        }

        public bool CanUseItem(ItemType itemType)
        {
            return GetItemCount(itemType) > 0 || CanWatchAdForItem();
        }

        #endregion

        #region 광고 시청 관리

        public bool CanWatchAdForItem()
        {
            return _remainingAdViews > 0;
        }

        public int GetRemainingAdViews()
        {
            return _remainingAdViews;
        }

        private bool TryWatchAdForItem(ItemType itemType)
        {
            if (!CanWatchAdForItem())
            {
                Debug.Log($"오늘의 광고 시청 횟수를 모두 사용했습니다. ({_remainingAdViews}/{MAX_AD_VIEWS_PER_DAY})");
                return false;
            }

            Debug.Log($"{GetItemTypeName(itemType)} 아이템이 부족하여 광고를 시청합니다.");
            
            if (Global.GoogleMobileAdsManager != null)
            {
                Global.GoogleMobileAdsManager.ShowRewardedAd(() => 
                {
                    // 광고 시청 성공 시 아이템 지급
                    _remainingAdViews--;
                    SaveRemainingAdViews();
                    
                    AddItem(itemType, 1);
                    Debug.Log($"광고 시청 완료! {GetItemTypeName(itemType)} 아이템을 1개 획득했습니다.");
                });
            }
            
            return false; // 광고 시청 중이므로 즉시 사용은 불가
        }

        private string GetItemTypeName(ItemType itemType)
        {
            return itemType switch
            {
                ItemType.Compass => "나침반",
                ItemType.Stopwatch => "초시계",
                ItemType.Hint => "힌트",
                _ => "알 수 없는 아이템"
            };
        }

        #endregion

        #region 광고 시청 횟수 저장/로드

        private void LoadRemainingAdViews()
        {
            string lastDateString = PlayerPrefs.GetString(LAST_AD_VIEWS_DATE_KEY, "");
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            
            if (lastDateString == today)
            {
                _remainingAdViews = PlayerPrefs.GetInt(AD_VIEWS_PREFS_KEY, MAX_AD_VIEWS_PER_DAY);
            }
            else
            {
                // 날짜가 다르면 광고 시청 횟수 초기화
                _remainingAdViews = MAX_AD_VIEWS_PER_DAY;
                SaveRemainingAdViews();
            }
        }

        private void SaveRemainingAdViews()
        {
            PlayerPrefs.SetInt(AD_VIEWS_PREFS_KEY, _remainingAdViews);
            PlayerPrefs.SetString(LAST_AD_VIEWS_DATE_KEY, DateTime.Now.ToString("yyyy-MM-dd"));
            PlayerPrefs.Save();
        }

        private void OnDailyCheck(object sender, DateTime e)
        {
            // 일일 체크 시 광고 시청 횟수 초기화
            _remainingAdViews = MAX_AD_VIEWS_PER_DAY;
            SaveRemainingAdViews();
        }

        #endregion
    }
} 