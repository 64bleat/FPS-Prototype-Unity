using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public class GraphicsModel : Models
    {
        public DataValue<float> fov;
        public DataValue<float> fovFirstPerson;
        public Material pixelationShader;
        public List<Material> pixelationOptions;
    }
}
