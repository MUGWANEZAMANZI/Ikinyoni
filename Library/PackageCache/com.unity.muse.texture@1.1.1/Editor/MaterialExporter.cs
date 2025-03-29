using System;
using System.IO;
using System.Linq;
using Unity.Muse.Common;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Texture.Editor
{
    internal class MaterialExporter
    {
        [InitializeOnLoadMethod]
        static void MaterialExportRegister()
        {
            ExportHandler.OnExportMaterialWithPrompt += OnExportMaterialWithPrompt;
            ExportHandler.OnExportMaterial += ExportMaterial;
        }

        static void OnExportMaterialWithPrompt(Artifact baseArtifact, ProcessedPbrMaterialData materialData, Action<string, Artifact> onMapExported = null)
        {
            var fileName = GetMaterialName(baseArtifact); 
            
            var path = EditorUtility.SaveFilePanelInProject("Save material", fileName, "mat", "");
            if (string.IsNullOrEmpty(path)) return;

            ExportMaterial(baseArtifact, materialData, path, onMapExported);
        }


        internal static void ExportMaterial(Artifact baseArtifact, ProcessedPbrMaterialData materialData, string path)
        {
            ExportMaterial(baseArtifact, materialData, path, null);
        }

        internal static void ExportMaterial(Artifact baseArtifact, ProcessedPbrMaterialData materialData, string path, Action<string, Artifact> onMapExported = null)
        {
            var material = new Material(MaterialGeneratorUtils.GetDefaultShaderForPipeline());

            var materialAsset = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (materialAsset == null)
            {
                materialAsset = new Material(material);
                AssetDatabase.CreateAsset(materialAsset, path);
            }

            var baseMap = SaveAndLoadTexture2D("rawColorMap", materialData.BaseMapPNGData, materialAsset, false, true);
            onMapExported?.Invoke(baseMap.Item2, baseArtifact);

            var diffuseMap = SaveAndLoadTexture2D("albedoMap", materialData.DiffuseMapPNGData, materialAsset);
            onMapExported?.Invoke(diffuseMap.Item2, baseArtifact);

            var normalMap = SaveAndLoadTexture2D("normalMap", materialData.NormalMapPNGData, materialAsset, true);
            onMapExported?.Invoke(normalMap.Item2, baseArtifact);

            var metallicMap = SaveAndLoadTexture2D("metallicMap", materialData.MetallicMapPNGData, materialAsset);
            onMapExported?.Invoke(metallicMap.Item2, baseArtifact);

            var smoothnessMap = SaveAndLoadTexture2D("smoothnessMap", materialData.SmoothnessMapPNGData, materialAsset);
            onMapExported?.Invoke(smoothnessMap.Item2, baseArtifact);

            var heightMap = SaveAndLoadTexture2D("heightMap", materialData.HeightmapPNGData, materialAsset);
            onMapExported?.Invoke(heightMap.Item2, baseArtifact);

            var aoMap = SaveAndLoadTexture2D("ambientOcclusionMap", materialData.AOMapPNGData, materialAsset);
            onMapExported?.Invoke(aoMap.Item2, baseArtifact);

            materialAsset.SetTexture(MuseMaterialProperties.baseMapKey, diffuseMap.Item1);
            materialAsset.SetTexture(MuseMaterialProperties.normalMapKey, normalMap.Item1);
            materialAsset.SetTexture(MuseMaterialProperties.metallicMapKey, metallicMap.Item1);
            materialAsset.SetTexture(MuseMaterialProperties.smoothnessMapKey, smoothnessMap.Item1);
            materialAsset.SetTexture(MuseMaterialProperties.heightMapKey, heightMap.Item1);
            materialAsset.SetTexture(MuseMaterialProperties.ambientOcclusionMapKey, aoMap.Item1);

#if !USING_HDRP
            materialAsset.SetFloat(MuseMaterialProperties.useDisplacement, 0f);
#endif

            CopyPbrMaterialProperty(materialAsset, baseArtifact);

            EditorUtility.SetDirty(materialAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(materialAsset));

            onMapExported?.Invoke(path, baseArtifact);
        }

        static (Texture2D, string) SaveAndLoadTexture2D(string name, byte[] pngData, Material materialAsset, bool isNormalMap = false, bool isSRGB = false)
        {
            // Get path of the base asset
            var baseAssetPath = AssetDatabase.GetAssetPath(materialAsset);

            // Get directory path from the base asset path
            var baseAssetDir = Path.GetDirectoryName(baseAssetPath);

            var folderName = materialAsset.name + "_Maps";

            // Create the new folder's path
            var newFolderPath = Path.Combine(baseAssetDir, folderName);

            // Check if the directory already exists, if not, create it
            if (!AssetDatabase.IsValidFolder(newFolderPath))
            {
                AssetDatabase.CreateFolder(baseAssetDir, folderName);
            }

            // Prepare the path for the new PNG
            var pngPath = Path.Combine(newFolderPath, name  + ".png");

            // Save the PNG data to a file
            if (pngData is null)
                return (new Texture2D(2,2), null);
            File.WriteAllBytes(pngPath, pngData);

            // Ensure the new asset is included in the AssetDatabase
            AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);

            if (isNormalMap || !isSRGB)
            {
                var textureImporter = AssetImporter.GetAtPath(pngPath) as TextureImporter;

                // Create TextureImporterSettings
                var textureImporterSettings = new TextureImporterSettings();
                textureImporter.ReadTextureSettings(textureImporterSettings);

                if (isNormalMap)
                {
                    // Enable normal map
                    textureImporterSettings.textureType = TextureImporterType.NormalMap;
                    textureImporterSettings.convertToNormalMap = false;
                }

                textureImporterSettings.sRGBTexture = isSRGB;

                // Apply settings
                textureImporter.SetTextureSettings(textureImporterSettings);

                // Ensure the new asset is included in the AssetDatabase
                AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);
            }

            // Load the texture from the saved PNG file
            var newTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath);

            return (newTexture, pngPath);
        }

        internal static void CopyPbrMaterialProperty(Material material, Artifact baseArtifact)
        {
            if (baseArtifact is not ImageArtifact imageArtifact) return;

            if (imageArtifact.MaterialMetaData is not { Initialized: true })
            {
                imageArtifact.MaterialMetaData = new ImageArtifact.MaterialData(true);
            }
            material.SetFloat(MuseMaterialProperties.heightIntensity, imageArtifact.MaterialMetaData.height);
            material.SetFloat(MuseMaterialProperties.metallicIntensity, imageArtifact.MaterialMetaData.metallic);
            material.SetFloat(MuseMaterialProperties.smoothnessIntensity, imageArtifact.MaterialMetaData.smoothness);
            material.SetInt(MuseMaterialProperties.useMetallic, imageArtifact.MaterialMetaData.useMetallic ? 1 : 0);
            material.SetInt(MuseMaterialProperties.useSmoothness, imageArtifact.MaterialMetaData.useSmoothness ? 1 : 0);
        }
        
        internal static string GetMaterialName(Artifact baseArtifact)
        {
            var prompt = baseArtifact?.GetOperator<PromptOperator>()?.GetPrompt();
            
            if (string.IsNullOrWhiteSpace(prompt))
                return baseArtifact?.Guid;

            try
            {
                string[] commonWords = { "a", "the", "an", "and", "of" }; // Add more common words as needed
            
                var words = prompt.Split(' ');
            
                // Remove common words
                var filteredWords = words
                    .Where(word => !commonWords.Contains(word.ToLower()))
                    .Where(word => !string.IsNullOrWhiteSpace(word))
                    .ToArray();

                // Concatenate and format the remaining words
                var filename = string.Join("", filteredWords.Select(word => char.ToUpper(word[0]) + word.Substring(1)));

                // Limit the filename to 25 characters
                if (filename.Length > 25)
                {
                    filename = filename[..25];
                } 

                return filename;
            }
            catch (Exception)
            {
                return baseArtifact?.Guid; 
            }
        }
    }
}