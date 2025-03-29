using System;
using System.Globalization;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Texture
{
    class FactorSliderFloat : TouchSliderFloat
    {
        public string TrailingString
        {
            get => m_TrailingString;
            set
            {
                m_TrailingString = value;
                UpdateLabel();
            }
        }

        LocalizedTextElement m_ValueLabel;
        string m_TrailingString;

        public FactorSliderFloat(string trailingString = "x") : base()
        {
            m_TrailingString = trailingString;
            m_ValueLabel = this.Q<LocalizedTextElement>(classes: valueUssClassName);
            this.RegisterValueChangingCallback(OnValueLabelChanging);
            UpdateLabel();
        }

        void OnValueLabelChanging(ChangingEvent<float> evt)
        {
           UpdateLabel();
        }

        void UpdateLabel()
        {
            if(m_ValueLabel == null)
                return;

            m_ValueLabel.text = value.ToString(formatString, CultureInfo.InvariantCulture.NumberFormat) + m_TrailingString;
        }

        public override void SetValueWithoutNotify(float newValue)
        {
            base.SetValueWithoutNotify(newValue);
            UpdateLabel();
        }
    }
}