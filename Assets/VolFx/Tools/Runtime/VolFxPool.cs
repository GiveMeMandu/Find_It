#if UNITY_6000_0_OR_NEWER
#define UNITY_RENDER_GRAPH
#else
#define UNITY_LEGACY
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_RENDER_GRAPH
using UnityEngine.Rendering.RenderGraphModule;
#endif

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx.Tools
{
    [DisallowMultipleRendererFeature("VolFx Pool")]
    public class VolFxPool : ScriptableRendererFeature
    {
        private static readonly int      s_CameraDepthAttachment = Shader.PropertyToID("_CameraDepthAttachment");
        private static List<ShaderTagId> k_ShaderTags;

        private static List<PoolPass> s_LayersExternal = new List<PoolPass>();

        public  SoCollection<Pool>    _list         = new SoCollection<Pool>();
        private List<PoolPass>        _bufferPasses = new List<PoolPass>();

        private static          Material _blit;
        internal static         Material _adj;

        // =======================================================================
        
        private class PoolPass : ScriptableRenderPass
        {
            public  Pool               _pool;
            private RenderTarget       _output;
            private RenderTarget       _depth;
            private RendererListParams _rlp;
            private ProfilingSampler   _profiler;
            private int                _layerMask;
            
            // =======================================================================
#if UNITY_RENDER_GRAPH
            public class PassData
            {
                public TextureHandle           _output;
                public TextureHandle           _camDepth;
                public TextureHandle           _passDepth;
                
                public RendererListHandle      _rl;
            }
#endif
            // =======================================================================
            public void Init()
            {
                renderPassEvent = _pool._event;
                _output         = new RenderTarget() { Handle = null, Id = Shader.PropertyToID(_pool.GlobalTexName) };
                _depth          = new RenderTarget().Allocate($"{_pool.GlobalTexName}_Depth");
                _profiler       = new ProfilingSampler(_pool.name);
                _initRenderList();
                //_pool._poolTex = new RenderTexture(Screen.width, Screen.height, GraphicsFormat.R8G8B8A8_SRGB, GraphicsFormat.None);
                
                if (SystemInfo.copyTextureSupport == CopyTextureSupport.None && _pool._depth == Pool.DepthStencil.Copy)
                    Debug.LogWarning($"\'{_pool.name}\' buffer depth mode is set to copy, but texture copy functionality is not supported by the platform");
                
                _layerMask = _pool._mask.value;
            }
            
#if UNITY_RENDER_GRAPH
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                var rndData = frameData.Get<UniversalRenderingData>();
                var resData = frameData.Get<UniversalResourceData>();
                var camData = frameData.Get<UniversalCameraData>();
                var lghData = frameData.Get<UniversalLightData>();

                ref var camRtDesc = ref camData.cameraTargetDescriptor;
                using (var builder = renderGraph.AddUnsafePass<PassData>(passName, out var passData, _profiler))
                {
                    //var tex  = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, _pool.GlobalTexName, false, FilterMode.Point, TextureWrapMode.Repeat);
                    var tex = renderGraph.ImportTexture(_pool.GetHandle(camRtDesc.width, camRtDesc.height));
                    // Shader.SetGlobalTexture(_pool.GlobalTexName, _pool.GetHandle(camRtDesc.width, camRtDesc.height));
                    passData._output    = tex;
                    passData._camDepth  = resData.activeDepthTexture;
                    
                    builder.UseTexture(passData._output, AccessFlags.ReadWrite);
                    //builder.UseTexture(passData._camDepth, AccessFlags.ReadWrite);
                    
                    if (_pool._depth == Pool.DepthStencil.Copy || _pool._depth == Pool.DepthStencil.Clean)
                    {
                        var desc = new RenderTextureDescriptor(camRtDesc.width, camRtDesc.height, RenderTextureFormat.Depth, 32);
                        passData._passDepth  = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, $"{_pool.GlobalTexName}_Depth", false);
                        builder.UseTexture(passData._passDepth, AccessFlags.ReadWrite);
                    }

                    _rlp.cullingResults = rndData.cullResults;
                    _rlp.drawSettings   = RenderingUtils.CreateDrawingSettings(k_ShaderTags, rndData, camData, lghData, SortingCriteria.CommonTransparent);
                    passData._rl        = renderGraph.CreateRendererList(_rlp);

                    builder.UseRendererList(passData._rl);

                    builder.AllowPassCulling(false);
                    
                    builder.SetRenderFunc((PassData data, UnsafeGraphContext context) => Execute(data, context));
                    builder.SetGlobalTextureAfterPass(passData._output, Shader.PropertyToID(_pool.GlobalTexName));
                }
            }
            
            internal static MaterialPropertyBlock _mat = new MaterialPropertyBlock();
            private void Execute(PassData data, UnsafeGraphContext context)
            {
                _pool._isLegacy = false;
                var cmd = context.cmd;
                switch (_pool._depth)
                {
                    case Pool.DepthStencil.None:
                        cmd.SetRenderTarget(data._output);
                        break;
                    case Pool.DepthStencil.Camera:
                        cmd.SetRenderTarget(data._output, data._camDepth);
                        break;
                    case Pool.DepthStencil.Clean:
                        cmd.SetRenderTarget(data._output, data._passDepth);
                        break;
                    case Pool.DepthStencil.Copy:
                    {
                        _mat.SetTexture(s_CameraDepthAttachment, data._camDepth);
                        //cmd.SetRenderTarget(data._output, data._passDepth);
                        cmd.SetRenderTarget(data._passDepth);
                        cmd.ClearRenderTarget(RTClearFlags.Depth, clearColor, 1f, 0);
                        cmd.DrawMesh(Utils.FullscreenMesh, Matrix4x4.identity, _blit, 0, 2, _mat);
                        
                        cmd.SetRenderTarget(data._output, data._passDepth);
                    } break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                if (_pool._clear.Enabled)
                    cmd.ClearRenderTarget(RTClearFlags.Color, _pool._clear.Value, 1f, 0);
                
                cmd.DrawRendererList(data._rl);
            }
            
            [Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                var colorDesc = renderingData.cameraData.cameraTargetDescriptor;
                
                if (_pool._format.Enabled)
                    colorDesc.colorFormat = _pool._format.Value;

                _output.Get(cmd, in colorDesc);
            }

            [Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
#if UNITY_EDITOR
                if (_pool == null)
                    return;
#endif
                _pool._isLegacy = true;
                
                // allocate resources
                var cmd = CommandBufferPool.Get(nameof(VolFxPool));
                _profiler.Begin(cmd);
                
#if UNITY_2022_1_OR_NEWER
                var depth = renderingData.cameraData.renderer.cameraDepthTargetHandle;
#else
                var depth = renderingData.cameraData.renderer.cameraDepthTarget == BuiltinRenderTextureType.CameraTarget
                    ? renderingData.cameraData.renderer.cameraColorTarget
                    : renderingData.cameraData.renderer.cameraDepthTarget;
#endif
                switch (_pool._depth)
                {
                    case Pool.DepthStencil.None:
                        cmd.SetRenderTarget(_output.Id);
                        break;
                    case Pool.DepthStencil.Copy:
                    {
                        var depthDesc = renderingData.cameraData.cameraTargetDescriptor;
                        depthDesc.colorFormat        = RenderTextureFormat.Depth;
                        depthDesc.graphicsFormat     = GraphicsFormat.None;
                        depthDesc.depthStencilFormat = GraphicsFormat.D32_SFloat;
                        depthDesc.depthBufferBits    = 32;
                        
                        _depth.Get(cmd, in depthDesc);
                        
                        if (SystemInfo.copyTextureSupport != CopyTextureSupport.None)
                        {
                            cmd.CopyTexture(depth, _depth.Id);
                        }
                        else
                        { 
                            // fix of hope (if api does not support CopyTexture functionality very unlikely it can handle SV_Depth)
                            cmd.SetGlobalTexture("_CameraDepthAttachment", depth);
                            Utils.Blit(cmd, _depth, _blit, 2);
                        }
                        
                        cmd.SetRenderTarget(_output.Id, _depth.Handle);
                    } break;
                    case Pool.DepthStencil.Camera:
                        cmd.SetRenderTarget(_output.Id, depth);
                        break;
                    case Pool.DepthStencil.Clean:
                    {
                        var depthDesc = renderingData.cameraData.cameraTargetDescriptor;
                        depthDesc.colorFormat        = RenderTextureFormat.Depth;
                        depthDesc.graphicsFormat     = GraphicsFormat.None;
                        depthDesc.depthStencilFormat = GraphicsFormat.D32_SFloat_S8_UInt;
                        
                        _depth.Get(cmd, in depthDesc);
                        
                        cmd.SetRenderTarget(_output.Id, _depth.Handle);
                    } break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                if (_pool._depth == Pool.DepthStencil.Camera)
                {
                    if (_pool._clear.Enabled)
                        cmd.ClearRenderTarget(RTClearFlags.Color, _pool._clear, 1f, 0);
                }
                else
                if (_pool._depth == Pool.DepthStencil.Copy)
                {
                    if (_pool._clear.Enabled)
                        cmd.ClearRenderTarget(RTClearFlags.Color, _pool._clear, 1f, 0);
                }
                else
                if (_pool._depth == Pool.DepthStencil.None)
                {
                    if (_pool._clear.Enabled)
                        cmd.ClearRenderTarget(RTClearFlags.Color | RTClearFlags.Depth, _pool._clear, 1f, 0);
                }
                else
                if (_pool._depth == Pool.DepthStencil.Clean)
                {
                    if (_pool._clear.Enabled)
                        cmd.ClearRenderTarget(RTClearFlags.Color | RTClearFlags.Depth, _pool._clear, 1f, 0);
                    else
                        cmd.ClearRenderTarget(RTClearFlags.Depth, Color.clear, 1f, 0);
                }
                    
                if (_pool._mask.Enabled && _pool._mask.Value != 0)
                {
#if UNITY_EDITOR
                    // editor validate fix
                    if (_layerMask != _pool._mask.value)
                    {
                        _initRenderList();
                        _layerMask = _pool._mask.Value;
                    }
#endif
                
                    ref var cameraData = ref renderingData.cameraData;
                    var     camera     = cameraData.camera;
                    camera.TryGetCullingParameters(out var cullingParameters);

                    _rlp.cullingResults = context.Cull(ref cullingParameters);
                    _rlp.drawSettings   = CreateDrawingSettings(k_ShaderTags, ref renderingData, SortingCriteria.CommonTransparent);

                    var rl = context.CreateRendererList(ref _rlp);
                    cmd.DrawRendererList(rl);
                }

                cmd.SetRenderTarget(renderingData.cameraData.renderer.cameraColorTargetHandle, depth);
                _profiler.End(cmd);
                
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
#endif
            
#if UNITY_LEGACY
            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                var colorDesc = renderingData.cameraData.cameraTargetDescriptor;
                
                if (_pool._format.Enabled)
                    colorDesc.colorFormat = _pool._format.Value;

                _output.Get(cmd, in colorDesc);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
#if UNITY_EDITOR
                if (_pool == null)
                    return;
#endif
                _pool._isLegacy = true;
                
                // allocate resources
                var cmd = CommandBufferPool.Get(nameof(VolFxPool));
                _profiler.Begin(cmd);
                
#if UNITY_2022_1_OR_NEWER
                var depth = renderingData.cameraData.renderer.cameraDepthTargetHandle;
#else
                var depth = renderingData.cameraData.renderer.cameraDepthTarget == BuiltinRenderTextureType.CameraTarget
                    ? renderingData.cameraData.renderer.cameraColorTarget
                    : renderingData.cameraData.renderer.cameraDepthTarget;
#endif
                switch (_pool._depth)
                {
                    case Pool.DepthStencil.None:
                        cmd.SetRenderTarget(_output.Id);
                        break;
                    case Pool.DepthStencil.Copy:
                    {
                        var depthDesc = renderingData.cameraData.cameraTargetDescriptor;
                        depthDesc.colorFormat        = RenderTextureFormat.Depth;
                        depthDesc.graphicsFormat     = GraphicsFormat.None;
                        depthDesc.depthStencilFormat = GraphicsFormat.D32_SFloat;
                        depthDesc.depthBufferBits    = 32;
                        
                        _depth.Get(cmd, in depthDesc);
                        
                        if (SystemInfo.copyTextureSupport != CopyTextureSupport.None)
                        {
                            cmd.CopyTexture(depth, _depth.Id);
                        }
                        else
                        { 
                            // fix of hope (if api does not support CopyTexture functionality very unlikely it can handle SV_Depth)
                            cmd.SetGlobalTexture("_CameraDepthAttachment", depth);
                            Utils.Blit(cmd, _depth, _blit, 2);
                        }
                        
                        cmd.SetRenderTarget(_output.Id, _depth.Handle);
                    } break;
                    case Pool.DepthStencil.Camera:
                        cmd.SetRenderTarget(_output.Id, depth);
                        break;
                    case Pool.DepthStencil.Clean:
                    {
                        var depthDesc = renderingData.cameraData.cameraTargetDescriptor;
                        depthDesc.colorFormat        = RenderTextureFormat.Depth;
                        depthDesc.graphicsFormat     = GraphicsFormat.None;
                        depthDesc.depthStencilFormat = GraphicsFormat.D32_SFloat_S8_UInt;
                        
                        _depth.Get(cmd, in depthDesc);
                        
                        cmd.SetRenderTarget(_output.Id, _depth.Handle);
                    } break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                if (_pool._depth == Pool.DepthStencil.Camera)
                {
                    if (_pool._clear.Enabled)
                        cmd.ClearRenderTarget(RTClearFlags.Color, _pool._clear, 1f, 0);
                }
                else
                if (_pool._depth == Pool.DepthStencil.Copy)
                {
                    if (_pool._clear.Enabled)
                        cmd.ClearRenderTarget(RTClearFlags.Color, _pool._clear, 1f, 0);
                }
                else
                if (_pool._depth == Pool.DepthStencil.None)
                {
                    if (_pool._clear.Enabled)
                        cmd.ClearRenderTarget(RTClearFlags.Color | RTClearFlags.Depth, _pool._clear, 1f, 0);
                }
                else
                if (_pool._depth == Pool.DepthStencil.Clean)
                {
                    if (_pool._clear.Enabled)
                        cmd.ClearRenderTarget(RTClearFlags.Color | RTClearFlags.Depth, _pool._clear, 1f, 0);
                    else
                        cmd.ClearRenderTarget(RTClearFlags.Depth, Color.clear, 1f, 0);
                }
                    
                if (_pool._mask.Enabled && _pool._mask.Value != 0)
                {
#if UNITY_EDITOR
                    // editor validate fix
                    if (_layerMask != _pool._mask.value)
                    {
                        _initRenderList();
                        _layerMask = _pool._mask.Value;
                    }
#endif
                
                    ref var cameraData = ref renderingData.cameraData;
                    var     camera     = cameraData.camera;
                    camera.TryGetCullingParameters(out var cullingParameters);

                    _rlp.cullingResults = context.Cull(ref cullingParameters);
                    _rlp.drawSettings   = CreateDrawingSettings(k_ShaderTags, ref renderingData, SortingCriteria.CommonTransparent);

                    var rl = context.CreateRendererList(ref _rlp);
                    cmd.DrawRendererList(rl);
                }

                _profiler.End(cmd);
                
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
#endif
            private void _initRenderList()
            {
                _rlp = new RendererListParams(new CullingResults(), new DrawingSettings(), new FilteringSettings(RenderQueueRange.all, _pool._mask.Value));
            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
                if (_pool._depth == Pool.DepthStencil.Copy)
                    _depth.Release(cmd);
                
                if (_pool._depth == Pool.DepthStencil.Clean)
                    _depth.Release(cmd);
                
                //_output.Release(cmd);
            }
        }
        
        // =======================================================================
        public override void Create()
        {
            _bufferPasses = _list
                      .Values
                      .Select(n => new PoolPass() { _pool = n })
                      .Where(n => n._pool != null)
                      .ToList();

            if (k_ShaderTags == null)
            {
                k_ShaderTags = new List<ShaderTagId>(new[]
                {
                    new ShaderTagId("SRPDefaultUnlit"),
                    new ShaderTagId("UniversalForward"),
                    new ShaderTagId("UniversalForwardOnly")
                });
            }
            
            foreach (var pass in _bufferPasses)
                pass.Init();
            
            if (_blit == null)
                _blit = new Material(Shader.Find("Hidden/VolFx/Blit"));
        }

        private void OnDestroy()
        {
            _list.Destroy();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // in game view only (ignore inspector draw)
            if (renderingData.cameraData.cameraType != CameraType.Game)
                return;
            
            foreach (var pass in _bufferPasses)
                renderer.EnqueuePass(pass);
            foreach (var pass in s_LayersExternal)
                renderer.EnqueuePass(pass);
        }
    }
}