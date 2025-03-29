using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

#if USING_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace Unity.Muse.Texture
{
    internal class MaterialPreviewSceneHandler: IDisposable
    {
        public const float DefaultHdriIntensity = 30f;

        Scene m_Scene;
        Camera m_Camera;
        List<Light> m_Lights;
        ReflectionProbe m_ReflectionProbe;
        Renderer m_MaterialTarget;
        GameObject m_Target;
        PrimitiveObjectTypes? m_CurrentPreviewType = null;
        string m_CurrentCustomModelGuid = null;
        Mesh m_CustomMesh = null;
        HdriEnvironment? m_CurrentHdriEnvironment = null;
        RenderSettingsData m_RenderSettingsData = new RenderSettingsData();
        WireFrame m_Wireframe;
#if USING_HDRP
        HDAdditionalReflectionData m_ReflectionProbeAdditionalData;
        VolumeProfile m_VolumeProfile;
        HDRISky m_HdriSky;
        VisualEnvironment m_VisualEnvironment;
        Exposure m_Exposure;
#endif

        public Scene Scene => m_Scene;
        public Camera Camera => m_Camera;
        public List<Light> Lights => m_Lights;
        public ReflectionProbe ReflectionProbe => m_ReflectionProbe;
        public Renderer MaterialTarget => m_MaterialTarget;
        public GameObject Target => m_Target;
        public WireFrame Wireframe => m_Wireframe;

        public MaterialPreviewSceneHandler(Scene scene)
        {
            m_Scene = scene;
            InitializeScene();
        }

        void InitializeScene()
        {
            if(!LayerManager.CreateLayer())
                return;

            InitializeCamera();
            InitializeLights();
            InitializePrimitiveTarget();
        }

        void InitializeCamera()
        {
            GameObject previewCamera = null;
#if USING_HDRP
            if (RenderPipelineUtils.IsUsingHdrp())
            {
                previewCamera = new GameObject("Preview Scene Camera", typeof(Camera))
                {
                    hideFlags = HideFlags.DontSave
                };

                previewCamera.hideFlags = HideFlags.DontSave;

                var volume = new GameObject("Global Volume", typeof(Volume)).GetComponent<Volume>();
                var collider = volume.gameObject.AddComponent<SphereCollider>().GetComponent<SphereCollider>();
                collider.radius = 10f;

                volume.isGlobal = false;
                m_VolumeProfile = volume.profile;

                m_HdriSky = m_VolumeProfile.Add<HDRISky>();
                m_HdriSky.exposure.Override(1f);

                var toneMapping = m_VolumeProfile.Add<Tonemapping>();
                toneMapping.mode.Override(TonemappingMode.None);

                m_VisualEnvironment = m_VolumeProfile.Add<VisualEnvironment>();
                m_VisualEnvironment.skyType.Override((int)SkyType.HDRI);
                m_VisualEnvironment.skyAmbientMode.Override(SkyAmbientMode.Dynamic);

                AddGameObject(volume.gameObject);

                m_Exposure = m_VolumeProfile.Add<Exposure>();
                m_Exposure.mode.Override(ExposureMode.Fixed);
                m_Exposure.fixedExposure.Override(0f);
            }
#endif
            if (!previewCamera)
            {
                previewCamera = new GameObject("Preview Scene Camera", typeof(Camera), typeof(WireFrame))
                {
                    hideFlags = HideFlags.DontSave
                };
            }

            AddGameObject(previewCamera);
            m_Wireframe = previewCamera.GetComponent<WireFrame>();
            m_Camera = previewCamera.GetComponent<Camera>();
            m_Camera.targetDisplay = -1;
            m_Camera.enabled = false;
            m_Camera.clearFlags = CameraClearFlags.Depth;
            m_Camera.fieldOfView = 15f;
            m_Camera.farClipPlane = 50f;
            m_Camera.nearClipPlane = 1f;
            m_Camera.renderingPath = RenderingPath.Forward;
            m_Camera.useOcclusionCulling = false;
            m_Camera.scene = m_Scene;
            m_Camera.clearFlags = CameraClearFlags.SolidColor;
            m_Camera.cullingMask = LayerMask.GetMask(LayerManager.MuseLayerName);

            var defaultBackgroundColor = Color.clear;
            var colorSpace = QualitySettings.activeColorSpace;
            m_Camera.backgroundColor = colorSpace == ColorSpace.Gamma ? defaultBackgroundColor : defaultBackgroundColor.linear;

#if USING_HDRP
            if (RenderPipelineUtils.IsUsingHdrp())
            {
                var cameraHighDefData = m_Camera.gameObject.GetComponent<HDAdditionalCameraData>();
                if (cameraHighDefData == null)
                    cameraHighDefData = m_Camera.gameObject.AddComponent<HDAdditionalCameraData>();

                cameraHighDefData.antialiasing = HDAdditionalCameraData.AntialiasingMode.FastApproximateAntialiasing;
                cameraHighDefData.dithering = true;
                cameraHighDefData.clearDepth = true;
                cameraHighDefData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
                cameraHighDefData.backgroundColorHDR = Color.clear;

                cameraHighDefData.customRenderingSettings = true;
                cameraHighDefData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Postprocess, true);
                cameraHighDefData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Dithering, true);
                cameraHighDefData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Tonemapping, false);
                cameraHighDefData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.SkyReflection, false);
                cameraHighDefData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ReflectionProbe, true);

                cameraHighDefData.probeLayerMask = LayerMask.GetMask(LayerManager.MuseLayerName);
                cameraHighDefData.volumeLayerMask = LayerMask.GetMask(LayerManager.MuseLayerName);

                //TODO: What other HDRP render features?
            }
#endif
        }

        void InitializeLights()
        {
            m_Lights = new List<Light>() { CreateLight(), CreateLight() };

            foreach (var light in m_Lights)
            {
                AddGameObject(light.gameObject);
            }

            m_Lights[0].intensity = 1f;
            m_Lights[0].transform.rotation = Quaternion.Euler(50f, -30f, 0.0f);
            m_Lights[0].transform.position = new Vector3(0.0f, 3.0f, 0.0f);
            m_Lights[0].color = Color.white;
            m_Lights[0].enabled = false;

            m_Lights[1].transform.rotation = Quaternion.Euler(340f, 218f, 177f);
            m_Lights[1].color = new Color(0.4f, 0.4f, 0.45f, 0.0f) * 0.7f;
            m_Lights[1].intensity = .6f;

#if USING_HDRP
            if (RenderPipelineUtils.IsUsingHdrp())
            {
                foreach (var light in m_Lights)
                {
                    var additionalLightData = light.gameObject.AddComponent<HDAdditionalLightData>();
#if USING_HDRP_16_OR_NEWER
                    light.type = LightType.Directional;
                    light.intensity = light.intensity;
#else
                    additionalLightData.type = HDLightType.Directional;
                    additionalLightData.intensity = light.intensity;
#endif
                    additionalLightData.color = light.color;
                    //TODO: What other HDRP light features?
                }
            }
#endif
        }

        static Light CreateLight()
        {
            var newPreviewLight = new GameObject("PreviewLight", typeof(Light))
            {
                hideFlags = HideFlags.DontSave
            };

            var component = newPreviewLight.GetComponent<Light>();
            component.type = LightType.Directional;
            component.intensity = 1f;
            component.enabled = false;
            return component;
        }

        internal void InitializeReflectionProbe(HdriEnvironment environment = HdriEnvironment.Default, float intensity = 1.0f)
        {
            intensity = GetIntensityForRP(intensity);

            m_CurrentHdriEnvironment = environment;
            var reflectionCubemap = HdriProvider.GetHdri(m_CurrentHdriEnvironment.Value);

            if (!RenderPipelineUtils.IsUsingHdrp())
            {
                m_RenderSettingsData.skybox ??= new Material(Shader.Find("Skybox/Cubemap"))
                {
                    hideFlags = HideFlags.DontSave
                };
                m_RenderSettingsData.ambientMode = AmbientMode.Skybox;
                m_RenderSettingsData.ambientIntensity = intensity;
                m_RenderSettingsData.ambientSkyColor = Color.white;
                m_RenderSettingsData.ambientGroundColor = Color.white;
                m_RenderSettingsData.ambientEquatorColor = Color.white;

                m_RenderSettingsData.skybox.SetTexture("_Tex", reflectionCubemap);
                m_RenderSettingsData.defaultReflectionMode = DefaultReflectionMode.Custom;
                m_RenderSettingsData.reflectionBounces = 0;
                m_RenderSettingsData.reflectionIntensity = 0f;

                if (m_ReflectionProbe == null)
                {
                    var reflectionGo = new GameObject();
                    m_ReflectionProbe = reflectionGo.AddComponent<ReflectionProbe>();
                    m_ReflectionProbe.intensity = 1f;
                    m_ReflectionProbe.mode = ReflectionProbeMode.Custom;

                    m_ReflectionProbe.clearFlags = ReflectionProbeClearFlags.SolidColor;
                    m_ReflectionProbe.importance = 1;
                    m_ReflectionProbe.size = new Vector3(100f, 100f, 100f);

                    AddGameObject(reflectionGo);
                }

                m_ReflectionProbe.intensity = intensity;
                m_ReflectionProbe.customBakedTexture = reflectionCubemap;
                m_RenderSettingsData.ApplyCurrentSettings();
                DynamicGI.UpdateEnvironment();
            }

#if USING_HDRP
            if (RenderPipelineUtils.IsUsingHdrp())
            {
                m_Exposure.compensation.Override(intensity);
                m_HdriSky.hdriSky.Override(reflectionCubemap);
            }
#endif
        }

        public void InitializePrimitiveTarget(PrimitiveObjectTypes primitiveObject = PrimitiveObjectTypes.Sphere, string customModelGuid = null, Mesh customMesh = null)
        {
            if (m_CurrentPreviewType == primitiveObject && m_CurrentCustomModelGuid == customModelGuid && m_CustomMesh == customMesh)
                return;

            m_CurrentPreviewType = primitiveObject;
            m_CurrentCustomModelGuid = customModelGuid;
            m_CustomMesh = customMesh;

            if (m_Target != null)
            {
                if(Application.isPlaying)
                    GameObject.Destroy(m_Target);
                else
                    GameObject.DestroyImmediate(m_Target);
            }

            m_Target = PrimitiveObjectsProvider.GetPrimitiveInstance(m_CurrentPreviewType.Value, customModelGuid, customMesh);
            m_Target.name = "PrimitiveTarget";
            m_Target.hideFlags = HideFlags.DontSave;

            m_MaterialTarget = m_Target.GetComponent<Renderer>();
            if (m_MaterialTarget == null)
            {
                m_MaterialTarget = m_Target.GetComponentInChildren<Renderer>();
            }

            AddGameObject(m_Target);
        }

        void AddGameObject(GameObject go)
        {
            SceneManager.MoveGameObjectToScene(go, m_Scene);
            LayerManager.AssignLayer(go);
        }

        public void Dispose()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                SceneManager.UnloadSceneAsync(m_Scene);
            }
            else
            {
                UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(m_Scene);
            }
#else
            SceneManager.UnloadSceneAsync(m_Scene);
#endif
        }

        static float GetIntensityForRP(float intensity)
        {
            var normalizedValue = intensity / 100f;
            if (RenderPipelineUtils.IsUsingHdrp())
                // more or less a gamma (re)mapping of [0, 1] to [-5, 5]
                return 10 * Mathf.Sqrt(normalizedValue) - 5;
            if (RenderPipelineUtils.IsUsingUrp())
                return Mathf.Lerp(0f, 5f, normalizedValue);
            return Mathf.Lerp(0f, 5f, normalizedValue);
        }
    }

    internal class WireFrame : MonoBehaviour
    {
        public void SetWireframeMode(bool wireframe)
        {
            GL.wireframe = wireframe;
        }
    }
}
