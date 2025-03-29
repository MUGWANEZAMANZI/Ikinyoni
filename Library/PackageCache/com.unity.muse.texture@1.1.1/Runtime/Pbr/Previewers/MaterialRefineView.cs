using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Texture
{
    internal class MaterialRefineView : ArtifactView
    {
        const string k_UssAssetPath = "Packages/com.unity.muse.texture/Runtime/Artifacts/Views/PackageResources/PreviewElement.uss";

        private Image m_Preview;
        private ResultItemVisualElement m_TargetVe;
        private GenericLoader m_GenericLoader;

        private RotationProxy m_RotationManipulator;
        private MaterialPreviewSelector m_MaterialPreviewSelector;
        private MaterialPreviewSettings m_MaterialPreviewSettings;
        private MaterialInspectorView m_MaterialInspectorView;

        private NodesList m_NodesList;

        VisualElement m_BottomCanvasContent;

        AssetsList m_AssetsList;
        private AppUI.UI.GridView m_GridView;

        public MaterialRefineView(Artifact artifact) : base(artifact)
        {
            styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.materialInspectorStyleSheet));

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }


        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            parent?.styleSheets.Add(ResourceManager.Load<StyleSheet>(k_UssAssetPath));

            m_BottomCanvasContent = evt.destinationPanel.visualTree.Q("control-top-content");

            m_MaterialPreviewSelector = new MaterialPreviewSelector();
            CurrentModel.AddToToolbar(m_MaterialPreviewSelector, 1, ToolbarPosition.Left);

            m_MaterialPreviewSettings = new MaterialPreviewSettings();
            m_BottomCanvasContent.Add(m_MaterialPreviewSettings);

            m_Preview = new Image
            {
                style =
                {
                    flexGrow = 1
                }
            };

            Add(m_Preview);

            m_GenericLoader = new GenericLoader(GenericLoader.State.Loading)
            {
                style =
                {
                    backgroundColor = new Color(0.1568628f, 0.1568628f, 0.1568628f, 1f),
                    position = Position.Absolute,
                    width = Length.Percent(100),
                    height = Length.Percent(100)
                }
            };

            Add(m_GenericLoader);

            m_MaterialInspectorView = new MaterialInspectorView();
            m_NodesList = evt.destinationPanel.visualTree.Q<NodesList>();

            m_NodesList.parent.Add(m_MaterialInspectorView);


            parent.style.backgroundColor = new Color(0f, 0f, 0f, 0f); //Removing the Background of the Node

            m_AssetsList = panel.visualTree.Q<AssetsList>();
            m_GridView = m_AssetsList.Q<AppUI.UI.GridView>();
            m_GridView.selectedIndicesChanged += OnSelectionRefreshed;

            RegisterTarget();

            m_MaterialPreviewSelector.OnPreviewSelected += OnPreviewSelected;
            m_MaterialPreviewSettings.OnTargetPrimitiveChanged += OnTargetPrimitiveChanged;
            m_MaterialPreviewSettings.OnHdriChanged += OnHdriChanged;
            m_MaterialPreviewSettings.OnIntensityChanged += OnHdriIntensityChanged;
            m_MaterialInspectorView.OnMaterialPropertiesChanged += OnMaterialPropertiesChanged;

            CurrentModel.OnActiveToolChanged += OnActiveToolChanged;

            UpdateView();
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            m_GridView.selectedIndicesChanged -= OnSelectionRefreshed;
            m_MaterialPreviewSelector.OnPreviewSelected -= OnPreviewSelected;
            m_MaterialPreviewSettings.OnTargetPrimitiveChanged -= OnTargetPrimitiveChanged;
            m_MaterialPreviewSettings.OnHdriChanged -= OnHdriChanged;
            m_MaterialPreviewSettings.OnIntensityChanged -= OnHdriIntensityChanged;
            m_MaterialInspectorView.OnMaterialPropertiesChanged -= OnMaterialPropertiesChanged;

            m_MaterialPreviewSelector.parent.Remove(m_MaterialPreviewSelector);

            if (CurrentModel is not null)
                CurrentModel.OnActiveToolChanged -= OnActiveToolChanged;

            m_AssetsList = null;
            m_GridView = null;

            UnRegisterTarget();

            m_NodesList.parent.Remove(m_MaterialInspectorView);
            m_MaterialInspectorView = null;

            m_NodesList.style.display = DisplayStyle.Flex;
            m_NodesList = null;

            m_BottomCanvasContent.Remove(m_MaterialPreviewSettings);
            m_MaterialPreviewSettings = null;

            m_MaterialPreviewSelector = null;

            Remove(m_Preview);
            m_Preview = null;

            Remove(m_GenericLoader);
            m_GenericLoader = null;
        }

        private void OnSelectionRefreshed(IEnumerable<int> index)
        {
            UnRegisterTarget();
            RegisterTarget();
            UpdateView();
        }

        void RegisterTarget()
        {
            m_TargetVe =
                (ResultItemVisualElement)m_AssetsList.GetView((Artifact)m_GridView.selectedItems.FirstOrDefault());

            if (m_TargetVe == null)
                return;

            m_TargetVe.OnPreviewTypeChanged += OnPreviewTypeChanged;
            m_TargetVe.OnLoadingStateChanged += OnTargetLoadingStateChanged;

            m_RotationManipulator = new RotationProxy(m_TargetVe.RotationManipulator);
            this.AddManipulator(m_RotationManipulator);
        }

        void UnRegisterTarget()
        {
            if (m_TargetVe != null)
            {
                m_TargetVe.OnLoadingStateChanged -= OnTargetLoadingStateChanged;
                m_TargetVe.OnPreviewTypeChanged -= OnPreviewTypeChanged;
                m_TargetVe = null;
            }


            if (m_RotationManipulator != null)
            {
                this.RemoveManipulator(m_RotationManipulator);
                m_RotationManipulator = null;
            }
        }

        private void OnTargetLoadingStateChanged(GenericLoader.State obj)
        {
            UpdateView();
        }

        private void OnMaterialPropertiesChanged()
        {
            m_TargetVe.PbrPreview.RefreshRender();

            if (m_TargetVe.Artifact is not ImageArtifact imageArtifact) return;

            if (imageArtifact.MaterialMetaData is not { Initialized: true })
            {
                imageArtifact.MaterialMetaData = new ImageArtifact.MaterialData(true);
            }

            imageArtifact.MaterialMetaData.GetValuesFromMaterial(m_TargetVe.Material);
        }

        private void OnPreviewSelected(MaterialPreviewItem obj)
        {
            if ((obj == MaterialPreviewItem.Artifact && m_TargetVe?.ActivePreviewState != PreviewType.Image) ||
                (obj != MaterialPreviewItem.Artifact && m_TargetVe?.ActivePreviewState != PreviewType.PBR))
            {
                m_TargetVe?.PerformAction((int)Actions.SwitchPreview, new ActionContext(), null);
            }

            UpdateView();
        }


        private void OnPreviewTypeChanged(PreviewType obj)
        {
            if (obj == PreviewType.PBR && m_MaterialPreviewSelector.SelectedPreviewItem == MaterialPreviewItem.Artifact)
            {
                m_MaterialPreviewSelector.SelectItem(MaterialPreviewItem.Material, false);
            }
            UpdateView();
        }

        public override void UpdateView()
        {
            base.UpdateView();
            if (m_TargetVe == null)
            {
                SetImageView();
                return;
            }

            m_GenericLoader.style.display = m_TargetVe.GetLoadingState() == GenericLoader.State.Loading ||
                                            m_TargetVe.GetLoadingState() == GenericLoader.State.Error
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            m_Preview.style.display = m_GenericLoader.style.display == DisplayStyle.Flex
                ? DisplayStyle.None
                : DisplayStyle.Flex;

            m_MaterialPreviewSelector.SetMaterial(m_TargetVe.Material);

            switch (m_TargetVe.ActivePreviewState)
            {
                case PreviewType.PBR:

                    tooltip = "Shift + Click + Drag to rotate the model.";

                    m_MaterialPreviewSettings.style.display = m_TargetVe.GetLoadingState() == GenericLoader.State.None
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;
                    m_MaterialInspectorView.style.display = m_TargetVe.GetLoadingState() == GenericLoader.State.None
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;
                    m_NodesList.style.display = m_TargetVe.GetLoadingState() == GenericLoader.State.None
                        ? DisplayStyle.None
                        : DisplayStyle.Flex;

                    m_MaterialPreviewSettings.SetMaterial(m_TargetVe.Material);
                    m_MaterialInspectorView.SetMaterial(m_TargetVe.Material);

                    m_MaterialPreviewSettings.SelectPrimitive(m_TargetVe.PbrPreview.CurrentPreviewType);
                    m_MaterialPreviewSettings.SelectHdri(m_TargetVe.PbrPreview.CurrentHdriEnvironment);


                    m_Preview.image = m_MaterialPreviewSelector.SelectedPreviewItem switch
                    {
                        MaterialPreviewItem.Material => m_TargetVe.PbrPreview.previewImage.image,
                        MaterialPreviewItem.Artifact => m_TargetVe.PbrPreview.previewImage.image,
                        MaterialPreviewItem.BaseMap => RenderMap(MuseMaterialProperties.baseMapKey),
                        MaterialPreviewItem.NormalMap => RenderMap(MuseMaterialProperties.normalMapKey),
                        MaterialPreviewItem.MetallicMap => RenderMap(MuseMaterialProperties.metallicMapKey),
                        MaterialPreviewItem.SmoothnessMap => RenderMap(MuseMaterialProperties.smoothnessMapKey),
                        MaterialPreviewItem.HeightMap => RenderMap(MuseMaterialProperties.heightMapKey),
                        MaterialPreviewItem.AOMap => RenderMap(MuseMaterialProperties.ambientOcclusionMapKey),
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    m_RotationManipulator.active =
                        m_MaterialPreviewSelector.SelectedPreviewItem == MaterialPreviewItem.Material;

                    CurrentModel.SetLeftOverlay(m_MaterialInspectorView);
                    break;

                case PreviewType.Image:
                    SetImageView();
                    m_Preview.image = m_TargetVe.ImagePreview;
                    m_MaterialPreviewSelector.SelectItem(MaterialPreviewItem.Artifact, false);
                    CurrentModel.SetLeftOverlay(m_NodesList.content);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            CurrentModel.UpdateToolState();
        }

        void SetImageView()
        {
            if (m_MaterialPreviewSelector is null)
                return;

            tooltip = string.Empty;

            m_MaterialPreviewSettings.style.display = DisplayStyle.None;
            m_MaterialInspectorView.style.display = DisplayStyle.None;
            m_NodesList.style.display = DisplayStyle.Flex;
        }

        UnityEngine.Texture RenderMap(int propertyId)
        {
            return m_TargetVe.Material != null ? m_TargetVe.Material.GetTexture(propertyId) : null;
        }

        private void OnHdriChanged(HdriEnvironment environment)
        {
            m_TargetVe.PbrPreview.SetHdriEnvironment(environment);
        }

        private void OnHdriIntensityChanged(float intensity)
        {
            m_TargetVe.PbrPreview.SetHdriIntensity(intensity);
        }

        private void OnTargetPrimitiveChanged(PrimitiveObjectTypes obj)
        {
            m_TargetVe.PbrPreview.SetPreviewType(obj);
        }

        private void OnActiveToolChanged(ICanvasTool obj)
        {
            if (obj == null || m_TargetVe?.ActivePreviewState == PreviewType.Image) return;

            m_TargetVe?.SetCurrentState(PreviewType.Image);
            UpdateView();
        }

        private void OnCloseButtonClicked()
        {
            CurrentModel.FinishRefineArtifact();
        }

        public override UnityEngine.Texture Preview => m_Preview.image;

        public override VisualElement PaintSurfaceElement => m_Preview;
    }
}