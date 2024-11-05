using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using InGame;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

public class Stage2Manager : LevelManagerCount, IStageManager
{
    [BoxGroup("낮밤 바뀌는 연출")] [LabelText("밤 전체 옵젝 부모")]
    [SerializeField] private Transform NightGroup;
    [BoxGroup("낮밤 바뀌는 연출")] [LabelText("밤 페이드 없는 옵젝 부모")]
    [SerializeField] private Transform NightNoFadeGroup;
    [BoxGroup("낮밤 바뀌는 연출")] [LabelText("아침 전체 옵젝 부모")]
    [SerializeField] private Transform DayGroup;
    [BoxGroup("스테이지 클리어 연출")]
    [SerializeField] private PlayableDirector _playableDirector;
    [BoxGroup("스테이지 클리어 연출2")]
    [SerializeField] private PlayableDirector _playableDirector2;
    [BoxGroup("낮밤 바뀌는 연출")] [LabelText("아침 토끼들")]
    [SerializeField] private Transform _dayRabbits;
    [BoxGroup("낮밤 바뀌는 연출")] [LabelText("밤 토끼들")]
    [SerializeField] private Transform _nightRabbits;
    public EventHandler OnStartStage;
    private List<NightObj> nightObjs = new List<NightObj>();
    private List<SpriteRenderer> nightObjsNoFade = new List<SpriteRenderer>();

    private EndingSequenceStage2 _endingSequenceStage2;
    protected override void Start()
    {
        base.Start();
        _endingSequenceStage2 = GetComponent<EndingSequenceStage2>();
        nightObjs = FindObjectsOfType<NightObj>().ToList();
        foreach (var n in nightObjs)
        {
            if (n.isDisableOnStart) n.gameObject.SetActive(false);
        }
        levelManager.OnEndEvnt.Add(ClearStageTask);
        nightObjsNoFade = NightNoFadeGroup.GetComponentsInChildren<SpriteRenderer>().ToList();
        foreach(var obj in nightObjsNoFade) {
            obj.gameObject.SetActive(false);
        }
    }


    protected override void OnFoundObj(object sender, int e)
    {
        base.OnFoundObj(sender, e);
    }
    public void StartStage()
    {
        var childs = NightGroup.GetComponentsInChildren<SpriteRenderer>();
        var dChilds = DayGroup.GetComponentsInChildren<SpriteRenderer>();
        foreach(var obj in nightObjsNoFade) {
            obj.gameObject.SetActive(true);
        }
        foreach (var c in childs)
        {
            c.DOFade(1, 0f).SetEase(Ease.Linear);
        }
        foreach (var d in dChilds)
        {
            d.DOFade(0, 3f);
        }

        foreach (var n in nightObjs)
        {
            n.gameObject.SetActive(true);
            n.OnNight();
        }
        _dayRabbits.gameObject.SetActive(false);
        _nightRabbits.gameObject.SetActive(true);
        OnStartStage?.Invoke(this, EventArgs.Empty);
    }

    public async UniTask ClearStageTask()
    {
        await _endingSequenceStage2.StartSequence();
        _playableDirector.initialTime = 0;
        _playableDirector.enabled = true;
        _playableDirector.Play();
        await UniTask.WaitUntil(() => _playableDirector.state != PlayState.Playing);

        _playableDirector2.initialTime = 0;
        _playableDirector2.enabled = true;
        _playableDirector2.Play();
        await UniTask.WaitUntil(() => _playableDirector2.state != PlayState.Playing);
    }

    public void ClearStage()
    {
        
    }

}

