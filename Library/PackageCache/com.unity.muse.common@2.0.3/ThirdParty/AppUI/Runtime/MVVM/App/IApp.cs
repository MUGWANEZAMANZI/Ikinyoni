using System;

namespace Unity.AppUI.MVVM
{
    /// <summary>
    /// Interface for an application.
    /// </summary>
    internal interface IApp : IDisposable
    {
        /// <summary>
        /// Called to initialize the application.
        /// </summary>
        /// <param name="serviceProvider"> The service provider to use. </param>
        /// <param name="host"> The host to use. </param>
        void Initialize(IServiceProvider serviceProvider, IHost host);
        
        /// <summary>
        /// Called to shutdown the application.
        /// </summary>
        void Shutdown();
    }
}
