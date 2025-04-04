using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Unity.AppUI.MVVM
{
    /// <summary>
    /// A base class to implement an App instance using UI Toolkit.
    /// </summary>
    internal class App : IUIToolkitApp
    {
        /// <summary>
        /// Event called when the application is shutting down.
        /// </summary>
        public static event Action shuttingDown;

        readonly List<IUIToolkitHost> m_Hosts = new List<IUIToolkitHost>();

        bool m_Disposed;

        /// <summary>
        /// The current App instance.
        /// </summary>
        /// <exception cref="InvalidOperationException"> Thrown when the current App instance is not available. </exception>
        public static App current { get; private set; }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~App() => Dispose(false);

        /// <summary>
        /// The main page of the application.
        /// </summary>
        public VisualElement mainPage { get; set; }

        /// <summary>
        /// The hosts of the application.
        /// </summary>
        public IEnumerable<IUIToolkitHost> hosts => m_Hosts;

        /// <summary>
        /// Initializes the current App instance.
        /// </summary>
        /// <param name="serviceProvider"> The service provider to use. </param>
        /// <param name="host"> The host to use. </param>
        /// <exception cref="InvalidOperationException"> Thrown when a current App instance already exists. </exception>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        public void Initialize(IServiceProvider serviceProvider, IHost host)
        {
            var uitkHost = host as IUIToolkitHost;
            
            if (current != null)
                throw new InvalidOperationException($"An {nameof(App)} has been already initialized.");

            if (m_Hosts.Count > 0)
                throw new InvalidOperationException($"Trying to create the {nameof(App)} main window more than once.");

            if (host == null)
                throw new ArgumentNullException(nameof(host));
            
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));
            
            if (uitkHost == null)
                throw new ArgumentException($"The host must implement {nameof(IUIToolkitHost)}.", nameof(host));

            SetCurrentApp(this);
            
            m_Hosts.Add(uitkHost);
            uitkHost.HostApplication(this, serviceProvider);
        }

        /// <summary>
        /// Called to shutdown the application.
        /// </summary>
        public virtual void Shutdown()
        {
            shuttingDown?.Invoke();
        }

        /// <summary>
        /// Disposes the current App instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the current App instance.
        /// </summary>
        /// <param name="disposing"> True to dispose managed resources. </param>
        protected virtual void Dispose(bool disposing)
        {
            if (m_Disposed)
                return;

            if (disposing)
            {
                for (var i = m_Hosts.Count - 1; i >= 0; i--)
                {
                    if (m_Hosts[i] != null)
                        m_Hosts[i].Dispose();
                }
                m_Hosts.Clear();
            }
            
            mainPage = null;
            SetCurrentApp(null);
            m_Disposed = true;
        }

        static void SetCurrentApp(App app)
        {
            if (app != null && current != null)
                throw new InvalidOperationException($"There's a conflict between {nameof(App)} instances");
            current = app;
        }
    }
}
