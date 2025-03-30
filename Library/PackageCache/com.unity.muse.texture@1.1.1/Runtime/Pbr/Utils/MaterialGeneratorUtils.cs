using Unity.Jobs;
using Unity.Muse.Common;
using UnityEngine;

namespace Unity.Muse.Texture
{
    internal static partial class MaterialGeneratorUtils
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="materialData"></param>
        /// <param name="targetMaterial"></param>
        /// <param name="convertNormalsToDxtNm">Will convert the loaded artifact for this material's bumpmap directly to DXTNm instead of relying on unity's importer. Use for runtime previews only.</param>
        public static void CreateTexturesAndMaterialForRP(ProcessedPbrMaterialData materialData,
            Material targetMaterial, bool convertNormalsToDxtNm = true, object context = null)
        {
            var baseMap = new Texture2D(2, 2, TextureFormat.RGBA32, true, false);
            baseMap.LoadImage(materialData.DiffuseMapPNGData);
            ObjectUtils.Retain(baseMap, context);
            targetMaterial.SetTexture(MuseMaterialProperties.baseMapKey, baseMap);

            var normalMap = new Texture2D(2, 2, TextureFormat.RGBA32, true, true);
            ObjectUtils.Retain(normalMap, context);
            normalMap.LoadImage(materialData.NormalMapPNGData);
            if (convertNormalsToDxtNm)
            {
                var oldNormalMap = normalMap;
                normalMap = ConvertToDxtNormalMap(oldNormalMap);
                ObjectUtils.Retain(normalMap, context);
                oldNormalMap.SafeDestroy();
            }

            targetMaterial.SetTexture(MuseMaterialProperties.normalMapKey, normalMap);

            var metallicMap = new Texture2D(2, 2, TextureFormat.RGBA32, true, true);
            ObjectUtils.Retain(metallicMap, context);
            metallicMap.LoadImage(materialData.MetallicMapPNGData);
            targetMaterial.SetTexture(MuseMaterialProperties.metallicMapKey, metallicMap);

            var smoothnessMap = new Texture2D(2, 2, TextureFormat.RGBA32, true, true);
            ObjectUtils.Retain(smoothnessMap, context);
            smoothnessMap.LoadImage(materialData.SmoothnessMapPNGData);
            targetMaterial.SetTexture(MuseMaterialProperties.smoothnessMapKey, smoothnessMap);

            var heighMap = new Texture2D(2, 2, TextureFormat.RGBA32, true, true);
            ObjectUtils.Retain(heighMap, context);
            heighMap.LoadImage(materialData.HeightmapPNGData);
            targetMaterial.SetTexture(MuseMaterialProperties.heightMapKey, heighMap);
            
            var aoMap = new Texture2D(2, 2, TextureFormat.RGBA32, true, true);
            ObjectUtils.Retain(aoMap, context);
            aoMap.LoadImage(materialData.AOMapPNGData);
            targetMaterial.SetTexture(MuseMaterialProperties.ambientOcclusionMapKey, aoMap); 
        }

        public static Shader GetDefaultShaderForPipeline()
        {
            return ResourceManager.Load<Shader>(PackageResources.museMaterialShaderGraph);
        }

        /// <summary>
        /// Converts the incoming bumpmap texture into the format expected by vanilla unity shaders using DXTNm. Only use for runtime previewing of bumpmaps.
        /// Do not save the results to disk as is, and this is a lossy one way process
        /// </summary>
        /// <param name="incoming"></param>
        /// <returns></returns>
        static Texture2D ConvertToDxtNormalMap(Texture2D incoming)
        {
            Texture2D normalTexture = new Texture2D(incoming.width, incoming.height, TextureFormat.RGBA32, true, true);
            normalTexture.filterMode = FilterMode.Trilinear;
            normalTexture.wrapMode = TextureWrapMode.Repeat;

            ConvertTextToDxtnmJob job = new ConvertTextToDxtnmJob()
            {
                IncomingPixels = incoming.GetPixelData<Color32>(0),
                OutputPixels = normalTexture.GetPixelData<Color32>(0)
            };

            var handle = job.Schedule(job.IncomingPixels.Length, 32);
            handle.Complete();

            normalTexture.SetPixelData(job.OutputPixels, 0);
            normalTexture.Apply();

            return normalTexture;
        }
        
        /// <summary>
        /// Generate an Ambient Occlusion map from a heightmap byte[]
        /// </summary>
        /// <param name="heightMap"></param>
        /// <returns></returns>
        public static Texture2D GenerateAOMap(byte[] heightMap)
        {
            var heighMap = new Texture2D(2, 2, TextureFormat.RGBA32, true, false);
            heighMap.LoadImage(heightMap);

            var returnTexture = GenerateAOMap(heighMap);
            heighMap.SafeDestroy();
            
            return returnTexture;
        } 
        
        /// <summary>
        /// Generate an Ambient Occlusion map from a heightmap byte[]
        /// </summary>
        /// <param name="heightMap"></param>
        /// <returns></returns>
        public static Texture2D GenerateAOMap(Texture2D heightMap)
        {
            var computeShader = ResourceManager.Load<ComputeShader>(PackageResources.ambientScaleShader);
            var resultBuffer = new ComputeBuffer(1, sizeof(float));

            int kernelHandle = computeShader.FindKernel("CSMain");
            computeShader.SetTexture(kernelHandle, "_InputTexture", heightMap);
            computeShader.SetBuffer(kernelHandle, "_ResultBuffer", resultBuffer);
            computeShader.SetFloat("_Width" , heightMap.width);
            computeShader.SetFloat("_Height" , heightMap.height);
            
            computeShader.Dispatch(kernelHandle, 1, 1, 1);

            float[] resultArray = new float[1];
            resultBuffer.GetData(resultArray);

            var scale = resultArray[0];
            
            var shader = ResourceManager.Load<Shader>(PackageResources.ambientOcclusionShader);
            var blitMaterial = new Material(shader);
            
            blitMaterial.SetFloat("_DispScale", scale);
            
            var heightmapRT = RenderTexture.GetTemporary(heightMap.width, heightMap.height, 0, RenderTextureFormat.R16, RenderTextureReadWrite.Linear);
            Graphics.Blit(heightMap, heightmapRT);
            
            var destRT = RenderTexture.GetTemporary(heightMap.width, heightMap.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Graphics.Blit(heightmapRT, destRT, blitMaterial);

            var activeRT = RenderTexture.active;
            RenderTexture.active = destRT;
            
            var aoMap = new Texture2D(heightMap.width, heightMap.height, TextureFormat.RGBA32, true, true);
            aoMap.ReadPixels(new Rect(0, 0, aoMap.width, aoMap.height), 0, 0);
            aoMap.Apply();
            
            RenderTexture.active = activeRT;
            RenderTexture.ReleaseTemporary(destRT);

            return aoMap;
        }
    }
}