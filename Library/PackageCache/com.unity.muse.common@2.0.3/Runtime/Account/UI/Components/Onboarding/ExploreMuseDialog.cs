#if UNITY_EDITOR
using System;
using Unity.Muse.AppUI.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Unity.Muse.Common.Account
{
    class ExploreMuseDialog : AccountDialog
    {
        interface IClientHandler
        {
            void Open();
            void Install();
            bool IsInstalled { get; }
        }
        class ClientHandler : IClientHandler
        {
            public Action open;
            public string packageName;

            // Consider no package to always be installed
            public bool IsInstalled =>
                string.IsNullOrEmpty(packageName) ||
                PackageInfo.FindForAssetPath($"Packages/{packageName}/package.json") != null;
            public void Open() => open();
            public void Install()
            {
                try
                {
                    UnityEditor.PackageManager.UI.Window.Open(packageName);
                }
                catch (Exception exception)
                {
                    Debug.LogError($"Package {packageName} is not available in the registry. It is possible you don't have preview packages visible.\n\n{exception.Message}");
                }
            }
        }

        class Section : VisualElement
        {
            Text m_SectionTitle = new();
            VisualElement m_Container = new();

            public Section(string title)
            {
                m_SectionTitle.text = title;
                m_SectionTitle.AddToClassList("muse-dialog-section-title");

                Add(m_SectionTitle);
                Add(m_Container);
            }
        }

        class SectionCard : VisualElement
        {
            VisualElement m_TextContainer = new();
            Text m_Title = new();
            Text m_Description = new();

            public SectionCard(string titleText, string descriptionText, IClientHandler handler, string iconName = "arrow-square-in")
            {
                m_Title.AddToClassList("muse-dialog-card-title");
                m_Title.text = titleText;

                m_Description.AddToClassList("muse-dialog-card-description");
                m_Description.text = descriptionText;

                m_TextContainer.Add(m_Title);
                m_TextContainer.Add(m_Description);
                Add(m_TextContainer);

                if (handler.IsInstalled)
                {
                    var action = new Icon { iconName = iconName, size = IconSize.M};
                    action.AddToClassList("action-icon");
                    Add(action);
                    this.AddManipulator(new Pressable(handler.Open));
                }
                else
                {
                    m_TextContainer.SetEnabled(false);

                    var action = new Button(handler.Install) {title = "Install"};
                    action.AddToClassList("action-icon");
                    Add(action);
                    this.AddManipulator(new Pressable(handler.Install));
                }
            }
        }

        public Action OnAccept;

        public ExploreMuseDialog()
        {
            AddToClassList("muse-subscription-dialog-explore");

            dialogDescription.Add(new Text {text = TextContent.exploreTitle, name="muse-description-title"});
            var descriptionText = new Text {text = TextContent.exploreDescription, name = "muse-description-secondary", enableRichText = true};
            descriptionText.AddToClassList("muse-description-section");
            dialogDescription.Add(descriptionText);
            dialogDescription.AddToClassList("muse-description-section");

            var learnSection = new VisualElement();
            learnSection.AddToClassList("muse-dialog-explore-section-learn");
            learnSection.AddToClassList("muse-description-section");
            learnSection.Add(new SectionCard(TextContent.exploreLearnSectionTitle, TextContent.exploreCardLearnDescription, Learn, "arrow-square-out"));

            var exploreSection = new Section(TextContent.exploreMuseSectionTitle);
            exploreSection.AddToClassList("muse-dialog-explore-section-muse");
            exploreSection.Add(new SectionCard(TextContent.exploreCardChatTitle, TextContent.exploreCardChatDescription, Chat));
            exploreSection.Add(new SectionCard(TextContent.exploreCardSpriteTitle, TextContent.exploreCardSpriteDescription, Sprite));
            exploreSection.Add(new SectionCard(TextContent.exploreCardTextureTitle, TextContent.exploreCardTextureDescription, Texture));
            exploreSection.Add(new SectionCard(TextContent.exploreCardAnimateTitle, TextContent.exploreCardAnimateDescription, Animate));
            exploreSection.Add(new SectionCard(TextContent.exploreCard2dWorkflowsTitle, TextContent.exploreCard2dWorkflowsDescription, SpriteEnhancers));

            dialogDescription.Add(learnSection);
            dialogDescription.Add(exploreSection);

            AddPrimaryButton(TextContent.exploreAccept, () =>
            {
                var chat = Chat;
                if (chat.IsInstalled)
                    chat.Open();

                OnAccept?.Invoke();
            });
        }

        static ClientHandler Chat => new()
        {
            open = () => EditorApplication.ExecuteMenuItem("Muse/Chat"),
            packageName = "com.unity.muse.chat"
        };

        static ClientHandler Texture => new()
        {
            open = () => EditorApplication.ExecuteMenuItem("Muse/New Texture Generator"),
            packageName = "com.unity.muse.texture"
        };

        static ClientHandler Sprite => new()
        {
            open = () => EditorApplication.ExecuteMenuItem("Muse/New Sprite Generator"),
            packageName = "com.unity.muse.sprite"
        };

        static ClientHandler Animate => new()
        {
            open = () => EditorApplication.ExecuteMenuItem("Muse/New Animate Generator"),
            packageName = "com.unity.muse.animate"
        };

        static ClientHandler SpriteEnhancers => new()
        {
            open = () => EditorApplication.ExecuteMenuItem("Window/2D/Sprite Editor"),
            packageName = "com.unity.2d.muse"
        };

        static ClientHandler Learn => new()
        {
            open = AccountLinks.LearnMuse,
            packageName = string.Empty
        };
    }
}
#endif