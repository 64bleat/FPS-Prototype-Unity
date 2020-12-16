#pragma warning disable CS0109 // Member does not hide an inherited member; new keyword is not required

using MPGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPWorld
{
    [ExecuteInEditMode, RequireComponent(typeof(Camera))]
    public class PixelationScript : MonoBehaviour
    {
        //public Material currentShader;
        public PixelationGUI settings;

        internal new Camera camera;

        private void Awake()
        {
            camera = GetComponent<Camera>();
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (settings && settings.currentShader)
            {
                settings.currentShader.SetFloat("_FOV", camera.fieldOfView * Mathf.PI / 180f);
                settings.currentShader.SetFloat("_AspectRatio", camera.aspect);
                settings.currentShader.SetFloat("_NearPlane", camera.nearClipPlane);
                settings.currentShader.SetFloat("_FarPlane", camera.farClipPlane);
                settings.currentShader.SetTexture("_Source", source);

                Graphics.Blit(source, destination, settings.currentShader);
            }
            else
                Graphics.Blit(source, destination);
        }
    }
}
