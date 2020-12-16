using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPWorld
{
    /// <summary> 
    ///     Interface for interacting with gravity zones. 
    /// </summary>
    public interface IGravityUser
    {
        List<GravityZone> GravityZones { get; set; }
        Vector3 Gravity { get; set; }
        Vector3 Velocity { get; set; }
        float Mass { get; }
    }
}
