using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

#if UNITY_EDITOR
using UnityEditor;
#endif

//  ScreenFx © NullTale - https://x.com/NullTale
namespace ScreenFx
{
    public class ScreenMixer : PlayableBehaviour
    {
        public static WaitForEndOfFrame s_WaitLateUpdate = new WaitForEndOfFrame();

        public int            _sortingOrder;
        public RenderMode     _renderMode;
        private Sprite        _screenShotTex;
        private ScreenOverlay _handle;
        public float          _weight;
        public int            _layer;
        public float          _planeDist;
        public string         _name;

        public Camera        _captureCamera;  // камера для скриншота (можно назначить извне)

        // =======================================================================
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            _handle = new ScreenOverlay(_sortingOrder, _renderMode, _layer, _planeDist, _name);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            _handle?.Dispose();
            
            if (_screenShotTex != null && _screenShotTex != Utils.s_SpriteClear)
                _releaseScreenShot();
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var inputCount = playable.GetInputCount();

            var scale       = 0f;
            var color       = Color.clear;
            var imageWeight = 0f;

            Sprite image = null;
            var fullWeight = 0f;
            var soloInput  = 0;
            var takeScreenShot = false;
            var useScale = false;

            for (var n = 0; n < inputCount; n++)
            {
                var inputWeight = playable.GetInputWeight(n);
                if (inputWeight <= 0f)
                    continue;

                soloInput  =  n;
                fullWeight += inputWeight;

                var inputPlayable = (ScriptPlayable<ScreenBehaviour>)playable.GetInput(n);
                var behaviour     = inputPlayable.GetBehaviour();

                scale += behaviour._scale * inputWeight;
                color += behaviour._color * inputWeight;

                if (imageWeight < inputWeight)
                {
                    imageWeight = inputWeight;

                    switch (behaviour._sprite)
                    {
                        case ScreenBehaviour.SpriteSource.Image:
                            image = behaviour._image;
                            useScale = true;
                            break;

                        case ScreenBehaviour.SpriteSource.Default:
                            {
                                var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                                tex.SetPixel(0, 0, Color.white);
                                tex.Apply();
                                image = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
                            }
                            break;

                        case ScreenBehaviour.SpriteSource.GradHor:
                            {
                                var size = 64;
                                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                                tex.wrapMode = TextureWrapMode.Clamp;
                                for (var x = 0; x < size; x++)
                                for (var y = 0; y < size; y++)
                                    tex.SetPixel(x, y, behaviour._gradient.Evaluate(x / (float)(size - 1)));
                                tex.Apply(false, true);
                                image = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
                            }
                            break;

                        case ScreenBehaviour.SpriteSource.GradVert:
                            {
                                var size = 64;
                                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                                tex.wrapMode = TextureWrapMode.Clamp;
                                for (var x = 0; x < size; x++)
                                for (var y = 0; y < size; y++)
                                    tex.SetPixel(x, y, behaviour._gradient.Evaluate(y / (float)(size - 1)));
                                tex.Apply(false, true);
                                image = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
                            }
                            break;

                        case ScreenBehaviour.SpriteSource.GradCircle:
                            {
                                var size = 64;
                                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                                tex.wrapMode = TextureWrapMode.Clamp;
                                var center = new Vector2(0.5f, 0.5f);
                                for (var x = 0; x < size; x++)
                                for (var y = 0; y < size; y++)
                                {
                                    var uv   = new Vector2(x / (float)(size - 1), y / (float)(size - 1));
                                    var dist = Mathf.Clamp01(Vector2.Distance(center, uv) / 0.7071f);
                                    tex.SetPixel(x, y, behaviour._gradient.Evaluate(1f - dist));
                                }
                                tex.Apply(false, true);
                                image = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
                            }
                            break;

                        case ScreenBehaviour.SpriteSource.ScreenShot:
                            image = _screenShotTex != null ? _screenShotTex : Utils.s_SpriteClear;
                            if (image == Utils.s_SpriteClear)
                                takeScreenShot = true;
                            useScale = true;
                            break;
                    }
                }
            }

            fullWeight *= _weight;


            if (fullWeight > 0f)
            {
                _handle.Open();
                if (takeScreenShot)
                    _takeScreenShot();
            }
            else
            {
                _handle.Close();
                _releaseScreenShot();
                return;
            }

            if (fullWeight < 1f)
            {
                var behaviour = ((ScriptPlayable<ScreenBehaviour>)playable.GetInput(soloInput)).GetBehaviour();
                scale = behaviour._scale;
                color = behaviour._color.MulA(fullWeight);
            }

            _handle.Scale  = (useScale ? scale : 1f).ToVector2XY();
            _handle.Color  = color;
            _handle.Sprite = image;
        }

        // =======================================================================
        private void _takeScreenShot()
        {
            if (Application.isPlaying)
            {
                if (_screenShotTex != null && _screenShotTex != Utils.s_SpriteClear)
                {
                    _releaseScreenShot();
                }

                _screenShotTex = Utils.s_SpriteClear;
                ScreenFx.Instance.StartCoroutine(_captureRoutine());

                IEnumerator _captureRoutine()
                {
                    yield return s_WaitLateUpdate;
                    var tex = ScreenCapture.CaptureScreenshotAsTexture();
                    tex.filterMode = FilterMode.Point;
                    _screenShotTex = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), Mathf.Max(tex.width, tex.height));
                }
            }
#if UNITY_EDITOR
            else
            {
                if (_screenShotTex != null && _screenShotTex != Utils.s_SpriteClear)
                {
                    _releaseScreenShot();
                }

                if (_captureCamera == null)
                    _captureCamera = Camera.main;
                
                if (_captureCamera == null)
                    return;

                if (_captureCamera.cameraType != CameraType.Game)
                    _captureCamera = null;

                if (_captureCamera != null)
                {
                    // Delayed screenshot capture in editor
                    EditorApplication.delayCall += () =>
                    {
                        var tex = CaptureEditorScreenshot(_captureCamera, Screen.width, Screen.height);
                        tex.filterMode = FilterMode.Point;
                        _screenShotTex = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), Mathf.Max(tex.width, tex.height));
                    };
                }
            }
#endif
        }

        private void _releaseScreenShot()
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
                if (_screenShotTex)
                {
                    Object.DestroyImmediate(_screenShotTex.texture);
                    Object.DestroyImmediate(_screenShotTex);
                    _screenShotTex = null;
                }
#endif

            if (_screenShotTex)
            {
                Object.Destroy(_screenShotTex.texture);
                Object.Destroy(_screenShotTex);
                _screenShotTex = null;
            }
        }

#if UNITY_EDITOR
        public static Texture2D CaptureEditorScreenshot(Camera cam, int width, int height)
        {
            var rt = new RenderTexture(width, height, 24);
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);

            return tex;
        }
#endif
    }
}