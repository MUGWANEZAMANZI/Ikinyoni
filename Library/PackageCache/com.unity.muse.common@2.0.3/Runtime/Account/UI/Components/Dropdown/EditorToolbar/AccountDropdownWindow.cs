#if UNITY_EDITOR
using System;
using Unity.Muse.AppUI.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Account.UI
{
    class AccountDropdownWindow : EditorWindow
    {
        internal static Func<Rect> toolbarPosition;
        internal static VisualElement toolbarButton;

        /// <summary>
        /// Show the account settings without it being clicked by the user.
        /// </summary>
        internal static void ShowMuseAccountSettingsAsPopup()
        {
            var rect = toolbarButton.worldBound;
            rect.position = VisualElementUtility.GetScreenPosition(toolbarPosition(), toolbarButton);
            ShowMuseAccountSettingsAsPopupInternal(rect);
        }

        /// <summary>
        /// Show muse account settings
        /// </summary>
        /// <param name="rect">Element bounds/Rect relative to its EditorWindow</param>
        internal static void ShowMuseAccountSettingsAsPopup(Rect rect) =>
            ShowMuseAccountSettingsAsPopupInternal(GUIUtility.GUIToScreenRect(rect));

        static void ShowMuseAccountSettingsAsPopupInternal(Rect rect)
        {
            ClearPreviousWindows();

            var popup = CreateInstance<AccountDropdownWindow>();
            popup.hideFlags = HideFlags.DontSave;
            popup.ShowAsDropDown(rect, new Vector2(300, 260));
        }

        static void ClearPreviousWindows()
        {
            var windows = Resources.FindObjectsOfTypeAll<AccountDropdownWindow>();
            foreach (var window in windows)
            {
                try
                {
                    window.Close();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        void CreateGUI()
        {
            var panel = new Panel();
            panel.AddToClassList("dropdown-editor-window");
            rootVisualElement.Add(panel);

            AccountController.Register(this);

            var scrollView = new ScrollView();  // Wrap in a scrollview to be certain all content will always be shown.
            var content = new AccountDropdownContent {OnAction = Close};
            scrollView.Add(content);
            panel.Add(scrollView);
        }
    }
}
#endif