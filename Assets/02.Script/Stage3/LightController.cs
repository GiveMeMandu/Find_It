using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

// Controls groups of SmartLighting2D lights (Light2D) by alpha and enabled state.
// Uses UniTask for async fades and AutoTaskControl for lifecycle cancellation.
public class LightController : AutoTaskControl
{
    [System.Serializable]
    public class LightGroup
    {
        public string name;
        // 그룹 자체의 GameObject (그룹 컨테이너)
        public GameObject groupObject;
        // Use fully-qualified type found in project
        public FunkyCode.Light2D[] lights;
        // LightSprite2D 라이트들
        public FunkyCode.LightSprite2D[] lightSprites;
        // 관련 GameObject들 (라이트와 함께 활성화/비활성화)
        public GameObject[] relatedObjects;
    }

    [Tooltip("Configure groups of Light2D for batch control in inspector.")]
    public List<LightGroup> groups = new List<LightGroup>();

    [Tooltip("라이트가 켜질 때 비활성화할 그룹들 (예: 다른 라이트 그룹들)")]
    public List<string> groupsToDisable = new List<string>();

    [Space(10)]
    [Tooltip("기본 라이트 페이드 시간 (초)")]
    [Range(0.1f, 10f)]
    public float defaultFadeDuration = 2.0f;

    // per-group cancellation sources so we can cancel/override fades
    private readonly Dictionary<string, CancellationTokenSource> groupCancellations = new Dictionary<string, CancellationTokenSource>();

    // Set alpha (0..1) for all lights in a group (immediate)
    public void SetGroupAlpha(string groupName, float alpha)
    {
        var group = groups.Find(g => g.name == groupName);
        if (group == null) return;

        // Light2D 라이트들 알파 설정
        foreach (var l in group.lights)
        {
            if (l == null) continue;
            var c = l.color;
            c.a = Mathf.Clamp01(alpha);
            l.color = c;
        }

        // LightSprite2D 라이트들 알파 설정
        foreach (var ls in group.lightSprites)
        {
            if (ls == null) continue;
            var c = ls.color;
            c.a = Mathf.Clamp01(alpha);
            ls.color = c;
        }
    }

    // Enable or disable all lights in a group
    public void SetGroupActive(string groupName, bool active)
    {
        var group = groups.Find(g => g.name == groupName);
        if (group == null) return;

        // Light2D 라이트 활성화/비활성화
        foreach (var l in group.lights)
        {
            if (l == null) continue;
            l.enabled = active;
        }

        // LightSprite2D 라이트 활성화/비활성화
        foreach (var ls in group.lightSprites)
        {
            if (ls == null) continue;
            ls.enabled = active;
        }

        // 관련 객체들 활성화/비활성화
        foreach (var obj in group.relatedObjects)
        {
            if (obj == null) continue;
            obj.SetActive(active);
        }

        // 활성화할 때 다른 그룹들 비활성화
        if (active)
        {
            foreach (var groupToDisable in groupsToDisable)
            {
                if (groupToDisable != groupName) // 자기 자신은 제외
                {
                    SetGroupActive(groupToDisable, false);
                }
            }
        }
    }

    // Fade group alpha over time (seconds) - wrapper that cancels previous fades
    public void FadeGroup(string groupName, float targetAlpha, float duration = -1f)
    {
        var group = groups.Find(g => g.name == groupName);
        if (group == null) return;

        // duration이 -1이면 기본 페이드 시간 사용
        if (duration < 0f)
            duration = defaultFadeDuration;

        // 페이드 인할 때 (targetAlpha > 0) 관련 객체들 활성화
        if (targetAlpha > 0)
        {
            foreach (var obj in group.relatedObjects)
            {
                if (obj == null) continue;
                obj.SetActive(true);
            }

            // 다른 그룹들 비활성화
            foreach (var groupToDisable in groupsToDisable)
            {
                if (groupToDisable != groupName)
                {
                    SetGroupActive(groupToDisable, false);
                }
            }
        }

        // cancel previous fade for this group if any
        CancelGroupFade(groupName);

        // create linked token so cancellation happens on destroy as well
        var linked = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellation.Token);
        groupCancellations[groupName] = linked;

        // fire-and-forget UniTask (cancellation handled)
        FadeGroupAsync(group, targetAlpha, duration, linked.Token).Forget();
    }

    // Cancel running fade for a group
    public void CancelGroupFade(string groupName)
    {
        if (groupCancellations.TryGetValue(groupName, out var src))
        {
            if (!src.IsCancellationRequested)
                src.Cancel();
            src.Dispose();
            groupCancellations.Remove(groupName);
        }
    }

    // Async fade using UniTask and CancellationToken
    public async UniTaskVoid FadeGroupAsync(LightGroup group, float targetAlpha, float duration, CancellationToken ct)
    {
        // capture initial alphas for Light2D
        float[] initialLights = new float[group.lights.Length];
        for (int i = 0; i < group.lights.Length; i++)
        {
            var l = group.lights[i];
            initialLights[i] = (l == null) ? 0f : l.color.a;
        }

        // capture initial alphas for LightSprite2D
        float[] initialSprites = new float[group.lightSprites.Length];
        for (int i = 0; i < group.lightSprites.Length; i++)
        {
            var ls = group.lightSprites[i];
            initialSprites[i] = (ls == null) ? 0f : ls.color.a;
        }

        float t = 0f;
        try
        {
            while (t < duration)
            {
                ct.ThrowIfCancellationRequested();
                await UniTask.Yield(ct);
                t += Time.deltaTime;
                float frac = duration > 0f ? t / duration : 1f;

                // Fade Light2D lights
                for (int i = 0; i < group.lights.Length; i++)
                {
                    var l = group.lights[i];
                    if (l == null) continue;
                    var col = l.color;
                    col.a = Mathf.Lerp(initialLights[i], targetAlpha, frac);
                    l.color = col;
                }

                // Fade LightSprite2D lights
                for (int i = 0; i < group.lightSprites.Length; i++)
                {
                    var ls = group.lightSprites[i];
                    if (ls == null) continue;
                    var col = ls.color;
                    col.a = Mathf.Lerp(initialSprites[i], targetAlpha, frac);
                    ls.color = col;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // cancelled - exit gracefully
            return;
        }

        // ensure final values for Light2D
        for (int i = 0; i < group.lights.Length; i++)
        {
            var l = group.lights[i];
            if (l == null) continue;
            var col = l.color;
            col.a = Mathf.Clamp01(targetAlpha);
            l.color = col;
        }

        // ensure final values for LightSprite2D
        for (int i = 0; i < group.lightSprites.Length; i++)
        {
            var ls = group.lightSprites[i];
            if (ls == null) continue;
            var col = ls.color;
            col.a = Mathf.Clamp01(targetAlpha);
            ls.color = col;
        }

        // dispose cancellation source if it belongs to this group
        foreach (var kv in new List<KeyValuePair<string, CancellationTokenSource>>(groupCancellations))
        {
            if (kv.Value.Token == ct)
            {
                kv.Value.Dispose();
                groupCancellations.Remove(kv.Key);
                break;
            }
        }
    }

    // UnityEvent용 편의 메서드들 (단일 인자)
    
    // 그룹 활성화 (UnityEvent에서 사용)
    public void ActivateGroup(string groupName)
    {
        SetGroupActive(groupName, true);
    }
    
    // 그룹 비활성화 (UnityEvent에서 사용)
    public void DeactivateGroup(string groupName)
    {
        SetGroupActive(groupName, false);
    }
    
    // 특정 그룹 활성화 + 알파 1로 페이드 + 나머지 그룹들 즉시 비활성화
    public void ActivateGroupWithAlpha(string groupName)
    {
        // 모든 그룹들을 먼저 비활성화
        foreach (var group in groups)
        {
            if (group.name != groupName)
            {
                // 다른 그룹의 그룹 객체 비활성화
                if (group.groupObject != null)
                    group.groupObject.SetActive(false);
                
                // 다른 그룹들의 Light2D GameObject들 비활성화
                foreach (var l in group.lights)
                {
                    if (l == null) continue;
                    l.gameObject.SetActive(false);
                }
                
                // 다른 그룹들의 LightSprite2D GameObject들 비활성화
                foreach (var ls in group.lightSprites)
                {
                    if (ls == null) continue;
                    ls.gameObject.SetActive(false);
                }
                
                // 다른 그룹들의 관련 객체들도 비활성화
                foreach (var obj in group.relatedObjects)
                {
                    if (obj == null) continue;
                    obj.SetActive(false);
                }
            }
        }
        
        // 대상 그룹 활성화 및 페이드 인
        var targetGroup = groups.Find(g => g.name == groupName);
        if (targetGroup != null)
        {
            // 그룹 객체 먼저 활성화
            if (targetGroup.groupObject != null)
                targetGroup.groupObject.SetActive(true);
            
            // 그룹의 Light2D GameObject들 활성화
            foreach (var l in targetGroup.lights)
            {
                if (l == null) continue;
                l.gameObject.SetActive(true);
                l.enabled = true;
            }
            
            // 그룹의 LightSprite2D GameObject들 활성화
            foreach (var ls in targetGroup.lightSprites)
            {
                if (ls == null) continue;
                ls.gameObject.SetActive(true);
                ls.enabled = true;
            }
            
            // 관련 객체들 활성화
            foreach (var obj in targetGroup.relatedObjects)
            {
                if (obj == null) continue;
                obj.SetActive(true);
            }
            
            // 알파값을 1로 페이드 (기본 페이드 시간 사용)
            FadeGroup(groupName, 1.0f);
        }
    }
 
    protected override void OnDisable()
    {
        base.OnDisable();

        // cancel all group fades
        foreach (var kv in groupCancellations)
        {
            if (!kv.Value.IsCancellationRequested)
                kv.Value.Cancel();
            kv.Value.Dispose();
        }
        groupCancellations.Clear();
    }
}
