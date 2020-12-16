using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostProcessTest : MonoBehaviour
{
	public Material postProcess;

	private Camera cam;
	private int cam2World;

	private void Awake()
	{
		if(cam = GetComponent<Camera>())
		{
			cam.depthTextureMode |= DepthTextureMode.Depth;
		}


	}

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		var p = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);// Unity flips its 'Y' vector depending on if its in VR, Editor view or game view etc... (facepalm)
		p[2, 3] = p[3, 2] = 0.0f;
		p[3, 3] = 1.0f;
		var clipToWorld = Matrix4x4.Inverse(p * cam.worldToCameraMatrix) * Matrix4x4.TRS(new Vector3(0, 0, -p[2, 2]), Quaternion.identity, Vector3.one);
		postProcess.SetMatrix("clipToWorld", clipToWorld);

		Graphics.Blit(source, destination, postProcess);
	}
}
