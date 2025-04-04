using System;
using UnityEngine;

namespace Unity.AppUI.Core
{
    /// <summary>
    /// The Scale context of the application.
    /// </summary>
    internal record ScaleContext(string scale) : IContext
    {
        /// <summary>
        /// The current scale.
        /// </summary>
        public string scale { get; } = scale;
    }
}
