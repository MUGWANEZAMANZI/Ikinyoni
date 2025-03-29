using Unity.Muse.Texture.Tools;

namespace Unity.Muse.Texture.Editor
{
    static class EditorFactoriesRegister
    {
        [UnityEditor.InitializeOnLoadMethod]
        static void RegisterArtifact()
        {
            ImageArtifact.RegisterArtifact();
            ImageArtifactDragAndDropHandler.Register();
            PbrArtifactDragAndDropHandler.Register();
        }

        [UnityEditor.InitializeOnLoadMethod]
        static void RegisterAvailableTools()
        {
           ToolFactoryRegister.RegisterAvailableTools();
        }
    }
}
