using System;
using Unity.Muse.Common;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.Muse.Texture
{
    internal static class PrimitiveObjectsProvider
    {
        public static GameObject GetPrimitiveInstance(PrimitiveObjectTypes primitiveType, string customModelGuid = null, Mesh customMesh = null)
        {
            var path = string.Empty;
            switch (primitiveType)
            {
                case PrimitiveObjectTypes.Sphere:
                    path = PackageResources.spherePreviewModel;
                    break;
                case PrimitiveObjectTypes.Cube:
                    path = PackageResources.cubePreviewModel;
                    break;
                case PrimitiveObjectTypes.Plane:
                    path = PackageResources.planePreviewModel;
                    break;
                case PrimitiveObjectTypes.Cylinder:
                    path = PackageResources.cylinderPreviewModel;
                    break;
                case PrimitiveObjectTypes.Custom:
#if UNITY_EDITOR
                    if (customMesh != null)
                    {
                        var go = new GameObject("",typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
                        go.GetComponent<MeshFilter>().sharedMesh = customMesh;
                        go.GetComponent<MeshCollider>().sharedMesh = customMesh;
                        return go;
                    }
                    else if(string.IsNullOrEmpty(customModelGuid))
                    {
                        path = UnityEditor.EditorUtility.OpenFilePanel("Select custom object", "", "fbx");
                    }
                    else
                    {
                        path = UnityEditor.AssetDatabase.GUIDToAssetPath(customModelGuid);
                    }
#endif
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(primitiveType), primitiveType, null);
            }

            var resource = ResourceManager.Load<GameObject>(path);
            if (!resource)
            {
                Debug.LogWarning($"Asset could not be loaded: {path}");
                return null;
            }

            return Object.Instantiate(resource);
        }
    }
}
