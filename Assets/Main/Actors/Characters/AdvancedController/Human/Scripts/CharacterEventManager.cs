using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore
{
    /// <summary>
    /// Store all the event references relating to a character in one place
    /// </summary>
    public class CharacterEventManager : MonoBehaviour
    {
        public HudEvents hud;
        public StringEvent onSpeedSet;

        private Character character;
        private CharacterBody body;

        private void Awake()
        {
            character = GetComponent<Character>();
            body = GetComponent<CharacterBody>();
        }

        private void Update()
        {
            if (character.isPlayer)
            {
                int groundSpeed = (int)Mathf.Round(Vector3.ProjectOnPlane(body.Velocity, transform.up).magnitude * 10);
                onSpeedSet.Invoke($"{groundSpeed,3}");
            }
        }
    }
}
