using MPCore;
using UnityEngine;

namespace MPWorld
{
    [ExecuteInEditMode, RequireComponent(typeof(Camera))]
    public class PixelationPostProcess : MonoBehaviour
    {
        private GraphicsModel _graphics;
        private Camera _camera;

        private void Awake()
        {
            _graphics = Models.GetModel<GraphicsModel>();
            _camera = GetComponent<Camera>();
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_graphics && _graphics.pixelationShader)
            {
                _graphics.pixelationShader.SetFloat("_FOV", _camera.fieldOfView * Mathf.PI / 180f);
                _graphics.pixelationShader.SetFloat("_AspectRatio", _camera.aspect);
                _graphics.pixelationShader.SetFloat("_NearPlane", _camera.nearClipPlane);
                _graphics.pixelationShader.SetFloat("_FarPlane", _camera.farClipPlane);
                _graphics.pixelationShader.SetTexture("_Source", source);

                Graphics.Blit(source, destination, _graphics.pixelationShader);
            }
            else
                Graphics.Blit(source, destination);
        }
    }
}
