using System.IO;
using Unity.Muse.Common;
using Unity.Muse.Common.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Texture.Editor
{
    internal class MuseAssetCreation : UnityEditor.ProjectWindowCallback.EndNameEditAction
    {
        const string k_MuseTextureModeKey = "TextToImage";

        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            pathName = Path.ChangeExtension(pathName, ".asset");

            var asset = CreateInstance<Model>();
            asset.Initialize();
            int mode = ModesFactory.GetModeIndexFromKey(k_MuseTextureModeKey);
            if (mode < 0)
                Debug.LogError($"Mode {k_MuseTextureModeKey} not found");
            asset.ModeChanged(mode);

            AssetDatabase.CreateAsset(asset, pathName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Create/Muse/Texture Generator", false, 0)]
        static void CreateSpriteLibrarySourceAssetMenu()
        {
            var action = CreateInstance<MuseAssetCreation>();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, action, "Muse Texture.asset", IconHelper.assetIcon, null);
        }
    }
}
