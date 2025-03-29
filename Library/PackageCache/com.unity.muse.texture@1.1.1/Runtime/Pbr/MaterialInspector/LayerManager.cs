using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.Muse.Texture
{
    internal static class LayerManager
    {
        public const string MuseLayerName = "MuseTextureLayer";
        
        public static bool CreateLayer()
        {
#if UNITY_EDITOR
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            
            // Check if the layer name is already used
            for (var i = 0; i < layers.arraySize; i++)
            {
                var layerSP = layers.GetArrayElementAtIndex(i);
                if (layerSP.stringValue == MuseLayerName)
                {
                    return true;
                }
            }

            for (var j = 8; j < layers.arraySize; j++) // Start at 8 to bypass Unity's built-in layers
            {
                var layerSP = layers.GetArrayElementAtIndex(j);
                
                if (!string.IsNullOrEmpty(layerSP.stringValue)) continue;
                layerSP.stringValue = MuseLayerName;
                tagManager.ApplyModifiedProperties();
                
                return true;
            }

            Debug.LogError($"No available layer for muse texture, if you want to use this feature, please free up a layer.");
            return false;
#else
            if (LayerMask.NameToLayer(MuseLayerName) != -1)
                return true;

            Debug.LogError($"No available layer for muse texture, if you want to use this feature, please create one named {MuseLayerName} before compilation.");
            return false;
#endif
        }
        
        public static bool AssignLayer(GameObject gameObject)
        {
            var layerIndex = LayerMask.NameToLayer(MuseLayerName);
            if (layerIndex != -1)
            {
                gameObject.layer = LayerMask.NameToLayer(MuseLayerName);
            }
            else
            {
                Debug.LogWarning("Layer " + MuseLayerName + " does not exist!");
                return false;
            }
            
            foreach (Transform transform in gameObject.transform)
            {
                if (!AssignLayer(transform.gameObject))
                {
                    return false; 
                }
            }

            return true;
        }
    }
}