using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Texture
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class MaterialMapPreview : MaterialPreviewElement
    {
#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<MaterialMapPreview, UxmlTraits>
        {
        }
#endif

        private MaterialPreviewItem m_SelectedPreviewItem;

        internal PrimitiveObjectTypes CurrentPreviewType => m_RenderConfiguration.previewType;
        internal HdriEnvironment CurrentHdriEnvironment => m_RenderConfiguration.environment;
        internal float CurrentFOV => m_RenderConfiguration.fov.Value;

        bool m_UserDefinedTooltip = false;

        public MaterialMapPreview()
        {
        }

        public new void SetMaterial(Material material)
        {
            base.SetMaterial(material);
        }

        public void SetMesh(Mesh mesh)
        {
            m_RenderConfiguration = m_RenderConfiguration with
            {
                customMesh = mesh,
                previewType = PrimitiveObjectTypes.Custom,
                fov = MaterialPreviewer.GetFOVForBounds(s_MaterialPreviewer.sceneHandler.Camera, MaterialPreviewer.CalculateBoundingSphere(mesh.bounds))
            };
            RefreshRender();
        }

        public void SetSelectedPreviewItem(MaterialPreviewItem previewItem)
        {
            m_SelectedPreviewItem = previewItem;
            Render(m_RenderConfiguration);
        }

        public void SetPreviewType(PrimitiveObjectTypes previewType)
        {
            m_RenderConfiguration = m_RenderConfiguration with { previewType = previewType };
            RefreshRender();
        }

        internal void SetHdriEnvironment(HdriEnvironment environment)
        {
            m_RenderConfiguration = m_RenderConfiguration with { environment = environment };
            RefreshRender();
        }

        internal void SetHdriIntensity(float intensity)
        {
            m_RenderConfiguration = m_RenderConfiguration with { intensity = intensity };
            RefreshRender();
        }

        internal void SetHdriEnvironmentAndIntensity(HdriEnvironment environment, float intensity)
        {
            m_RenderConfiguration = m_RenderConfiguration with { environment = environment, intensity = intensity };
            RefreshRender();
        }

        internal void SetCustomModelGuid(string customModelGuid)
        {
            m_RenderConfiguration = m_RenderConfiguration with
            {
                customModelGuid = customModelGuid, previewType = PrimitiveObjectTypes.Custom, fov = null
            };
            RefreshRender();
        }

        internal override void Render(MaterialPreviewer.RenderConfiguration configuration)
        {
            switch (m_SelectedPreviewItem)
            {
                case MaterialPreviewItem.Material:
                    if (!m_UserDefinedTooltip)
                        tooltip = "Shift + Drag to rotate the model.";
                    m_PreviewImage.image = configuration.renderTexture;
                    base.Render(configuration);
                    break;
                case MaterialPreviewItem.Artifact:
                case MaterialPreviewItem.BaseMap:
                    RenderMap(MuseMaterialProperties.baseMapKey);
                    break;
                case MaterialPreviewItem.NormalMap:
                    RenderMap(MuseMaterialProperties.normalMapKey);
                    break;
                case MaterialPreviewItem.MetallicMap:
                    RenderMap(MuseMaterialProperties.metallicMapKey);
                    break;
                case MaterialPreviewItem.SmoothnessMap:
                    RenderMap(MuseMaterialProperties.smoothnessMapKey);
                    break;
                case MaterialPreviewItem.HeightMap:
                    RenderMap(MuseMaterialProperties.heightMapKey);
                    break;
                case MaterialPreviewItem.AOMap:
                    RenderMap(MuseMaterialProperties.ambientOcclusionMapKey);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal void AllowUserDefinedTooltip(bool userDefinedTooltip)
        {
            m_UserDefinedTooltip = userDefinedTooltip;
        }

        void RenderMap(int propertyId)
        {
            if (!m_UserDefinedTooltip)
                tooltip = "";
            if (m_RenderConfiguration.material == null)
                return;

            m_PreviewImage.image = m_RenderConfiguration.material.GetTexture(propertyId);
        }
    }
}
