using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controls groups of SmartLighting2D lights (Light2D) by alpha and enabled state.
public class LightController : MonoBehaviour
{
    [System.Serializable]
    public class LightGroup
    {
        public string name;
        // Use fully-qualified type found in project
        public FunkyCode.Light2D[] lights;
    }

    [Tooltip("Configure groups of Light2D for batch control in inspector.")]
    public List<LightGroup> groups = new List<LightGroup>();

    // Set alpha (0..1) for all lights in a group (immediate)
    public void SetGroupAlpha(string groupName, float alpha)
    {
        var group = groups.Find(g => g.name == groupName);
        if (group == null) return;

        foreach (var l in group.lights)
        {
            if (l == null) continue;
            var c = l.color;
            c.a = Mathf.Clamp01(alpha);
            l.color = c;
        }
    }

    // Enable or disable all lights in a group
    public void SetGroupActive(string groupName, bool active)
    {
        var group = groups.Find(g => g.name == groupName);
        if (group == null) return;

        foreach (var l in group.lights)
        {
            if (l == null) continue;
            l.enabled = active;
        }
    }

    // Fade group alpha over time (seconds)
    public void FadeGroup(string groupName, float targetAlpha, float duration)
    {
        var group = groups.Find(g => g.name == groupName);
        if (group == null) return;

        StopCoroutine(DoFadeGroup(group));
        StartCoroutine(DoFadeGroup(group, targetAlpha, duration));
    }

    IEnumerator DoFadeGroup(LightGroup group, float targetAlpha = 0f, float duration = 1f)
    {
        float t = 0f;
        // capture initial alphas
        var initial = new float[group.lights.Length];
        for (int i = 0; i < group.lights.Length; i++)
        {
            var l = group.lights[i];
            initial[i] = (l == null) ? 0f : l.color.a;
        }

        while (t < duration)
        {
            t += Time.deltaTime;
            float frac = duration > 0f ? t / duration : 1f;
            float cur = Mathf.Lerp(0f, targetAlpha, frac);

            for (int i = 0; i < group.lights.Length; i++)
            {
                var l = group.lights[i];
                if (l == null) continue;
                var col = l.color;
                col.a = Mathf.Lerp(initial[i], targetAlpha, frac);
                l.color = col;
            }

            yield return null;
        }

        // ensure final
        for (int i = 0; i < group.lights.Length; i++)
        {
            var l = group.lights[i];
            if (l == null) continue;
            var col = l.color;
            col.a = Mathf.Clamp01(targetAlpha);
            l.color = col;
        }
    }
}
