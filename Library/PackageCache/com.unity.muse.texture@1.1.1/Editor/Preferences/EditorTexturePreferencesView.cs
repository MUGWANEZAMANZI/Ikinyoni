using Unity.Muse.Common.Editor;
using Unity.Muse.Common;
using UnityEngine.UIElements;
using UnityEditor;

namespace Unity.Muse.Texture.Editor
{
    class EditorTexturePreferencesView : IMuseEditorPreferencesView
    {
        TextField m_TextureAssetGeneratedPathField;

        public VisualElement CreateGUI()
        {
            var root = new VisualElement();
            var textureAssetRow = new VisualElement();
            textureAssetRow.AddToClassList(MuseProjectSettings.rowUssClassName);
            var assetLabel = new Label {text = TextContent.textureAssetGeneratedPath};
            assetLabel.AddToClassList(MuseProjectSettings.propertyLabelUssClassName);
            textureAssetRow.Add(assetLabel);
            m_TextureAssetGeneratedPathField = new TextField { isDelayed = true };
            m_TextureAssetGeneratedPathField.RegisterValueChangedCallback(evt =>
            {
                var newValue = GlobalPreferences.SanitizeMuseGeneratedPath(evt.newValue);
                TexturePreferences.assetGeneratedFolderPath = newValue;
                if (newValue != evt.newValue)
                    m_TextureAssetGeneratedPathField.SetValueWithoutNotify(newValue);
            });
            textureAssetRow.Add(m_TextureAssetGeneratedPathField);
            var browseTextureAssetButton = new Button(() =>
            {
                var newValue = EditorUtility.OpenFolderPanel(
                    TextContent.selectFolder, 
                    TexturePreferences.assetGeneratedFolderPath, "");
                newValue = MuseProjectSettings.SanitizePath(newValue);
                if (!string.IsNullOrEmpty(newValue))
                    m_TextureAssetGeneratedPathField.value = newValue;
            }) {text = TextContent.browse};
            textureAssetRow.Add(browseTextureAssetButton);
            root.Add(textureAssetRow);
            
            return root;
        }

        public void Refresh()
        {
            var textureAssetGeneratedPath = TexturePreferences.assetGeneratedFolderPath;
            m_TextureAssetGeneratedPathField.SetValueWithoutNotify(textureAssetGeneratedPath);
        }
    }
}
