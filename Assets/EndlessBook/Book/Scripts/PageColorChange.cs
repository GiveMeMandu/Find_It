using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class PageColorChange : MonoBehaviour
{
    [System.Serializable]
    public struct PageColorMapping
    {
        public int pageNumber;
        public Color color;
    }

    public SpriteRenderer targetRenderer;

    [Header("Color Mappings")]
    [Tooltip("특정 페이지 번호에 매칭될 색상을 설정합니다. (0: 앞표지, 999: 뒷표지)")]
    public List<PageColorMapping> colorMappings = new List<PageColorMapping>();

    public Color defaultColor = Color.white;

    private Tween _colorTween;

    /// <summary>
    /// 페이지 번호에 맞춰 SpriteRenderer의 색상을 변경합니다.
    /// </summary>
    /// <param name="pageNumber">현재 페이지 번호</param>
    public void SetColorByPage(int pageNumber)
    {
        if (targetRenderer == null) return;

        _colorTween?.Kill();
        targetRenderer.color = GetColor(pageNumber);
    }

    /// <summary>
    /// 페이지 번호에 맞춰 SpriteRenderer의 색상을 부드럽게 변경합니다.
    /// </summary>
    public void BlendColorByPage(int pageNumber, float duration)
    {
        if (targetRenderer == null) return;

        Color targetColor = GetColor(pageNumber);

        _colorTween?.Kill();
        if (duration <= 0)
        {
            targetRenderer.color = targetColor;
        }
        else
        {
            _colorTween = targetRenderer.DOColor(targetColor, duration).SetEase(Ease.Linear);
        }
    }

    private Color GetColor(int pageNumber)
    {
        // 리스트에서 해당 페이지 번호에 맞는 설정을 찾습니다.
        var mapping = colorMappings.FirstOrDefault(m => m.pageNumber == pageNumber);

        if (mapping.pageNumber == pageNumber || (mapping.pageNumber == 0 && pageNumber == 0))
        {
            return mapping.color;
        }

        return defaultColor;
    }
}
