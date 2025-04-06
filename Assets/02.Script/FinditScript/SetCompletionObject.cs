using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using DeskCat.FindIt.Scripts.Core.Main.System;

public class SetCompletionObject : MonoBehaviour, IPointerClickHandler
{
    public string SetName;
    public GameObject CompletionView;  // 세트 완성 모습을 보여줄 GameObject
    public GameObject AlertView; // 세트 완성 알림을 보여줄 GameObject
    public bool IsFound { get; private set; }
    public bool PlaySoundWhenFound = true;
    public AudioClip AudioWhenClick;
    
    public event Action OnSetFound;

    private void Awake()
    {
        if (CompletionView != null)
        {
            CompletionView.SetActive(false);
            AlertView.SetActive(false);
        }
        gameObject.SetActive(false);  // 처음에는 숨김
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsFound)
        {
            IsFound = true;
            if (AudioWhenClick != null && PlaySoundWhenFound)
            {
                LevelManager.PlayItemFx(AudioWhenClick);
            }
            OnSetFound?.Invoke();
            CompletionView.SetActive(true);
            AlertView.SetActive(false);
        }
    }

    public void ShowSetCompletionNotification()
    {
        gameObject.SetActive(true);  // 세트가 완성되면 알림 표시
        if (CompletionView != null)
        {
            AlertView.SetActive(true);
        }
    }
}
