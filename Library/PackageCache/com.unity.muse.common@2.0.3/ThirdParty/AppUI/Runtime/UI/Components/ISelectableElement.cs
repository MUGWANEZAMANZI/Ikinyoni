using System;

namespace Unity.Muse.AppUI.UI
{
    /// <summary>
    /// Interface used on UI elements which handle a *selected* state.
    /// </summary>
    internal interface ISelectableElement
    {
        /// <summary>
        /// **True** of the UI element is in selected state, **False** otherwise.
        /// </summary>
        bool selected { get; set; }

        /// <summary>
        /// Set the *selected* state of a UI element without sending an event through the visual tree.
        /// </summary>
        /// <param name="newValue">The new *selected* state value.</param>
        void SetSelectedWithoutNotify(bool newValue);
    }
}
