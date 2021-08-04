using MPCore;
using MPGUI;
using UnityEngine;

namespace MPWorld
{
    [ExecuteInEditMode, RequireComponent(typeof(Camera))]
    public class PixelationViewModel : MonoBehaviour
    {
        private PixelationModel _pixelation;
        private Camera _camera;

        private void Awake()
        {
            _pixelation = Models.GetModel<PixelationModel>();
            _camera = GetComponent<Camera>();
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_pixelation && _pixelation.currentShader)
            {
                _pixelation.currentShader.SetFloat("_FOV", _camera.fieldOfView * Mathf.PI / 180f);
                _pixelation.currentShader.SetFloat("_AspectRatio", _camera.aspect);
                _pixelation.currentShader.SetFloat("_NearPlane", _camera.nearClipPlane);
                _pixelation.currentShader.SetFloat("_FarPlane", _camera.farClipPlane);
                _pixelation.currentShader.SetTexture("_Source", source);

                Graphics.Blit(source, destination, _pixelation.currentShader);
            }
            else
                Graphics.Blit(source, destination);
        }
    }
}
