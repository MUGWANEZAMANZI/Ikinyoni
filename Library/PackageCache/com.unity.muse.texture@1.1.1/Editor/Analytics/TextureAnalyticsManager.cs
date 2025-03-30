using Unity.Muse.Common.Analytics;
using Unity.Muse.Texture.Analytics;
using UnityEditor;
#if ENABLE_UNITYENGINE_ANALITICS
using UnityEngine.Analytics;
#endif

namespace Unity.Muse.Texture.Editor.Analytics
{
    static class TextureAnalyticsManager
    {
        [InitializeOnLoadMethod]
        static void Init()
        {
#if !ENABLE_UNITYENGINE_ANALITICS
            // so the correct package is recorded as having sent the event in Unity <6
            EditorAnalytics.RegisterEventWithLimit(SaveTextureData.eventName, AnalyticsManager.maxEventsPerHour, 6, AnalyticsManager.vendorKey);
            EditorAnalytics.RegisterEventWithLimit(GenerateAnalyticsData.eventName, AnalyticsManager.maxEventsPerHour, 6, AnalyticsManager.vendorKey);
#endif

            void SendAnalytic(IAnalytic analytic)
            {
#if ENABLE_UNITYENGINE_ANALITICS
                EditorAnalytics.SendAnalytic(analytic);
#else
                analytic.TryGatherData(out var data, out _);
                var result = EditorAnalytics.SendEventWithLimit(data.EventName, data, data.Version);
#endif
            }

            // so the correct package is recorded as having sent the event in Unity 6+
            AnalyticsManager.RegisterEvent<SaveTextureAnalytic>(SendAnalytic);
            AnalyticsManager.RegisterEvent<GenerateAnalytic>(SendAnalytic);
        }
    }
}
