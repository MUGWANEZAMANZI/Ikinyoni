using Unity.Muse.Common;
using Unity.Muse.Common.Tools;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.Muse.Texture.Tools
{
    internal static class ToolFactoryRegister
    {
#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        [Preserve]
        public static void RegisterAvailableTools()
        {
            AvailableToolsFactory.RegisterTool<TextureBrushTool>(UIMode.modeKey);
            AvailableToolsFactory.RegisterTool<PanTool>(UIMode.modeKey);
        }
    }
}
