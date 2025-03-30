using System;
using Unity.Muse.Common;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.Muse.Texture
{
    class UIMode : IUIMode
    {
        public const string modeKey = "TextToImage";
#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#else
        [UnityEditor.InitializeOnLoadMethod]
#endif
        [Preserve]
        public static void RegisterUIMode()
        {
            UIModeFactory.RegisterUIMode<UIMode>(modeKey);
        }

        Model m_Model;

        public void Activate(MainUI mainUI, string modeKey1)
        {
            m_Model = mainUI.model;

            AddListeners();
        }

        public void Deactivate()
        {
            RemoveListeners();
        }

        void AddListeners()
        {
            m_Model.OnCurrentPromptChanged += OnPromptChanged;
        }

        void RemoveListeners()
        {
            m_Model.OnCurrentPromptChanged -= OnPromptChanged;
        }

        void OnPromptChanged(string prompt)
        {
            var hasValidPrompt = !string.IsNullOrWhiteSpace(prompt) && prompt.Length >= PromptOperator.MinimumPromptLength;
            m_Model.GetData<GenerateButtonData>().SetGenerateButtonData(hasValidPrompt, hasValidPrompt ? null : TextContent.generateButtonEnterPromptTooltip);
        }
    }
}
