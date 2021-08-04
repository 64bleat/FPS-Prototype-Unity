using MPCore;
using UnityEngine;

namespace MPGUI
{
    public class BloomModel : Models
    {
        public bool enableShader = true;
        public int iterations = 8;
        public float filmGrainScale = 1;
        public Vector3 grainSeed = new Vector3(25.36535f, 13.12572f, 96.23642f);
    }
}
