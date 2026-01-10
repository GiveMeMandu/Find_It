using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityWeld.Binding;
using UnityWeld;
using Unity.VisualScripting;
using Manager;
using DeskCat.FindIt.Scripts.Core.Main.System;

namespace UI
{
    [Binding]
    public class MissionGroupViewModel : GroupView
    {
        private void Start() {
            var itemSetDataList = ItemSetManager.Instance.GetItemSetDataList();
            if (itemSetDataList == null)
            {
                return;
            }
            if(itemSetDataList.Count == 0)
            {
                return;
            }
            PrepareViewModels(itemSetDataList.Count);

            for (int i = 0; i < GetViewModels().Count; i++)
            {
                var missionElementViewModel = GetViewModels()[i] as MissionElementViewModel;
                missionElementViewModel.Initialize(itemSetDataList[i]);
            }
        }
    }
}