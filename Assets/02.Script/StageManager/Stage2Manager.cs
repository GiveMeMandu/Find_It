using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

public class Stage2Manager : MonoBehaviour, IStageManager
{
    [BoxGroup("낮밤 바뀌는 연출")] [LabelText("밤 전체 옵젝 부모")]
    [SerializeField] private Transform NightGroup;
    [BoxGroup("낮밤 바뀌는 연출")]
    [SerializeField] private PlayableDirector _playableDirector;
    public EventHandler OnStartStage;

    private void Start() {
    }

    public void StartStage() {
        _playableDirector.initialTime = 0;
        _playableDirector.enabled = true;
        var childs = NightGroup.GetComponentsInChildren<SpriteRenderer>();
        foreach(var c in childs) {
            c.DOFade(0,0);
            c.DOFade(1,6.5f).SetEase(Ease.OutQuart);
        }
        OnStartStage?.Invoke(this, EventArgs.Empty);
    }

    public void ClearStage() {
    }
}
    
