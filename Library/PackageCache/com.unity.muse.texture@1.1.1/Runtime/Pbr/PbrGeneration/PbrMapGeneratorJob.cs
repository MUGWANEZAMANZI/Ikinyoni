using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.Common;
using Unity.Muse.Texture.Pbr.Cache;
using UnityEngine;
using UnityEngine.Pool;

namespace Unity.Muse.Texture
{
    internal class PbrMapGeneratorJob : IDisposable
    {
        public delegate void MaterialGenerationCompleteDelegate(bool success, ProcessedPbrMaterialData materialData);

        public event MaterialGenerationCompleteDelegate Completed;

        public event Action<float> ProgressUpdated;

        public float Progress { get; private set; }

        public bool IsRunning { get; private set; }
        public bool IsCancelled { get; private set; }
        public ImageArtifact BaseMapArtifact => m_BaseMapSourceArtifact;

        static readonly PbrMapTypes[] k_MapTypesToGenerate = {
            PbrMapTypes.Height,
            PbrMapTypes.Metallic,
            PbrMapTypes.Normal,
            PbrMapTypes.BaseMap,
            PbrMapTypes.Smoothness,
        };

        readonly ImageArtifact m_BaseMapSourceArtifact;
        readonly ImageArtifact m_HeightmapSourceArtifact;
        readonly ImageArtifact m_NormalMapSourceArtifact;
        readonly ImageArtifact m_MetallicSourceArtifact;
        readonly ImageArtifact m_SmoothnessSourceArtifact;

        List<CreateBatchPbrMapJob> m_BatchJobs;

        bool m_Disposed;
        bool m_IsDisposing;

        public PbrMapGeneratorJob(ImageArtifact baseMapSourceArtifact,
                                ImageArtifact heightmapSourceArtifact,
                                ImageArtifact normalMapSourceArtifact,
                                ImageArtifact metallicSourceArtifact,
                                ImageArtifact smoothnessSourceArtifact)
        {
            m_BatchJobs = ListPool<CreateBatchPbrMapJob>.Get();
            m_BaseMapSourceArtifact = baseMapSourceArtifact;
            m_HeightmapSourceArtifact = heightmapSourceArtifact;
            m_NormalMapSourceArtifact = normalMapSourceArtifact;
            m_MetallicSourceArtifact = metallicSourceArtifact;
            m_SmoothnessSourceArtifact = smoothnessSourceArtifact;
        }

        public void Start()
        {
            if (IsRunning || IsCancelled) return;

            IsRunning = true;

            // TODO: This needs to be a batch/one shot job on the BE
            var mapTypes = new Dictionary<PbrMapTypes, ImageArtifact>
            {
                {PbrMapTypes.BaseMap, m_BaseMapSourceArtifact},
                {PbrMapTypes.Height, m_HeightmapSourceArtifact},
                {PbrMapTypes.Metallic, m_MetallicSourceArtifact},
                {PbrMapTypes.Smoothness, m_SmoothnessSourceArtifact}
            };

            var batchJob = CreateBatchGenerateJobForMap(m_BaseMapSourceArtifact, mapTypes);

            // Heightmap is used as source for the normal map, so use its completion to drive normal map generation
            batchJob.OnGenerationCompleted += OnHeightmapGenerationCompleted;
            batchJob.Start();
        }

        public void Cancel()
        {
            if (!IsRunning || IsCancelled || m_Disposed) return;

            IsRunning = false;
            foreach (var job in m_BatchJobs ?? Enumerable.Empty<CreateBatchPbrMapJob>())
            {
                job.OnGenerationCompleted -= OnBatchPBRMapGenerationCompleted;
                job.OnGenerationCompleted -= OnHeightmapGenerationCompleted;
                job.Cancel();
            }

            IsCancelled = true;
            Completed?.Invoke(false, ProcessedPbrMaterialData.k_FailedData);
            Dispose();
        }

        public void Dispose()
        {
            if (m_Disposed || m_IsDisposing) return;

            m_IsDisposing = true;

            if (IsRunning)
            {
                Cancel();
            }

            if (m_BatchJobs != null)
            {
                ListPool<CreateBatchPbrMapJob>.Release(m_BatchJobs);
                m_BatchJobs = null;
            }

            ProgressUpdated = null;
            Completed = null;
            m_Disposed = true;
        }

        CreateBatchPbrMapJob CreateBatchGenerateJobForMap(ImageArtifact sourceArtifact,
                                                    Dictionary<PbrMapTypes, ImageArtifact> mapTypes)
        {
            if (IsCancelled)
            {
                return null;
            }

            var batchJob = new CreateBatchPbrMapJob(sourceArtifact, mapTypes);
            m_BatchJobs.Add(batchJob);
            batchJob.OnGenerationCompleted += OnBatchPBRMapGenerationCompleted;

            return batchJob;
        }

        void OnAllJobsCompleted()
        {
            if (IsCancelled)
            {
                Completed?.Invoke(false, ProcessedPbrMaterialData.k_FailedData);
                return;
            }

            var allSucceeded = true;
            foreach (var job in m_BatchJobs)
            {
                allSucceeded &= job.Success;
            }

            IsRunning = false;

            var artifacts = new Dictionary<PbrMapTypes, ImageArtifact>();
            var rawArtifacts = new Dictionary<PbrMapTypes, byte[]>();
            foreach (var job in m_BatchJobs)
            {
                foreach (var mapType in job.MapTypes)
                {
                    artifacts[mapType.Key] = mapType.Value;
                    rawArtifacts[mapType.Key] = job.MapsRawData[mapType.Key];
                }
            }

            var processedData = new ProcessedPbrMaterialData
            {
                BaseMap = artifacts[PbrMapTypes.BaseMap],
                BaseMapPNGData = rawArtifacts[PbrMapTypes.BaseMap],
                NormalMap = artifacts[PbrMapTypes.Normal],
                NormalMapPNGData = rawArtifacts[PbrMapTypes.Normal],
                MetallicMap = artifacts[PbrMapTypes.Metallic],
                MetallicMapPNGData = rawArtifacts[PbrMapTypes.Metallic],
                SmoothnessMap = artifacts[PbrMapTypes.Smoothness],
                SmoothnessMapPNGData = rawArtifacts[PbrMapTypes.Smoothness],
                HeightmapMap = artifacts[PbrMapTypes.Height],
                HeightmapPNGData = rawArtifacts[PbrMapTypes.Height]
            };

            Completed?.Invoke(allSucceeded, processedData);

            Dispose();
        }

        void OnBatchPBRMapGenerationCompleted(CreateBatchPbrMapJob job)
        {
            if (!string.IsNullOrEmpty(job.Error))
            {
                Debug.LogError($"Batch PBR Map job failed ({string.Join(", ", job.MapTypes.Keys)}): {job.Error} (Source GUID: '{job.SourceArtifact.Guid}')");
            }
            EvaluateGenerationCompleteness();
        }

        void OnHeightmapGenerationCompleted(CreateBatchPbrMapJob job)
        {
            if (!job.Success)
            {
                Debug.LogError($"Failed to generated heightmap. Normal map skipped: {job.Error}");
                Cancel();
            }
            else
            {
                var mapTypes = new Dictionary<PbrMapTypes, ImageArtifact>
                {
                    {PbrMapTypes.Normal, m_NormalMapSourceArtifact}
                };
                var heightJob = CreateBatchGenerateJobForMap(job.MapTypes[PbrMapTypes.Height], mapTypes);
                heightJob.Start();
            }

            EvaluateGenerationCompleteness();
        }

        void EvaluateGenerationCompleteness()
        {
            UpdateProgress();
            if (IsCancelled)
            {
                if (IsRunning)
                {
                    OnAllJobsCompleted();
                }
                return;
            }

            if (IsCompleted)
            {
                var allCompleted = true;
                foreach (var job in m_BatchJobs)
                {
                    allCompleted &= job.IsDone;
                }

                if (allCompleted)
                {
                    OnAllJobsCompleted();
                }
            }
        }

        bool IsCompleted => Progress + float.Epsilon >= 1;

        void UpdateProgress()
        {
            var currProgress = Progress;

            var jobsProgress = 0f;

            if (!IsCancelled)
            {
                foreach (var job in m_BatchJobs ?? Enumerable.Empty<CreateBatchPbrMapJob>())
                {
                    jobsProgress += job.IsDone ? job.MapsRawData.Count : 0f;
                }
                jobsProgress /= k_MapTypesToGenerate.Length;
            }
            else
            {
                jobsProgress = 1f;
            }

            if (Math.Abs(currProgress - jobsProgress) > float.Epsilon)
            {
                Progress = jobsProgress;
                ProgressUpdated?.Invoke(jobsProgress);
            }
        }
    }
}
