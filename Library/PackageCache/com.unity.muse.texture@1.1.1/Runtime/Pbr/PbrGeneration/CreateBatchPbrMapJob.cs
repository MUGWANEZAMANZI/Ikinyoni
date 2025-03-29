using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.Common;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity.Muse.Texture
{
    internal class CreateBatchPbrMapJob
    {
        public bool IsDone { get; private set; }
        public bool IsCancelled { get; private set; }
        public bool Success { get; private set; }
        public string Error { get; private set; }

        public Dictionary<PbrMapTypes, byte[]> MapsRawData { get; }

        public ImageArtifact SourceArtifact { get; }

        public event Action<CreateBatchPbrMapJob> OnGenerationCompleted;

        public Dictionary<PbrMapTypes, ImageArtifact> MapTypes { get; }
        private Dictionary<PbrMapTypes, ImageArtifact> RequestMapTypes { get; set;  }

        UnityWebRequestAsyncOperation m_MapGenerationRequest;

        bool m_Started;

        // Easily the best piece of code ever created. Nevertheless, should get refactored with async or something more elegant.
        int m_CompletionCount;
        void Completions(int count)
        {
            m_CompletionCount += count;
            if (m_CompletionCount <= 0)
                OnGenerationCompleted?.Invoke(this);
        }

        public CreateBatchPbrMapJob(ImageArtifact sourceArtifact, Dictionary<PbrMapTypes, ImageArtifact> mapTypes)
        {
            m_CompletionCount = 0;
            SourceArtifact = sourceArtifact;
            MapTypes = mapTypes;
            MapsRawData = new Dictionary<PbrMapTypes, byte[]>();
            ResetMaps();
        }

        void ResetMaps()
        {
            foreach (var mapType in MapTypes.Keys)
                MapsRawData[mapType] = Array.Empty<byte>();
        }
        public void Start()
        {
            if (m_Started)
            {
                Debug.LogWarning("Attempted to start PBR map generation job while already in progress. Ignoring.");
                return;
            }

            if (IsDone)
            {
                Debug.LogWarning("Attempted ot start a completed job. Ignoring.");
                return;
            }

            m_Started = true;

            // No artifact specified, generate and download a new map
            Completions(MapTypes.Count);

            RequestMapTypes = MapTypes.Where(mapType => !mapType.Value.IsValid()).ToDictionary(t => t.Key, t => t.Value);
            if (RequestMapTypes.Any())
            {
                m_MapGenerationRequest = MuseTextureBackend.GenerateBatchPbrMap(SourceArtifact, RequestMapTypes.Keys.ToArray(), OnPbrMapGenerationDone);
            }

            var otherArtifacts = MapTypes.Where(mapType => mapType.Value.IsValid()).ToDictionary(t => t.Key, t => t.Value);

            foreach (var mapType in otherArtifacts)
            {
                var artifact = mapType.Value;
                //Attempt to grab from cache or download, this map was already generated once
                if (artifact.IsCached)
                {
                    //Get from cache
                    // TODO: Cloudlab
                    //GeneratedArtifactCache.TryGetCachedArtifactRaw(GeneratedArtifact, out var data);

                    IsDone = true;
                    Success = true;

                    m_Started = false;
                    MapsRawData[mapType.Key] = ArtifactCache.ReadRawData(artifact);
                    Completions(-1);
                }
                else
                {
                    //Download instead
                    artifact.GetArtifact((_, rawData, message) =>
                    {
                        IsDone = true;
                        if (!string.IsNullOrEmpty(message))
                        {
                            Error = message;
                        }
                        else
                        {
                            Success = true;
                        }

                        m_Started = false;
                        MapsRawData[mapType.Key] = rawData;
                        Completions(-1);
                    }, false);
                }
            }
        }

        void OnPbrMapGenerationDone(BatchPbrResponse response, string s)
        {
            if (!string.IsNullOrEmpty(s) || !response.success)
            {
                Error = s + $" {response?.error ?? s}";
                IsDone = true;
                Success = false;
                m_Started = false;
                ResetMaps();
                OnGenerationCompleted?.Invoke(this);
            }
            else
            {
                foreach (var mapTypeItem in RequestMapTypes.ToArray())
                {
                    var guid = MuseTextureBackend.GetPBRMapGuid(response.pbrs, mapTypeItem.Key);
                    if (string.IsNullOrEmpty(guid))
                    {
                        Debug.LogWarning($"Texture could not be retrieved for {mapTypeItem.Key}.");
                        continue;
                    }

                    MapTypes[mapTypeItem.Key] = new ImageArtifact(guid, uint.MinValue);
                    MapTypes[mapTypeItem.Key].GetArtifact((_, rawData, message) =>
                    {
                        try
                        {

                            IsDone = true;
                            if (!string.IsNullOrEmpty(message))
                            {
                                Error = message;
                            }
                            else
                            {
                                Success = true;
                            }

                            m_Started = false;
                            MapsRawData[mapTypeItem.Key] = rawData;
                        }
                        finally
                        {
                            Completions(-1);
                        }
                    }, false);
                }
            }
        }

        public void Cancel()
        {
            if (m_Started)
            {
                m_MapGenerationRequest?.webRequest.Abort();
                IsCancelled = true;
            }
        }
    }
}
