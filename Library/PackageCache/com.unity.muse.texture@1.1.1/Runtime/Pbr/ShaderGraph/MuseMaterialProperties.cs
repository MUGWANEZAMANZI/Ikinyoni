using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

namespace Unity.Muse.Texture
{
    /// <summary>
    /// Properties used for the Muse Material Shader Graph
    /// </summary>
    internal static class MuseMaterialProperties
    {
        //Maps
        /// <summary>
        /// Key for base map
        /// </summary>
        public static readonly int baseMapKey = Shader.PropertyToID("_AlbedoMap");
        /// <summary>
        /// Key for normal map
        /// </summary>
        public static readonly int normalMapKey = Shader.PropertyToID("_NormalMap");
        /// <summary>
        /// Key for metallic map
        /// </summary>
        public static readonly int metallicMapKey = Shader.PropertyToID("_MetallicMap");
        /// <summary>
        /// Key for smoothness map
        /// </summary>
        public static readonly int smoothnessMapKey = Shader.PropertyToID("_SmoothnessMap");
        /// <summary>
        /// Key for height map
        /// </summary>
        public static readonly int heightMapKey = Shader.PropertyToID("_HeightMap"); 
        /// <summary>
        /// key for ambient occlusion map
        /// </summary>
        public static readonly int ambientOcclusionMapKey = Shader.PropertyToID("_AmbientOcclusionMap");
        
        //Material Edit properties
        /// <summary>
        /// Key for tiling 
        /// </summary>
        public static readonly int tilingKey = Shader.PropertyToID("_Tiling");
        /// <summary>
        /// Key for offset 
        /// </summary>
        public static readonly int offsetKey = Shader.PropertyToID("_Offset");
        /// <summary>
        /// Key for rotation 
        /// </summary>
        public static readonly int rotationKey = Shader.PropertyToID("_Rotation");
        /// <summary>
        /// Key for flip vertical 
        /// </summary>
        public static readonly int flipVertical = Shader.PropertyToID("_FlipVertical");
        /// <summary>
        /// Key for flip horizontal 
        /// </summary>
        public static readonly int flipHorizontal = Shader.PropertyToID("_FlipHorizontal");
        
        //Material Maps Properties
        /// <summary>
        /// Key for height value 
        /// </summary>
        public static readonly int heightIntensity = Shader.PropertyToID("_HeightIntensity");
        /// <summary>
        /// Key for metallic value 
        /// </summary>
        public static readonly int metallicIntensity = Shader.PropertyToID("_MetallicIntensity");
        /// <summary>
        /// Key for roughness value 
        /// </summary>
        public static readonly int smoothnessIntensity = Shader.PropertyToID("_SmoothnessIntensity");
        
        /// <summary>
        /// Using vertex displacement
        /// </summary>
        public static readonly int useDisplacement = Shader.PropertyToID("_VertexDisplacement");
        
        /// <summary>
        /// Using Metallic map
        /// </summary>
        public static readonly int useMetallic = Shader.PropertyToID("_UseMetallicMap");
        
        /// <summary>
        /// Using Smoothness map
        /// </summary>
        public static readonly int useSmoothness = Shader.PropertyToID("_UseSmoothnessMap");
        
        /// <summary>
        /// Enabling HDRP keyword for the Muse Material Shader Graph
        /// </summary>
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod]
#endif
        [Preserve]
        public static void EnableShaderKeywords()
        {
            GlobalKeyword.Create("USING_HDRP");
#if USING_HDRP
            Shader.EnableKeyword("USING_HDRP");
#else
            Shader.DisableKeyword("USING_HDRP");
#endif
        }
    }
}