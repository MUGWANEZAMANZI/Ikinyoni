using System;
using Unity.Muse.Common.Analytics;
#if ENABLE_UNITYENGINE_ANALITICS
using UnityEngine.Analytics;
#endif

namespace Unity.Muse.Texture.Editor.Analytics
{
    [Serializable]
    class SaveTextureData : IAnalytic.IData
    {
        public const string eventName = "muse_textureTool_save";
        public const int version = 1;
#if !ENABLE_UNITYENGINE_ANALITICS
        public string EventName => eventName;
        public int Version => version;
#endif
        public bool is_pbr_material;
        public string material_hash;
    }

#if ENABLE_UNITYENGINE_ANALITICS
    [AnalyticInfo(eventName: SaveTextureData.eventName, vendorKey: AnalyticsManager.vendorKey, SaveTextureData.version, AnalyticsManager.maxEventsPerHour)]
#endif
    class SaveTextureAnalytic : IAnalytic
    {
        readonly bool m_IsPbrMaterial;
        readonly string m_MaterialHash;

        internal SaveTextureAnalytic(bool isPbrMaterial, string materialHash)
        {
            m_IsPbrMaterial = isPbrMaterial;
            m_MaterialHash = materialHash;
        }

        public bool TryGatherData(out IAnalytic.IData data, out Exception error)
        {
            error = null;
            var parameters = new SaveTextureData
            {
                is_pbr_material = m_IsPbrMaterial,
                material_hash = m_MaterialHash
            };
            data = parameters;
            return data != null;
        }
    }
}
