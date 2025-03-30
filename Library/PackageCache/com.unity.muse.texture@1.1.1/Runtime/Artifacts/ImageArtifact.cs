using System;
using System.Collections.Generic;
using Unity.Muse.Common;
using Unity.Muse.Common.Services;
using Unity.Muse.Texture.Analytics;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Unity.Muse.Texture
{
    [Serializable]
    internal sealed class ImageArtifact : SimpleImageArtifact, IGenerateArtifact, IVariateArtifact, IInpaintArtifact, IUpscaleArtifact
    {
        const float k_DefaultVariateStrength = 0.15f;

        [SerializeField]
        internal bool IsPbrMode = false;

        /// <summary>
        /// Material meta data for the generated material
        /// </summary>
        internal MaterialData MaterialMetaData;

        public ImageArtifact(string guid, uint seed)
            : base(guid, seed) { }

        public ImageArtifact()
            : base(string.Empty, 0) { }

        /// <summary>
        /// Only called once for a generation group
        /// </summary>
        /// <param name="model">Model used.</param>
        public override void StartGenerate(Model model)
        {
            try
            {
                Model.SendAnalytics(new GenerateAnalytic(
                    this.GetOperator<PromptOperator>()?.GetPrompt(),
                    this.GetOperator<PromptOperator>()?.GetNegativePrompt(),
                    this.GetOperator<MaskOperator>()?.Enabled() ?? false,
                    m_Operators.GetOperator<GenerateOperator>()?.GetCount() ?? 0,
                    m_Operators.GetOperator<ReferenceOperator>()?.Enabled() ?? false,
                    m_Operators.GetOperator<ReferenceOperator>()?.Enabled() ?? false
                ));
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        public override void Generate(Model model)
        {
            var buffer = new byte[4];
            var random = new System.Random();

            random.NextBytes(buffer);
            var maskOperator = m_Operators.GetOperator<MaskOperator>();
            var promptOperator = m_Operators.GetOperator<PromptOperator>();
            var prompt = promptOperator?.GetPrompt();

            //var loraOperator = m_Operators.FirstOrDefault(x => x.GetType() == typeof(LoraOperator)) as LoraOperator;
            //we will pass the loraOperator guid to settings when it's possible
            var settings = new TextToImageRequest(promptOperator?.GetNegativePrompt(), true, BitConverter.ToUInt32(buffer, 0), (int)GenerativeAIBackend.GeneratorModel.StableDiffusionV_1_4, 512, 512, k_DefaultVariateStrength);

            if (maskOperator != null && maskOperator.Enabled())
            {
                if (model.SelectedArtifact is not IInpaintArtifact inpaintArtifact) return;

                settings.seamless = maskOperator.GetSeamless();
                var parentGuid = model.SelectedArtifact.Guid;

                inpaintArtifact.GenerateInpaint(prompt, parentGuid, maskOperator.GetMaskTexture(), MaskType.UserDefined, settings, OnGeneratingDone);
            }
            else
            {
                var generateArtifact = model.SelectedArtifact ?? new ImageArtifact();

                if (generateArtifact is not IGenerateArtifact generateAbleArtifact) return;

                generateAbleArtifact.Generate(prompt, settings, OnGeneratingDone);
            }
        }

        public override void RetryGenerate(Model model)
        {
            if (GenerationService.IsVariation(m_Operators))
                Variate(m_Operators);
            else
                Generate(model);
        }

        public override void Variate(List<IOperator> ops)
        {
            byte[] buffer = new byte[4];
            var random = new System.Random();
            random.NextBytes(buffer);
            var promptOperator = ops.GetOperator<PromptOperator>();
            var referenceOperator = ops.GetOperator<ReferenceOperator>();

            var settings = new ImageVariationSettingsRequest(
                promptOperator?.GetNegativePrompt(),
                true,
                (uint)BitConverter.ToUInt32(buffer, 0),
                (int)GenerativeAIBackend.GeneratorModel.StableDiffusionV_1_4,
                512,
                512,
                Math.Abs(referenceOperator.GetSettingInt(ReferenceOperator.Setting.Strength)) / 100.0f); //Server expects a value between 0 and 1

            var guid = referenceOperator.GetSettingString(ReferenceOperator.Setting.Guid);
            var imageBase64 = referenceOperator.GetSettingString(ReferenceOperator.Setting.Image);

            SetOperators(ops);
            MuseTextureBackend.VariateImage(
                guid,
                imageBase64,
                promptOperator?.GetPrompt(),
                settings,
                OnGeneratingDone);
        }

        public override void Shape(List<IOperator> ops)
        {
            var buffer = new byte[4];
            var random = new System.Random();
            random.NextBytes(buffer);
            var promptOperator = ops.GetOperator<PromptOperator>();
            var referenceOperator = ops.GetOperator<ReferenceOperator>();

            var settings = new ImageVariationSettingsRequest(
                promptOperator?.GetNegativePrompt(),
                true,
                (uint)BitConverter.ToUInt32(buffer, 0),
                (int)GenerativeAIBackend.GeneratorModel.StableDiffusionV_1_4,
                512,
                512,
                Math.Abs(referenceOperator.GetSettingInt(ReferenceOperator.Setting.Strength)) / 100.0f); //Server expects a value between 0 and 1

            SetOperators(ops);
            MuseTextureBackend.ControlNetGenerate(
                referenceOperator.GetSettingString(ReferenceOperator.Setting.Guid),
                referenceOperator.GetSettingString(ReferenceOperator.Setting.Image),
                promptOperator?.GetPrompt(),
                referenceOperator.GetSettingString(ReferenceOperator.Setting.Color),
                settings,
                OnGeneratingDone);
        }

        public void Variate(Model model, int variationNbr = 4)
        {
            byte[] buffer = new byte[4];
            var random = new System.Random();

            var promptOperator = m_Operators.GetOperator<PromptOperator>();

            for (var i = 0; i < variationNbr; ++i)
            {
                random.NextBytes(buffer);
                var settings = new ImageVariationSettingsRequest(
                    promptOperator?.GetNegativePrompt(),
                    true,
                    (uint)BitConverter.ToUInt32(buffer, 0),
                    (int)GenerativeAIBackend.GeneratorModel.StableDiffusionV_1_4,
                    512,
                    512,
                    k_DefaultVariateStrength);

                var newArtifact = new ImageArtifact();
                newArtifact.SetOperators(m_Operators);
                MuseTextureBackend.VariateImage(
                    Guid,
                    string.Empty,
                    promptOperator?.GetPrompt(),
                    settings,
                    newArtifact.OnGeneratingDone);
                model.AddAsset(newArtifact);
            }
        }

        public bool CanUpscale()
        {
            return this.GetOperator<UpscaleOperator>() == null;
        }

        public void Upscale(Model model)
        {
            if (this.GetOperator<UpscaleOperator>() != null)
                return;

            var ops = new List<IOperator>();

            var upscaleOp = new UpscaleOperator();
            upscaleOp.SetParent(this);
            upscaleOp.Enable(true);
            ops.Add(upscaleOp);

            var promptOp = this.GetOperator<PromptOperator>();
            var jsonOp = JsonUtility.ToJson(promptOp); //We do a deep copy
            promptOp = JsonUtility.FromJson<PromptOperator>(jsonOp);
            promptOp.Enable(false);

            ops.Add(promptOp);

            var newArtifact = new ImageArtifact();
            newArtifact.SetOperators(ops);
            model.AddAsset(newArtifact);

            MuseTextureBackend.UpscaleImage(this, newArtifact.OnGeneratingDone);
        }

        void OnGeneratingDone(GuidResponse response, string error)
        {
            if (response != null)
            {
                Guid = response.guid;
                Seed = response.seed;
            }

            OnGenerationDone?.Invoke(this, error);
        }

        /// <summary>
        /// Creating a new MaterialRefineView
        /// </summary>
        /// <returns>new Instance of MaterialRefineView</returns>
        public override ArtifactView CreateCanvasView()
        {
            var view = new MaterialRefineView(this);
            view.UpdateView();
            return view;
        }

        public override ArtifactView CreateView()
        {
            return new ResultItemVisualElement(this);
        }

#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        [Preserve]
        public static void RegisterArtifact()
        {
            ArtifactFactory.SetArtifactTypeForMode<ImageArtifact>("TextToImage");
        }

        public void Generate(string prompt, TextToImageRequest settings, Action<TextToImageResponse, string> onDone)
        {
            MuseTextureBackend.GenerateImage(prompt, settings, onDone);
        }

        public void GenerateInpaint(string prompt,
            string sourceGuid,
            Texture2D mask,
            MaskType maskType,
            TextToImageRequest settings,
            Action<TextToImageResponse, string> onDone)
        {
            MuseTextureBackend.GenerateInpainting(prompt, sourceGuid, mask, maskType, settings, onDone);
        }

        /// <summary>
        /// Will download the artifact, or load it from the cache on your current machine, if it exists.
        /// </summary>
        /// <param name="onReceived">Callback for when the artifact was successfully loaded and created. <b>T</b> will be <b>default</b> on error, and the error message will be in the 2nd parameter detailing the reason</param>
        /// <param name="useCache">When <b>true</b> will attempt to load the cached representation of this artifact from the local machine's GeneratedArtifactCache, and if not found will download it from the client. <b>false</b> will always reach out to the client and update the cache</param>
        public override void GetArtifact(ArtifactCreationDelegate onReceived, bool useCache)
        {
            if (useCache && IsCached)
            {
                var artifact = ReadFromCache(out var rawData);
                onReceived?.Invoke(artifact, rawData, string.Empty);

                return;
            }

            void HandleReceiveArtifactData(object data, string msg)
            {
                if (!string.IsNullOrEmpty(msg))
                {
                    onReceived?.Invoke(default, Array.Empty<byte>(), msg);
                    return;
                }
                var deserializedArtifact = CreateFromData((byte[])data, true);
                onReceived(deserializedArtifact, (byte[])data, msg);
            }
            MuseTextureBackend.OnStatusChange -= OnStatusChange;
            MuseTextureBackend.OnStatusChange += OnStatusChange;
            MuseTextureBackend.AddGuidToCheckStatusPeriodically(Guid);

            void OnStatusChange(string guid, string status)
            {
                if (Guid == guid)
                {
                    MuseTextureBackend.OnStatusChange -= OnStatusChange;
                    if (status == GenerativeAIBackend.StatusEnum.failed)
                    {
                        onReceived?.Invoke(default, Array.Empty<byte>(), $"Artifact generation failed. GUID={guid}");
                    }
                    else if (status == GenerativeAIBackend.StatusEnum.done)
                    {
                        GenerativeAIBackend.DownloadArtifact(this, HandleReceiveArtifactData);
                    }
                }
            }
        }

        /// <summary>
        /// Meta data associated with Generated Materials
        /// </summary>
        [System.Serializable]
        internal class MaterialData
        {
            /// <summary>
            /// if the material data has been initialized with default values
            /// </summary>
            public bool Initialized => m_Initialized;

            /// <summary>
            /// Tiling value for the material
            /// </summary>
            public Vector2 tiling;
            /// <summary>
            /// offset value for the material
            /// </summary>
            public Vector2 offset;
            /// <summary>
            /// rotation value for the material
            /// </summary>
            public float rotation;
            /// <summary>
            /// vertical flip value for the material
            /// </summary>
            public bool flipVertical;
            /// <summary>
            /// horizontal flip value for the material
            /// </summary>
            public bool flipHorizontal;
            /// <summary>
            /// displacement value for the material
            /// </summary>
            public bool useDisplacement;
            /// <summary>
            /// Height value for the material
            /// </summary>
            public float height;
            /// <summary>
            /// Metallic value for the material
            /// </summary>
            public float metallic;
            /// <summary>
            /// smoothness value for the material
            /// </summary>
            [FormerlySerializedAs("roughness")]
            public float smoothness;
            /// <summary>
            /// if the material uses metallic map
            /// </summary>
            public bool useMetallic;
            /// <summary>
            /// if the material uses smoothness map
            /// </summary>
            public bool useSmoothness;

            /// <summary>
            /// value if it was initialized with default values
            /// </summary>
            [SerializeField]
            bool m_Initialized;

            /// <summary>
            /// We don't want to initialize the data by default aka when serialization occurs
            /// </summary>
            /// <param name="initializeData"></param>
            public MaterialData(bool initializeData = false)
            {
                m_Initialized = true;
                if (!initializeData) return;

                var shader = MaterialGeneratorUtils.GetDefaultShaderForPipeline();
                tiling = shader.GetPropertyDefaultVectorValue(shader.FindPropertyIndex("_Tiling"));
                offset = shader.GetPropertyDefaultVectorValue(shader.FindPropertyIndex("_Offset"));
                rotation = shader.GetPropertyDefaultFloatValue(shader.FindPropertyIndex("_Rotation"));
                flipVertical = shader.GetPropertyDefaultFloatValue(shader.FindPropertyIndex("_FlipVertical")) == 1.0f;
                flipHorizontal = shader.GetPropertyDefaultFloatValue(shader.FindPropertyIndex("_FlipHorizontal")) == 1.0f;
                useDisplacement = shader.GetPropertyDefaultFloatValue(shader.FindPropertyIndex("_VertexDisplacement")) == 1.0f;
                height = shader.GetPropertyDefaultFloatValue(shader.FindPropertyIndex("_HeightIntensity"));
                metallic = shader.GetPropertyDefaultFloatValue(shader.FindPropertyIndex("_MetallicIntensity"));
                smoothness = shader.GetPropertyDefaultFloatValue(shader.FindPropertyIndex("_SmoothnessIntensity"));
                useMetallic = shader.GetPropertyDefaultFloatValue(shader.FindPropertyIndex("_UseMetallicMap")) == 1.0f;
                useSmoothness = shader.GetPropertyDefaultFloatValue(shader.FindPropertyIndex("_UseSmoothnessMap")) == 1.0f;
            }

            public void GetValuesFromMaterial(Material material)
            {
                tiling = material.GetVector(MuseMaterialProperties.tilingKey);
                offset = material.GetVector(MuseMaterialProperties.offsetKey);
                rotation = material.GetFloat(MuseMaterialProperties.rotationKey);
                flipVertical = material.GetFloat(MuseMaterialProperties.flipVertical) == 1.0f;
                flipHorizontal = material.GetFloat(MuseMaterialProperties.flipHorizontal) == 1.0f;
                useDisplacement = material.GetFloat(MuseMaterialProperties.useDisplacement) == 1.0f;
                height = material.GetFloat(MuseMaterialProperties.heightIntensity);
                metallic = material.GetFloat(MuseMaterialProperties.metallicIntensity);
                smoothness = material.GetFloat(MuseMaterialProperties.smoothnessIntensity);
                useMetallic = material.GetFloat(MuseMaterialProperties.useMetallic) == 1.0f;
                useSmoothness = material.GetFloat(MuseMaterialProperties.useSmoothness) == 1.0f;
            }
            public void ApplyToMaterial(Material material)
            {
                material.SetVector(MuseMaterialProperties.tilingKey, tiling);
                material.SetVector(MuseMaterialProperties.offsetKey, offset);
                material.SetFloat(MuseMaterialProperties.rotationKey, rotation);
                material.SetFloat(MuseMaterialProperties.flipVertical, flipVertical ? 1.0f : 0.0f);
                material.SetFloat(MuseMaterialProperties.flipHorizontal, flipHorizontal ? 1.0f : 0.0f);
                material.SetFloat(MuseMaterialProperties.useDisplacement, useDisplacement ? 1.0f : 0.0f);
                material.SetFloat(MuseMaterialProperties.heightIntensity, height);
                material.SetFloat(MuseMaterialProperties.metallicIntensity, metallic);
                material.SetFloat(MuseMaterialProperties.smoothnessIntensity, smoothness);
                material.SetFloat(MuseMaterialProperties.useMetallic, useMetallic ? 1.0f : 0.0f);
                material.SetFloat(MuseMaterialProperties.useSmoothness, useSmoothness ? 1.0f : 0.0f);
            }
        }
    }
}
