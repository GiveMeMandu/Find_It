using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using DeskCat.FindIt.Scripts.Core.Main.System;
using UnityEngine.Events;
using NaughtyAttributes;

public class SetCompletionObject : MonoBehaviour, IPointerClickHandler
{
    [InfoBox("이 컴포넌트는 세트의 완성 상태를 관리합니다. 세트가 완성되면 `OnSetComplete` 이벤트가 호출됩니다.")]
    [Label("시작 시 숨기기")]
    public bool IsHideOnStart = true;

    [Label("세트 이름")]
    [InfoBox("완성 체크를 위해 사용하는 세트의 고유 이름을 입력하세요.")]
    public string SetName;

    [Label("완성 뷰")]
    [InfoBox("세트 완성 시 표시할 GameObject를 연결하세요. 예: 세트 완성 아트 또는 이펙트.")]
    public GameObject CompletionView;  // 세트 완성 모습을 보여줄 GameObject
    public bool IsFound { get; private set; }

    [Label("발견 시 사운드 재생")]
    public bool PlaySoundWhenFound = true;

    [Label("클릭 시 재생할 오디오")]
    public AudioClip AudioWhenClick;

    [Label("세트 완성 시 이벤트")]
    [InfoBox("세트가 완성되었을 때 실행할 이벤트를 연결하세요. 예: UI 알림, 애니메이션 재생 등.")]
    public UnityEvent OnSetComplete;
    

    private void Awake()
    {
        if (CompletionView != null)
        {
            CompletionView.SetActive(false);
        }
        if (IsHideOnStart) gameObject.SetActive(false);  // 처음에는 숨김
    }

    public void OnPointerClick(PointerEventData eventData)
    {
    //     if (!IsFound)
    //     {
    //         IsFound = true;
    //         if (AudioWhenClick != null && PlaySoundWhenFound)
    //         {
    //             LevelManager.PlayItemFx(AudioWhenClick);
    //         }
    //         CompletionView.SetActive(true);
            
    //         // ItemSetManager에 클릭되었음을 알림
    //         if (ItemSetManager.Instance != null)
    //         {
    //             ItemSetManager.Instance.OnCompletionObjectClicked(SetName);
    //         }
    //     }
    }

    public void ShowSetCompletionNotification()
    {
        if (!IsFound)
        {
            IsFound = true;
            if (AudioWhenClick != null && PlaySoundWhenFound)
            {
                LevelManager.PlayItemFx(AudioWhenClick);
            }
            // CompletionView.SetActive(true);
            
            // ItemSetManager에 클릭되었음을 알림
            if (ItemSetManager.Instance != null)
            {
                ItemSetManager.Instance.OnCompletionObjectClicked(SetName);
            }
        }
        // gameObject.SetActive(true);  // 세트가 완성되면 알림 표시
        OnSetComplete?.Invoke();
        
        // Debug.Log($"SetCompletionObject {SetName} is now active and ready to be clicked!");
    }
}
