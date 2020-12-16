using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Keeps a target close to the scene origin to prevent visual jittering
/// </summary>
public class FloatingOriginManager : MonoBehaviour
{
    public float boundDistance = 1024;
    public Vector3 startingDistance = Vector3.zero;
    public Vector3d reference;
    public Transform target;
    public string[] ignoreLayers;

    private int layermask;

    private void Awake()
    {
        reference = new Vector3d(startingDistance);

        if (!target)
            target = Camera.main.transform;

        layermask = ~LayerMask.GetMask(ignoreLayers);
    }

    private void Update()
    {
        if (target)
        {

            Rebase(target.position);
        }
    }

    public void Rebase(Vector3 offset)
    {
        offset.x -= offset.x % boundDistance;
        offset.y -= offset.y % boundDistance;
        offset.z -= offset.z % boundDistance;

        if (offset.magnitude >= boundDistance)
        {
            Debug.Log("Debase!");
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();

            foreach (GameObject go in roots)
                if ((1 << go.layer & layermask) != 0)
                    go.transform.position -= offset;

            reference += new Vector3d(offset);
        }
    }
}
