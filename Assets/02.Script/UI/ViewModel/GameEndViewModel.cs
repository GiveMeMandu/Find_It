using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Manager;
using SO;
using UnityEngine;
using UnityWeld;
using UnityWeld.Binding;

namespace UI
{
    /// <summary>
    /// 결과 아이템 데이터를 담는 구조체입니다.
    /// 스티커, 코인 등 다양한 결과를 범용적으로 표현합니다.
    /// </summary>
    public struct ResultItemData
    {
        public Sprite Icon;
        public string DisplayName;
        public int Count;

        public ResultItemData(Sprite icon, string displayName, int count = 1)
        {
            Icon = icon;
            DisplayName = displayName;
            Count = count;
        }
    }

    [Binding]
    public class GameEndViewModel : GroupView
    {
        /// <summary>
        /// 이번 스테이지에서 획득한 결과 아이템들을 표시합니다.
        /// GroupView의 PrepareViewModels를 사용하여 아이템 수만큼 UI를 생성합니다.
        /// </summary>
        public void SetResultItems(List<ResultItemData> resultItems)
        {
            if (resultItems == null || resultItems.Count == 0)
            {
                PrepareViewModels(0);
                return;
            }

            PrepareViewModels(resultItems.Count);
            var viewModels = GetViewModels();
            for (int i = 0; i < viewModels.Count; i++)
            {
                var elementVM = viewModels[i] as ResultElementViewModel;
                if (elementVM != null)
                {
                    var item = resultItems[i];
                    elementVM.Init(item.Icon, item.DisplayName, item.Count);
                }
            }

            Debug.Log($"[GameEndViewModel] 결과 아이템 {resultItems.Count}개 표시 완료");
        }

        [Binding]
        public void OnClickHomeBtn()
        {
            LoadingSceneManager.LoadScene(SceneNum.START);
        }
        [Binding]
        public void OnClickRetryBtn()
        {
            if (Global.CurrentScene != null)
            {
                LoadingSceneManager.LoadScene((int)Global.CurrentScene.SceneName);
            }
            else
            {
                // CurrentScene이 null인 경우 현재 씬 재로드
                LoadingSceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}