using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

//  ScreenFx © NullTale - https://twitter.com/NullTale/
namespace ScreenFx
{
    public class ScreenOverlay : System.IDisposable
    {
        protected Image         m_Image;
        protected RectTransform m_Transform;
        protected Canvas        m_Canvas;

        public Color Color
        {
            get => m_Image.color;
            set
            {
                if (m_Image.color != value)
                    m_Image.color = value;
            }
        }

        public Sprite Sprite
        {
            get => m_Image.sprite;
            set
            {
                if (m_Image.sprite != value)
                    m_Image.sprite = value;
            }
        }

        public Vector2 Scale
        {
            get => m_Transform.localScale.To2DXY();
            set
            {
                if (m_Transform.localScale.To2DXY() != value)
                    m_Transform.localScale = value.To3DXY(1.0f);
            }
        }

        public Image  Image  => m_Image;
        public Canvas Canvas => m_Canvas;

        // =======================================================================
        public ScreenOverlay(int sortingOrder, RenderMode mode, int layer, float planeDist, string title)
        {
            var go = new GameObject($"Scr_{title}", typeof(Canvas));
            go.hideFlags = HideFlags.DontSave;
            go.layer = layer;

            // instantiate canvas & image
            m_Canvas               = go.GetComponent<Canvas>();
            m_Canvas.renderMode    = mode;
            m_Canvas.worldCamera   = UnityEngine.Camera.main;
            m_Canvas.sortingOrder  = sortingOrder;
            m_Canvas.planeDistance = planeDist;
            // 
            m_Canvas.transform.SetParent(ScreenFx.Instance.transform, false);

            var imgGO = new GameObject("Image", typeof(CanvasRenderer), typeof(Image));
            imgGO.transform.SetParent(m_Canvas.transform, false);
                
            m_Image               = imgGO.GetComponent<Image>();
            m_Image.raycastTarget = false;
            m_Image.maskable      = false;

            m_Transform               = m_Image.GetComponent<RectTransform>();
            m_Transform.localPosition = m_Transform.localPosition.WithZ(0f);
            m_Transform.anchorMax     = new Vector2(1.0f, 1.0f);
            m_Transform.anchorMin     = new Vector2(0.0f, 0.0f);

            m_Transform.offsetMin = new Vector2(0.0f, 0.0f);
            m_Transform.offsetMax = new Vector2(0.0f, 0.0f);

            go.transform.SetParent(ScreenFx.Instance.transform);
            go.layer = layer;
            go.SetActive(false);
        }

        public void Open()
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                if (m_Canvas == null)
                    return;
            }
#endif
            m_Canvas.gameObject.SetActive(true);
        }

        public void Close()
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                if (m_Canvas == null)
                    return;
            }
#endif
            m_Canvas.gameObject.SetActive(false);
        }

        public void Dispose()
        {
            if (m_Canvas != null && m_Canvas.gameObject != null)
            {
#if UNITY_EDITOR
                if (Application.isPlaying == false)
                    Object.DestroyImmediate(m_Canvas.gameObject);
                else
                    Object.Destroy(m_Canvas.gameObject);
#else
                    Object.Destroy(m_Canvas.gameObject);
#endif
            }
        }
    }
}