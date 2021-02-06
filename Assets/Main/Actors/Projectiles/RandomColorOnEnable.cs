using UnityEngine;

namespace MPWorld
{
    public class RandomColorOnEnable : MonoBehaviour
    {
        public float hue;
        
        private MaterialPropertyBlock mpb;
        private Renderer[] renderers;

        private void Awake()
        {
            mpb = new MaterialPropertyBlock();
            renderers = GetComponentsInChildren<Renderer>();
        }

        private void OnEnable()
        {
            Color originalColor = Color.HSVToRGB(Mathf.Repeat(Random.Range(-0.12f, 0.12f) + hue, 1), 0.6f, 4f, true);
            mpb.SetColor("_EmissionColor", originalColor);

            foreach (Renderer r in renderers)
                r.SetPropertyBlock(mpb);
        }
    }
}
