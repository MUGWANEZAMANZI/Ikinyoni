using System;
using System.Collections.Generic;
using System.IO;
using Unity.Muse.Common;
using Unity.Muse.Common.Editor;
using Unity.Muse.Texture.Editor.Analytics;
using Unity.Muse.Texture.Pbr.Cache;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.Muse.Texture.Editor
{
    [Preserve]
    internal sealed class PbrArtifactDragAndDropHandler : IArtifactDragAndDropHandler
    {
        ImageArtifact m_BaseMap;
        ProcessedPbrMaterialData m_ProcessedMaterialData;

        public event Action<string, Artifact> ArtifactDropped;

        public static void Register()
        {
            DragAndDropFactory.SetHandlerForArtifact("PBR Material", typeof(PbrArtifactDragAndDropHandler));
        }

        public PbrArtifactDragAndDropHandler(IList<Artifact> imageArtifacts)
        {
            Debug.Assert(imageArtifacts != null);

            m_BaseMap = (ImageArtifact)imageArtifacts[0];
            m_ProcessedMaterialData = PbrDataCache.GetPbrMaterialData(m_BaseMap);

            Debug.Assert(m_BaseMap != null);
            Debug.Assert(m_ProcessedMaterialData.MetallicMapPNGData != null);
            Debug.Assert(m_ProcessedMaterialData.HeightmapPNGData != null);
            Debug.Assert(m_ProcessedMaterialData.NormalMapPNGData != null);
            Debug.Assert(m_ProcessedMaterialData.SmoothnessMapPNGData != null);
        }

        bool EvaluateDropTarget(GameObject go)
        {
            var canDrop = !(go == null || go.GetComponent<MeshRenderer>() == null);
            return canDrop;
        }

        public bool CanDropSceneView(GameObject dropUpon, Vector3 worldPosition)
        {
            return EvaluateDropTarget(dropUpon);
        }

        public void HandleDropSceneView(GameObject dropUpon, Vector3 worldPosition)
        {
            Model.SendAnalytics(new SaveTextureAnalytic(false, ""));

            DropOnGameObject(dropUpon, worldPosition, m_ProcessedMaterialData);

            ArtifactDropped?.Invoke(null, m_BaseMap);
        }

        void DropOnGameObject(GameObject go, Vector3 worldPosition, ProcessedPbrMaterialData materialData)
        {
            Model.SendAnalytics(new SaveTextureAnalytic(false, ""));

            if (!EvaluateDropTarget(go))
            {
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(go, $"Add PBR material ({go.name})");

            var meshRenderer = go.GetComponent<MeshRenderer>();

            var material = new Material(MaterialGeneratorUtils.GetDefaultShaderForPipeline());
            MaterialGeneratorUtils.CreateTexturesAndMaterialForRP(materialData, material, true);

            MaterialExporter.CopyPbrMaterialProperty(material, m_BaseMap);

#if !USING_HDRP
            material.SetFloat(MuseMaterialProperties.useDisplacement, 0f);
#endif
            meshRenderer.sharedMaterial = material;
        }

        public bool CanDropHierarchy(GameObject dropUpon)
        {
            return EvaluateDropTarget(dropUpon);
        }

        public void HandleDropHierarchy(GameObject dropUpon)
        {
            Model.SendAnalytics(new SaveTextureAnalytic(true, ""));

            HandleDropSceneView(dropUpon, Vector3.zero);

            ArtifactDropped?.Invoke(null, m_BaseMap);
        }

        public bool CanDropProject(string path)
        {
            return true;
        }

        public void HandleDropProject(string path)
        {
            Model.SendAnalytics(new SaveTextureAnalytic(true, ""));

            path = GetPathRelativeToRoot(path);
            if (string.IsNullOrWhiteSpace(path))
                path = "Assets";

            var fileName = MaterialExporter.GetMaterialName(m_BaseMap);

            path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, fileName + ".mat"));

            MaterialExporter.ExportMaterial(m_BaseMap, m_ProcessedMaterialData, path, ArtifactDropped);
        }

        static string GetPathRelativeToRoot(string path)
        {
            var pathStartIndex = path.IndexOf("Assets");
            return pathStartIndex == -1 ? string.Empty : path.Substring(pathStartIndex);
        }
    }
}
