using System.Collections.Generic;
using UnityEngine;

public class KeyMover : MonoBehaviour
{
    public float keyTime = 0;
    public float timeScale = 1;

    private readonly List<MovementKey> keys = new List<MovementKey>();
    private readonly List<Rigidbody> movers = new List<Rigidbody>();
    private float trackLength = 0f;

    private void Awake()
    {
        keys.AddRange(GetComponentsInChildren<MovementKey>());
        movers.AddRange(GetComponentsInChildren<Rigidbody>());

        // Initialize Track Length
        foreach (MovementKey key in keys)
            trackLength += key.transitionTime;
    }

    private void FixedUpdate()
    {
        for(int i = 0; i < movers.Count; i++)
        {
            float timePos = Mathf.Repeat(keyTime + (float)i / (float)movers.Count * trackLength, trackLength);

            for(int k = 0; k < keys.Count && timePos > 0; k++)
                if((timePos -= keys[k].transitionTime) <= 0)
                {
                    float lerpPos = keys[k].transitionTime == 0 ? 1 : Mathf.Repeat(timePos, keys[k].transitionTime) / keys[k].transitionTime;
                    int l = (k + 1) % keys.Count;

                    if (keys[k].transitionTime == 0)
                        movers[i].gameObject.SetActive(false);
                    movers[i].MovePosition(Vector3.Lerp(keys[k].transform.position, keys[l].transform.position, lerpPos));
                    movers[i].MoveRotation(Quaternion.Lerp(keys[k].transform.rotation, keys[l].transform.rotation, lerpPos));
                    movers[i].transform.localScale = Vector3.Lerp(keys[k].transform.localScale, keys[l].transform.localScale, lerpPos);
                    if (keys[k].transitionTime == 0)
                        movers[i].gameObject.SetActive(true);


                }
        }

        keyTime = Mathf.Repeat(keyTime + Time.fixedDeltaTime * timeScale, trackLength);
    }
}
