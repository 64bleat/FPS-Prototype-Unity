using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore.Movers
{
    public class Rotator : MonoBehaviour
    {

        //private float y;
        // Use this for initialization
        void Start()
        {
            //y = 0;
        }

        // Update is called once per frame
        void Update()
        {
            // y += Time.deltaTime * 180;
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + Time.deltaTime * 180, 0);
        }
    }
}
