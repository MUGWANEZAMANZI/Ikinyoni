#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

using System;
using System.Collections.Generic;
using System.IO;
using Unity.Muse.Common;
using Unity.Muse.Common.Account;
using Unity.Muse.Common.Baryon.UI.Manipulators;
using Unity.Muse.Texture.Pbr.Cache;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Texture
{
    internal class ResultItemVisualElement : PreviewElement
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        public static extern void DownloadFile(byte[] array, int byteLength, string fileName);
#endif
        static List<PbrMaterialData> s_PbrMaterialData = new();

        public event Action<GenericLoader.State> OnLoadingStateChanged;

        public ResultItemVisualElement(Artifact artifact)
            : base(s_PbrMaterialData, artifact)
        {
            EnableInClassList("no-mouse", !Input.mousePresent);

            AddToClassList("muse-asset-image");
            SetPreviewImage(artifact);

            m_PreviewImage.GenericLoader.OnLoadingStateChanged += LoadingStateChanged;
            m_PreviewPbr.GenericLoader.OnLoadingStateChanged += LoadingStateChanged;
            m_PreviewImage.OnDelete += OnDelete;
            m_PreviewPbr.OnDelete += OnDelete;

            if (artifact is ImageArtifact imageArtifact)
            {
                SetCurrentState(imageArtifact.IsPbrMode ? PreviewType.PBR : PreviewType.Image);
            }
        }

        private void OnDelete()
        {
            PerformAction((int)Actions.Delete, new ActionContext(), null);
        }


        void LoadingStateChanged(GenericLoader.State state)
        {
            OnLoadingStateChanged?.Invoke(state);
        }

        public override bool TryGoToRefineMode()
        {
            if (canRefine)
            {
                OnRefineClicked();
                return true;
            }

            return false;
        }

        public override IEnumerable<ContextMenuAction> GetAvailableActions(ActionContext context)
        {
            var actions = new List<ContextMenuAction>();
            if (m_IsError)
            {
                return actions;
            }

            actions.Add(new ContextMenuAction
            {
                id = (int)Actions.GenerationSettings,
                label = "Generation Data",
                enabled = !context.isMultiSelect
            });

            var isUpscale = Artifact.GetOperators().Find(x => x is UpscaleOperator upscaleOperator && upscaleOperator.Enabled()) != null;

            if (CurrentModel.isRefineMode)
            {
                actions.Add(new ContextMenuAction
                {
                    id = (int)Actions.SetAsThumbnail,
                    label = "Set as Thumbnail",
                    enabled = !context.isMultiSelect && !CurrentModel.IsThumbnail(Artifact)
                });

                actions.Add(new ContextMenuAction
                {
                    id = (int)Actions.Branch,
                    label = "Branch",
                    enabled = !context.isMultiSelect
                });
            }

            if (Artifact is ImageArtifact imageArtifact)
            {
                var changeStateStr = m_ActivePreviewState == PreviewType.Image ? "PBR Material" : "Image";
                actions.Add(new ContextMenuAction
                {
                    id = (int)Actions.SwitchPreview,
                    label = context.isMultiSelect ? "Switch Preview" : $"View as {changeStateStr}",
                    enabled = AccountInfo.Instance.IsEntitled ||
                        (!AccountInfo.Instance.IsEntitled && PbrDataCache.IsInCache(imageArtifact)),
                });
            }

            actions.Add(new ContextMenuAction
            {
                id = (int)Actions.SetAsReference,
                label = "Set as Reference",
                enabled = !context.isMultiSelect
            });

            if (!isUpscale && Artifact is IVariateArtifact variateArtifact)
            {
                var numVariations = CurrentModel.CurrentOperators.GetOperator<GenerateOperator>()?.GetCount() ?? 4;
                var label = $"Create {numVariations} Variation{(numVariations > 1 ? "s" : "")}";
                actions.Add(new ContextMenuAction
                {
                    id = (int)Actions.CreateVariations,
                    label = label,
                    enabled = AccountInfo.Instance.IsEntitled,
                });
            }

            var exportAvailable = m_ActivePreviewState switch
            {
                PreviewType.PBR => m_PreviewPbr.CurrentMaterial != null,
                PreviewType.Image => m_PreviewImage.image != null,
                _ => false
            };

            actions.Add(new ContextMenuAction
            {
                // Context menu Delete is available even if the generation is not ready yet, it's to have the option
                // to delete the item when there is an error with the generation, otherwise, delete was only
                // available with the keyboard shortcut
                enabled = true,
                id = (int)Actions.Delete,
                label = context.isMultiSelect ? TextContent.deleteMultiple : TextContent.deleteSingle
            });

            if (Artifact is Artifact<Texture2D> && exportAvailable)
            {
                actions.Add(new ContextMenuAction
                {
                    enabled = true,
                    id = (int)Actions.Save,
                    label = context.isMultiSelect ? TextContent.exportMultiple : TextContent.exportSingle
                });

                if (context.isMultiSelect)
                {
                    actions.Add(new ContextMenuAction
                    {
                        enabled = true,
                        id = (int)Actions.Star,
                        label = TextContent.starMultiple
                    });
                    actions.Add(new ContextMenuAction
                    {
                        enabled = true,
                        id = (int)Actions.UnStar,
                        label = TextContent.unStarMultiple,
                    });
                }
                else
                {
                    if (!ShouldLeftSideButtonBeVisible())
                    {
                        if (IsBookmarked())
                        {
                            actions.Add(new ContextMenuAction
                            {
                                enabled = true,
                                id = (int)Actions.UnStar,
                                label = TextContent.unStarSingle,
                            });
                        }
                        else
                        {
                            actions.Add(new ContextMenuAction
                            {
                                enabled = true,
                                id = (int)Actions.Star,
                                label = TextContent.starSingle
                            });
                        }
                    }

                    if (!ShouldEditButtonBeVisible() && canRefine)
                    {
                        actions.Add(new ContextMenuAction
                        {
                            enabled = true,
                            id = (int)Actions.Refine,
                            label = TextContent.refineSingle
                        });
                    }
                }

                if (!isUpscale && Artifact is IUpscaleArtifact)
                {
                    actions.Add(new ContextMenuAction
                    {
                        id = (int)Actions.Upscale,
                        label = "Upscale",
                        enabled = AccountInfo.Instance.IsEntitled,
                    });
                }
            }

            return actions;
        }

        public override bool TrySaveAsset(string directory, Action<string> onExport = null)
        {
            if (m_ActivePreviewState == PreviewType.PBR)
            {
                var path = Path.Combine(directory, $"{m_Artifact.Guid}.mat");
                path = path.Replace(Application.dataPath, "Assets");
                ExportHandler.ExportWithoutPrompt(m_Artifact, m_PreviewPbr.CurrentMaterialData, path, (path, artifact) =>
                {
                    onExport?.Invoke(path);
                });

                onExport?.Invoke(path);

                return true;
            }

            return base.TrySaveAsset(directory);
        }

        /// <summary>
        /// Perform action to perform on the selected artifact.
        /// </summary>
        /// <param name="actionId">The action to perform.</param>
        /// <param name="context">The action context.</param>
        /// <param name="pointerEvent">The pointer event at the source of the action.</param>
        public override void PerformAction(int actionId, ActionContext context, IPointerEvent pointerEvent)
        {
            if (CurrentModel == null)
                return;

            var id = (Actions)actionId;

            switch (id)
            {
                case Actions.SwitchPreview:
                    var changeState = m_ActivePreviewState == PreviewType.Image ? PreviewType.PBR : PreviewType.Image;
                    SetCurrentState(changeState);
                    var tool = CurrentModel.isRefineMode && changeState == PreviewType.Image ? CurrentModel.DefaultRefineTool : null;
                    CurrentModel.SetActiveTool(tool);
                    break;
                case Actions.Refine:
                    CurrentModel.RefineArtifact(m_Artifact);
                    break;
                case Actions.Save:
#if UNITY_EDITOR
                    switch (m_ActivePreviewState)
                    {
                        case PreviewType.Image:
                            CurrentModel.ExportArtifact(m_Artifact);
                            break;
                        case PreviewType.PBR:
                            ExportHandler.ExportWithPrompt(m_Artifact, m_PreviewPbr.CurrentMaterialData, (path, artifact) =>
                            {
                                var unityGuid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
                                CurrentModel.AddExportedArtifact(unityGuid, artifact.Guid);
                            });
                            break;
                    }
#endif
                    break;
                case Actions.Download:
#if UNITY_WEBGL && !UNITY_EDITOR
                    (m_Artifact as Artifact<Texture2D>)?.GetArtifact((Texture2D artifactInstance, byte[] rawData, string errorMessage) =>
                    {
                        if (artifactInstance == null)
                            return;

                        var bytes = artifactInstance.EncodeToPNG();
                        DownloadFile(bytes, bytes.Length, m_Artifact.Guid + ".png");
                    }, true);
#endif
                    break;
                default:
                    base.PerformAction(actionId, context, pointerEvent);
                    break;
            }
        }

        public override UnityEngine.Texture Preview
        {
            get
            {
                return ActivePreviewState switch
                {
                    PreviewType.PBR => m_PreviewPbr.previewImage.image,
                    PreviewType.Image => m_PreviewImage.image as Texture2D,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        public override VisualElement PaintSurfaceElement => m_PreviewImage;

        public override void DragEditor()
        {
            var artifactsAndType = GetArtifactsAndType();
            CurrentModel.EditorStartDrag(artifactsAndType.name, artifactsAndType.artifacts);
        }

        public override (string name, IList<Artifact> artifacts) GetArtifactsAndType()
        {
            if (m_ActivePreviewState == PreviewType.Image)
            {
                return ("Texture Image", new List<Artifact> { Artifact });
            }
            else
            {
                var artifacts = new List<Artifact>(5) { Artifact };
                foreach (var pbrMaterialData in s_PbrMaterialData)
                {
                    if (pbrMaterialData.BaseMapSourceArtifact == Artifact)
                    {
                        artifacts.Add(pbrMaterialData.MetallicMapSourceArtifact);
                        artifacts.Add(pbrMaterialData.HeightmapSourceArtifact);
                        artifacts.Add(pbrMaterialData.NormalMapSourceArtifact);
                        artifacts.Add(pbrMaterialData.SmoothnessMapSourceArtifact);
                    }
                }

                return ("PBR Material", artifacts);
            }
        }
    }
}