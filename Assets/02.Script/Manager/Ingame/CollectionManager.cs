using System.Collections;
using System.Collections.Generic;
using SO;
using Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Manager
{
    public class CollectionManager : MonoBehaviour
    {
        public List<CollectionSO> collectionSOList;
        private const string collectionSOPath = "Assets/02.Script/Data/SO/Collection";
#if UNITY_EDITOR
        [Button("SO 불러오기")]
        public void LoadAssets()
        {
            collectionSOList = Data.HelperFunctions.GetScriptableObjects<CollectionSO>(collectionSOPath);
        }
#endif
        public List<CollectionSO> GetAllCollections()
        {
            return collectionSOList;
        }

        /// <summary>
        /// Returns the design-time/default count from the SO.
        /// If you want player-owned counts, replace this implementation to use UserDataManager.
        /// </summary>
        public int GetCollectionCount(CollectionSO collection)
        {
            return Global.UserDataManager.GetCollectionCount(collection);
        }

        /// <summary>
        /// Returns collections that belong to a specific scene.
        /// </summary>
        public List<CollectionSO> GetCollectionsForScene(SceneName scene)
        {
            List<CollectionSO> results = new List<CollectionSO>();
            foreach (var item in collectionSOList)
            {
                if (item != null && item.scene == scene)
                {
                    results.Add(item);
                }
            }
            return results;
        }
    }
}
