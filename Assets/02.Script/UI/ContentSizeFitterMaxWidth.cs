using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(ContentSizeFitter))]
public class ContentSizeFitterMaxWidth : MonoBehaviour
{
    public float maxWidth;

    private RectTransform _rtfm;
    private ContentSizeFitter _fitter;
    private ILayoutElement _layout;

    private void OnEnable()
    {
        if (_rtfm == null) _rtfm = (RectTransform)transform;
        if (_fitter == null) _fitter = GetComponent<ContentSizeFitter>();
        if (_layout == null) _layout = GetComponent<ILayoutElement>();
    }

    private void Update()
    {
        _fitter.horizontalFit = _layout.preferredWidth > maxWidth
            ? ContentSizeFitter.FitMode.Unconstrained
            : ContentSizeFitter.FitMode.PreferredSize;

        if (_layout.preferredWidth > maxWidth)
        {
            _fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            _rtfm.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);
        }
        else
            _fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private void OnValidate() => OnEnable();
}