#pragma warning disable CS0108 // Member hides inherited member; missing new keyword

using MPConsole;
using UnityEngine;

namespace MPCore
{
    /// <summary>
    /// A generic post process effect manager
    /// </summary>
    //[ExecuteInEditMode]
    [ContainsConsoleCommands]
    public class PostProcesser : MonoBehaviour
    {
        public Blitable worldPos;

        private Camera camera;

        private void Awake()
        {
            camera = GetComponent<Camera>();
            Console.RegisterInstance(this);
        }
        private void OnDestroy()
        {
            Console.RemoveInstance(this);
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            camera.depthTextureMode = DepthTextureMode.Depth;

            RenderTexture worldPosition = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBFloat);

            if (worldPos)
            {
                worldPos.material.SetMatrix("clipToWorld", GetGPUProjectionMatrix(camera));
                Graphics.Blit(source, worldPosition, worldPos.material, worldPos.passIndex);
                Graphics.Blit(worldPosition, destination);
            }
            else
                Graphics.Blit(source, destination);

            RenderTexture.ReleaseTemporary(worldPosition);
        }

        [ConsoleCommand("postprocess", "toggles the postprocessing pipeline on/off")]
        public void ToggleActive()
        {
            enabled = !enabled;
        }

        private static Matrix4x4 GetGPUProjectionMatrix(Camera camera)
        {
            Matrix4x4 projection = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
            projection[2, 3] = 0;
            projection[3, 2] = 0;
            projection[3, 3] = 1;
            projection[1, 1] *= -1; // This line cost me 6 hours of my life.
            Matrix4x4 clipToWorld = Matrix4x4.Inverse(projection * camera.worldToCameraMatrix) * Matrix4x4.TRS(new Vector3(0, 0, -projection[2, 2]), Quaternion.identity, Vector3.one);

            return clipToWorld;
        }
    }
}
