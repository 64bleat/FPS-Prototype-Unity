using MPConsole;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Console = MPConsole.Console;

namespace MPCore.AI
{
    [ContainsConsoleCommands]
    public class CharacterAIAnimator : MonoBehaviour
    {
        public float viewAngle = 45;
        [System.NonSerialized] public TargetInfo moveTarget;
        [System.NonSerialized] public TargetInfo visualTarget;
        [System.NonSerialized] public TargetInfo mentalTarget;
        [System.NonSerialized] public TargetInfo touchTarget;
        [System.NonSerialized] public int layerMask;
        [System.NonSerialized] public readonly List<Vector3> path = new List<Vector3>();

        private static readonly string[] layers = { "Default", "Physical", "Player" };

        #region Unity Messages
        private void Awake()
        {
            PauseManager.AddListener(Pause);
            Console.AddInstance(this);

            if (TryGetComponent(out DamageEvent damageEvent))
                damageEvent.OnHit += OnHit;    

            if (TryGetComponent(out Character character))
                character.OnRegistered += SetPlayer;

            layerMask = LayerMask.GetMask(layers);
        }

        private void OnEnable()
        {
            if (TryGetComponent(out Animator animator))
                animator.enabled = true;
        }

        private void OnDisable()
        {

            if (TryGetComponent(out Animator animator))
                animator.enabled = false;
        }

        private void OnDestroy()
        {
            PauseManager.RemoveListener(Pause);
            Console.RemoveInstance(this);

            if (TryGetComponent(out DamageEvent damageEvent))
                damageEvent.OnHit -= OnHit;

            if (TryGetComponent(out Character character))
                character.OnRegistered -= SetPlayer;
        }

        private void Update()
        {
            for (int i = 1; i < path.Count; i++)
                Debug.DrawLine(path[i - 1], path[i], Color.Lerp(Color.magenta, Color.cyan, (float)i / path.Count), 0.25f, true);
        }
        #endregion

        private void Pause(bool paused)
        {
            if(TryGetComponent(out Character character) 
                && !character.isPlayer
                && TryGetComponent(out Animator animator))
                animator.enabled = !paused;
        }

        private void SetPlayer(bool isPlayer)
        {
            if (TryGetComponent(out Animator animator))
                animator.enabled = enabled = !isPlayer;
        }

        private void OnHit(DamageTicket ticket)
        {
            if (!ticket.instigatorBody || !touchTarget.component)
                return;

            if (ticket.instigatorBody != touchTarget.component.gameObject)
                touchTarget.firstSeen = Time.time;

            //touchTarget.gameObject = ticket.instigatorBody;
            touchTarget.mentalPosition = ticket.instigatorBody.transform.position;
            touchTarget.priority = ticket.damage * 10;
            touchTarget.lastSeen = Time.time;
        }

        [ConsoleCommand("posess", "Toggle AI for players")]
        public string Posess()
        {
            if (TryGetComponent(out Character character)
                && character.isPlayer
                && TryGetComponent(out Animator animator))
            {
                animator.enabled = !animator.enabled;

                return character.isPlayer ? animator.enabled ? "Surrendered control" : "Control restored" : null;
            }
            else 
                return null;
        }
    }
}
