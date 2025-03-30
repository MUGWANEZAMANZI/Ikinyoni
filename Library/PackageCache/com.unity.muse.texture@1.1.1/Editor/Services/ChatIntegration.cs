using Unity.Muse.Common.Editor;
using Unity.Muse.Common.Editor.Integration;
using Unity.Muse.Common.Services;

namespace Unity.Muse.Texture.Editor.Services
{
    static class ChatIntegration
    {
        [Plugin("Plugin for creating a texture given a prompt.")]
        public static void GenerateTexture(
            [Parameter("The prompt to guide what texture will be generated")]
            string prompt)
        {
            GenerationService.GenerateImage(prompt,
                result => { },
                ImageModel.Texture,
                model => EditorModelAssetEditor.OpenEditorTo(model));
        }
    }
}
