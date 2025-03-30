using Unity.Muse.Common;
using UnityEditor;
using Unity.Muse.Common.Editor;

namespace Unity.Muse.Texture.Editor
{
    /// <summary>
    /// Initializes the Muse Texture tool preferences system for the editor.
    /// </summary>
    /// <remarks>
    /// You should not need to use this class directly.
    /// It is used to initialize the sprite preferences system for the editor.
    /// </remarks>
    [InitializeOnLoad]
    static class EditorTexturePreferences
    {
        static EditorTexturePreferences()
        {
            GlobalPreferences.RegisterAssetGeneratedPath("TextToImage", GetAssetPath);
            TexturePreferences.Init(new EditorPreferences(
                prefix: "Unity.Muse.Texture.Preferences",
                settingsPath:  "ProjectSettings/Packages/com.unity.muse.texture/Settings.json"));
            MuseProjectSettings.RegisterSection(TextContent.textureSettingsCategory, new EditorTexturePreferencesView());
        }
        
        static string GetAssetPath() => TexturePreferences.assetGeneratedFolderPath;
    }
}
