using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using InGame;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

public class Stage2Manager : LevelManagerCount, IStageManager
{
    [BoxGroup("낮밤 바뀌는 연출")]
    [LabelText("밤 전체 옵젝 부모")]
    [SerializeField] private Transform NightGroup;
    [BoxGroup("스테이지 클리어 연출")]
    [SerializeField] private PlayableDirector _playableDirector;
    public EventHandler OnStartStage;
    private List<NightObj> nightObjs = new List<NightObj>();


    private void Start()
    {
        nightObjs = FindObjectsOfType<NightObj>().ToList();
        foreach (var n in nightObjs)
        {
            if (n.isDisableOnStart) n.gameObject.SetActive(false);
        }
    }


    protected override void OnFoundObj(object sender, int e)
    {
        base.OnFoundObj(sender, e);
        if(curFoundCount == curFoundCountMax)
        {
            ClearStage();
        }
    }
    public void StartStage()
    {
        var childs = NightGroup.GetComponentsInChildren<SpriteRenderer>();
        foreach (var c in childs)
        {
            c.DOFade(0, 0);
            c.DOFade(1, 6.5f).SetEase(Ease.OutQuart);
        }

        foreach (var n in nightObjs)
        {
            n.gameObject.SetActive(true);
            n.OnNight();
        }
        OnStartStage?.Invoke(this, EventArgs.Empty);
    }

    public void ClearStage()
    {
        _playableDirector.initialTime = 0;
        _playableDirector.enabled = true;
        _playableDirector.Play();
    }
}

