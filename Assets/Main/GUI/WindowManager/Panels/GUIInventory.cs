using MPCore;
using UnityEngine;

namespace MPGUI
{
    public class GUIInventory : MonoBehaviour
    {
        private void OnEnable()
        {
            if (CameraManager.target && CameraManager.target.GetComponentInParent<Character>() is var character && character
                && GetComponent<GUITable>() is var table && table)
                table.GenerateTable(character.inventory.ToArray());
        }
    }
}