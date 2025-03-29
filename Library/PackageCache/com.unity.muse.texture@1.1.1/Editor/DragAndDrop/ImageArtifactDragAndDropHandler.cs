using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.Common;
using Unity.Muse.Common.Editor;
using Unity.Muse.Texture.Editor.Analytics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.Muse.Texture.Editor
{
    [Preserve]
    internal sealed class ImageArtifactDragAndDropHandler : IArtifactDragAndDropHandler
    {
        ImageArtifact m_ImageArtifact;

        public event Action<string, Artifact> ArtifactDropped;

        public static void Register()
        {
            DragAndDropFactory.SetHandlerForArtifact("Texture Image", typeof(ImageArtifactDragAndDropHandler));
        }

        public ImageArtifactDragAndDropHandler(IList<Artifact> imageArtifacts)
        {
            m_ImageArtifact = (ImageArtifact)imageArtifacts.FirstOrDefault();

            Debug.Assert(m_ImageArtifact != null);
        }

        public bool CanDropSceneView(GameObject dropUpon, Vector3 worldPosition) => true;

        public void HandleDropSceneView(GameObject dropUpon, Vector3 worldPosition)
        {
            Model.SendAnalytics(new SaveTextureAnalytic(false, ""));

            if (dropUpon != null)
            {
                Undo.RegisterFullObjectHierarchyUndo(dropUpon, $"Set material ({dropUpon.name})");
                AddToGameObject(dropUpon);
            }
            else
            {
                var newGameObject = CreateNewGameObject(worldPosition);
                Undo.RegisterCreatedObjectUndo(newGameObject, $"Create Game Object with material.");
            }

            ArtifactDropped?.Invoke(null, m_ImageArtifact);
        }

        public bool CanDropHierarchy(GameObject dropUpon) => true;

        public void HandleDropHierarchy(GameObject dropUpon)
        {
            Model.SendAnalytics(new SaveTextureAnalytic(false, ""));

            if (dropUpon != null)
            {
                Undo.RegisterFullObjectHierarchyUndo(dropUpon, $"Set material ({dropUpon.name})");
                AddToGameObject(dropUpon);
            }
            else
            {
                var newGameObject = CreateNewGameObject(Vector3.zero);
                Undo.RegisterCreatedObjectUndo(newGameObject, $"Create Game Object with material.");
            }

            ArtifactDropped?.Invoke(null, m_ImageArtifact);
        }

        public bool CanDropProject(string path) => true;

        public void HandleDropProject(string path)
        {
            Model.SendAnalytics(new SaveTextureAnalytic(false, ""));

            if (string.IsNullOrWhiteSpace(path))
                path = ExporterHelpers.assetsRoot;

            m_ImageArtifact.ExportToDirectory(path, true, exportedPath =>
            {
                ArtifactDropped?.Invoke(exportedPath, m_ImageArtifact);
            });
        }

        void AddToGameObject(GameObject gameObject)
        {
            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                var tex = (Texture2D)ArtifactCache.Read(m_ImageArtifact);
                var material = new Material(MaterialGeneratorUtils.GetDefaultShaderForPipeline());
                material.SetTexture(MuseMaterialProperties.baseMapKey, tex);
                renderer.material = material;
            }
        }

        GameObject CreateNewGameObject(Vector3 position)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(Vector3.right * (-90));
            AddToGameObject(go);
            return go;
        }
    }
}
