using System;
using Unity.Muse.Common;
using Unity.Muse.Texture.Pbr.Cache;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Muse.Texture
{
    [Serializable]
    internal class PbrMaterialData
    {
        [SerializeField]
        public ImageArtifact BaseMapSourceArtifact;
        [SerializeField]
        public ImageArtifact NormalMapSourceArtifact;
        [SerializeField]
        public ImageArtifact MetallicMapSourceArtifact;
        [FormerlySerializedAs("RoughnessMapSourceArtifact")] [SerializeField]
        public ImageArtifact SmoothnessMapSourceArtifact;
        [SerializeField]
        public ImageArtifact HeightmapSourceArtifact;
        [SerializeField]
        public ImageArtifact DiffuseMapSourceArtifact;

        public PbrMaterialData(){}
        public PbrMaterialData(ProcessedPbrMaterialData processedData)
        {
            BaseMapSourceArtifact = processedData.BaseMap;
            NormalMapSourceArtifact = processedData.NormalMap;
            MetallicMapSourceArtifact = processedData.MetallicMap;
            SmoothnessMapSourceArtifact = processedData.SmoothnessMap;
            HeightmapSourceArtifact = processedData.HeightmapMap;
        }
    }

    internal struct ProcessedPbrMaterialData
    {
        public static readonly ProcessedPbrMaterialData k_FailedData = new ProcessedPbrMaterialData()  {
                                                                                            BaseMap = null,
                                                                                            BaseMapPNGData = null,
                                                                                            NormalMap = null,
                                                                                            NormalMapPNGData = null,
                                                                                            MetallicMap = null,
                                                                                            MetallicMapPNGData = null,
                                                                                            SmoothnessMap = null,
                                                                                            SmoothnessMapPNGData = null,
                                                                                            HeightmapMap = null,
                                                                                            HeightmapPNGData = null,
                                                                                            AOMapPNGData = null,
                                                                                            DiffuseMap = null,
                                                                                            DiffuseMapPNGData = null
                                                                                        };

        public ImageArtifact BaseMap;
        public byte[] BaseMapPNGData;
        public ImageArtifact DiffuseMap;
        public byte[] DiffuseMapPNGData;
        public ImageArtifact NormalMap;
        public byte[] NormalMapPNGData;
        public ImageArtifact MetallicMap;
        public byte[] MetallicMapPNGData;
        [FormerlySerializedAs("RoughnessMap")] public ImageArtifact SmoothnessMap;
        [FormerlySerializedAs("RoughnessMapPNGData")] public byte[] SmoothnessMapPNGData;
        public ImageArtifact HeightmapMap;
        public byte[] HeightmapPNGData;

        public byte[] AOMapPNGData
        {
            get
            {
                if (HeightmapPNGData == null)
                    return null;

                if (m_AOMapPNGData != null) return m_AOMapPNGData;

                var cached = AOCache.FindOne(BaseMap.Guid);
                
                if(cached != null)
                {
                    m_AOMapPNGData = cached.AOMapPNGData;
                    return m_AOMapPNGData;
                }

                var texture = MaterialGeneratorUtils.GenerateAOMap(HeightmapPNGData);
                m_AOMapPNGData = texture.EncodeToPNG();
                
                AOCache.Write(BaseMap, m_AOMapPNGData);
                texture.SafeDestroy();

                return m_AOMapPNGData;
            }
            set => m_AOMapPNGData = value;
        }

        private byte[] m_AOMapPNGData;
    }
}
