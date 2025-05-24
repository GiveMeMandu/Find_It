using System;
using System.Collections.Generic;
using System.Numerics;
using Data;
using UnityEngine;

namespace Manager
{

    public class Storage
    {
        //! ES3는 ScriptableObject를 지원하지 않음
        public Storage()
        {
            sceneData = new List<SceneData>();
            recipeUpgradeData = new List<UpgradeData>();
            GoldData = "0";
            CashData = "0";
            SpinTicketData = "0";
            EPS = "0";
            dailyRewardData = new DailyRewardData();
            spinRewardNames = new List<string>();
            
            // 아이템 데이터 초기화
            itemData = new List<ItemData>();
        }
        // public HashSet<int> CollectedItem = new();
        public List<SceneData> sceneData;
        public SceneName curScene;
        public List<UpgradeData> recipeUpgradeData;
        public string GoldData;
        public string CashData;
        public string SpinTicketData;
        public DailyRewardData dailyRewardData;
        public DateTime lastUTCTime;
        public DateTime lastOfflineRewardTime;
        public string EPS;
        public List<string> spinRewardNames;
        public bool AdsRemoved;
        
        // 아이템 수량 저장
        public List<ItemData> itemData;
    }
    public partial class UserDataManager
    {
        public Storage userStorage { get; private set; } = new Storage();
        private string KeyName = "Storage";
        private string FileName = "SaveFile.es3";
        /// <summary>
        /// TODO : 
        /// 1. 스팀 서버 또는 로컬 스토리지에서 클래스를 읽음 (비동기)
        /// 2. 클래스를 UserDataManager에 저장함
        /// 3. 필요한 시점마다 스팀 서버 또는 로컬 스토리지에 세이브 데이터를 저장함 (비동기)
        /// </summary>
        private ES3Settings _saveSettings = new(ES3.Location.File);
        /// <summary>
        /// 게임 켤 때 한 번 호출
        /// </summary>
        public void Load()
        {
            try 
            {
                if (ES3.FileExists(FileName) && ES3.KeyExists(KeyName, FileName))
                {
                    ES3.LoadInto(KeyName, userStorage, _saveSettings);
                }
                else
                {
                    userStorage = new Storage();
                    Save();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"저장 데이터 로드 중 오류 발생: {e.Message}");
                userStorage = new Storage();
                Save();
            }
        }
        public void Save()
        {
            ES3.Save(KeyName, userStorage, _saveSettings);
        }
        /// <summary>
        /// 모든 게임 데이터를 초기화합니다
        /// </summary>
        /// <returns>초기화 성공 여부</returns>
        public bool Reset()
        {
            try
            {
                // 파일이 존재하는 경우에만 삭제 시도
                if (ES3.FileExists(FileName))
                {
                    ES3.DeleteFile(FileName);
                }
                
                // PlayerPrefs 초기화
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                
                userStorage = new Storage();
                Save();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"데이터 리셋 중 오류 발생: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 게임 끌 때 한 번 호출
        /// </summary>
        private void OnApplicationQuit() {
            Save();
        }
        private void OnApplicationPause(bool pauseStatus) {
            if(pauseStatus)
            {
                Save();
            }   
        }

        #region 아이템 데이터 관리 헬퍼 메서드들
        
        /// <summary>
        /// 특정 아이템 타입의 ItemData를 찾습니다. 없으면 새로 생성합니다.
        /// </summary>
        public ItemData GetOrCreateItemData(ItemType itemType)
        {
            var item = userStorage.itemData.Find(x => x.itemType == itemType);
            if (item == null)
            {
                item = new ItemData { itemType = itemType, count = 0 };
                userStorage.itemData.Add(item);
            }
            return item;
        }

        /// <summary>
        /// 특정 아이템의 수량을 가져옵니다.
        /// </summary>
        public int GetItemCount(ItemType itemType)
        {
            return GetOrCreateItemData(itemType).count;
        }

        /// <summary>
        /// 특정 아이템의 수량을 설정합니다.
        /// </summary>
        public void SetItemCount(ItemType itemType, int count)
        {
            var item = GetOrCreateItemData(itemType);
            item.count = Math.Max(0, count); // 0 이하로 떨어지지 않도록
            Save();
        }

        /// <summary>
        /// 특정 아이템을 추가합니다.
        /// </summary>
        public void AddItem(ItemType itemType, int count)
        {
            var item = GetOrCreateItemData(itemType);
            item.count += count;
            Save();
        }

        /// <summary>
        /// 특정 아이템을 사용합니다. 성공하면 true 반환.
        /// </summary>
        public bool UseItem(ItemType itemType, int count = 1)
        {
            var item = GetOrCreateItemData(itemType);
            if (item.count >= count)
            {
                item.count -= count;
                Save();
                return true;
            }
            return false;
        }

        #endregion
    }
}