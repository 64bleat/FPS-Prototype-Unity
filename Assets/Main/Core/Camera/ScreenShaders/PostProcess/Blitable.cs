using UnityEngine;

namespace MPCore
{
    public class Blitable : ScriptableObject
    {
        public Material material;
        public int passIndex;

        public virtual void Blit(Texture source, RenderTexture dest)
        {
            Graphics.Blit(source, dest, material, passIndex);
        }
    }
}
