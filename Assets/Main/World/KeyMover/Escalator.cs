using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Escalator : MonoBehaviour
{
    public Transform keySet;
    public Rigidbody mover;
    public int stepCount;
    public float startTime;

    private void Awake()
    {
        //if (keySet && mover) 
        //{
        //    // Key Array
        //    MovementKey[] keys = new MovementKey[keySet.childCount];
        //    for (int i = 0; i < keys.Length; i++)
        //        keys[i] = keySet.GetChild(i).gameObject.GetComponent<MovementKey>();

        //    // TotalTime
        //    float totalTime = 0;
        //    for (int i = 0; i < keys.Length; i++)
        //        totalTime += keys[i].transitionTime;

        //    float timeOffset = totalTime / stepCount;

        //    for(int i = 0; i < stepCount; i++)
        //    {
        //        float stepTime = Mathf.Repeat(startTime + timeOffset * i, totalTime);
        //        int keyIndex = 0;

        //        while (keys[keyIndex].transitionTime < stepTime)
        //            stepTime -= keys[keyIndex++].transitionTime;

        //        KeyMover newStep = gameObject.AddComponent<KeyMover>();
        //        newStep.keySet = keySet;
        //        newStep.mover = Instantiate(mover, transform.parent);
        //        newStep.keyIndex = keyIndex;
        //        newStep.keyTime = stepTime;
        //        newStep.SetKeys(keyIndex);
        //    }
        //}
    }
}
