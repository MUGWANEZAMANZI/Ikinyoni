using System;
using System.Collections.Generic;
using Unity.Muse.Common;
using UnityEngine.UIElements;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common.Utils;
using UnityEngine;

namespace Unity.Muse.Texture
{
    internal abstract class PreviewElement : ArtifactView, IDisposable
    {
        public bool MaterialPreviewEnabled => m_PreviewPbr.PreviewEnabled;
        public event Action<Artifact> OnSelected;

        const string k_UssAssetPath = "Packages/com.unity.muse.texture/Runtime/Artifacts/Views/PackageResources/PreviewElement.uss";

        protected PreviewImage m_PreviewImage;
        protected PreviewPbrArtifact m_PreviewPbr;

        protected PreviewType m_ActivePreviewState = PreviewType.Image;

        ActionButton m_EditButton;
        ActionButton m_ActionButton;

        ActionGroup m_FeedbackGroup;

        VisualElement m_ButtonContainer;
        readonly ActionButton m_BookmarkButton;
        VisualElement m_LeftVerticalContainer;

        protected bool m_IsError =>
            m_PreviewImage?.LoadingState == GenericLoader.State.Error ||
            m_PreviewPbr?.GenericLoader.LoadingState == GenericLoader.State.Error;

        public PreviewType ActivePreviewState => m_ActivePreviewState;
        public UnityEngine.Texture ImagePreview => m_PreviewImage.image;
        public MaterialMapPreview PbrPreview => m_PreviewPbr;
        public event Action<PreviewType> OnPreviewTypeChanged;

        public RotationManipulator RotationManipulator => m_PreviewPbr.RotationManipulator;

        public Material Material => m_PreviewPbr.CurrentMaterial;

        float m_ButtonWidth = 0f;
        float m_ButtonContainerWidth = 0f;
        const int k_MaxButtonColumnsEdit = 2;
        const int k_MaxButtonColumnsLeftSide = 3;

        VisualElement FrontElement => m_ActivePreviewState == PreviewType.Image ? m_PreviewImage : m_PreviewPbr;
        bool isPreviewImage => m_ActivePreviewState == PreviewType.Image;
        bool isPreviewPbr => m_ActivePreviewState == PreviewType.PBR;

        bool m_Hovered;
        bool hovered
        {
            get => m_Hovered;
            set
            {
                m_Hovered = value;
                RefreshButtonsVisibility();
                UpdateButtons();
            }
        }

        bool m_MenuOpened;
        bool menuOpened
        {
            get => m_MenuOpened;
            set
            {
                m_MenuOpened = value;
                RefreshButtonsVisibility();
                UpdateButtons();
            }
        }


        protected PreviewElement(List<PbrMaterialData> pbrMaterialData, Artifact artifact)
            : base(artifact)
        {
            if (artifact is ImageArtifact imageArtifact)
            {
                m_ActivePreviewState = imageArtifact.IsPbrMode ? PreviewType.PBR : PreviewType.Image;
            }

            EnableInClassList("no-mouse", !Input.mousePresent);

            styleSheets.Add(ResourceManager.Load<StyleSheet>(Common.PackageResources.resultItemStyleSheet));
            m_PreviewImage = new PreviewImage();
            m_PreviewImage.OnSelected += ArtifactSelected;
            m_PreviewImage.OnLoadedPreview += UpdateView;

            m_PreviewPbr = new PreviewPbrArtifact(pbrMaterialData);
            m_PreviewPbr.OnArtifactSelected += ArtifactSelected;
            m_PreviewPbr.GenericLoader.OnLoadingStateChanged += state =>
            {
                if (state == GenericLoader.State.None)
                    UpdateView();
            };

            style.flexGrow = 1;
            m_ButtonContainer = new VisualElement();
            m_ButtonContainer.AddToClassList("muse-asset-image__control-buttons-container");

            m_EditButton = new ActionButton { name = "refine", icon = "pen", tooltip = TextContent.refineTooltip };
            m_EditButton.AddToClassList("refine-button");
            m_EditButton.AddToClassList("refine-button-item");
            m_EditButton.clicked += OnRefineClicked;
            m_EditButton.RegisterCallback<GeometryChangedEvent>(OnEditButtonGeometryChangedEvent);

            m_ActionButton = new ActionButton { name = "more", icon = "ellipsis", tooltip = "More options" };
            m_ActionButton.AddToClassList("refine-button");
            m_ActionButton.AddToClassList("refine-button-item");
            m_ActionButton.clicked += () => OnMenuTriggerClicked();
            m_ActionButton.SetEnabled(true);
            m_ButtonContainer.Add(m_ActionButton);
            m_ButtonContainer.Add(m_EditButton);

            m_LeftVerticalContainer = new VisualElement();
            m_LeftVerticalContainer.AddToClassList("left-vertical-container");
            m_ButtonContainer.Add(m_LeftVerticalContainer);
            m_ButtonContainer.RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);

            m_BookmarkButton = new ActionButton
            {
                tooltip = TextContent.bookmarkButtonTooltip,
                icon = "star"
            };
            m_BookmarkButton.clicked += OnBookmarkClicked;
            m_BookmarkButton.AddToClassList("container-button");

            m_FeedbackGroup = new ActionGroup()
            {
                compact = true,
                justified = false,
                direction = Direction.Vertical,
                selectionType = SelectionType.None,
                style =
                {
                    flexGrow = 0f
                }
            };

            m_FeedbackGroup.AddToClassList("container-button");

            m_LeftVerticalContainer.Add(m_BookmarkButton);
            m_LeftVerticalContainer.Add(m_FeedbackGroup);

            UpdateVisuals();
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
            RegisterCallback<PointerEnterEvent>(_ => hovered = true );
            RegisterCallback<PointerLeaveEvent>(_ => hovered = false);

            hovered = this.Query().Hovered().ToList().Contains(this);
        }

        void OnEditButtonGeometryChangedEvent(GeometryChangedEvent evt)
        {
            if (Mathf.Approximately(m_ButtonWidth, 0f) && !Mathf.Approximately(0f, m_EditButton.resolvedStyle.width))
            {
                m_ButtonWidth = m_EditButton.resolvedStyle.width + m_EditButton.resolvedStyle.marginLeft
                    + m_EditButton.resolvedStyle.marginRight;

                UpdateButtons();
            }
        }

        void RefreshButtonsVisibility()
        {
            m_ButtonContainer.SetDisplay(FrontElement, !m_IsError && canRefineBookmark && (hovered || menuOpened || IsBookmarked()));
        }

        void OnGeometryChangedEvent(GeometryChangedEvent evt)
        {
            m_ButtonContainerWidth = m_ButtonContainer.resolvedStyle.width - m_ButtonContainer.resolvedStyle.paddingLeft;
            UpdateButtons();
        }

        void UpdateButtons()
        {
            UpdateEditButton();
            UpdateLeftSideButtons();
            UpdateBookmark();
        }

        void OnBookmarkClicked()
        {
            var bookmark = CurrentModel.GetData<BookmarkManager>();
            bookmark.Bookmark(m_Artifact, !bookmark.IsBookmarked(m_Artifact));
        }

        protected override void OnBookmarkChanged()
        {
            UpdateBookmark();
            RefreshButtonsVisibility();
        }

        void UpdateBookmark()
        {
            var isBookmarked = IsBookmarked();
            m_BookmarkButton.EnableInClassList("bookmarked", isBookmarked);
            m_BookmarkButton.icon = isBookmarked ? "star-filled" : "star";
        }

        protected bool IsBookmarked()
        {
            if (CurrentModel == null)
                return false;

            return CurrentModel.GetData<BookmarkManager>().IsBookmarked(m_Artifact);
        }

        void UpdateEditButton()
        {
            m_EditButton.EnableInClassList("refine-button-hidden", !ShouldEditButtonBeVisible());
            m_EditButton.EnableInClassList("refine-button", ShouldEditButtonBeVisible());
        }

        void UpdateLeftSideButtons()
        {
            m_LeftVerticalContainer.EnableInClassList("container-hidden", !ShouldLeftSideButtonBeVisible());
        }

        internal bool ShouldLeftSideButtonBeVisible()
        {
            return (m_ButtonContainerWidth >= m_ButtonWidth * k_MaxButtonColumnsLeftSide);
        }

        protected bool ShouldEditButtonBeVisible()
        {
            return (m_ButtonContainerWidth >= m_ButtonWidth * k_MaxButtonColumnsEdit);
        }

        void OnMenuTriggerClicked()
        {
            m_ButtonContainer.AddToClassList("is-hovered");
            menuOpened = true;
            OnActionMenu(m_ActionButton);
        }

        protected override void MenuDismissed()
        {
            m_ButtonContainer.RemoveFromClassList("is-hovered");
            menuOpened = false;
        }

        void ArtifactSelected(Artifact artifact)
        {
            OnSelected?.Invoke(artifact);
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            parent?.styleSheets.Add(ResourceManager.Load<StyleSheet>(k_UssAssetPath));

            UpdateVisuals();
            UpdateView();
            UpdateBookmark();
        }

        void UpdateVisuals()
        {
            m_PreviewImage.SetDisplay(this, isPreviewImage);
            m_PreviewPbr.SetDisplay(this, isPreviewPbr);
        }

        bool isArtifactAvailable
        {
            get
            {
                return ActivePreviewState switch
                {
                    PreviewType.PBR => m_PreviewPbr.CurrentMaterial != null,
                    PreviewType.Image => m_PreviewImage.image != null,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        protected bool canRefine => isArtifactAvailable && CurrentModel && !CurrentModel.isRefineMode;

        protected bool canRefineBookmark => isArtifactAvailable && CurrentModel;

        public override void UpdateView()
        {
            base.UpdateView();
            m_ActionButton.visible = isArtifactAvailable;
            m_EditButton.visible = canRefine;
        }

        public void SetCurrentState(PreviewType type)
        {
            m_ActivePreviewState = type;
            UpdateVisuals();
            SetPreviewImage(m_Artifact);
            if (m_Artifact is ImageArtifact imageArtifact)
                imageArtifact.IsPbrMode = m_ActivePreviewState == PreviewType.PBR;

            OnPreviewTypeChanged?.Invoke(m_ActivePreviewState);
        }

        protected void SetPreviewImage(Artifact image)
        {
            m_Artifact = image;

            switch (m_ActivePreviewState)
            {
                case PreviewType.Image:
                    m_PreviewPbr.Disable();
                    m_PreviewImage.SetAsset(image);
                    break;
                case PreviewType.PBR:
                    m_PreviewPbr.Enable();
                    m_PreviewPbr.SetAsset(image);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Dispose()
        {
            m_PreviewPbr?.Dispose();
        }

        public GenericLoader.State GetLoadingState()
        {
            return m_ActivePreviewState switch
            {
                PreviewType.Image => m_PreviewImage.LoadingState,
                PreviewType.PBR => m_PreviewPbr.LoadingState,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}