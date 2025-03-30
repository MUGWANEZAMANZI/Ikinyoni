#if !MUSE_TOOLBAR_BUTTON_ENABLED && UNITY_EDITOR
using System;
using System.Reflection;
using Unity.AppUI.Core;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common.Account.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;

namespace Unity.Muse.Common.Bridge
{
    [InitializeOnLoad]
    public static class AccessToolbar
    {
        static Assembly EditorAssembly => typeof(UnityEditor.Editor).Assembly;
        static Type MuseDropDownType => Type.GetType("UnityEditor.Toolbars.MuseDropdown, UnityEditor");

        static bool UseNativeToolbarInterface => MuseDropDownType != null;

        static AccessToolbar()
        {
            if (UseNativeToolbarInterface)
                ShowNative();
            else
                ShowCustom();

        }

        static void ShowCustom()
        {
            EditorApplication.delayCall += () =>
            {
                var toolbarType = EditorAssembly.GetType("UnityEditor.Toolbar");
                if (toolbarType == null)
                {
                    Debug.LogError("Could not find UnityEditor.Toolbar type. Muse will not be available.");
                    return;
                }

                var rootField = toolbarType.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                if (rootField != null)
                {
                    // Note: Not using `UnityEditor.Toolbar.get` directly here since 2023.2.20f1 does not expose `Toolbar` publicly
                    var getField = toolbarType.GetField("get", BindingFlags.Public | BindingFlags.Static);
                    if (getField == null)
                    {
                        Debug.LogError("Could not find the Toolbar instance field. Muse will not be available");
                        return;
                    }

                    object toolbarInstance = getField.GetValue(null);
                    if (rootField.GetValue(toolbarInstance) is VisualElement root)
                    {
                        var toolbarZoneLeftAlign = root.Q<VisualElement>("ToolbarZoneLeftAlign");
                        if (toolbarZoneLeftAlign != null)
                        {
                            var container = new Panel();
                            container.AddToClassList("muse-editor-toolbar-container");

                            // Ensure the position is relative to avoid taking any more space than necessary.
                            container.contentContainer.style.position = Position.Relative;
                            container.ProvideContext(new ThemeContext(EditorGUIUtility.isProSkin ? "editor-dark" : "editor-light"));
                            container.styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.museTheme));

                            Button museButton = new Button {title = "Muse"};
                            var caretIcon = new VisualElement();
                            caretIcon.AddToClassList("unity-icon-arrow");
                            museButton.Add(caretIcon);
                            museButton.clicked += () =>
                                AccountDropdownWindow.ShowMuseAccountSettingsAsPopup(museButton?.worldBound ?? Rect.zero);
                            museButton.AddToClassList("unity-toolbar-button");
                            AccountDropdownWindow.toolbarButton = museButton;

                            var positionField = toolbarType.GetProperty("screenPosition", BindingFlags.Public | BindingFlags.Instance);
                            AccountDropdownWindow.toolbarPosition = () =>
                            {
                                if (positionField?.GetValue(toolbarInstance) is Rect rect)
                                    return rect;

                                return Rect.zero;
                            };

                            container.Add(museButton);

                            toolbarZoneLeftAlign.Add(container);
                        }
                    }
                }
            };
        }

        static void ShowNative()
        {
            var onDropdownOpenedField = MuseDropDownType.GetField("OnDropdownOpened", BindingFlags.Static | BindingFlags.NonPublic);
            if (onDropdownOpenedField == null)
                return;

            // Assign the delegate to the event field
            onDropdownOpenedField.SetValue(null, (Action<Rect, VisualElement>) NativeHandler);
        }

        static void NativeHandler(Rect rect, VisualElement visualElement) => AccountDropdownWindow.ShowMuseAccountSettingsAsPopup(rect);
    }
}
#endif
