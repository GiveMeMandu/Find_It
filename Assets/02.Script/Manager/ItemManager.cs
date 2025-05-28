using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    public class ItemManager : MonoBehaviour
    {
        public event EventHandler<ItemType> OnItemCountChanged;
        
        public void Initial()
        {
            if (Global.DailyCheckManager != null)
            {
                Global.DailyCheckManager.SubscribeToDayChanged(OnDailyCheck);
            }
        }

        public void Dispose()
        {
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
            return true; // 항상 가능
        }

        public int GetRemainingAdViews()
        {
            return 0; // 광고 시청 횟수 관련 필드 제거
        }

        private bool TryWatchAdForItem(ItemType itemType)
        {
            Debug.Log($"{GetItemTypeName(itemType)} 아이템이 부족하여 바로 지급합니다.");
            
            // 광고 시청 없이 바로 아이템 지급 (횟수 제한 없음)
            AddItem(itemType, 1);
            Debug.Log($"아이템 지급 완료! {GetItemTypeName(itemType)} 아이템을 1개 획득했습니다.");
            
            return true; // 바로 사용 가능
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

        private void OnDailyCheck(object sender, DateTime e)
        {
            // 일일 체크 시 광고 시청 횟수 초기화
            // 광고 시청 횟수 관련 필드 제거
        }

        #endregion
    }
} 