using Unity.Muse.Common;

namespace Unity.Muse.Texture
{
    static class PackageResources
    {
        [ResourcePath]
        internal const string defaultHDRCubemap =
            "Packages/com.unity.muse.texture/Runtime/Pbr/MaterialPreview/Assets/PackageResources/HDRI/Tall Hall_lo.hdr";

        [ResourcePath]
        internal const string indoorHDRCubemap =
            "Packages/com.unity.muse.texture/Runtime/Pbr/MaterialPreview/Assets/PackageResources/HDRI/IndoorEnvironmentHDRI002_4K-HDR.exr";

        [ResourcePath]
        internal const string daylightOutdoorHDRCubemap =
            "Packages/com.unity.muse.texture/Runtime/Pbr/MaterialPreview/Assets/PackageResources/HDRI/DayEnvironmentHDRI030_4K-HDR.exr";

        [ResourcePath]
        internal const string nightOutdoorHDRCubemap =
            "Packages/com.unity.muse.texture/Runtime/Pbr/MaterialPreview/Assets/PackageResources/HDRI/NightEnvironmentHDRI002_4K-HDR.exr";

        [ResourcePath]
        internal const string outdoorNeutralHDRCubemap =
            "Packages/com.unity.muse.texture/Runtime/Pbr/MaterialPreview/Assets/PackageResources/HDRI/green_point_park_256_bw.exr";

        [ResourcePath]
        internal const string materialInspectorStyleSheet =
            "Packages/com.unity.muse.texture/Runtime/Pbr/MaterialInspector/PackageResources/MaterialInspector.uss";

        [ResourcePath]
        internal const string spherePreviewModel =
            "Packages/com.unity.muse.texture/Runtime/Pbr/MaterialPreview/Assets/PackageResources/PreviewSphere.prefab";
        
        [ResourcePath]
        internal const string cubePreviewModel =
            "Packages/com.unity.muse.texture/Runtime/Pbr/MaterialPreview/Assets/PackageResources/PreviewCube.prefab";
        
        [ResourcePath]
        internal const string planePreviewModel =
            "Packages/com.unity.muse.texture/Runtime/Pbr/MaterialPreview/Assets/PackageResources/PreviewPlane.prefab";
        
        [ResourcePath]
        internal const string cylinderPreviewModel =
            "Packages/com.unity.muse.texture/Runtime/Pbr/MaterialPreview/Assets/PackageResources/PreviewCylinder.prefab";

        [ResourcePath]
        internal const string museMaterialShaderGraph =
            "Packages/com.unity.muse.texture/Runtime/Pbr/ShaderGraph/PackageResources/MuseMaterialShaderGraph.shadergraph";

        [ResourcePath]
        internal const string ambientOcclusionShader =
            "Packages/com.unity.muse.texture/Runtime/Pbr/ShaderGraph/PackageResources/AO.shader";
        
        [ResourcePath]
        internal const string ambientScaleShader =
            "Packages/com.unity.muse.texture/Runtime/Pbr/ShaderGraph/PackageResources/AoScale.compute";
    }
}
