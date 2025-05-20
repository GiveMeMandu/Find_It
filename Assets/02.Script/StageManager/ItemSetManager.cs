using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Manager;
using UI.Page;
using UnityEngine;
using UnityEngine.Events;

namespace DeskCat.FindIt.Scripts.Core.Main.System
{
    [Serializable]
    public class ItemSetData
    {
        public string SetName;  // "당근 디저트 세트"
        public List<string> RequiredGroups;  // ["포크", "딸기", "케이크", "체리"]
        public SetCompletionObject CompletionObject;  // Inspector에서 설정
    }

    public class ItemSetManager : MMSingleton<ItemSetManager>
    {
        [SerializeField]
        private List<ItemSetData> itemSetDataList;  // Unity Inspector에서 설정 가능

        private Dictionary<string, HashSet<string>> completedGroups = new Dictionary<string, HashSet<string>>();
        public event Action<string> OnSetCompleted;  // 세트 완성시 발생하는 이벤트
        private HashSet<string> foundSets = new HashSet<string>();
        public event Action<string> OnAllSetsFound;  // 모든 세트를 찾았을 때 발생하는 이벤트

        // public 속성 추가
        public int FoundSetsCount => foundSets.Count;
        public int TotalSetsCount => itemSetDataList.Count;

        private void Start()
        {
            // 각 세트별로 완료된 그룹을 추적하기 위한 초기화
            foreach (var setData in itemSetDataList)
            {
                completedGroups[setData.SetName] = new HashSet<string>();
            }

            // LevelManager의 OnFoundObj 이벤트 구독
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnFoundObj += OnObjectFound;
            }

            // 각 세트의 CompletionObject 이벤트 설정
            foreach (var setData in itemSetDataList)
            {
                if (setData.CompletionObject != null)
                {
                    setData.CompletionObject.OnSetFound += () => OnSetObjectFound(setData.SetName);
                }
            }
        }

        private void OnObjectFound(object sender, HiddenObj obj)
        {
            string groupName = GetGroupName(obj);
            
            foreach (var setData in itemSetDataList)
            {
                if (setData.RequiredGroups.Contains(groupName))
                {
                    var (exists, isComplete, _) = LevelManager.Instance.GetGroupStatus(groupName);
                    if (exists && isComplete)
                    {
                        completedGroups[setData.SetName].Add(groupName);
                        CheckSetCompletion(setData);
                    }
                }
            }
        }

        private string GetGroupName(HiddenObj obj)
        {
            return InGameObjectNameFilter.GetBaseGroupName(obj.gameObject.name);
        }

        private void CheckSetCompletion(ItemSetData setData)
        {
            var completedGroupsForSet = completedGroups[setData.SetName];
            bool allGroupsCompletelyFound = setData.RequiredGroups.All(groupName => 
            {
                var (exists, isComplete, _) = LevelManager.Instance.GetGroupStatus(groupName);
                return exists && isComplete && completedGroupsForSet.Contains(groupName);
            });

            if (allGroupsCompletelyFound && !foundSets.Contains(setData.SetName))
            {
                Debug.Log($"세트 완성 : {setData.SetName}");
                if (setData.CompletionObject != null)
                {
                    setData.CompletionObject.ShowSetCompletionNotification();
                }
                ClearStageTask(setData).Forget();
                OnSetCompleted?.Invoke(setData.SetName);
            }
        }

        private void OnSetObjectFound(string setName)
        {
            foundSets.Add(setName);
            CheckAllSetsFound();
        }

        private void CheckAllSetsFound()
        {
            if (foundSets.Count == itemSetDataList.Count)
            {
                OnAllSetsFound?.Invoke(null);
                // LevelManager의 종료 이벤트에 등록
                LevelManager.Instance.OnEndEvnt.Add(async () => 
                {
                    await UniTask.CompletedTask;
                    return;
                });
            }
        }

        // 세트 진행 상황 확인을 위한 메서드
        public (int completed, int total) GetSetProgress(string setName)
        {
            if (!completedGroups.ContainsKey(setName)) return (0, 0);

            var setData = itemSetDataList.Find(x => x.SetName == setName);
            if (setData == null) return (0, 0);

            return (completedGroups[setName].Count, setData.RequiredGroups.Count);
        }

        protected virtual async UniTask ClearStageTask(ItemSetData setData)
        {
            var page = Global.UIManager.OpenPage<InGameMissionCompletePage>();
            page.MissionName = setData.SetName;
            await page.WaitForClose();
        }


        public List<ItemSetData> GetItemSetDataList()
        {
            return itemSetDataList;
        }
    }
}
