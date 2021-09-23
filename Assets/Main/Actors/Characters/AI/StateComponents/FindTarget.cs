using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore.AI
{
    public class FindTarget : StateMachineBehaviour
    {
        private static readonly HashSet<string> storedInventoryTEMP = new HashSet<string>();

        private Character character;
        private CharacterAIAnimator ai;
        private Animator animator;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            this.animator = animator;
            animator.TryGetComponent(out ai);
            animator.TryGetComponent(out character);

            PickTarget();
        }

        private void PickTarget()
        {
            if (!animator)
                return;

            InventoryManager container = ai.GetComponent<InventoryManager>();

            Vector3 position = ai.transform.position;

            TargetInfo visualTarget = default;
            TargetInfo mentalTarget = default;
            TargetInfo moveTarget = default;

            storedInventoryTEMP.Clear();
            foreach (Inventory i in container.Inventory)
                storedInventoryTEMP.Add(i.resourcePath);

            // temp
            float range = 50f;

            // Visual Target
            foreach (Component component in AIBlackboard.visualTargets)
            {
                if(component != null
                    && component is Character compChar
                    && compChar
                    && compChar.Info
                    && (compChar.Info.team < 0 || compChar.Info.team != character.Info.team)
                    && IsTargetVisible(component, ai.viewAngle, out RaycastHit hit))
                {
                    float distance = Vector3.Distance(position, component.transform.position);
                    float priority = range - distance;

                    // Knife Check
                    if (ai.TryGetComponent(out WeaponSwitcher mySwitcher)
                        && component.TryGetComponent(out WeaponSwitcher theirSwitcher)
                        && mySwitcher.currentWeapon
                        && theirSwitcher.currentWeapon
                        && mySwitcher.currentWeapon.shortName == "Knife"
                        && theirSwitcher.currentWeapon.shortName != "Knife")
                        priority += 2000f;

                    if (priority > visualTarget.priority)
                    {
                        visualTarget.component = component;
                        visualTarget.priority = priority;
                        visualTarget.mentalPosition = hit.point;
                    }
                }
            }

            if (visualTarget.priority > 0)
            {
                if (visualTarget.component != ai.visualTarget.component)
                    visualTarget.firstSeen = Time.time;
                else
                    visualTarget.firstSeen = ai.visualTarget.firstSeen;

                //visualTarget.gameObject = visualTarget.component.gameObject;
                visualTarget.lastSeen = Time.time;

                ai.visualTarget = visualTarget;
            }

            // Mental Target
            foreach (Component component in AIBlackboard.mentalTargets)
                if (component is InventoryPickup io)
                    if (io.inventory is HealthPickup hp 
                        && character.Health != null 
                        && character.Health?.Value < character.Health.MaxValue)
                    {
                        float distance = Vector3.Distance(position, component.transform.position);
                        float hpFactor = 1f - character.Health.Value / character.Health.MaxValue;
                        float priority = (range - distance) * hpFactor ;

                        if (priority > mentalTarget.priority)
                        {
                            mentalTarget.component = component;
                            mentalTarget.priority = priority;
                            mentalTarget.mentalPosition = component.transform.position;
                        }
                    }
                    else if (io.inventory is Weapon w && !storedInventoryTEMP.Contains(w.resourcePath))
                    {
                        float distance = Vector3.Distance(position, component.transform.position);
                        float priority = (range - distance);


                        if (priority > mentalTarget.priority)
                        {
                            mentalTarget.component = component;
                            mentalTarget.priority = priority;
                            mentalTarget.mentalPosition = component.transform.position;
                        }
                    }

            if (mentalTarget.priority > 0)
            {
                if (mentalTarget.component != ai.mentalTarget.component)
                    mentalTarget.firstSeen = Time.time;
                else
                    visualTarget.firstSeen = ai.mentalTarget.firstSeen;

                mentalTarget.lastSeen = Time.time;
                //mentalTarget.gameObject = mentalTarget.component.gameObject;

                ai.mentalTarget = mentalTarget;
            }

            // Move Target
            if (mentalTarget.priority > 0 
                && mentalTarget.priority > visualTarget.priority
                && mentalTarget.priority > ai.moveTarget.priority)
                moveTarget = mentalTarget;
            else if(visualTarget.priority > 0
                && visualTarget.priority > ai.moveTarget.priority)
                moveTarget = visualTarget;

            //if (moveTarget.component && moveTarget.component == ai.moveTarget.component)
            if(moveTarget.priority > 0)
            {
                if (moveTarget.component == ai.moveTarget.component)
                {
                    //moveTarget.mentalEstimatedPosition = ai.moveTarget.mentalEstimatedPosition;
                    moveTarget.mentalPosition = ai.moveTarget.mentalPosition;
                    moveTarget.firstSeen = ai.moveTarget.firstSeen;
                    ai.moveTarget = moveTarget;
                }
                else
                {
                    ai.moveTarget = moveTarget;
                    ai.path.Clear();
                } 
            }
        }

        private bool IsTargetVisible(Component target, float viewAngle, out RaycastHit hit)
        {
            CharacterBody body = ai.GetComponent<CharacterBody>();
            hit = default;

            return Vector3.Angle(target.transform.position - body.cameraSlot.position, body.cameraSlot.forward) <= viewAngle
                && Physics.Raycast(body.cameraSlot.position,
                    target.transform.position - body.cameraSlot.position,
                    out hit,
                    Vector3.Distance(body.cameraSlot.position, target.transform.position),
                    ai.layerMask,
                    QueryTriggerInteraction.Ignore)
                && hit.collider.gameObject.Equals(target.gameObject);
        }
    }
}
