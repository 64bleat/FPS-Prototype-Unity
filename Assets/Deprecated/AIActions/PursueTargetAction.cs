using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Diagnostics;

namespace MPCore
{
    public class PursueTargetAction : AIAction
    {
        public List<AIAction> lookActions,
            moveActions,
            otherActions;

        private readonly string[] eyeLayers = { "Default", "Physical", "Player" };
        private readonly Stopwatch time = new Stopwatch();
        private readonly GOAP moveGoap = new GOAP();
        private readonly GOAP goap = new GOAP();
        private CharacterAI ai;
        private GameObject priorityTarget;
        private Character character;
        private int eyeMask;

        public override void InstanceAwake()
        {
            ai = gameObject.GetComponent<CharacterAI>();
            character = gameObject.GetComponent<Character>();

            eyeMask = LayerMask.GetMask(eyeLayers);

            // Load actions into goaps
            foreach (AIAction act in moveActions)
                moveGoap.AddActions(act.InstantiateFor(gameObject));
            foreach (AIAction act in otherActions)
                goap.AddActions(act.InstantiateFor(gameObject));
        }

        public override float? Priority(IGOAPAction successor)
        {
            //bool seekHealth = character.info.ResourceManagement.ValueOf(character.hurtResource) < 90;
            bool seekHealth = ResourceManager.ValueOf(character./*info.*/resources, character.hurtResource) < 90;

            GameObject[] list;

            if (seekHealth)
                list = (from spawner in GameObject.FindObjectsOfType<Respawner>()
                        where spawner.itemToSpawn
                        let invobj = spawner.itemToSpawn.GetComponent<InventoryPickup>()
                        where invobj && invobj.inventory is HealthPickup
                        select spawner.gameObject).ToArray();
            else
                list = (from pick in GameObject.FindObjectsOfType<Character>()
                        //where CheckSight(pick.gameObject)
                       select pick.gameObject).ToArray();

            var result = (from target in list
                          where !gameObject.Equals(target)
                          let priority = 10f / Mathf.Max(1f, Vector3.Distance(gameObject.transform.position, target.transform.position))
                          orderby priority descending
                          select new { target, priority }).FirstOrDefault();

            priorityTarget = result?.target;

            return result?.priority;
        }

        public override void OnStart()
        {
            ai.target = priorityTarget;
            time.Restart();
        }

        public override GOAPStatus Update()
        {
            if (ai && ai.target)
            {
                moveGoap.GOAPUpdate();
                goap.GOAPUpdate();

                return time.Elapsed.TotalSeconds < 0.5 ? GOAPStatus.Running : GOAPStatus.Continue;
            }
            else
                return GOAPStatus.Fail;
        }

        public override void OnEnd()
        {
            ai.targetPath = null;
            ai.targetPathIndex = 0;
        }

        private bool CheckSight(GameObject target)
        {
            Vector3 targetOffset = target.transform.position - gameObject.transform.position;

            return Physics.Raycast(gameObject.transform.position, targetOffset, out RaycastHit hit,
                targetOffset.magnitude, eyeMask, QueryTriggerInteraction.Ignore)
                && hit.collider.gameObject.Equals(target);
        }
    }
}
