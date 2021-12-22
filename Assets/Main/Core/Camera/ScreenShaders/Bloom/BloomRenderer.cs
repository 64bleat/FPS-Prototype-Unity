using MPConsole;
using MPCore;
using UnityEngine;
using UnityEngine.Serialization;

namespace MPGUI
{
	[ContainsConsoleCommands]
	[ExecuteAlways, ImageEffectAllowedInSceneView]
	public class BloomRenderer : MonoBehaviour
	{
		[SerializeField] Material _bloomMat;
		[SerializeField] Material _filmGrain;
		[SerializeField] bool _enableShader = true;
		[SerializeField, Range(0, 10)] float _intensity = 1;
		[SerializeField, Range(0, 10)] float _threshold = 1;
		[SerializeField, Range(0, 1)] float _softThreshold = 0.5f;
		[SerializeField, Range(0, 10)] float _scale = 1;
		[SerializeField] bool _debug = false;

		GraphicsModel _graphicsModel;
		int _downblendPrefilter;
		int _downblendPass;
		int _upblendPass;
		int _addPass;
		readonly RenderTexture[] mipmaps = new RenderTexture[16];

		void Awake()
		{
			_graphicsModel = Models.GetModel<GraphicsModel>();
			_downblendPrefilter = _bloomMat.FindPass("DownblendPrefilter");
			_downblendPass = _bloomMat.FindPass("Downblend");
			_upblendPass = _bloomMat.FindPass("Upblend");
			_addPass = _bloomMat.FindPass("ApplyBloom");

			Console.AddInstance(this);

			_graphicsModel.bloomEnabled.Subscribe(SetEnabled);
		}

		void OnDestroy()
		{
			Console.RemoveInstance(this);
			_graphicsModel.bloomEnabled.Unsubscribe(SetEnabled);
		}

		void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			{// SetBloomMatValues
				float knee = _threshold * _softThreshold;
				Vector4 filter = new Vector4(
					_threshold,
					_threshold - knee,
					2f * knee,
					0.25f / (knee + float.Epsilon));

				_bloomMat.SetVector("_Filter", filter);
				_bloomMat.SetFloat("_Intensity", Mathf.GammaToLinearSpace(_intensity));
				_bloomMat.SetFloat("_Scale", _scale);
			}

			// Iteration Check
			int iterations = Mathf.Min(_graphicsModel.iterations, (int)Mathf.Log(source.height, 2));
			RenderTexture predest = RenderTexture.GetTemporary(source.width, source.height, 24, RenderTextureFormat.ARGBFloat);

			// Instantiate Mipmaps
			for (int i = 0; i < iterations; i++)
				mipmaps[i] = RenderTexture.GetTemporary(source.width / (1 << i), source.height / (1 << i), 0, RenderTextureFormat.ARGBFloat);

			Graphics.Blit(source, mipmaps[0], _bloomMat, _downblendPrefilter);
			for (int i = 1; i < iterations; i++)
				Graphics.Blit(mipmaps[i - 1], mipmaps[i], _bloomMat, _downblendPass);

			for (int i = iterations - 2; i >= 0; i--)
				Graphics.Blit(mipmaps[i + 1], mipmaps[i], _bloomMat, _upblendPass);

			_bloomMat.SetTexture("_SourceTex", source);

			Graphics.Blit(mipmaps[0], predest, _bloomMat, _addPass);

			_filmGrain.SetFloat("_Grain", _graphicsModel.filmGrainScale / 64f);
			_filmGrain.SetVector("_Seed", GraphicsModel.GRAIN_SEED);
			Graphics.Blit(destination, _filmGrain);

			Graphics.Blit(predest, destination);

			//Release
			for (int i = 0; i < iterations; i++)
				RenderTexture.ReleaseTemporary(mipmaps[i]);

			RenderTexture.ReleaseTemporary(predest);
		}

		void SetEnabled(DeltaValue<bool> value) => enabled = value.newValue;

		[ConsoleCommand("bloomy")]
		string DebugBloomTexture()
		{
			_debug = !_debug;
			_addPass = _bloomMat.FindPass(_debug ? "DebugBloomPass" : "ApplyBloom");

			return _debug ? "All you see is bloom" : "Bloom normalized";
		}
	}
}
