using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DeskCat.FindIt.Scripts.Core.Main.System;
using DG.Tweening;
using InGame;
using Manager;
using OutGame;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

public class Stage2Manager : InGameSceneBase
{
    [BoxGroup("낮밤 바뀌는 연출")] [LabelText("낮 바뀌는 시간 수정")] public float DayToNightTime = 1.5f; 
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
    
    // ChangeDayObject 관리
    private List<ChangeDayObject> changeDayObjects = new List<ChangeDayObject>();
    private int foundChangeDayObjectCount = 0;

    [SerializeField] private EndingSequenceStage2 _endingSequenceStage2;
    protected override void Start()
    {
        NightObj.IsGlobalNight = false;
        base.Start();
        if(NightGroup != null)
        {
            NightGroup.gameObject.SetActive(false);
        }
        nightObjs = FindObjectsByType<NightObj>(findObjectsInactive:FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
        foreach (var n in nightObjs)
        {
            // Debug.Log("<color=green>" + n.gameObject.name + "</color>" + n.isDisableOnStart);
            if (n.isDisableOnStart) n.gameObject.SetActive(false);
        }
        if(NightNoFadeGroup != null)
            nightObjsNoFade = NightNoFadeGroup.GetComponentsInChildren<SpriteRenderer>().ToList();
        if(nightObjsNoFade.Count > 0)
        {
            foreach (var obj in nightObjsNoFade)
            {
                obj.gameObject.SetActive(false);
            }
        }
        
        // 모든 ChangeDayObject 캐싱
        changeDayObjects = FindObjectsByType<ChangeDayObject>(FindObjectsSortMode.None).ToList();
        foundChangeDayObjectCount = 0;
        
        // 각 ChangeDayObject에 이벤트 리스너 등록
        foreach (var changeDayObj in changeDayObjects)
        {
            changeDayObj.OnFound += OnChangeDayObjectFound;
        }
        
        Debug.Log($"[Stage2Manager] ChangeDayObject 총 {changeDayObjects.Count}개 캐싱됨");
        
        StartStageBase();
    }
    [Button("밤 스테이지 시작")]
    public void StartNightStage()
    {
        NightGroup.gameObject.SetActive(true);
        var childs = NightGroup.GetComponentsInChildren<SpriteRenderer>();
        var dChilds = DayGroup.GetComponentsInChildren<SpriteRenderer>();
        foreach(var obj in nightObjsNoFade) {
            obj.gameObject.SetActive(true);
        }
        foreach (var c in childs)
        {
            // Debug.Log("<color=green>" + c.name + "</color>");
            c.DOFade(1, 0f).SetEase(Ease.Linear);
        }
        foreach (var d in dChilds)
        {
            if(d != null) 
            {
                if(d.TryGetComponent<NightObj>(out var nightObj))
                {
                    if(nightObj.isNoFade) continue;
                }
            }
            d.DOFade(0, DayToNightTime);
        }

        foreach (var n in nightObjs)
        {
            if(n.isDisableOnNight) 
            {
                n.gameObject.SetActive(false);
                continue;
            }
            if(n.isActiveOnNight) n.gameObject.SetActive(true);
            // Debug.Log("<color=red>" + n.gameObject.name + "</color>");
            n.OnNight();
        }
        _dayRabbits.gameObject.SetActive(false);
        _nightRabbits.gameObject.SetActive(true);
        OnStartStage?.Invoke(this, EventArgs.Empty);
    }

    protected override async UniTask ClearStageTask()
    {
        if (_endingSequenceStage2 != null)
        {
            await _endingSequenceStage2.StartSequence();
        }
        
        if (_playableDirector != null)
        {
            _playableDirector.initialTime = 0;
            _playableDirector.enabled = true;
            _playableDirector.Play();
            await UniTask.WaitUntil(() => _playableDirector.state != PlayState.Playing);
        }

        if (_playableDirector2 != null)
        {
            _playableDirector2.initialTime = 0;
            _playableDirector2.enabled = true;
            _playableDirector2.Play();
            await UniTask.WaitUntil(() => _playableDirector2.state != PlayState.Playing);
        }
    }

    public void ClearStage()
    {
        
    }
    
    private void OnChangeDayObjectFound(object sender, EventArgs e)
    {
        foundChangeDayObjectCount++;
        Debug.Log($"[Stage2Manager] ChangeDayObject 찾음: {foundChangeDayObjectCount}/{changeDayObjects.Count}");
        
        // 모든 ChangeDayObject를 찾았는지 확인
        if (foundChangeDayObjectCount >= changeDayObjects.Count && changeDayObjects.Count > 0)
        {
            Debug.Log("[Stage2Manager] 모든 ChangeDayObject를 찾았습니다! 날짜 변경 효과 시작");
            StartNightStage();
        }
    }

}

