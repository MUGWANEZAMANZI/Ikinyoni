using System;
using UnityEngine.Rendering;
#if USING_URP
using UnityEngine.Rendering.Universal;
#endif
#if USING_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace Unity.Muse.Texture
{
    static class RenderPipelineUtils
    {
#if USING_HDRP
        public static bool IsUsingHdrp() => GraphicsSettings.currentRenderPipeline is HDRenderPipelineAsset;
#else
        public static bool IsUsingHdrp() => false;
#endif
#if USING_URP
        public static bool IsUsingUrp() => GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset;
#else
        public static bool IsUsingUrp() => false;
#endif
    }
}
