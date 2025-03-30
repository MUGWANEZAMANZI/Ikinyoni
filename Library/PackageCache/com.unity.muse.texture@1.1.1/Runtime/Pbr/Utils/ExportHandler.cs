using System;
using Unity.Muse.Common;

namespace Unity.Muse.Texture
{
    /// <summary>
    /// Export handler for texture
    /// </summary>
    internal static class ExportHandler
    {
        /// <summary>
        /// Event invoked when a material is exported with prompt
        /// </summary>
        public static event Action<Artifact, ProcessedPbrMaterialData, Action<string, Artifact>> OnExportMaterialWithPrompt;
        
        /// <summary>
        /// Event invoked when a material is exported without prompt
        /// </summary>
        public static event Action<Artifact, ProcessedPbrMaterialData, string, Action<string, Artifact>> OnExportMaterial;
        
        internal static void ExportWithPrompt(Artifact artifact, ProcessedPbrMaterialData materialData, Action<string, Artifact> onMapExported = null)
        {
            OnExportMaterialWithPrompt?.Invoke(artifact, materialData, onMapExported);
        }
        
        internal static void ExportWithoutPrompt(Artifact baseArtifact, ProcessedPbrMaterialData materialData, string path, Action<string, Artifact> onMapExported = null)
        {
            OnExportMaterial?.Invoke(baseArtifact, materialData, path, onMapExported); 
        }
    }
}