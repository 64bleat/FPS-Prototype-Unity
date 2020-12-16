using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InstantiateObjectEvent : MonoBehaviour
{
    public int maxCopies = 1;

    private readonly HashSet<GameObject> copies = new HashSet<GameObject>();

    public void SelfInstantiate()
    {
        copies.RemoveWhere(c => c == null);

        if (copies.Count < maxCopies)
        {
            GameObject obj = Instantiate(gameObject, transform.position, transform.rotation, transform.parent);

            obj.SetActive(true);
            copies.Add(obj);
        }
    }
}
