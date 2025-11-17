using System.Collections;
using System.Collections.Generic;
using SO;

namespace Data
{
    public class CollectionData
    {
        public CollectionData()
        {
            CollectionInventory = new Dictionary<string, CollectionInfo>();
        }

        [System.Serializable]
        public class CollectionInfo
        {
            public int Count;
            public string CollectionName;

            public CollectionInfo(int count, string collectionName)
            {
                Count = count;
                CollectionName = collectionName;
            }
        }

        public Dictionary<string, CollectionInfo> CollectionInventory;

        public void AddCollection(CollectionSO collection, int count = 1)
        {
            string collectionKey = collection.name;
            if (CollectionInventory.ContainsKey(collectionKey))
            {
                CollectionInventory[collectionKey].Count += count;
            }
            else
            {
                CollectionInventory.Add(collectionKey, new CollectionInfo(count, collection.collectionName));
            }
        }

        public bool RemoveCollection(CollectionSO collection, int count = 1)
        {
            string collectionKey = collection.name;
            if (CollectionInventory.ContainsKey(collectionKey))
            {
                if (CollectionInventory[collectionKey].Count >= count)
                {
                    CollectionInventory[collectionKey].Count -= count;
                    if (CollectionInventory[collectionKey].Count <= 0)
                    {
                        CollectionInventory.Remove(collectionKey);
                    }
                    return true;
                }
            }
            return false;
        }

        public int GetCollectionCount(CollectionSO collection)
        {
            string collectionKey = collection.name;
            if (CollectionInventory.ContainsKey(collectionKey))
            {
                return CollectionInventory[collectionKey].Count;
            }
            return 0;
        }
    }
}