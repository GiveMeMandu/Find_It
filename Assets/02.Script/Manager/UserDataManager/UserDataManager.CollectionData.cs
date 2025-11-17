using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using SO;

namespace Manager
{
    public partial class UserDataManager
    {
        // 컬렉션 관련 유틸리티 메서드들
        public CollectionSO FindCollectionByName(string collectionName)
        {
            var manager = UnityEngine.Resources.FindObjectsOfTypeAll<Manager.CollectionManager>();
            if (manager != null && manager.Length > 0)
            {
                foreach (var collection in manager[0].GetAllCollections())
                {
                    if (collection != null && collection.name == collectionName)
                    {
                        return collection;
                    }
                }
            }
            return null;
        }

        // 컬렉션 추가 메서드
        public void AddCollection(CollectionSO collection, int count = 1)
        {
            userStorage.collectionData.AddCollection(collection, count);
            Save();
        }

        // 컬렉션 제거 메서드
        public bool RemoveCollection(CollectionSO collection, int count = 1)
        {
            bool result = userStorage.collectionData.RemoveCollection(collection, count);
            if (result)
            {
                Save();
            }
            return result;
        }

        // 특정 컬렉션의 개수 확인 메서드
        public int GetCollectionCount(CollectionSO collection)
        {
            return userStorage.collectionData.GetCollectionCount(collection);
        }

        // 모든 컬렉션 목록 가져오기
        public Dictionary<string, CollectionData.CollectionInfo> GetAllCollections()
        {
            return userStorage.collectionData.CollectionInventory;
        }
    }
}