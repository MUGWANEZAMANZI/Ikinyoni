using System;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common;
using UnityEngine;
using UnityEngine.UIElements;
using FloatField = Unity.Muse.AppUI.UI.FloatField;
using Toggle = Unity.Muse.AppUI.UI.Toggle;
using Vector2Field = Unity.Muse.AppUI.UI.Vector2Field;

namespace Unity.Muse.Texture
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class MaterialInspectorView : ScrollView
    {
#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<MaterialInspectorView, UxmlTraits> { }
#endif

        MaterialPreviewItem m_SelectedItem;
        Material m_Material;

        bool m_AttachedToPanel;

        Vector2Field m_TilingField;
        Vector2Field m_OffsetField;
        FloatField m_RotationField;
        Toggle m_FlipVerticalField;
        Toggle m_FlipHorizontalField;
        Toggle m_UseDisplacement;

        TouchSliderFloat m_HeightIntensityField;
        FactorSliderFloat m_MetallicIntensityField;
        FactorSliderFloat m_SmoothnessIntensityField;
        Toggle m_UseMetallicField;
        Toggle m_UseSmoothnessField;

        public event Action OnMaterialPropertiesChanged;

        public MaterialInspectorView()
        {
            GenerateVisualTree();
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        void GenerateVisualTree()
        {
            styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.materialInspectorStyleSheet));

            name = "MaterialInspectorView";
            AddToClassList("muse-material-inspector--view");

            GenerateEditSection();
            GenerateMapsSection();
        }

        void GenerateEditSection()
        {
            var editSection = new ExVisualElement();
            editSection.AddToClassList("muse-material--section");
            Add(editSection);

            var previewTitle = new Text()
            {
                text = "Preview"
            };

            previewTitle.AddToClassList("muse-inspector__title");

            editSection.Add(previewTitle);

            var tilingContainer = new ExVisualElement()
            {
                tooltip = "Tiling the maps"
            };
            tilingContainer.AddToClassList("muse-label--component");

            editSection.Add(tilingContainer);

            tilingContainer.Add(new Text()
            {
                text = "Tiling"
            });

            tilingContainer.Add(new Vector2Field()
            {
                name = "TilingField",
                size = Size.M
            });

            var offsetContainer = new ExVisualElement()
            {
                tooltip = "Offset the maps"
            };
            offsetContainer.AddToClassList("muse-label--component");
            editSection.Add(offsetContainer);

            offsetContainer.Add(new Text()
            {
                text = "Offset"
            });

            offsetContainer.Add(new Vector2Field()
            {
                name = "OffsetField",
                size = Size.M
            });

            var rotationContainer = new ExVisualElement()
            {
                tooltip = "Rotate the maps"
            };
            rotationContainer.AddToClassList("muse-label--component");
            editSection.Add(rotationContainer);

            rotationContainer.Add(new Text()
            {
                text = "Rotation"
            });

            rotationContainer.Add(new FloatField()
            {
                name = "RotationField",
                size = Size.M,
                unit = "DEG"
            });

            var verticalFlipContainer = new ExVisualElement()
            {
                tooltip = "Flip the maps vertically"
            };
            verticalFlipContainer.AddToClassList("muse-label--component-toggle");
            editSection.Add(verticalFlipContainer);

            verticalFlipContainer.Add(new Text()
            {
                text = "Flip Vertical"
            });

            verticalFlipContainer.Add(new Toggle()
            {
                name = "VerticalFlipField"
            });

            var horizontalFlipContainer = new ExVisualElement()
            {
                tooltip = "Flip the maps horizontally"
            };
            horizontalFlipContainer.AddToClassList("muse-label--component-toggle");
            editSection.Add(horizontalFlipContainer);

            horizontalFlipContainer.Add(new Text()
            {
                text = "Flip Horizontal"
            });

            horizontalFlipContainer.Add(new Toggle()
            {
                name = "HorizontalFlipField"
            });

            var useDisplacementContainer = new ExVisualElement()
            {
                tooltip = "Use Vertex Displacement"
            };
            useDisplacementContainer.AddToClassList("muse-label--component-toggle");
            editSection.Add(useDisplacementContainer);

            useDisplacementContainer.Add(new Text()
            {
                text = "Vertex Displacement"
            });

            useDisplacementContainer.Add(new Toggle()
            {
                name = "UseDisplacementField"
            });
        }

        void GenerateMapsSection()
        {
            var mapsSection = new ExVisualElement();
            mapsSection.AddToClassList("muse-material--section");
            Add(mapsSection);

            var mapsTitle = new Text()
            {
                text = "Maps"
            };

            mapsTitle.AddToClassList("muse-inspector__title");

            mapsSection.Add(mapsTitle);

            mapsSection.Add(new FactorSliderFloat()
            {
                name = "HeightIntensityField",
                label = "Height",
                tooltip = "Height",
                lowValue = -3f,
                highValue = 3f,
                value = 0f,
                size = Size.M,
                formatString = "F1"
            });

            mapsSection.Add(new FactorSliderFloat()
            {
                name = "MetallicIntensityField",
                label = "Metallic",
                tooltip = "Metallic intensity",
                lowValue = 0f,
                highValue = 20f,
                value = 1f,
                size = Size.M,
                formatString = "F1"
            });

            mapsSection.Add(new FactorSliderFloat()
            {
                name = "SmoothnessIntensityField",
                label = "Smoothness",
                tooltip = "Smoothness intensity",
                lowValue = 0f,
                highValue = 3f,
                value = 1f,
                size = Size.M,
                formatString = "F1"
            });

            var useDisplacementContainer = new ExVisualElement()
            {
                tooltip = "Use Metallic Map"
            };
            useDisplacementContainer.AddToClassList("muse-label--component-toggle");
            mapsSection.Add(useDisplacementContainer);

            useDisplacementContainer.Add(new Text()
            {
                text = "Use Metallic Map"
            });

            useDisplacementContainer.Add(new Toggle()
            {
                name = "UseMetallicField"
            });

            var useSmoothnessContainer = new ExVisualElement()
            {
                tooltip = "Use Smoothness Map"
            };
            useSmoothnessContainer.AddToClassList("muse-label--component-toggle");
            mapsSection.Add(useSmoothnessContainer);

            useSmoothnessContainer.Add(new Text()
            {
                text = "Use Smoothness Map"
            });

            useSmoothnessContainer.Add(new Toggle()
            {
                name = "UseSmoothnessField"
            });
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            m_AttachedToPanel = true;

            m_TilingField = this.Q<Vector2Field>("TilingField");
            m_OffsetField = this.Q<Vector2Field>("OffsetField");
            m_RotationField = this.Q<FloatField>("RotationField");
            m_FlipVerticalField = this.Q<Toggle>("VerticalFlipField");
            m_FlipHorizontalField = this.Q<Toggle>("HorizontalFlipField");
            m_UseDisplacement = this.Q<Toggle>("UseDisplacementField");
            m_HeightIntensityField = this.Q<TouchSliderFloat>("HeightIntensityField");
            m_MetallicIntensityField = this.Q<FactorSliderFloat>("MetallicIntensityField");
            m_SmoothnessIntensityField = this.Q<FactorSliderFloat>("SmoothnessIntensityField");
            m_UseMetallicField = this.Q<Toggle>("UseMetallicField");
            m_UseSmoothnessField = this.Q<Toggle>("UseSmoothnessField");

            InitializePropertiesValues();
            RegisterPropertyChangeCallbacks();
        }

        void InitializePropertiesValues()
        {
            if (m_Material == null || !m_AttachedToPanel)
                return;

            m_TilingField.SetValueWithoutNotify(m_Material.GetVector(MuseMaterialProperties.tilingKey));
            m_OffsetField.SetValueWithoutNotify(m_Material.GetVector(MuseMaterialProperties.offsetKey));
            m_RotationField.SetValueWithoutNotify(m_Material.GetFloat(MuseMaterialProperties.rotationKey));
            m_FlipVerticalField.SetValueWithoutNotify(m_Material.GetFloat(MuseMaterialProperties.flipVertical) > 0.0);
            m_FlipHorizontalField.SetValueWithoutNotify(
                m_Material.GetFloat(MuseMaterialProperties.flipHorizontal) > 0.0);
            m_UseDisplacement.SetValueWithoutNotify(m_Material.GetFloat(MuseMaterialProperties.useDisplacement) > 0.0);
            m_HeightIntensityField.SetValueWithoutNotify(m_Material.GetFloat(MuseMaterialProperties.heightIntensity));
            m_MetallicIntensityField.SetValueWithoutNotify(
                m_Material.GetFloat(MuseMaterialProperties.metallicIntensity));
            m_SmoothnessIntensityField.SetValueWithoutNotify(
                m_Material.GetFloat(MuseMaterialProperties.smoothnessIntensity));
            m_UseMetallicField.SetValueWithoutNotify(m_Material.GetFloat(MuseMaterialProperties.useMetallic) > 0.0);
            m_UseSmoothnessField.SetValueWithoutNotify(m_Material.GetFloat(MuseMaterialProperties.useSmoothness) > 0.0);

            UpdateVisuals();
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            m_AttachedToPanel = false;

            UnRegisterPropertyChangeCallbacks();

            m_TilingField = null;
            m_OffsetField = null;
            m_RotationField = null;
            m_FlipVerticalField = null;
            m_FlipHorizontalField = null;
            m_UseDisplacement = null;
            m_HeightIntensityField = null;
            m_MetallicIntensityField = null;
            m_SmoothnessIntensityField = null;
        }

        void RegisterPropertyChangeCallbacks()
        {
            m_TilingField.RegisterValueChangingCallback(evt => MaterialPropertiesChanged());
            m_OffsetField.RegisterValueChangingCallback(evt => MaterialPropertiesChanged());
            m_RotationField.RegisterValueChangingCallback(evt => MaterialPropertiesChanged());
            m_FlipVerticalField.RegisterValueChangedCallback(evt => MaterialPropertiesChanged());
            m_FlipHorizontalField.RegisterValueChangedCallback(evt => MaterialPropertiesChanged());
            m_UseDisplacement.RegisterValueChangedCallback(evt => MaterialPropertiesChanged());
            m_HeightIntensityField.RegisterValueChangingCallback(evt => MaterialPropertiesChanged());
            m_HeightIntensityField.RegisterValueChangedCallback(evt => MaterialPropertiesChanged());
            m_MetallicIntensityField.RegisterValueChangingCallback(evt => MaterialPropertiesChanged());
            m_MetallicIntensityField.RegisterValueChangedCallback(evt => MaterialPropertiesChanged());
            m_SmoothnessIntensityField.RegisterValueChangingCallback(evt => MaterialPropertiesChanged());
            m_SmoothnessIntensityField.RegisterValueChangedCallback(evt => MaterialPropertiesChanged());
            m_UseMetallicField.RegisterValueChangedCallback(evt => MaterialPropertiesChanged());
            m_UseSmoothnessField.RegisterValueChangedCallback(evt => MaterialPropertiesChanged());
        }

        void UnRegisterPropertyChangeCallbacks()
        {
            m_TilingField.UnregisterValueChangingCallback(evt => MaterialPropertiesChanged());
            m_OffsetField.UnregisterValueChangingCallback(evt => MaterialPropertiesChanged());
            m_RotationField.UnregisterValueChangingCallback(evt => MaterialPropertiesChanged());
            m_FlipVerticalField.UnregisterValueChangedCallback(evt => MaterialPropertiesChanged());
            m_FlipHorizontalField.UnregisterValueChangedCallback(evt => MaterialPropertiesChanged());
            m_UseDisplacement.UnregisterValueChangedCallback(evt => MaterialPropertiesChanged());
            m_HeightIntensityField.UnregisterValueChangingCallback(evt => MaterialPropertiesChanged());
            m_MetallicIntensityField.UnregisterValueChangingCallback(evt => MaterialPropertiesChanged());
            m_SmoothnessIntensityField.UnregisterValueChangingCallback(evt => MaterialPropertiesChanged());
            m_UseMetallicField.UnregisterValueChangedCallback(evt => MaterialPropertiesChanged());
            m_UseSmoothnessField.UnregisterValueChangedCallback(evt => MaterialPropertiesChanged());
        }

        void SetPropertiesValues()
        {
            if (m_Material == null || !m_AttachedToPanel)
                return;

            m_Material.SetVector(MuseMaterialProperties.tilingKey, m_TilingField.value);
            m_Material.SetVector(MuseMaterialProperties.offsetKey, m_OffsetField.value);
            m_Material.SetFloat(MuseMaterialProperties.rotationKey, m_RotationField.value);
            m_Material.SetFloat(MuseMaterialProperties.flipVertical, m_FlipVerticalField.value ? 1.0f : 0.0f);
            m_Material.SetFloat(MuseMaterialProperties.flipHorizontal, m_FlipHorizontalField.value ? 1.0f : 0.0f);
            m_Material.SetFloat(MuseMaterialProperties.useDisplacement, m_UseDisplacement.value ? 1.0f : 0.0f);
            m_Material.SetFloat(MuseMaterialProperties.heightIntensity, m_HeightIntensityField.value);
            m_Material.SetFloat(MuseMaterialProperties.metallicIntensity, m_MetallicIntensityField.value);
            m_Material.SetFloat(MuseMaterialProperties.smoothnessIntensity, m_SmoothnessIntensityField.value);
            m_Material.SetFloat(MuseMaterialProperties.useMetallic, m_UseMetallicField.value ? 1.0f : 0.0f);
            m_Material.SetFloat(MuseMaterialProperties.useSmoothness, m_UseSmoothnessField.value ? 1.0f : 0.0f);
        }

        public void SetMaterial(Material material)
        {
            m_Material = material;
            InitializePropertiesValues();
        }

        public void SetSelectedPreviewItem(MaterialPreviewItem previewItem)
        {
            m_SelectedItem = previewItem;
        }

        void MaterialPropertiesChanged()
        {
            UpdateVisuals();
            SetPropertiesValues();
            OnMaterialPropertiesChanged?.Invoke();
        }

        void UpdateVisuals()
        {
            if (m_UseMetallicField.value)
            {
                UpdateSliderVisual(m_MetallicIntensityField, 0.0f, 20.0f, m_MetallicIntensityField.value, "x");
            }
            else
            {
                UpdateSliderVisual(m_MetallicIntensityField, 0.0f, 1.0f, m_MetallicIntensityField.value, string.Empty);
            }

            if (m_UseSmoothnessField.value)
            {
                UpdateSliderVisual(m_SmoothnessIntensityField, 0.0f, 3.0f, m_SmoothnessIntensityField.value, "x");
            }
            else
            {
                UpdateSliderVisual(m_SmoothnessIntensityField, 0.0f, 1.0f, m_SmoothnessIntensityField.value, string.Empty);
            }
        }

        void UpdateSliderVisual(FactorSliderFloat slider, float lowValue, float highValue, float value,
            string trailingString)
        {
            var newValue = Mathf.Clamp(value, lowValue, highValue);
            slider.lowValue = lowValue;
            slider.highValue = highValue;
            slider.SetValueWithoutNotify(newValue);
            slider.TrailingString = trailingString;
        }
    }
}