using System;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common
{
    enum GenerationSettingsView
    {
        HideUse
    }

    class GenerationSettings : ExVisualElement
    {
        const string k_UseAll = "use-all";
        Model m_CurrentModel;
        Artifact m_Artifact;
        Action m_Dismiss;
        private VisualElement m_OperatorContainer;

        internal static void ShowGenerationSettings(Artifact artifact, VisualElement parent, Model currentModel)
        {
            var settings = new GenerationSettings(artifact);
            var modal = Popover.Build(parent, settings);

            settings.m_Dismiss += modal.Dismiss;


            modal.SetOffset(-34);
            modal.SetAnchor(parent);
            modal.SetPlacement(currentModel.isRefineMode ? PopoverPlacement.Left : PopoverPlacement.Right);
            modal.Show();
        }

        public GenerationSettings(Artifact artifact)
        {
            this.ApplyTemplate(PackageResources.generationSettingsTemplate);

            passMask = Passes.Clear | Passes.BackgroundColor;

            m_OperatorContainer = this.Q<VisualElement>(classes: "operators");
            this.Q<ActionButton>(k_UseAll).clicked += UseAll;
            m_Artifact = artifact;

            this.RegisterContextChangedCallback<Model>(context => SetModel(context.context));
        }

        void Initialize()
        {
            var operatorContainer = this.Q<VisualElement>(classes: "operators");
            operatorContainer.Clear();
            foreach (var op in m_Artifact.GetOperators())
            {
                if (!op.Enabled() || op.Hidden)
                    continue;

                var isCustomSection = false;
                var view = op.GetSettingsView(m_CurrentModel, ref isCustomSection, m_Dismiss);
                if (view == null)
                    continue;

                m_OperatorContainer.Add(isCustomSection ? view : CreateView(op.Label, view, () => Use(op)));
            }

            m_OperatorContainer.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            var preferredWidth = 0.0f;
            var query = m_OperatorContainer.Query<Text>(name: "label");

            var texts = query.ToList();
            foreach (var text in texts)
            {
                if(text.worldBound.width > preferredWidth)
                    preferredWidth = text.worldBound.width;
            }

            foreach (var text in texts)
            {
               text.style.width =preferredWidth;
            }

            m_OperatorContainer.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }
        void SetModel(Model model)
        {
            m_CurrentModel = model;
            Initialize();
        }

        public static VisualElement CreateView(string label, VisualElement view, Action useAction)
        {
            var row = new VisualElement();
            row.AddToClassList("row");
            row.AddToClassList("operator-settings");

            var center = new VisualElement {name = "content"};
            center.Add(view);

            row.Add(new Text(label) {name = "label"});
            row.Add(center);
            if (!(view.userData is GenerationSettingsView settingsView && settingsView == GenerationSettingsView.HideUse))
                row.Add(new ActionButton(useAction) {name = "use", label = "Use"});

            if (!view.enabledSelf)
                row.SetEnabled(false);

            return row;
        }

        void Use(IOperator op)
        {
            m_CurrentModel.UpdateOperators(op.Clone());
            m_Dismiss?.Invoke();
        }

        void UseAll()
        {
            var operators = m_Artifact.CloneOperators();
            var proxyOperators = operators.FindAll(x => x is IProxyOperator);

            for(var i = operators.Count - 1; i >= 0; i--)
            {
                if (operators[i] is not IOperatorAddHandler handler ||
                    handler.EvaluateAddOperator(m_CurrentModel)) continue;

                if(operators[i] is IOperatorRemoveHandler removeHandler)
                    removeHandler.OnOperatorRemoved(operators);

                operators.RemoveAt(i);
            }

            foreach (var proxy in proxyOperators)
            {
                var clonedOperators = (proxy as IProxyOperator)?.CloneProxyOperators();
                foreach (var op in clonedOperators)
                {
                    var sameOpFound = false;
                    foreach (var @operator in operators.FindAll(x => x.GetOperatorData().type == op.GetOperatorData().type))
                    {
                        sameOpFound = true;
                        @operator.SetOperatorData(op.GetOperatorData());
                    }
                    if(!sameOpFound)
                        operators.Add(op);
                }

                operators.Remove(proxy);
            }

            var currentGenerateOperator = m_CurrentModel.CurrentOperators.GetOperator<GenerateOperator>();
            var generateOperatorIndex = operators.FindIndex(o => o is GenerateOperator);

            if (generateOperatorIndex == 0)
            {
                // We don't want to override the amount of Images to generate
                operators[generateOperatorIndex] = currentGenerateOperator;
            }

            if (generateOperatorIndex > 0)
            {
                //Put the Generate Operator as the first one by swapping it with the first operator
                var otherOp = operators[0];
                operators[generateOperatorIndex] = otherOp;
                operators[0] = currentGenerateOperator;
            }

            m_CurrentModel.UpdateOperators(operators, true);
            m_Dismiss?.Invoke();
        }
    }
}