using System;
using System.Collections.Generic;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Texture
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    internal partial class MaterialPreviewSettings : VisualElement
    {
#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<MaterialPreviewSettings, UxmlTraits> { }
#endif

        private Dropdown m_PrimitivesDropdown;
        private Dropdown m_HdriDropdown;
        private TouchSliderFloat m_IntensitySlider;

        public event Action<PrimitiveObjectTypes> OnTargetPrimitiveChanged;
        internal event Action<HdriEnvironment> OnHdriChanged;
        internal event Action<float> OnIntensityChanged;

        private List<string> m_PrimitivesDropdownSrc = new()
        {
            "Sphere",
            "Cube",
            "Plane",
            "Cylinder",
            //"Custom"
        };

        private List<string> m_HdriDropdownSrc = new()
        {
            "Default Environment",
            "Outside Neutral",
            "Inside",
            "Day Outside",
            "Night Outside"
        };

        public MaterialPreviewSettings()
        {
            GenerateVisualTree();
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        private void GenerateVisualTree()
        {
            styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.materialInspectorStyleSheet));

            name = "PreviewSettings";
            AddToClassList("muse-material--container-horizontal");
            AddToClassList("muse-material-preview--settings");

            var primitive = new Dropdown()
            {
                name= "PrimitivesSelector",
                tooltip = "Primitive Selection",

            };

            primitive.AddToClassList("muse-preview--primitives");

            Add(primitive);

            var hdri = new Dropdown()
            {
                name= "HdriSelector",
                tooltip = "Reflection Probe Selection",
            };

            hdri.AddToClassList("muse-preview--hdri");

            Add(hdri);

            var touchSlider = new TouchSliderFloat()
            {
                name = "Intensity",
                label = "Intensity",
                lowValue = 0.0f,
                highValue = 100.0f,
                tooltip = "Light Intensity",
                value = MaterialPreviewSceneHandler.DefaultHdriIntensity,
                formatString = "F1"
            };

            touchSlider.AddToClassList("muse-preview--intensity");

            Add(touchSlider);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            m_PrimitivesDropdown = this.Q<Dropdown>("PrimitivesSelector");
            m_HdriDropdown = this.Q<Dropdown>("HdriSelector");
            m_IntensitySlider = this.Q<TouchSliderFloat>("Intensity");

            m_PrimitivesDropdown.RegisterValueChangedCallback(OnPrimitiveSelected);
            m_HdriDropdown.RegisterValueChangedCallback(OnHdriSelected);
            m_IntensitySlider.RegisterValueChangingCallback(OnIntensitySelected);
            m_IntensitySlider.RegisterValueChangedCallback(OnIntensitySelected);

            InitializeDropDowns();
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            m_PrimitivesDropdown.UnregisterValueChangedCallback(OnPrimitiveSelected);
            m_HdriDropdown.UnregisterValueChangedCallback(OnHdriSelected);
            m_IntensitySlider.UnregisterValueChangedCallback(OnIntensitySelected);

            m_PrimitivesDropdown = null;
            m_HdriDropdown = null;
        }

        private void InitializeDropDowns()
        {
            m_PrimitivesDropdown.bindItem = (item, i) => item.label = m_PrimitivesDropdownSrc[i];
            m_PrimitivesDropdown.sourceItems = m_PrimitivesDropdownSrc;
            m_PrimitivesDropdown.SetValueWithoutNotify(new []{ 0 });

            m_HdriDropdown.bindItem = (item, i) => item.label = m_HdriDropdownSrc[i];
            m_HdriDropdown.sourceItems = m_HdriDropdownSrc;
            m_HdriDropdown.SetValueWithoutNotify(new []{ 0 });
        }

        private void OnPrimitiveSelected(ChangeEvent<IEnumerable<int>> evt)
        {
            using var selection = evt.newValue.GetEnumerator();
            if (!selection.MoveNext())
                return;

            OnTargetPrimitiveChanged?.Invoke(selection.Current switch
            {
                0 => PrimitiveObjectTypes.Sphere,
                1 => PrimitiveObjectTypes.Cube,
                2 => PrimitiveObjectTypes.Plane,
                3 => PrimitiveObjectTypes.Cylinder,
                4 => PrimitiveObjectTypes.Custom,
                _ => throw new ArgumentOutOfRangeException()
            });
        }


        private void OnHdriSelected(ChangeEvent<IEnumerable<int>> evt)
        {
            using var selection = evt.newValue.GetEnumerator();
            if (!selection.MoveNext())
                return;

            OnHdriChanged?.Invoke(selection.Current switch
            {
                0 => HdriEnvironment.Default,
                1 => HdriEnvironment.OutsideNeutral,
                2 => HdriEnvironment.Inside,
                3 => HdriEnvironment.DayOutside,
                4 => HdriEnvironment.NightOutside,
                _ => throw new ArgumentOutOfRangeException()
            });
        }
        private void OnIntensitySelected(ChangingEvent<float> evt)
        {
            OnIntensityChanged?.Invoke(evt.newValue);
        }
        private void OnIntensitySelected(ChangeEvent<float> evt)
        {
            OnIntensityChanged?.Invoke(evt.newValue);
        }

        public void SelectPrimitive(PrimitiveObjectTypes type)
        {
            m_PrimitivesDropdown?.SetValueWithoutNotify(new []{ (int)type });
        }

        internal void SelectHdri(HdriEnvironment environment)
        {
            m_HdriDropdown?.SetValueWithoutNotify(new []{ (int)environment });
        }

        internal void SetIntensity(float intensity)
        {
            m_IntensitySlider?.SetValueWithoutNotify(intensity);
        }

        public void SetMaterial(Material mMaterial)
        {
        }
    }
}
