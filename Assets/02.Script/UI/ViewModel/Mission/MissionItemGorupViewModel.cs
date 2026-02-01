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
    public class MissionItemGorupViewModel : GroupView
    {
        public void Initialize(ItemSetData itemSetData) {
            PrepareViewModels(itemSetData.RequiredGroups.Count);

            for (int i = 0; i < GetViewModels().Count; i++)
            {
                var missionItemViewModel = GetViewModels()[i] as MissionItemViewMdoel;
                missionItemViewModel.Initialize(itemSetData.RequiredGroups[i]);
                missionItemViewModel.gameObject.SetActive(true);
            }
        }
    }
}