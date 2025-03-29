using System;
using Unity.Muse.Common;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
#if USING_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Unity.Muse.Texture
{
    internal class MaterialPreviewer : IDisposable
    {
        RenderPipelineAsset m_LastRenderPipelineAsset = null;
        MaterialPreviewSceneHandler m_SceneHandler;
        internal MaterialPreviewSceneHandler sceneHandler => m_SceneHandler;

        internal bool isValid => m_LastRenderPipelineAsset == GraphicsSettings.currentRenderPipeline;

        public MaterialPreviewer()
        {
            m_LastRenderPipelineAsset = GraphicsSettings.currentRenderPipeline;
            InitializePreviewScene();
        }

        void InitializePreviewScene()
        {
            Scene previewScene;
#if UNITY_EDITOR
            previewScene = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
            previewScene.name = "Material Previewer";
#else
            previewScene = SceneManager.CreateScene(Guid.NewGuid().ToString());
#endif

            m_SceneHandler = new MaterialPreviewSceneHandler(previewScene);
        }

        internal record RenderConfiguration (
            Material material,
            RenderTexture renderTexture,
            Vector3 cameraRotation,
            float cameraDistance,
            PrimitiveObjectTypes previewType,
            HdriEnvironment environment,
            float intensity,
            bool useWireframe = false,
            string customModelGuid = null,
            Mesh customMesh = null,
            float? fov = null,
            Color backgroundColor = default //equivalent to Color.clear
            );

        internal void Render(RenderConfiguration configuration)
        {
            if (m_SceneHandler == null || !m_SceneHandler.MaterialTarget)
            {
                Debug.LogWarning("Preview scene is not valid. Skipping render.");
                return;
            }

            RenderSettingsData currentRenderSettings = null;
            if (!RenderPipelineUtils.IsUsingHdrp())
            {
                currentRenderSettings = new RenderSettingsData();
                currentRenderSettings.CopyRenderSettings();
            }

            m_SceneHandler.InitializePrimitiveTarget(configuration.previewType,configuration.customModelGuid, configuration.customMesh);
            m_SceneHandler.InitializeReflectionProbe(configuration.environment, configuration.intensity);
            m_SceneHandler.MaterialTarget.material = configuration.material;

            var rotation = Quaternion.Euler(configuration.cameraRotation.y, configuration.cameraRotation.x, 0);
            var negDistance = new Vector3(0.0f, 0f, -configuration.cameraDistance);

            var position = rotation * negDistance + m_SceneHandler.MaterialTarget.bounds.center;

            var camera = m_SceneHandler.Camera;
            camera.backgroundColor = configuration.backgroundColor;
#if USING_HDRP
            if (RenderPipelineUtils.IsUsingHdrp() && camera.TryGetComponent<HDAdditionalCameraData>(out var hdAdditionalCameraData))
                hdAdditionalCameraData.backgroundColorHDR = configuration.backgroundColor.linear;
#endif
            if (RenderPipelineUtils.IsUsingHdrp())
                camera.enabled = true;

            var transform = camera.transform;
            transform.position = position;
            transform.rotation = rotation;

            camera.targetTexture = configuration.renderTexture;
            m_SceneHandler.Wireframe?.SetWireframeMode(configuration.useWireframe);
            camera.fieldOfView = configuration.fov ?? GetFOVForBounds(camera, CalculateBoundingSphere(m_SceneHandler.MaterialTarget.transform));

            // rendering takes a frame so we can't reset the fov immediately, we could once the frame completes but we don't really need to
            camera.Render();
            m_SceneHandler.Wireframe?.SetWireframeMode(false);

            if (!RenderPipelineUtils.IsUsingHdrp())
            {
                currentRenderSettings?.ApplyCurrentSettings();
                DynamicGI.UpdateEnvironment();
            }
        }

        public RenderTexture CreateDefaultRenderTexture(int width = 2048, int height = 2048)
        {
            var colorFormat = m_SceneHandler.Camera.allowHDR
                ? GraphicsFormat.R16G16B16A16_SFloat
                : GraphicsFormat.R8G8B8A8_UNorm;
            var rt = new RenderTexture(width, height, colorFormat,
                SystemInfo.GetGraphicsFormat(DefaultFormat.DepthStencil));
            ObjectUtils.Retain(rt);

            return rt;
        }

        public void Dispose()
        {
            m_SceneHandler.Dispose();
        }

        internal static float GetFOVForBounds(Camera camera, BoundingSphere bounds)
        {
            // Calculate the distance of the bounding sphere from the camera
            var distance = Vector3.Distance(bounds.position, camera.transform.position);

            // Calculate half the FOV as an angle
            var halfFOV = Mathf.Atan(bounds.radius / distance);

            // Convert to degrees for vertical FOV
            var vFOV = 2 * halfFOV * Mathf.Rad2Deg;

            return vFOV;
        }

        internal static BoundingSphere CalculateBoundingSphere(Bounds bounds)
        {
            return new BoundingSphere(bounds.center, bounds.extents.magnitude);
        }

        internal static BoundingSphere CalculateBoundingSphere(Transform tr)
        {
            var bounds = new BoundingSphere(Vector3.zero, 0);

            foreach (var r in tr.GetComponentsInChildren<Renderer>(false))
            {
                if (r && r.bounds.size.sqrMagnitude > 0)
                {
                    var other = CalculateBoundingSphere(r.bounds);
                    if (bounds.radius == 0)
                        bounds = other;
                    else
                        bounds = bounds.Encapsulate(other);
                }
            }

            return bounds;
        }
    }

    static class BoundingSphereExtensions
    {
        public static BoundingSphere Encapsulate(this BoundingSphere first, BoundingSphere second)
        {
            // Calculate the vector between the centers of the original spheres
            var centerVector = first.position - second.position;

            // Calculate the distance between the centers
            var centersDistance = centerVector.magnitude;

            // If one sphere completely encloses the other, return the larger one
            if (first.radius >= centersDistance + second.radius)
            {
                return new BoundingSphere(first.position, first.radius);
            }
            if (second.radius >= centersDistance + first.radius)
            {
                return new BoundingSphere(second.position, second.radius);
            }

            // Calculate the new radius
            var newRadius = (centersDistance + first.radius + second.radius) / 2;

            // Calculate the new center
            var newCenter = first.position + centerVector.normalized * (newRadius - first.radius);

            return new BoundingSphere(newCenter, newRadius);
        }
    }
}

// Add this magic code, you will be able to use record and init.
// I only tested in editor, not sure whether it would work on other platforms.
namespace System.Runtime.CompilerServices
{
    class IsExternalInit
    {

    }
}
