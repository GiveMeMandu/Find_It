using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Manager;
using NaughtyAttributes;
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
        [InfoBox("각 세트 요구 그룹 적을 땐 hide 글자 없애기", EInfoBoxType.Warning)]
        [SerializeField]
        private List<ItemSetData> itemSetDataList;  // Unity Inspector에서 설정 가능

        private Dictionary<string, HashSet<string>> completedGroups = new Dictionary<string, HashSet<string>>();
        public event Action<string> OnSetCompleted;  // 세트 완성시 발생하는 이벤트
        private HashSet<string> foundSets = new HashSet<string>();
        public event Action<string> OnAllSetsFound;  // 모든 세트를 찾았을 때 발생하는 이벤트

        // SetCompletionObject 클릭 상태 추적
        private HashSet<string> clickedCompletionObjects = new HashSet<string>();
        private bool allSetsFoundAndWaitingForClicks = false;

        // 카메라 연출 중 상태
        public bool IsPlayingCameraEffect { get; set; } = false;

        // public 속성 추가
        public int FoundSetsCount => foundSets.Count;
        public int TotalSetsCount => itemSetDataList?.Count ?? 0;
        public bool AllSetsFoundAndWaitingForClicks => allSetsFoundAndWaitingForClicks;

        private void Start()
        {
            // itemSetDataList null 체크
            if (itemSetDataList != null && itemSetDataList.Count != 0)
            {
                // 각 세트별로 완료된 그룹을 추적하기 위한 초기화
                foreach (var setData in itemSetDataList)
                {
                    if (setData != null && !string.IsNullOrEmpty(setData.SetName))
                    {
                        completedGroups[setData.SetName] = new HashSet<string>();
                    }
                }
            }
            else
            {
                Debug.LogWarning("[ItemSetManager] itemSetDataList is null or empty");
            }

            // LevelManager의 OnFoundObj 이벤트 구독
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnFoundObj += OnObjectFound;
            }
        }

        private void OnObjectFound(object sender, HiddenObj obj)
        {
            // obj null 체크
            if (obj == null) return;
            
            string groupName = GetGroupName(obj);
            if (string.IsNullOrEmpty(groupName)) return;
            
            // itemSetDataList null 체크
            if (itemSetDataList == null) return;
            
            foreach (var setData in itemSetDataList)
            {
                if (setData != null && setData.RequiredGroups != null && setData.RequiredGroups.Contains(groupName))
                {
                    var (exists, isComplete, _) = LevelManager.Instance.GetGroupStatus(groupName);
                    if(exists)
                    {
                        // 세트에 속한 물건을 찾았을 때 알림 페이지 표시
                        ShowTaskAlertPage(setData, obj).Forget();

                        // CompletionObject에 알림 (찾음 표시 등)
                        if (setData.CompletionObject != null)
                        {
                            setData.CompletionObject.OnIngredientFound();
                        }
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
                // Debug.Log($"세트 완성 : {setData.SetName}");
                if (setData.CompletionObject != null)
                {
                    setData.CompletionObject.ShowSetCompletionNotification();
                    // Debug.Log($"SetCompletionObject notification shown for: {setData.SetName}");
                }
                foundSets.Add(setData.SetName);
                
                // 세트 완성 시 미션 완료 페이지와 연출 페이지 동시 오픈
                ShowSetCompletePages(setData).Forget();

                OnSetCompleted?.Invoke(setData.SetName);
                CheckAllSetsFound();
            }
        }

        private async UniTaskVoid ShowSetCompletePages(ItemSetData setData)
        {
            // 미션 완료 페이지 (재료 표시)
            var infoPage = Global.UIManager.OpenPage<InGameMissionCompletePage>();
            
            // 정보 초기화 (아이콘은 첫 번째 그룹이나 null 처리)
            int currentFound = setData.RequiredGroups.Count;
            int totalNeeded = setData.RequiredGroups.Count;
            
            infoPage.Initialize(
                missionName: setData.SetName,
                missionNameDivider: string.Format("<alpha=#00>{0}", setData.SetName),
                missionSetIcon: null, 
                missionSetFoundLeft: $"({currentFound} / {totalNeeded})",
                isSetFoundComplete: true,
                isGroupComplete: true,
                missionStatus: "Mission Complete!",
                missionNameAlpha: 1f
            );

            // 연출 페이지 동시 오픈
            var visualPage = Global.UIManager.OpenPage<IngameMissionCompleteVisualPage>();
            if (visualPage != null)
            {
                // CompletionObject가 있으면 그 위치로 카메라 이동, 없으면 null 전달 (연출 페이지에서 예외처리됨)
                GameObject target = setData.CompletionObject != null ? setData.CompletionObject.gameObject : null;
                bool enableCameraEffect = setData.CompletionObject != null && setData.CompletionObject.enableCameraEffect;
                float cameraZoomSize = setData.CompletionObject != null ? setData.CompletionObject.defaultCameraZoomSize : 0f;
                visualPage.Initialize(setData.SetName, target, enableCameraEffect, cameraZoomSize);
            }

            await infoPage.WaitForClose();
        }

        // SetCompletionObject에서 호출: 미션 현황판 보기
        public void ShowMissionIngredientsPage(string setName)
        {
            var setData = itemSetDataList.Find(x => x.SetName == setName);
            if (setData != null)
            {
                ShowMissionStatusPage(setData).Forget();
            }
        }

        protected virtual async UniTask ShowMissionStatusPage(ItemSetData setData)
        {
            var page = Global.UIManager.OpenPage<InGameMissionCompletePage>();
            
            // 현재 진행도 계산
            int currentFound = completedGroups.ContainsKey(setData.SetName) ? completedGroups[setData.SetName].Count : 0;
            int totalNeeded = setData.RequiredGroups.Count;
            bool isSetComplete = currentFound >= totalNeeded;

            page.Initialize(
                missionName: setData.SetName,
                missionNameDivider: string.Format("<alpha=#00>{0}", setData.SetName),
                missionSetIcon: null, 
                missionSetFoundLeft: $"({currentFound} / {totalNeeded})",
                isSetFoundComplete: isSetComplete,
                isGroupComplete: false,
                missionStatus: $"Mission! ({FoundSetsCount}/{TotalSetsCount})",
                missionNameAlpha: 1f
            );
            
            await page.WaitForClose();
        }

        private void CheckAllSetsFound()
        {
            // itemSetDataList null 체크
            if (itemSetDataList == null) return;
            
            if (foundSets.Count == itemSetDataList.Count)
            {
                OnAllSetsFound?.Invoke(null);
                allSetsFoundAndWaitingForClicks = true;
                
                // LevelManager의 종료 이벤트에 등록 - 모든 SetCompletionObject가 클릭될 때까지 대기
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.OnEndEvent.Add(async () => 
                    {
                        await WaitForAllCompletionObjectsClicked();
                    });
                }
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

        protected virtual async UniTask ShowTaskAlertPage(ItemSetData setData, HiddenObj obj)
        {
            var page = Global.UIManager.OpenPage<InGameMissionCompletePage>();
            
            string groupName = GetGroupName(obj);
            // LevelManager에서 그룹 정보를 가져옴
            var groupList = LevelManager.Instance.TargetObjDic.Values
                .FirstOrDefault(g => g.BaseGroupName == groupName);
            
            int foundInGroup = groupList != null ? groupList.FoundCount : 0;
            int totalInGroup = groupList != null ? groupList.TotalCount : 0;
            bool isGroupComplete = foundInGroup == totalInGroup;
            
            float missionNameAlpha = isGroupComplete && !completedGroups[setData.SetName].Contains(groupName) ? 0.6f : 1f;
            
            page.Initialize(
                missionName: setData.SetName,
                missionNameDivider: string.Format("<alpha=#00>{0}", setData.SetName),
                missionSetIcon: obj.UISprite,
                missionSetFoundLeft: $"({foundInGroup} / {totalInGroup})",
                isSetFoundComplete: completedGroups[setData.SetName].Count == setData.RequiredGroups.Count,
                isGroupComplete: isGroupComplete,
                missionStatus: $"Mission! ({FoundSetsCount}/{TotalSetsCount})",
                missionNameAlpha: missionNameAlpha
            );
            
            // 그룹이 완전히 찾아졌고 아직 세트에 추가되지 않았다면
            if (isGroupComplete && !completedGroups[setData.SetName].Contains(groupName))
            {
                completedGroups[setData.SetName].Add(groupName);
                CheckSetCompletion(setData);
            }
            
            await page.WaitForClose();
            // Debug.Log("MissionCompletePage closed");
        }


        public List<ItemSetData> GetItemSetDataList()
        {
            return itemSetDataList;
        }

        // SetCompletionObject 클릭 처리
        public void OnCompletionObjectClicked(string setName)
        {
            if (!clickedCompletionObjects.Contains(setName))
            {
                clickedCompletionObjects.Add(setName);
            }
        }

        // 모든 SetCompletionObject가 클릭되었는지 확인
        public bool AreAllCompletionObjectsClicked()
        {
            // CompletionObject가 있는 세트들만 확인 (Unity Object null 체크 포함)
            var setsWithCompletionObject = itemSetDataList.Where(x => 
                x.CompletionObject != null && 
                !ReferenceEquals(x.CompletionObject, null) && 
                x.CompletionObject).ToList();
            
            // CompletionObject가 있는 모든 세트가 클릭되었는지 확인
            return setsWithCompletionObject.All(setData => clickedCompletionObjects.Contains(setData.SetName));
        }

        // 모든 SetCompletionObject가 클릭될 때까지 대기
        private async UniTask WaitForAllCompletionObjectsClicked()
        {
            // CompletionObject가 없는 경우 즉시 완료 (Unity Object null 체크 포함)
            var setsWithCompletionObject = itemSetDataList.Where(x => 
                x.CompletionObject != null && 
                !ReferenceEquals(x.CompletionObject, null) && 
                x.CompletionObject).ToList();
            
            if (setsWithCompletionObject.Count == 0)
            {
                return;
            }

            // 모든 SetCompletionObject가 클릭될 때까지 대기
            await UniTask.WaitUntil(() => AreAllCompletionObjectsClicked());
        }
    }
}
