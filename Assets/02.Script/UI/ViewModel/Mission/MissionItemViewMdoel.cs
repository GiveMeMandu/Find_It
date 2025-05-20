using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityWeld.Binding;
using UnityWeld;
using Unity.VisualScripting;
using Manager;
using DeskCat.FindIt.Scripts.Core.Main.System;
using System.Linq;
using System.Collections.Generic;

namespace UI
{
    [Binding]
    public class MissionItemViewMdoel : ViewModel
    {
        private string _missionItemCount;

        [Binding]
        public string MissionItemCount
        {
            get => _missionItemCount;
            set
            {
                _missionItemCount = value;
                OnPropertyChanged(nameof(MissionItemCount));
            }
        }

        private Sprite _missionItemIcon;

        [Binding]
        public Sprite MissionItemIcon
        {
            get => _missionItemIcon;
            set
            {
                _missionItemIcon = value;
                OnPropertyChanged(nameof(MissionItemIcon));
            }
        }
        private string itemName;
        public void Initialize(string itemName)
        {
            this.itemName = itemName;
            // LevelManager에서 그룹 상태를 확인
            var (exists, isComplete, groupName) = LevelManager.Instance.GetGroupStatus(itemName);

            if (exists)
            {
                UpdateCount();
            }
            else
            {
                // 일치하는 그룹이 없을 경우 기본값 설정
                MissionItemCount = "0 / 0";
            }
        }
        private void OnDestroy()
        {
            LevelManager.Instance.OnFoundObj -= OnFoundObj;
        }
        private void OnEnable()
        {
            LevelManager.Instance.OnFoundObj += OnFoundObj;
        }

        private void OnFoundObj(object sender, HiddenObj e)
        {
            UpdateCount();
        }

        private void UpdateCount()
        {
            if(LevelManager.Instance.GetBaseGroupName(itemName) != itemName)
            {
                return;
            }
            // 씬에서 해당 그룹 이름과 일치하는 HiddenObj 찾기
            var hiddenObjs = LevelManager.Instance.GetHiddenObjsByGroupName(itemName);
            int totalCount = hiddenObjs.Count;
            int foundCount = hiddenObjs.Count(obj => obj.IsFound);

            // "찾은 수 / 전체 수" 형식으로 설정
            MissionItemCount = string.Format("{0} / {1}", foundCount, totalCount);

            // 대표 HiddenObj를 사용하여 아이콘 설정 (첫 번째 아이템 사용)
            if (hiddenObjs.Count > 0)
            {
                MissionItemIcon = hiddenObjs[0].UISprite;
            }
        }
    }
}