using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Muse.Common;
using Unity.Muse.Texture.Pbr.Cache;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity.Muse.Texture
{
    internal class PbrFactory
    {
        public event Action<Material, ProcessedPbrMaterialData> OnMaterialCreated;
        public event Action<float> OnProgress;

        PbrMapGeneratorJob m_CurrentMapGenerationJob;
        UnityWebRequestAsyncOperation m_CurrentDiffuseGenerationRequest;

        readonly List<PbrMaterialData> m_PbrMaterialData;

        ProcessedPbrMaterialData m_CurrentProcessedData;

        public PbrFactory(List<PbrMaterialData> viewModel)
        {
            m_PbrMaterialData = viewModel;
        }

        public void StartPbrMapCreation(ImageArtifact forArtifact)
        {
            if (m_CurrentMapGenerationJob != null && m_CurrentMapGenerationJob.BaseMapArtifact == forArtifact)
            {
                return;
            }

            CancelCurrentGeneration();

            var data = m_PbrMaterialData.Find(x => x.BaseMapSourceArtifact.Guid == forArtifact.Guid);

            if (data == null)
            {
                data = new PbrMaterialData
                {
                    BaseMapSourceArtifact = forArtifact
                };

                m_PbrMaterialData.Add(data);
            }

            m_CurrentMapGenerationJob = new PbrMapGeneratorJob(forArtifact,
                data.HeightmapSourceArtifact,
                data.NormalMapSourceArtifact,
                data.MetallicMapSourceArtifact,
                data.SmoothnessMapSourceArtifact);


            OnProgress?.Invoke(0f);
            m_CurrentMapGenerationJob.Completed += OnPBRMapRequestCompleted;
            m_CurrentMapGenerationJob.ProgressUpdated += OnPBRJobProgressChanged;

            // We need to wait for the image generation to complete in the back end if it wasn't already.
            // So Only start the job once we confirmed the source artifact is available
            forArtifact.GetArtifact((_, _, error) =>
            {
                if (string.IsNullOrEmpty(error))
                {
                    OnImageGenerationConfirmed(forArtifact);
                }
                else
                {
                    Debug.LogError($"Failed to start PBR job because the base map generation failed: {error}");
                }
            }, true);
        }

        void OnImageGenerationConfirmed(Artifact artifact)
        {
            if (m_CurrentMapGenerationJob is { IsRunning: false }
                && artifact == m_CurrentMapGenerationJob.BaseMapArtifact)
            {
                m_CurrentMapGenerationJob.Start();
                m_CurrentDiffuseGenerationRequest =
                    MuseTextureBackend.GenerateBatchPbrMap(m_CurrentMapGenerationJob.BaseMapArtifact,
                        new [] {PbrMapTypes.Delighted},
                        OnDiffuseRequestCompleted);
            }
        }

        void OnPBRMapRequestCompleted(bool success, ProcessedPbrMaterialData materialData)
        {
            if (!success)
            {
                CancelCurrentGeneration();
                OnProgress?.Invoke(100f);
                OnMaterialCreated?.Invoke(null, materialData);
                return;
            }

            var currentMaterialData =
                m_PbrMaterialData.First(data => data.BaseMapSourceArtifact.Guid == materialData.BaseMap.Guid);

            currentMaterialData.MetallicMapSourceArtifact = materialData.MetallicMap;
            currentMaterialData.HeightmapSourceArtifact = materialData.HeightmapMap;
            currentMaterialData.NormalMapSourceArtifact = materialData.NormalMap;
            currentMaterialData.SmoothnessMapSourceArtifact = materialData.SmoothnessMap;
            m_CurrentProcessedData.BaseMap = materialData.BaseMap;
            m_CurrentProcessedData.BaseMapPNGData = materialData.BaseMapPNGData;
            m_CurrentProcessedData.NormalMap = materialData.NormalMap;
            m_CurrentProcessedData.NormalMapPNGData = materialData.NormalMapPNGData;
            m_CurrentProcessedData.MetallicMap = materialData.MetallicMap;
            m_CurrentProcessedData.MetallicMapPNGData = materialData.MetallicMapPNGData;
            m_CurrentProcessedData.SmoothnessMap = materialData.SmoothnessMap;
            m_CurrentProcessedData.SmoothnessMapPNGData = materialData.SmoothnessMapPNGData;
            m_CurrentProcessedData.HeightmapMap = materialData.HeightmapMap;
            m_CurrentProcessedData.HeightmapPNGData = materialData.HeightmapPNGData;

            EvaluateJobsCompleteness();
        }

        void OnDiffuseRequestCompleted(BatchPbrResponse response, string error)
        {
            if (response == null || !string.IsNullOrEmpty(error))
            {
                CancelCurrentGeneration();
                OnProgress?.Invoke(100f);
                OnMaterialCreated?.Invoke(null, new ProcessedPbrMaterialData());
                return;
            }

            var currentMaterialData =
                m_PbrMaterialData.First(data => data.BaseMapSourceArtifact.Guid == m_CurrentMapGenerationJob.BaseMapArtifact.Guid);

            currentMaterialData.DiffuseMapSourceArtifact = new ImageArtifact(response.pbrs.delighted, uint.MinValue);
            m_CurrentProcessedData.DiffuseMap = currentMaterialData.DiffuseMapSourceArtifact;

            m_CurrentProcessedData.DiffuseMap.GetArtifact((_, rawData, message) =>
            {
                if (!string.IsNullOrEmpty(message))
                {
                    CancelCurrentGeneration();
                    OnProgress?.Invoke(100f);
                    OnMaterialCreated?.Invoke(null, new ProcessedPbrMaterialData());
                    return;
                }

                m_CurrentProcessedData.DiffuseMapPNGData = rawData;
                EvaluateJobsCompleteness();


            }, false);
        }

        void OnPBRJobProgressChanged(float obj)
        {
            OnProgress?.Invoke(obj * 100f);
        }

        public void CancelCurrentGeneration()
        {
            m_CurrentProcessedData = new ProcessedPbrMaterialData();

            if (m_CurrentDiffuseGenerationRequest != null)
            {
                if (!m_CurrentDiffuseGenerationRequest.isDone)
                {
                    m_CurrentDiffuseGenerationRequest?.webRequest?.Abort();
                    m_CurrentDiffuseGenerationRequest?.webRequest?.Dispose();
                }

                m_CurrentDiffuseGenerationRequest = null;
            }

            if (m_CurrentMapGenerationJob != null)
            {
                m_CurrentMapGenerationJob.Completed -= OnPBRMapRequestCompleted;
                m_CurrentMapGenerationJob.Cancel();
                m_CurrentMapGenerationJob = null;
            }
        }

        public void EvaluateJobsCompleteness()
        {
           if(m_CurrentDiffuseGenerationRequest == null || !m_CurrentDiffuseGenerationRequest.isDone || m_CurrentMapGenerationJob.IsRunning)
               return;

           PbrDataCache.Write(m_CurrentProcessedData);

           var newMaterial = new Material(MaterialGeneratorUtils.GetDefaultShaderForPipeline());

           MaterialGeneratorUtils.CreateTexturesAndMaterialForRP(m_CurrentProcessedData, newMaterial, false);

           OnMaterialCreated?.Invoke(newMaterial, m_CurrentProcessedData);
           CancelCurrentGeneration();
        }
    }
}