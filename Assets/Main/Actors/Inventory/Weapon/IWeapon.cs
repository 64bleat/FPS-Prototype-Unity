using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    public interface IWeapon
    {
        GameObject WeaponEquip{get;}
        int WeaponSlot { get; }
    }
}
