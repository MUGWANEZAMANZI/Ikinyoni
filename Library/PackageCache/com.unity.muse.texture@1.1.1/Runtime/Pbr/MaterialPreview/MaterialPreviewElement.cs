using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.UIElements;
using static Unity.Muse.Texture.MaterialPreviewer;

namespace Unity.Muse.Texture
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    internal partial class MaterialPreviewElement : VisualElement
    {
        protected internal readonly Image m_PreviewImage;
        readonly Vector2Int k_PreviewSize = new(2048, 2048);
        const float k_CameraLookDistance = 5f;

        internal protected RenderConfiguration m_RenderConfiguration;
        internal RotationManipulator RotationManipulator { get; private set; }

        protected internal static MaterialPreviewer s_MaterialPreviewer;
        internal static MaterialPreviewSceneHandler sceneHandler => s_MaterialPreviewer?.sceneHandler;

        internal Image previewImage => m_PreviewImage;

        internal RenderConfiguration renderConfiguration => m_RenderConfiguration;

        protected internal bool m_PreviewEnabled = true;

        internal event Action<Vector2> OnRotationChanged;

        public MaterialPreviewElement()
        {
            RotationManipulator ??= new RotationManipulator();
            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            style.flexGrow = 1;

            m_PreviewImage = new Image
            {
                style =
                {
                    flexGrow = 1f
                }
            };

            Add(m_PreviewImage);

            if (s_MaterialPreviewer == null || !s_MaterialPreviewer.isValid)
            {
                s_MaterialPreviewer?.Dispose();
                s_MaterialPreviewer = new MaterialPreviewer();
            }
            m_PreviewImage.image = s_MaterialPreviewer.CreateDefaultRenderTexture();

            m_RenderConfiguration = new RenderConfiguration(
                null,
                (RenderTexture)m_PreviewImage.image,
                RotationManipulator.TotalRotation,
                k_CameraLookDistance,
                PrimitiveObjectTypes.Sphere,
                HdriEnvironment.Default,
                MaterialPreviewSceneHandler.DefaultHdriIntensity,
                false,
                null,
                null,
                11.25f);

            #if UNITY_EDITOR
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            #endif
        }

        #if UNITY_EDITOR
        private void OnBeforeAssemblyReload()
        {
            if (s_MaterialPreviewer == null)
                return;

            s_MaterialPreviewer.Dispose();
            s_MaterialPreviewer = null;
        }
        #endif


        internal void SetMaterial(Material material)
        {
            m_RenderConfiguration = m_RenderConfiguration with { material = material };
            Render(m_RenderConfiguration);
        }

        void OnAttach(AttachToPanelEvent evt)
        {
            RotationManipulator.OnDrag += OnDrag;
            this.AddManipulator(RotationManipulator);
        }

        void OnDetach(DetachFromPanelEvent evt)
        {
            this.RemoveManipulator(RotationManipulator);
            RotationManipulator.OnDrag -= OnDrag;
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            var backgroundColor = resolvedBackgroundColor;
            backgroundColor.a = 0; // keep it transparent for RPs that support alpha rendering
            m_RenderConfiguration = m_RenderConfiguration with { backgroundColor = backgroundColor };
            Render(m_RenderConfiguration);
        }

        void OnDrag(Vector2 dragValue)
        {
            m_RenderConfiguration = m_RenderConfiguration with { cameraRotation = RotationManipulator.TotalRotation };
            Render(m_RenderConfiguration);
            OnRotationChanged?.Invoke(dragValue);
        }

        internal virtual void Render(RenderConfiguration renderConfiguration)
        {
            s_MaterialPreviewer.Render(renderConfiguration);
        }

        internal void RefreshRender()
        {
            Render(m_RenderConfiguration);
        }

        Color resolvedBackgroundColor
        {
            get
            {
                var currentColor = Color.clear;
                VisualElement currentElement = this;

                while (currentElement != null)
                {
                    if (currentColor.a >= 1)
                        break;

                    var backgroundColor = currentElement.resolvedStyle.backgroundColor;
                    currentColor = AlphaBlend(currentColor, backgroundColor);

                    currentElement = currentElement.parent;
                }

                return currentColor;
            }
        }

        static Color AlphaBlend(Color bottomColor, Color topColor)
        {
            if (bottomColor.a == 0)
                return topColor;

            if (topColor.a == 0 || bottomColor.a >= 1)
                return bottomColor;

            var topAlpha = topColor.a;
            var bottomAlpha = Mathf.Clamp01(bottomColor.a * (1 - topAlpha));

            var outAlpha = Mathf.Clamp01(topAlpha + bottomAlpha);
            var outRed = (topColor.r * topAlpha + bottomColor.r * bottomAlpha) / outAlpha;
            var outGreen = (topColor.g * topAlpha + bottomColor.g * bottomAlpha) / outAlpha;
            var outBlue = (topColor.b * topAlpha + bottomColor.b * bottomAlpha) / outAlpha;

            return new Color(outRed, outGreen, outBlue, outAlpha);
        }

        internal void SetRotation(Vector2 rotation)
        {
            RotationManipulator?.SetRotation(rotation);
        }

#if ENABLE_UXML_TRAITS

        public new class UxmlFactory : UxmlFactory<MaterialPreviewElement, UxmlTraits> { }

#endif
    }
}
