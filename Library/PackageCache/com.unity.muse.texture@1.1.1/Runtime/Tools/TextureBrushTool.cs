using System.Collections.Generic;
using Unity.Muse.Common;
using Unity.Muse.Common.Tools;
using UnityEngine;

namespace Unity.Muse.Texture.Tools
{
    internal class TextureBrushTool: BrushTool<MaskOperator>
    {
        protected override void Initialize()
        {
            base.Initialize();

            m_CurrentModel.OnActiveToolChanged -= OnActiveToolChanged;
            m_CurrentModel.OnActiveToolChanged += OnActiveToolChanged;

            m_CurrentModel.OnRefineArtifact -= OnRefineArtifact;
            m_CurrentModel.OnRefineArtifact += OnRefineArtifact;

            m_CurrentModel.OnOperatorUpdated -= OnOperatorUpdated;
            m_CurrentModel.OnOperatorUpdated += OnOperatorUpdated;

            m_CurrentModel.OnMaskPaintDone -= OnMaskPaintDone;
            m_CurrentModel.OnMaskPaintDone += OnMaskPaintDone;
        }

        void OnMaskPaintDone(Texture2D texture, bool isClear)
        {
            UpdatePromptOperator(isClear);
        }

        void OnOperatorUpdated(IEnumerable<IOperator> operators, bool set)
        {
            if (!m_CurrentModel || !m_CurrentModel.isRefineMode)
                return;

            var maskOperator = operators.GetOperator<MaskOperator>();
            if (maskOperator == null)
                return;

            if (maskOperator.Enabled())
            {
                var referenceOperator = operators.GetOperator<ReferenceOperator>();
                if (referenceOperator != null && referenceOperator.Enabled())
                {
                    // Disable reference image when using the mask from the Generation Data as they are currently exclusive
                    m_CurrentModel.SetOperatorEnable<ReferenceOperator>(false);
                }
            }

            UpdatePromptOperator(maskOperator.IsClear());
        }

        protected override void OnArtifactSelected(Artifact artifact)
        {
            if (m_CurrentModel && m_CurrentModel.isRefineMode && artifact is not ImageArtifact { IsPbrMode: true })
                m_CurrentModel.SetActiveTool(m_PanTool);
            else if (m_CurrentModel && m_CurrentModel.isRefineMode && artifact is ImageArtifact { IsPbrMode: true })
                m_CurrentModel.SetActiveTool(null);
        }

        void OnActiveToolChanged(ICanvasTool canvasTool)
        {
            UpdateExclusiveOperators(canvasTool);
        }

        void UpdateExclusiveOperators(ICanvasTool canvasTool)
        {
            if (canvasTool is TextureBrushTool)
            {
                // Disable reference image when going to in-painting mask as they are currently exclusive
                m_CurrentModel.SetOperatorEnable<ReferenceOperator>(false);
            }
            else if (m_CurrentModel.CurrentOperators.GetOperator<ReferenceOperator>() is not null)
            {
                // Disable in-painting mask when setting a new reference image as they are currently exclusive
                m_CurrentModel.SetOperatorEnable<MaskOperator>(false);
                m_CurrentModel.SetOperatorEnable<ReferenceOperator>(true);
            }

            // Disable in-painting mask when going out of paint mode
            if (canvasTool is not TextureBrushTool)
            {
                m_CurrentModel.SetOperatorEnable<MaskOperator>(false);
            }
        }

        void UpdatePromptOperator(bool isMaskClear)
        {
            var maskOperator = m_CurrentModel.CurrentOperators.GetOperator<MaskOperator>();
            if (maskOperator == null)
                return;

            SetPromptFieldsEnabled(!maskOperator.Enabled() || isMaskClear);
            if (maskOperator.Enabled())
            {
                ChangePromptsFromArtifact(m_CurrentModel.SelectedArtifact);
            }
        }

        public override bool EvaluateEnableState(Artifact artifact)
        {
            if (artifact is ImageArtifact imageArtifact)
            {
                return m_CurrentModel.isRefineMode && !imageArtifact.IsPbrMode && ArtifactCache.IsInCache(artifact);
            }

            return base.EvaluateEnableState(artifact);
        }

        void OnRefineArtifact(Artifact artifact)
        {
            ChangePromptsFromArtifact(artifact);
        }

        void ChangePromptsFromArtifact(Artifact artifact)
        {
            var artifactPromptOperator = artifact.GetOperator<PromptOperator>();
            var refinePrompt = artifactPromptOperator.GetPrompt();
            var refineNegativePrompt = artifactPromptOperator.GetNegativePrompt();

            var currentModelPromptOperator =  m_CurrentModel.CurrentOperators.GetOperator<PromptOperator>();
            currentModelPromptOperator.SetPrompt(refinePrompt);
            currentModelPromptOperator.SetNegativePrompt(refineNegativePrompt);
        }

        void SetPromptFieldsEnabled(bool enable)
        {
            var currentModelPromptOperator =  m_CurrentModel.CurrentOperators.GetOperator<PromptOperator>();
            currentModelPromptOperator.SetPromptFieldEnabled(enable);
            currentModelPromptOperator.SetNegativePromptFieldEnabled(enable);
        }
    }
}