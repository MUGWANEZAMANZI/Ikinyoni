using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Muse.Common;
using UnityEngine.Networking;

namespace Unity.Muse.Texture
{
    abstract class MuseTextureBackend: GenerativeAIBackend
    {

        /// <summary>
        /// Initiate Text to Texture generation on Cloud. It only allocates texture ids and actual generation occurs in background.
        /// Use `RequestStatus` to query progress and `DownloadImage` to download intermediate or final result.
        /// </summary>
        /// <param name="prompt">Text prompt for texture generation</param>
        /// <param name="settings">Additional request settings</param>
        /// <param name="onDone">Callback called when results are received. Callback parameters (TextToImageResponse, string).
        /// <returns>The the reference to the async operation this generates so that it may be cancelled</returns>
        /// In case error occured error string is non-null and other parameters are null</param>
        public static UnityWebRequestAsyncOperation GenerateImage(string prompt,
            TextToImageRequest settings,
            Action<TextToImageResponse, string> onDone)
        {
            return SendJsonRequest(TexturesUrl.generate, new TextToImageItemRequest(prompt, settings),
                RequestHandler<TextToImageResponse>((data, error) =>
                {
                    if (data != null)
                        data.seed = settings.seed;
                    onDone?.Invoke(data, error);
                }));
        }

        /// <summary>
        /// Takes an image artifact and requests a PBR map to be generated from it. Must have one call made for each map type you wish to extract.
        /// </summary>
        /// <param name="fromImage">The image artifact to send for PBR map extraction. When requesting a normal map, the heightmap must be used and not the source image artifact</param>
        /// <param name="mapTypes"></param>
        /// <param name="onDone"></param>
        /// <returns>The the reference to the async operation this generates so that it may be cancelled</returns>
        internal static UnityWebRequestAsyncOperation GenerateBatchPbrMap(ImageArtifact fromImage, PbrMapTypes[] mapTypes, Action<BatchPbrResponse, string> onDone)
        {
            var backendMapTypeNames = mapTypes.Select(GetMapTypeName).ToArray();
            var request = new GenerateBatchPbrMapRequest(fromImage.Guid, backendMapTypeNames);
            return SendJsonRequest(TexturesUrl.pbr, request, RequestHandler(onDone));
        }

        internal static string GetMapTypeName(PbrMapTypes mapType) =>
            mapType switch
            {
                PbrMapTypes.BaseMap => "emission",
                PbrMapTypes.Emission => "emission",
                PbrMapTypes.Height => "height",
                PbrMapTypes.Normal => "normal",
                PbrMapTypes.Metallic => "metallic",
                PbrMapTypes.Smoothness => "roughness",
                PbrMapTypes.AO => "ao",
                PbrMapTypes.Delighted => "delighted",
                _ => string.Empty
            };

        internal static string GetPBRMapGuid(PbrMapGuids guids, PbrMapTypes mapType) =>
            mapType switch
            {
                PbrMapTypes.BaseMap => guids.emission,
                PbrMapTypes.Emission => guids.emission,
                PbrMapTypes.Height => guids.height,
                PbrMapTypes.Normal => guids.normal,
                PbrMapTypes.Metallic => guids.metallic,
                PbrMapTypes.Smoothness => guids.roughness,
                PbrMapTypes.AO => guids.ao,
                PbrMapTypes.Delighted => guids.delighted,
                _ => string.Empty
            };

        /// <summary>
        /// Request image upscale
        /// </summary>
        /// <param name="artifact">Previously generated texture artifact</param>
        /// <param name="onDone">Callback called when results are received. Callback parameters (UpscaleImageResponse, string).
        /// <returns>The the reference to the async operation this generates so that it may be cancelled</returns>
        /// UpscaleImageResponse contains newly generated image guid, you should track it's progress via RequestStatus call.
        /// In case error occured error string is non-null and other parameters are null</param>
        public static UnityWebRequestAsyncOperation UpscaleImage(Artifact artifact, Action<UpscaleImageResponse, string> onDone)
        {
            return SendJsonRequest(TexturesUrl.upscale, new GuidItemRequest(artifact.Guid), RequestHandler(onDone));
        }

        /// <summary>
        /// Initiate Image variation generation on Cloud. It only allocates texture ids and actual generation occurs in background.
        /// Use `RequestStatus` to query progress and `DownloadImage` to download intermediate or final result.
        /// </summary>
        public static UnityWebRequestAsyncOperation VariateImage(
            string sourceGuid,
            string imageB64,
            string prompt,
            ImageVariationSettingsRequest settings,
            Action<TextToImageResponse, string> onDone)
        {
            object request;

            if (string.IsNullOrEmpty(sourceGuid))
                request = new ImageVariationBase64Request(imageB64, prompt, settings);
            else
                request = new ImageVariationRequest(sourceGuid, prompt, settings);

            return SendJsonRequest(TexturesUrl.variate, request,
                RequestHandler<TextToImageResponse>((data, error) =>
                {
                    if (data != null)
                        data.seed = settings.seed;
                    onDone?.Invoke(data, error);
                }));
        }

        public static UnityWebRequestAsyncOperation ControlNetGenerate(
            string sourceGuid,
            string sourceBase64,
            string prompt,
            string controlColor,
            ImageVariationSettingsRequest settings,
            Action<TextToImageResponse, string> onDone)
        {
            return SendJsonRequest(TexturesUrl.generate, new ControlNetGenerateRequest(sourceGuid, sourceBase64, prompt, controlColor, settings),
                RequestHandler<TextToImageResponse>((data, error) =>
                {
                    if (data != null)
                        data.seed = settings.seed;
                    onDone?.Invoke(data, error);
                }));
        }

        public static UnityWebRequestAsyncOperation GenerateInpainting(string prompt,
                                        string sourceGuid,
                                        Texture2D mask,
                                        MaskType maskType,
                                        TextToImageRequest settings,
                                        Action<TextToImageResponse, string> onDone)
        {
            return SendJsonRequest(TexturesUrl.inpaint, new InpaintingItemRequest(prompt, sourceGuid, mask, maskType, settings),
                RequestHandler<TextToImageResponse>((data, error) =>
                {
                    if (data != null)
                        data.seed = settings.seed;
                    onDone?.Invoke(data, error);
                }));
        }

        private static List<string> m_ActiveGuids = new List<string>();
        private static bool m_IsCheckingStatus = false;
        private static int m_PollIntervalInMillsec = 1000;
        public static event Action<string, string> OnStatusChange;

        public static void AddGuidToCheckStatusPeriodically(string guid)
        {
            m_ActiveGuids.Add(guid);
            if (!m_IsCheckingStatus)
            {
                CheckStatusPeriodically();
            }
        }

        private static async void CheckStatusPeriodically()
        {
            string serviceURL = null;
            ItemRequest itemData = null;
            m_IsCheckingStatus = true;
            while (m_ActiveGuids.Count > 0)
            {
                if (itemData == null)
                {
                    itemData = new ItemRequest();
                    serviceURL = GetBaseJobsUrl(TexturesUrl.textures, m_ActiveGuids, itemData);
                }

                bool anyDone = await CheckArtifactsStatusAsync(serviceURL, m_ActiveGuids, itemData, OnStatusChange);
                if (anyDone)
                    itemData = null;

                if (m_ActiveGuids.Any())
                    await Task.Delay(m_PollIntervalInMillsec); // Convert seconds to milliseconds
            }
            m_IsCheckingStatus = false;
        }
    }
}
