#if UNITY_6000_0_OR_NEWER
#define UNITY_RENDER_GRAPH
#else
#define UNITY_LEGACY
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

#if UNITY_RENDER_GRAPH
using UnityEngine.Rendering.RenderGraphModule;
#endif

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    public partial class VolFx : ScriptableRendererFeature
    {
        private static List<ShaderTagId> k_ShaderTags;
        
        [Tooltip("When to execute")]
        public RenderPassEvent               _event  = RenderPassEvent.BeforeRenderingPostProcessing;
        [Tooltip("If not set camera format will be used (usually it looses alpha and can't be used as overlay,\\n if the camera loses colors and there are some lines in the blur areas, it is better to set the format to DefaultHDR)")] //[HideInInspector]
        public Optional<RenderTextureFormat> _format = new Optional<RenderTextureFormat>(RenderTextureFormat.ARGB32, true);
        [FormerlySerializedAs("_volumeMask")]
        [Tooltip("Layer mask for Volume Settings and draw source for LayerMask Source")]
        public Optional<LayerMask>           _mask = new Optional<LayerMask>(false);
        [Tooltip("Post processing input source")]
        public SourceOptions                 _source = new SourceOptions();
        [Tooltip("Post processing result output")]
        public OutputOptions                 _output = new OutputOptions();
        [Tooltip("RenderPasses and his execution order")]
        [SocUnique(typeof(BlitPass), typeof(MaskPass))] [SocFormat(_regexClear = "Pass$")]
        public SoCollection<Pass>            _passes = new SoCollection<Pass>();
        
        [HideInInspector]
        public Shader _blitShader;

        [NonSerialized]
        public Material _blit;

        [NonSerialized]
        public PassExecution _execution;
        
        /// <summary>
        /// Volume stack settings
        /// </summary>
        public VolumeStack Stack
        {
            get
            {
                if (_execution._stack == null)
                    _execution._stack = _mask.Enabled ? VolumeManager.instance.CreateStack() : VolumeManager.instance.stack;
                
                return _execution._stack;
            }
        }

        // =======================================================================
        [Serializable]
        public class SourceOptions
        {
            [Tooltip("Post processing source")]
            public Source              _source;
            [Tooltip("Custom Global Texture to process (can be set via Shader.SetGlobalTexture")]
            public string              _globalTex = "_inputTex";
            [Tooltip("Render Texture to process")]
            public RenderTexture       _renderTex;
            [Tooltip("VolFx Pool to process")]
            public Tools.Pool          _pool;
            
            public enum Source
            {
                [Tooltip("Camera Source")]
                Camera,
                [Tooltip("LayerMask objects, objects can be excluded from rendering using filtering options in the Urp asset")]
                LayerMask,
                [Tooltip("Render pool from VolFx Pool")]
                Pool,
                [Tooltip("Custom texture source (global texture)")]
                Custom
            }
        }
        
        [Serializable]
        public class OutputOptions
        {
            [Tooltip("Output source")]
            public Output              _output;
            [Tooltip("Render Texture")]
            public RenderTexture       _renderTex;
            [Tooltip("Name of the Global Texture to output")]
            public string              _outputTex = "_VolFxTex";
            public int                 _sortingOrder;
            public float               _camDistance = 100f;

            public enum Output
            {
                [Tooltip("Self target")]
                ____,
                [Tooltip("Screen camera")]
                Camera,
                [Tooltip("Create global texture")]
                GlobalTex,
                [Tooltip("Use user render texture")]
                RenderTex,
                [Tooltip("Draw to custom screen sprite")] [InspectorName(null)]
                Sprite
            }
        }
        
        [Serializable]
        public abstract class Pass : ScriptableObject
        {
            [NonSerialized]
            public VolFx               _owner;
            [SerializeField]
            internal  bool             _active = true;
            [SerializeField] [HideInInspector]
            private  Shader            _shader;
            protected Material         _material;
            private   bool             _isActive;
            
            protected         VolumeStack Stack   => _owner.Stack;
            protected virtual bool        Invert  => false;
            protected virtual int         MatPass => 0;
            
            // =======================================================================
            internal bool IsActiveCheck
            {
                get => _isActive && _active && _material != null;
                set => _isActive = value;
            }

            public bool IsActive
            {
                get => _active;
                set => _active = value;
            }
            
            public abstract string ShaderName { get; }
            
            internal void _init()
            {
#if UNITY_EDITOR
                if (_shader == null || _material == null)
                {
                    var sna = (GetType().GetCustomAttributes(typeof(ShaderNameAttribute), true).FirstOrDefault() as ShaderNameAttribute);
                    var shaderName = sna == null ? ShaderName : sna._name;
                    if (string.IsNullOrEmpty(shaderName) == false)
                    {
                        _shader   = Shader.Find(shaderName);
                        var assetPath = UnityEditor.AssetDatabase.GetAssetPath(_shader);
                        if (_editorValidate && string.IsNullOrEmpty(assetPath) == false) 
                            _editorSetup(Path.GetDirectoryName(assetPath), Path.GetFileNameWithoutExtension(assetPath));

                        UnityEditor.EditorUtility.SetDirty(this);
                    }
                }
#endif
                
                if (_shader != null)
                    _material = new Material(_shader);
                
                Init();
            }

            /// <summary>
            /// called to init resources
            /// </summary>
            public virtual void Init(InitApi initApi)
            {
            }
            
            /// <summary>
            /// called to perform rendering
            /// </summary>
            public virtual void Invoke(RTHandle source, RTHandle dest, CallApi callApi)
            {
                callApi.Blit(source, dest, _material, MatPass);
            }

            public void Validate()
            {
#if UNITY_EDITOR
                if (_shader == null || _editorValidate)
                {
                    var shaderName = GetType().GetCustomAttributes(typeof(ShaderNameAttribute), true).FirstOrDefault() as ShaderNameAttribute;
                    if (shaderName != null)
                    {
                        _shader = Shader.Find(shaderName._name);
                        var assetPath = UnityEditor.AssetDatabase.GetAssetPath(_shader);
                        if (string.IsNullOrEmpty(assetPath) == false)
                            _editorSetup(Path.GetDirectoryName(assetPath), Path.GetFileNameWithoutExtension(assetPath));
                        
                        UnityEditor.EditorUtility.SetDirty(this);
                    }
                }
                
                if ((_material == null || _material.shader != _shader) && _shader != null)
                {
                    _material = new Material(_shader);
                    Init();
                }
#endif
                
                IsActiveCheck = Validate(_material);
            }

            /// <summary>
            /// called to initialize pass when material is created
            /// </summary>
            public virtual void Init()
            {
            }

            /// <summary>
            /// called each frame to check is render is required and setup render material
            /// </summary>
            public abstract bool Validate(Material mat);
            
            /// <summary>
            /// frame clean up function used if implemented custom Invoke function to release resources
            /// </summary>
            public virtual void Cleanup(CommandBuffer cmd)
            {
            }
            
            /// <summary>
            /// used for optimization purposes, returns true if we need to call _editorSetup function
            /// </summary>
            protected virtual bool _editorValidate => false;
            
            /// <summary>
            /// editor validation function, used to gather additional references 
            /// </summary>
            protected virtual void _editorSetup(string folder, string asset)
            {
            }
        }
        
        public class PassExecution : ScriptableRenderPass
        {
            public   VolFx            _owner;
            internal RenderTargetFlip _renderTarget;
            private  Pass[]           _passes = Array.Empty<Pass>();
            internal VolumeStack      _stack;
            
            private RenderTarget        _output;
            private RendererListParams  _rlp;
            
            private ProfilingSampler    _profiler;
            
            
#if UNITY_RENDER_GRAPH
            private InitApiRg _initApiRg;
            private CallApiRg _callApiRg;
#endif
            
            private InitApiLeg   _initApiLeg;
            private CallApiLeg   _callApiLeg;
            private ScreenSprite _sprite;

            // =======================================================================
#if UNITY_RENDER_GRAPH
            public class PassData
            {
                public TextureHandle           _source;
                public TextureHandle           _camDepth;
                
                public TextureHandle           _output;
                public TextureHandle           _flipA;
                public TextureHandle           _flipB;
                
                public TextureHandle           _pool;
                public RendererListHandle      _rl;
                
                public Camera                  _cam;
                
                public bool                    _selfTarget;
            }

            public class NullData
            {
            }
#endif
            // =======================================================================
            public void Init()
            {
                renderPassEvent = _owner._event;
                
                _renderTarget = new RenderTargetFlip(nameof(_renderTarget));
               
                var mask   = _owner._mask.Enabled ? _owner._mask.Value.value : int.MaxValue;
                
                _output = new RenderTarget().Allocate( _owner._output._outputTex);
                _rlp    = new RendererListParams(new CullingResults(), new DrawingSettings(), new FilteringSettings(RenderQueueRange.all, mask));
                
                _profiler  = new ProfilingSampler(_owner.name);

#if UNITY_RENDER_GRAPH
                _initApiRg = new InitApiRg();
                _callApiRg = new CallApiRg();
#endif
                _initApiLeg = new InitApiLeg();
                _callApiLeg = new CallApiLeg();
            }
#if UNITY_RENDER_GRAPH
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                var rndData = frameData.Get<UniversalRenderingData>();
                var resData = frameData.Get<UniversalResourceData>();
                var camData = frameData.Get<UniversalCameraData>();
                var lghData = frameData.Get<UniversalLightData>();
                    
                ref var camRtDesc = ref camData.cameraTargetDescriptor;
                var     width     = camRtDesc.width;
                var     height    = camRtDesc.height;
                var     global    = TextureHandle.nullHandle;

                var _initApi = _initApiRg;
                var _callApi = _callApiRg;
                
                _initApi.Width  = width;
                _initApi.Height = height;
                
                if (_owner._output._output == OutputOptions.Output.GlobalTex)
                {
                    using (var bld = renderGraph.AddRasterRenderPass<NullData>("Allocate Global Texture", out var passData))
                    {
                        var desc = new RenderTextureDescriptor(camRtDesc.width, camRtDesc.height, RenderTextureFormat.ARGB32, 0);
                        var tex  = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, _owner._output._outputTex, false);
                        bld.SetRenderAttachment(tex, 0, AccessFlags.ReadWrite);

                        bld.AllowPassCulling(false);
                        
                        bld.SetRenderFunc((NullData data, RasterGraphContext ctx) => { 
                            // Shader.SetGlobalTexture(_owner._output._outputTex, tex);
                        });

                        bld.SetGlobalTextureAfterPass(in tex, Shader.PropertyToID(_owner._output._outputTex));
                        
                        global = tex;
                    }
                }
                
                using (var builder = renderGraph.AddUnsafePass<PassData>(passName, out var passData, _profiler))
                {
                    var camDesc = new TextureDesc(camRtDesc.width, camRtDesc.height, false, false);
                    if (_owner._format.Enabled)
                        camDesc.colorFormat = _owner._format.Value.ToGraphicsFormat();
                    
                    passData._camDepth = resData.activeDepthTexture;
                    builder.UseTexture(passData._camDepth, AccessFlags.ReadWrite);

                    passData._cam      = camData.camera;

                    passData._selfTarget = _isSelfTarget();
                    switch (_owner._source._source)
                    {
                        case SourceOptions.Source.Camera:
                        {
                            passData._source = resData.cameraColor;
                        } break;
                        case SourceOptions.Source.LayerMask:
                        {
                            passData._source = resData.cameraColor;
                            passData._pool   = builder.CreateTransientTexture(camDesc);
                            
                            builder.UseTexture(passData._pool, AccessFlags.ReadWrite);
                        } break;
                        case SourceOptions.Source.Custom:
                        {
                            var tex = Shader.GetGlobalTexture(_owner._source._globalTex);
                            if (tex == null)
                            {
                                passData._source = TextureHandle.nullHandle;
                                builder.SetRenderFunc((PassData data, UnsafeGraphContext context) => { });
                                return;
                            } 
                            
                            var texInfo = new RenderTargetInfo()
                            {
                                format = tex.graphicsFormat,
                                width  = tex.width, height = tex.height, msaaSamples = 1, volumeDepth = 1, bindMS = false
                            };

                            passData._source = renderGraph.ImportTexture(RTHandles.Alloc(tex), texInfo);
                            //builder.UseGlobalTexture(Shader.PropertyToID(_owner._source._sourceTex), AccessFlags.ReadWrite);
                        } break;
                        /*case SourceOptions.Source.RenderTex:
                        {
                            var rndTex = _owner._source._renderTex;
                            if (rndTex == null)
                            {
                                builder.SetRenderFunc((PassData data, UnsafeGraphContext context) => { });
                                return;
                            }
                            
                            var texInfo = new RenderTargetInfo()
                            {
                                format = rndTex.graphicsFormat,
                                width  = rndTex.width, height = rndTex.height,
                                msaaSamples = 1, volumeDepth = 1, bindMS = false
                            };

                            passData._source = renderGraph.ImportTexture(RTHandles.Alloc(_owner._source._renderTex), texInfo);
                        } break;*/
                        case SourceOptions.Source.Pool:
                        {
                            if (_owner._source._pool == null)
                            {
                                builder.SetRenderFunc((PassData data, UnsafeGraphContext context) => { });
                                return;
                            }
                            
                            var tex = _owner._source._pool.GetHandle(camRtDesc.width, camRtDesc.height);
                            if (tex == null)
                            {
                                passData._source = TextureHandle.nullHandle;
                                builder.SetRenderFunc((PassData data, UnsafeGraphContext context) => { });
                                return;
                            } 
                            
                            var texInfo = new RenderTargetInfo()
                            {
                                format = GraphicsFormat.R8G8B8A8_UNorm,
                                width  = camRtDesc.width, height = camRtDesc.height, msaaSamples = 1, volumeDepth = 1, bindMS = false
                            };
                            
                            passData._source = renderGraph.ImportTexture(tex, texInfo);
                        } break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    
                    if (_isSourceUnvalidResource() == false)
                        builder.UseTexture(passData._source, AccessFlags.ReadWrite);
                    
                    // for some reasons unity produce errors if we use camera color tex and color depth text, even though it uses it in her official documentation...
                    // builder.UseTexture(resData.cameraColor, AccessFlags.ReadWrite);
                    // builder.UseTexture(resData.activeDepthTexture, AccessFlags.ReadWrite);

                    //builder.UseAllGlobalTextures(true);
                    switch (_owner._output._output)
                    {
                        case OutputOptions.Output.____:
                        {
                            passData._output = passData._source;
                        } break;
                        case OutputOptions.Output.Camera:
                        {
                            passData._output = resData.cameraColor;
                        } break;
                        case OutputOptions.Output.GlobalTex:
                        {
                            passData._output = global;
                            if (_owner._source._source == SourceOptions.Source.LayerMask)
                                builder.UseGlobalTexture(Shader.PropertyToID(_owner._output._outputTex), AccessFlags.Write);
                        } break;
                        case OutputOptions.Output.RenderTex:
                        {
                            var rndTex = _owner._output._renderTex;
                            if (rndTex == null)
                            {
                                builder.SetRenderFunc((PassData data, UnsafeGraphContext context) => { });
                                return;
                            }
                            
                            var texInfo = new RenderTargetInfo()
                            {
                                format = _owner._output._renderTex.graphicsFormat,
                                width  = rndTex.width, height = rndTex.height, msaaSamples = 1, volumeDepth = 1, bindMS = false
                            };

                            passData._output = renderGraph.ImportTexture(RTHandles.Alloc(_owner._output._renderTex), texInfo);
                        } break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    
                    if (passData._selfTarget == false && _owner._source._source != SourceOptions.Source.LayerMask && _isOutputUnvalidResource() == false)
                        builder.UseTexture(passData._output, AccessFlags.Write);

                    passData._flipA     = builder.CreateTransientTexture(camDesc);
                    passData._flipB     = builder.CreateTransientTexture(camDesc);

                    _rlp.cullingResults = rndData.cullResults;
                    _rlp.drawSettings   = RenderingUtils.CreateDrawingSettings(k_ShaderTags, rndData, camData, lghData, SortingCriteria.CommonTransparent);
                    passData._rl        = renderGraph.CreateRendererList(_rlp);

                    
                    // command buffer and validation
                    foreach (var pass in _owner._passes.Values.Where(n => n != null))
                        pass.Validate();
                    _passes = _owner._passes.Values.Where(n => n != null && n.IsActiveCheck).ToArray();
                    
                    _initApi._builder = builder;
                    _initApi._frameData = frameData;
                    _initApi._renderGraph = renderGraph;
                    
                    foreach (var pass in _passes)
                    {
                        pass.Init(_initApi);
                    }
                    
                    builder.UseRendererList(passData._rl);
                    builder.AllowPassCulling(false);
                    builder.SetRenderFunc((PassData data, UnsafeGraphContext context) => Execute(data, context));

                    // -----------------------------------------------------------------------
                    bool _isSourceUnvalidResource()
                    {
                        return _owner._source._source == SourceOptions.Source.Camera;
                    }
                    
                    bool _isOutputUnvalidResource()
                    {
                        return _owner._output._output == OutputOptions.Output.Camera;
                    }
                }
            }
            
            private void Execute(PassData data, UnsafeGraphContext context)
            {
                if (_owner._mask.Enabled && _stack != null)
                    VolumeManager.instance.Update(_stack, null, _owner._mask.Value);
                
                var cmd = context.cmd;
                var _initApi = _initApiRg;
                var _callApi = _callApiRg;
                
                _callApi._blit = _owner._blit;
                _callApi._cmd  = context.cmd;
                _callApi._cam  = data._cam;
                
                cmd.SetRenderTarget(data._camDepth);
                
                _renderTarget.From.Handle = data._flipA;
                _renderTarget.To.Handle   = data._flipB;
                
                if (_passes.Length == 0 && _isDrawler() == false)
                    return;
                
                var source = _getSourceTex();
                var output = data._output;
                
                _renderTarget.From.Handle = data._flipA;
                _renderTarget.To.Handle   = data._flipB;
                
                // draw post process chain
                if (_passes.Length != 0)
                {
                    _callApi._camColor = source;
                    _callApi.Mat.Clear();
                    _passes[0].Invoke(source, _renderTarget.From, _callApi);
                    for (var n = 1; n < _passes.Length; n++)
                    {
                        var pass = _passes[n];
                        _callApi.Mat.Clear();
                        pass.Invoke(_renderTarget.From, _renderTarget.To, _callApi);
                        _renderTarget.Flip();
                    }
                    
                    _callApi.Mat.Clear();
                    _callApi.Blit(_renderTarget.From, output, _owner._blit, data._selfTarget ? 0 : 1);
                }

                if (_passes.Length == 0 && _isDrawler())
                {
                    _callApi.Mat.Clear();
                    _callApi.Blit(source, output, _owner._blit, 1);
                }

                // =======================================================================
                RTHandle _getSourceTex()
                {
                    switch (_owner._source._source)
                    {
                        case SourceOptions.Source.Camera:
                            return data._source;
                        case SourceOptions.Source.Custom:
                            return data._source;
                       /* case SourceOptions.Source.RenderTex:
                            return data._source;*/
                        case SourceOptions.Source.LayerMask:
                        {
                            cmd.SetRenderTarget(data._pool, data._camDepth);
                            cmd.ClearRenderTarget(RTClearFlags.Color, Color.clear, 1f, 0);
                        
                            cmd.DrawRendererList(data._rl);
                            
                            return data._pool;
                        }
                        case SourceOptions.Source.Pool:
                            return data._source;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                
                bool _isDrawler()
                {
                    return _owner._source._source == SourceOptions.Source.LayerMask || data._selfTarget == false;
                }
            }
            
            // Render Graph has their own bugs in legacy mode so we need to reimplement execute pass for compability mode... 
            [Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (_owner._mask.Enabled && _stack != null)
                    VolumeManager.instance.Update(_stack, null, _owner._mask.Value);
            
                // command buffer and validation
                var cmd = CommandBufferPool.Get(_owner.name);
                ref var cameraData = ref renderingData.cameraData;
                var camColor = cameraData.renderer.cameraColorTargetHandle;
                
                foreach (var pass in _owner._passes.Values.Where(n => n != null))
                    pass.Validate();
                
                _passes = _owner._passes.Values.Where(n => n != null && n.IsActiveCheck).ToArray();
                
                if (_passes.Length == 0 && _isDrawler() == false)
                {
                    cmd.Clear();
                    CommandBufferPool.Release(cmd);
                    return;
                }
                
                var _initApi = _initApiLeg;
                var _callApi = _callApiLeg;
                
                _initApi.Width  = cameraData.cameraTargetDescriptor.width;
                _initApi.Height = cameraData.cameraTargetDescriptor.height;
                
                _initApi._cmd = cmd;
                foreach (var pass in _passes)
                    pass.Init(_initApi);
                
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                
                var desc = cameraData.cameraTargetDescriptor;
                if (_owner._format.Enabled)
                    desc.colorFormat = _owner._format.Value;
                _renderTarget.Get(cmd, in desc);

#if UNITY_EDITOR
                if (Application.isPlaying == false && _canGetSourceTex() == false)
                {
                    cmd.Clear();
                    CommandBufferPool.Release(cmd);
                    return; 
                }
#endif
                var source = _getSourceTex(ref renderingData);
                var output = _getOutputTex(ref renderingData);
                cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                
                _callApi._blit = _owner._blit;
                _callApi._cam = renderingData.cameraData.camera;
                _callApi._cmd = cmd;
                _callApi._camColor = cameraData.renderer.cameraColorTargetHandle;
                
                // draw post process chain
                if (_passes.Length != 0)
                {
                    _callApi.Mat.Clear();
                    _passes[0].Invoke(source, _renderTarget.From, _callApi);
                    for (var n = 1; n < _passes.Length ; n++)
                    {
                        var pass = _passes[n];
                        _callApi.Mat.Clear();
                        pass.Invoke(_renderTarget.From, _renderTarget.To, _callApi);
                        _renderTarget.Flip();
                    }

                    _callApi.Mat.Clear();
                    Utils.Blit(cmd, _renderTarget.From, output, _owner._blit, _isSelfTarget() ? 0 : 1);
                }
                
                if (_passes.Length == 0 && _isDrawler())
                    Utils.Blit(cmd, source, output, _owner._blit, 1);

                cmd.SetViewProjectionMatrices(cameraData.GetViewMatrix(), cameraData.GetProjectionMatrix());
                 
                cmd.SetRenderTarget(cameraData.renderer.cameraColorTargetHandle, renderingData.cameraData.renderer.cameraDepthTargetHandle);
                
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);

                // -----------------------------------------------------------------------
#if UNITY_EDITOR
                bool _canGetSourceTex()
                {
                    switch (_owner._source._source)
                    {
                        case SourceOptions.Source.Camera:
                            return true;
                        case SourceOptions.Source.Custom:
                            return true;
                        case SourceOptions.Source.LayerMask:
                            return true;
                        case SourceOptions.Source.Pool:
                            return _owner._source._pool != null;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
#endif
                
                RTHandle _getSourceTex(ref RenderingData renderingData)
                {
                    switch (_owner._source._source)
                    {
                        case SourceOptions.Source.Camera:
                            return _getCameraOutput(ref renderingData);
                        case SourceOptions.Source.Custom:
                        {
                            var gt = Shader.GetGlobalTexture(_owner._source._globalTex);
                            if (gt == null)
                                return RTHandles.Alloc(_owner._source._globalTex, name: _owner._source._globalTex);
                            else
                                return RTHandles.Alloc(gt);
                        }
                        case SourceOptions.Source.LayerMask:
                        {
                            var desc = renderingData.cameraData.cameraTargetDescriptor;
                            if (_owner._format.Enabled)
                                desc.colorFormat = _owner._format.Value;
                            
                            _output.Get(cmd, in desc);
                            
#if UNITY_2022_1_OR_NEWER
                            var depth = renderingData.cameraData.renderer.cameraDepthTargetHandle;
#else
                            var depth = renderingData.cameraData.renderer.cameraDepthTarget == BuiltinRenderTextureType.CameraTarget
                                ? renderingData.cameraData.renderer.cameraColorTarget
                                : renderingData.cameraData.renderer.cameraDepthTarget;
#endif
                            cmd.SetRenderTarget(_output.Id, depth);
                            cmd.ClearRenderTarget(RTClearFlags.Color, Color.clear, 1f, 0);
                        
                            ref var cameraData = ref renderingData.cameraData;
                            var     camera     = cameraData.camera;
                            camera.TryGetCullingParameters(out var cullingParameters);

                            _rlp.cullingResults = context.Cull(ref cullingParameters);
                            _rlp.drawSettings   = CreateDrawingSettings(k_ShaderTags, ref renderingData, SortingCriteria.CommonTransparent);
                            
                            var rl = context.CreateRendererList(ref _rlp);
                            cmd.DrawRendererList(rl);
                            
                            return _output.Handle;
                        }
                        case SourceOptions.Source.Pool:
                        {
                            var tx = Shader.GetGlobalTexture(_owner._source._pool.GlobalTexName);
                            if (tx == null || tx.width == 4)
                                tx = _owner._source._pool.GetHandle(Screen.width, Screen.height);
                            return RTHandles.Alloc(tx);
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                
                RTHandle _getOutputTex(ref RenderingData renderingData)
                {
                    switch (_owner._output._output)
                    {
                        case OutputOptions.Output.Camera:
                            return _getCameraOutput(ref renderingData);
                        
                        case OutputOptions.Output.____:
                        {
                            if (_owner._source._source == SourceOptions.Source.LayerMask)
                                return _getCameraOutput(ref renderingData);
                            
                            return source;
                        }
                        case OutputOptions.Output.GlobalTex:
                        {
                            var tex = Shader.GetGlobalTexture(_owner._output._outputTex);
                            if (tex == null || (tex as Texture2D)?.width == 4)
                            {
                                var handle = RTHandles.Alloc(Screen.width, Screen.height, TextureWrapMode.Repeat, TextureWrapMode.Repeat, colorFormat: GraphicsFormat.R8G8B8A8_UNorm, name: _owner._output._outputTex);
                                tex = handle;
                                //tex = new RenderTexture(Screen.width, Screen.height, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.None, 0);
                                Shader.SetGlobalTexture(_owner._output._outputTex, tex);
                            }
                            return RTHandles.Alloc(tex);
                        }
                        case OutputOptions.Output.RenderTex:
                        {
                            return RTHandles.Alloc(_owner._output._renderTex);
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                bool _isDrawler()
                {
                    return _owner._source._source == SourceOptions.Source.LayerMask || _isSelfTarget() == false;
                }
                
                RTHandle _getCameraOutput(ref RenderingData renderingData)
                {
                    ref var cameraData = ref renderingData.cameraData;
#if UNITY_2022_1_OR_NEWER                
                    return cameraData.renderer.cameraColorTargetHandle;
#else
                    return RTHandles.Alloc(cameraData.renderer.cameraColorTarget);
#endif
                }
            }
#endif
            
#if UNITY_LEGACY
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (_owner._mask.Enabled && _stack != null)
                    VolumeManager.instance.Update(_stack, null, _owner._mask.Value);
            
                // command buffer and validation
                var cmd = CommandBufferPool.Get(_owner.name);
                ref var cameraData = ref renderingData.cameraData;
                var camColor = cameraData.renderer.cameraColorTargetHandle;
                
                foreach (var pass in _owner._passes.Values.Where(n => n != null))
                    pass.Validate();
                
                _passes = _owner._passes.Values.Where(n => n != null && n.IsActiveCheck).ToArray();
                
                if (_passes.Length == 0 && _isDrawler() == false)
                {
                    cmd.Clear();
                    CommandBufferPool.Release(cmd);
                    return;
                }
                
                var _initApi = _initApiLeg;
                var _callApi = _callApiLeg;
                
                _initApi.Width  = cameraData.cameraTargetDescriptor.width;
                _initApi.Height = cameraData.cameraTargetDescriptor.height;
                
                _initApi._cmd = cmd;
                
                foreach (var pass in _passes)
                    pass.Init(_initApi);
                
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                
                var desc = cameraData.cameraTargetDescriptor;
                if (_owner._format.Enabled)
                    desc.colorFormat = _owner._format.Value;
                _renderTarget.Get(cmd, in desc);

#if UNITY_EDITOR
                if (Application.isPlaying == false && _canGetSourceTex() == false)
                {
                    cmd.Clear();
                    CommandBufferPool.Release(cmd);
                    return; 
                }
                
                if (_owner._output._renderTex == null && _owner._output._output == OutputOptions.Output.RenderTex)
                    return;
#endif
                var source = _getSourceTex(ref renderingData);
                var output = _getOutputTex(ref renderingData);
                cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                
                _callApi._blit = _owner._blit;
                _callApi._cam = renderingData.cameraData.camera;
                _callApi._cmd = cmd;
                _callApi._camColor = cameraData.renderer.cameraColorTargetHandle;
                
                // draw post process chain
                if (_passes.Length != 0)
                {
                    _callApi.Mat.Clear();
                    _passes[0].Invoke(source, _renderTarget.From, _callApi);
                    for (var n = 1; n < _passes.Length ; n++)
                    {
                        var pass = _passes[n];
                        _callApi.Mat.Clear();
                        pass.Invoke(_renderTarget.From, _renderTarget.To, _callApi);
                        _renderTarget.Flip();
                    }

                    _callApi.Mat.Clear();
                    Utils.Blit(cmd, _renderTarget.From, output, _owner._blit, _isSelfTarget() ? 0 : 1);
                }
                
                if (_passes.Length == 0 && _isDrawler())
                    Utils.Blit(cmd, source, output, _owner._blit, 1);

                cmd.SetViewProjectionMatrices(cameraData.GetViewMatrix(), cameraData.GetProjectionMatrix());
                
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);

                // -----------------------------------------------------------------------
#if UNITY_EDITOR
                bool _canGetSourceTex()
                {
                    switch (_owner._source._source)
                    {
                        case SourceOptions.Source.Camera:
                            return true;
                        case SourceOptions.Source.Custom:
                            return true;
                        case SourceOptions.Source.LayerMask:
                            return true;
                        case SourceOptions.Source.Pool:
                            return _owner._source._pool != null;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
#endif
                
                RTHandle _getSourceTex(ref RenderingData renderingData)
                {
                    switch (_owner._source._source)
                    {
                        case SourceOptions.Source.Camera:
                            return _getCameraOutput(ref renderingData);
                        case SourceOptions.Source.Custom:
                        {
                            var gt = Shader.GetGlobalTexture(_owner._source._globalTex);
                            if (gt == null)
                                return RTHandles.Alloc(_owner._source._globalTex, name: _owner._source._globalTex);
                            else
                                return RTHandles.Alloc(gt);
                        }
                        case SourceOptions.Source.LayerMask:
                        {
                            var desc = renderingData.cameraData.cameraTargetDescriptor;
                            if (_owner._format.Enabled)
                                desc.colorFormat = _owner._format.Value;
                            
                            _output.Get(cmd, in desc);
                            
#if UNITY_2022_1_OR_NEWER
                            var depth = renderingData.cameraData.renderer.cameraDepthTargetHandle;
#else
                            var depth = renderingData.cameraData.renderer.cameraDepthTarget == BuiltinRenderTextureType.CameraTarget
                                ? renderingData.cameraData.renderer.cameraColorTarget
                                : renderingData.cameraData.renderer.cameraDepthTarget;
#endif
                            cmd.SetRenderTarget(_output.Id, depth);
                            cmd.ClearRenderTarget(RTClearFlags.Color, Color.clear, 1f, 0);
                        
                            ref var cameraData = ref renderingData.cameraData;
                            var     camera     = cameraData.camera;
                            camera.TryGetCullingParameters(out var cullingParameters);

                            _rlp.cullingResults = context.Cull(ref cullingParameters);
                            _rlp.drawSettings   = CreateDrawingSettings(k_ShaderTags, ref renderingData, SortingCriteria.CommonTransparent);
                            
                            var rl = context.CreateRendererList(ref _rlp);
                            cmd.DrawRendererList(rl);
                            
                            return _output.Handle;
                        }
                        case SourceOptions.Source.Pool:
                        {
                            var tx = _owner._source._pool.Texture;
                            if (tx == null)
                                tx = Texture2D.blackTexture;
                            return RTHandles.Alloc(tx);
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                
                RTHandle _getOutputTex(ref RenderingData renderingData)
                {
                    switch (_owner._output._output)
                    {
                        case OutputOptions.Output.Camera:
                            return _getCameraOutput(ref renderingData);
                        
                        case OutputOptions.Output.____:
                        {
                            if (_owner._source._source == SourceOptions.Source.LayerMask)
                                return _getCameraOutput(ref renderingData);
                            
                            return source;
                        }
                        case OutputOptions.Output.GlobalTex:
                        {
                            var tex = Shader.GetGlobalTexture(_owner._output._outputTex);
                            if (tex == null)
                            {
                                tex = RTHandles.Alloc(Screen.width, Screen.height, TextureWrapMode.Repeat, TextureWrapMode.Repeat, colorFormat: GraphicsFormat.R8G8B8A8_UNorm, name: _owner._output._outputTex);
                                Shader.SetGlobalTexture(_owner._output._outputTex, tex);
                            }
                            return RTHandles.Alloc(tex);
                        }
                        case OutputOptions.Output.RenderTex:
                        {
                            return RTHandles.Alloc(_owner._output._renderTex);
                        }
                        case OutputOptions.Output.Sprite:
                        {
                            if (_sprite == null)
                            {
                                _sprite = ScreenSprite.Create(_owner._output._sortingOrder, _owner._output._camDistance, Shader.Find("Sprites/Default"));
                            }
                            
                            var st = _sprite.Prepare(renderingData.cameraData.camera, _initApi);
                            cmd.SetRenderTarget(st);
                            cmd.ClearRenderTarget(RTClearFlags.Color, Color.clear, 1f, 0);
                            
                            return st;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                bool _isDrawler()
                {
                    return _owner._source._source == SourceOptions.Source.LayerMask || _isSelfTarget() == false;
                }
                
                RTHandle _getCameraOutput(ref RenderingData renderingData)
                {
                    ref var cameraData = ref renderingData.cameraData;
#if UNITY_2022_1_OR_NEWER                
                    return cameraData.renderer.cameraColorTargetHandle;
#else
                    return RTHandles.Alloc(cameraData.renderer.cameraColorTarget);
#endif
                }
            }
#endif
            bool _isSelfTarget()
            {
                // check if we need to override initial source
                if (_owner._output._output == OutputOptions.Output.____ && _owner._source._source != SourceOptions.Source.LayerMask)
                    return true;
                
                switch (_owner._source._source)
                {
                    case SourceOptions.Source.Camera:
                    {
                        if (_owner._output._output == OutputOptions.Output.Camera)
                            return true;
                    } break;
                    case SourceOptions.Source.LayerMask:
                        break;
                    case SourceOptions.Source.Custom:
                    {
                        if (_owner._output._output == OutputOptions.Output.GlobalTex 
                            && _owner._source._globalTex == _owner._output._outputTex)
                            return true;
                    } break;
                    /*case SourceOptions.Source.RenderTex:
                    {
                        if (_owner._output._output == OutputOptions.Output.RenderTex 
                            && _owner._source._renderTex == _owner._output._renderTex)
                            return true;
                    } break;*/
                    case SourceOptions.Source.Pool:
                    {
                        if (_owner._output._output == OutputOptions.Output.____)
                            return true;
                    } break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                return false;
            }
            
            public override void FrameCleanup(CommandBuffer cmd)
            {
                _renderTarget.Release(cmd);
                if (_output != null)
                    _output.Release(cmd);
                foreach (var pass in _passes)
                    pass.Cleanup(cmd);
            }
        }
        
        [ExecuteAlways]
        [DefaultExecutionOrder(1000)]
        public class ScreenSprite : MonoBehaviour
        {
            public float           m_ScaleAdd;
            public SpriteRenderer  m_Sprite;
            public float           m_Distance;
            public bool            m_DrawCall;
            public Material        m_Material;
            private RenderTarget   m_Output;
            
            private int _countdown;
            
            // =======================================================================
            public static ScreenSprite Create(int order, float dist, Shader shader)
            {
                var go = new GameObject($"ScreenSprite");

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = Sprite.Create(new Texture2D(4, 4, TextureFormat.RGBA32, false),  new Rect(.0f, .0f, 4, 4), new Vector2(0.5f, 0.5f), 4f);
                sr.sortingOrder = order;
                sr.material = new Material(Shader.Find("Unlit/ScreenSprite"));
                
                var ss =  go.AddComponent<ScreenSprite>();
                ss.m_Distance = dist;
                ss.m_Sprite = sr;
                
                ss.m_Material = sr.material;
                ss.m_Output   = new RenderTarget().Allocate($"ss_{ss.gameObject.GetInstanceID()}");
                
                //go.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
                
                return ss;
            }
            
            public RTHandle Prepare(Camera cam, InitApi init)
            {
                init.Allocate(m_Output, Screen.width, Screen.height, GraphicsFormat.R8G8B8A8_UNorm, TextureWrapMode.Clamp, FilterMode.Point);
                m_Material.SetTexture("_OverTex", m_Output.Handle);
                //m_Sprite.sprite.texture = m_Output.Handle;
                
                _update(cam);
                
                return m_Output;
            }

            private void Awake()
            {
                if (Application.isPlaying)
                    DontDestroyOnLoad(gameObject);
            }

            private void Update()
            {
                if (_countdown > 3)
                {
                    if (Application.isPlaying)
                        Destroy(gameObject);
                    else
                        DestroyImmediate(gameObject);
                }
                
                _countdown ++;
            }

            private void _update(Camera cam)
            {
                var dist = m_Distance;
                var frustumHeight = cam.orthographic 
                    ? cam.orthographicSize * 2f 
                    : 2f * dist * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
                var frustumWidth  = frustumHeight * cam.aspect;

                transform.SetPositionAndRotation(cam.transform.position + cam.transform.forward * (dist), cam.transform.rotation);
                transform.localScale = new Vector3(frustumWidth +  m_ScaleAdd, frustumHeight +  m_ScaleAdd, 1f);
                
                _countdown = 0;
            }
        }
        
        // =======================================================================
        public override void Create()
        {
#if UNITY_EDITOR
            _blitShader = Shader.Find("Hidden/VolFx/Blit");
            
            UnityEditor.EditorUtility.SetDirty(this);
#endif
            _blit      = new Material(_blitShader);
            _execution = new PassExecution() { _owner = this};
            _execution.Init();
            
            foreach (var pass in _passes)
            {
                if (pass == null)
                    continue;
                
                pass._owner = this;
                pass._init();
            }

            if (k_ShaderTags == null)
            {
                k_ShaderTags = new List<ShaderTagId>(new[]
                {
                    new ShaderTagId("SRPDefaultUnlit"),
                    new ShaderTagId("UniversalForward"),
                    new ShaderTagId("UniversalForwardOnly")
                });
            }
        }

        private void OnDestroy()
        {
            _passes.Destroy();
        }
        
#if UNITY_RENDER_GRAPH
        [Obsolete]
#endif
        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            _execution.ConfigureInput(ScriptableRenderPassInput.Color);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType != CameraType.Game && (Application.isPlaying == false && renderingData.cameraData.cameraType != CameraType.SceneView))
                return;
            
#if UNITY_EDITOR
            if (_blit == null)
                _blit = new Material(_blitShader);
#endif
            
            renderer.EnqueuePass(_execution);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing == false)
                return;
            
            if (_mask.Enabled && _execution != null && _execution._stack != null)
                 VolumeManager.instance.DestroyStack(_execution._stack);
        }
    }
}