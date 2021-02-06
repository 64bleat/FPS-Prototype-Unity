using MPConsole;
using MPCore;
using UnityEngine;

namespace MPGUI
{
    [ContainsConsoleCommands]
    [ExecuteInEditMode, ImageEffectAllowedInSceneView]
    public class BloomRenderer : MonoBehaviour
    {
        public BloomSettings settings;
        public ObjectEvent qualityChannel;
        public Material bloomMat;
        public Material filmGrain;
        public bool enableShader = true;
        [Range(0, 10)] public float intensity = 1;
        [Range(1, 16)] public int iterations = 1;
        [Range(0, 10)] public float threshold = 1;
        [Range(0, 1)] public float softThreshold = 0.5f;
        [Range(0, 10)] public float scale = 1;
        public bool debug = false;

        private int downblendPrefilter;
        private int downblendPass;
        private int upblendPass;
        private int addPass;

        private readonly RenderTexture[] mipmaps = new RenderTexture[16];

        private void Awake()
        {
            downblendPrefilter = bloomMat.FindPass("DownblendPrefilter");
            downblendPass = bloomMat.FindPass("Downblend");
            upblendPass = bloomMat.FindPass("Upblend");
            addPass = bloomMat.FindPass("ApplyBloom");

            Console.RegisterInstance(this);
        }
        private void OnDestroy()
        {
            Console.RemoveInstance(this);
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (settings.enableShader)
            {
                {// SetBloomMatValues
                    float knee = threshold * softThreshold;
                    Vector4 filter = new Vector4(
                        threshold,
                        threshold - knee,
                        2f * knee,
                        0.25f / (knee + float.Epsilon));

                    bloomMat.SetVector("_Filter", filter);
                    bloomMat.SetFloat("_Intensity", Mathf.GammaToLinearSpace(intensity));
                    bloomMat.SetFloat("_Scale", scale);
                }

                // Iteration Check
                int iterations = Mathf.Min(settings.iterations, (int)Mathf.Log(source.height, 2));
                RenderTexture predest = RenderTexture.GetTemporary(source.width, source.height, 24, RenderTextureFormat.ARGBFloat);

                // Instantiate Mipmaps
                for (int i = 0; i < iterations; i++)
                    mipmaps[i] = RenderTexture.GetTemporary(source.width / (1 << i), source.height / (1 << i), 0, RenderTextureFormat.ARGBFloat);

                Graphics.Blit(source, mipmaps[0], bloomMat, downblendPrefilter);
                for (int i = 1; i < iterations; i++)
                    Graphics.Blit(mipmaps[i - 1], mipmaps[i], bloomMat, downblendPass);

                for (int i = iterations - 2; i >= 0; i--)
                    Graphics.Blit(mipmaps[i + 1], mipmaps[i], bloomMat, upblendPass);

                bloomMat.SetTexture("_SourceTex", source);

                Graphics.Blit(mipmaps[0], predest, bloomMat, addPass);

                filmGrain.SetFloat("_Grain", settings.filmGrainScale / 64f);
                filmGrain.SetVector("_Seed", settings.grainSeed);
                Graphics.Blit(destination, filmGrain);

                Graphics.Blit(predest, destination);

                //Release
                for (int i = 0; i < iterations; i++)
                    RenderTexture.ReleaseTemporary(mipmaps[i]);

                RenderTexture.ReleaseTemporary(predest);
            }
            else
                Graphics.Blit(source, destination);
        }

        [ConsoleCommand("bloomy")]
        public string DebugBloomTexture()
        {
            debug = !debug;
            addPass = bloomMat.FindPass(debug ? "DebugBloomPass" : "ApplyBloom");

            return debug ? "All you see is bloom" : "Bloom normalized";
        }
    }
}
