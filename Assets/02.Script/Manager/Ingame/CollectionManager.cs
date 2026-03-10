using System.Collections;
using System.Collections.Generic;
using SO;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Manager
{
    public class CollectionManager : MonoBehaviour
    {
        public List<CollectionSO> collectionSOList;
        private const string collectionSOPath = "Assets/02.Script/Data/SO/Collection";

        /// <summary>
        /// 현재 스테이지에서 획득한 컬렉션(스티커) 목록
        /// </summary>
        private List<CollectionSO> _earnedThisStage = new List<CollectionSO>();

        /// <summary>
        /// 현재 스테이지에서 획득한 컬렉션 목록을 반환합니다.
        /// </summary>
        public List<CollectionSO> GetEarnedThisStage() => new List<CollectionSO>(_earnedThisStage);

        /// <summary>
        /// 스테이지 시작 시 호출하여 획득 목록을 초기화합니다.
        /// </summary>
        public void ClearEarnedThisStage() => _earnedThisStage.Clear();
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
        /// HiddenObj가 발견되었을 때, 매칭되는 CollectionSO를 찾아 유저 데이터에 추가합니다.
        /// 매칭 조건:
        /// 1. HiddenObj의 GameObject 이름 == CollectionSO 에셋 이름
        /// 2. HiddenObj의 스프라이트 이름 == CollectionSO의 collectionImage 스프라이트 이름
        /// 하나라도 해당하면 컬렉션을 획득합니다.
        /// </summary>
        public void TryCollectFromHiddenObj(DeskCat.FindIt.Scripts.Core.Main.System.HiddenObj hiddenObj)
        {
            if (hiddenObj == null || collectionSOList == null) return;

            string objName = hiddenObj.gameObject.name;
            // HiddenObj의 스프라이트 이름 (UISprite 또는 SpriteRenderer)
            string objSpriteName = null;
            Sprite uiSprite = hiddenObj.GetUISprite();
            if (uiSprite != null)
            {
                objSpriteName = uiSprite.name;
            }

            foreach (var collection in collectionSOList)
            {
                if (collection == null) continue;

                bool nameMatch = !string.IsNullOrEmpty(objName) && objName == collection.name;
                bool spriteMatch = !string.IsNullOrEmpty(objSpriteName)
                    && collection.collectionImage != null
                    && objSpriteName == collection.collectionImage.name;

                if (nameMatch || spriteMatch)
                {
                    Global.UserDataManager.AddCollection(collection);
                    _earnedThisStage.Add(collection);
                    Debug.Log($"[CollectionManager] 컬렉션 획득: {collection.name} (nameMatch={nameMatch}, spriteMatch={spriteMatch})");
                }
            }
        }

        /// <summary>
        /// Returns collections that belong to a specific chapter.
        /// </summary>
        /// <param name="chapterIndex">StageManager의 chapters 리스트 인덱스</param>
        public List<CollectionSO> GetCollectionsForChapter(int chapterIndex)
        {
            List<CollectionSO> results = new List<CollectionSO>();
            foreach (var item in collectionSOList)
            {
                if (item != null && item.chapterIndex == chapterIndex)
                {
                    results.Add(item);
                }
            }
            return results;
        }
    }
}
