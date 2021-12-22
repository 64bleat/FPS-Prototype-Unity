using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
	public class GraphicsModel : Models
	{
		public static readonly Vector3 GRAIN_SEED = new Vector3(25.36535f, 13.12572f, 96.23642f);

		// Camera
		public DataValue<float> fov = new();
		public DataValue<float> fovFirstPerson = new();
		public DataValue<float> canvasScale = new();

		// Pixelation
		public Material pixelationShader;
		public List<Material> pixelationOptions = new();

		// Bloom
		public DataValue<bool> bloomEnabled = new(true);
		public int iterations = 8;
		public float filmGrainScale = 1;
	}
}
