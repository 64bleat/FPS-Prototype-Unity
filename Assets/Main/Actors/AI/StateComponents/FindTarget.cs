using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPCore.AI
{
	public class FindTarget : StateMachineBehaviour
	{
		static readonly HashSet<string> _invBuffer = new HashSet<string>();

		AIModel _aiModel;
		Character _character;
		CharacterAIAnimator _ai;
		Animator _animator;

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			_aiModel = Models.GetModel<AIModel>();
			_animator = animator;
			animator.TryGetComponent(out _ai);
			animator.TryGetComponent(out _character);

			PickTarget();
		}

		void PickTarget()
		{
			if (!_animator)
				return;

			InventoryManager container = _ai.GetComponent<InventoryManager>();

			Vector3 position = _ai.transform.position;

			TargetInfo visualTarget = default;
			TargetInfo mentalTarget = default;
			TargetInfo moveTarget = default;

			_invBuffer.Clear();
			foreach (Inventory i in container.Inventory)
				_invBuffer.Add(i.resourcePath);

			// temp
			float range = 50f;

			// Visual Target
			foreach(Character character in _aiModel.characters)
			{
				if(character && character.Info
					&& (character.Info.team < 0 || character.Info.team != _character.Info.team)
					&& IsTargetVisible(character, _ai.viewAngle, out RaycastHit hit))
				{
					float distance = Vector3.Distance(position, character.transform.position);
					float priority = range - distance;

					// Knife Check
					if (_ai.TryGetComponent(out WeaponSwitcher mySwitcher)
						&& character.TryGetComponent(out WeaponSwitcher theirSwitcher)
						&& mySwitcher.currentWeapon
						&& theirSwitcher.currentWeapon
						&& mySwitcher.currentWeapon.shortName == "Knife"
						&& theirSwitcher.currentWeapon.shortName != "Knife")
						priority += 2000f;

					if (priority > visualTarget.priority)
					{
						visualTarget.component = character;
						visualTarget.priority = priority;
						visualTarget.mentalPosition = hit.point;
					}
				}
			}

			if (visualTarget.priority > 0)
			{
				if (visualTarget.component != _ai.visualTarget.component)
					visualTarget.firstSeen = Time.time;
				else
					visualTarget.firstSeen = _ai.visualTarget.firstSeen;

				//visualTarget.gameObject = visualTarget.component.gameObject;
				visualTarget.lastSeen = Time.time;

				_ai.visualTarget = visualTarget;
			}

			// Mental Target
			foreach(InventoryPickup pickup in _aiModel.pickups)
				if (pickup.inventory is HealthPickup hp 
					&& _character.Health != null 
					&& _character.Health?.Value < _character.Health.MaxValue)
				{
					float distance = Vector3.Distance(position, pickup.transform.position);
					float hpFactor = 1f - _character.Health.Value / _character.Health.MaxValue;
					float priority = (range - distance) * hpFactor ;

					if (priority > mentalTarget.priority)
					{
						mentalTarget.component = pickup;
						mentalTarget.priority = priority;
						mentalTarget.mentalPosition = pickup.transform.position;
					}
				}
				else if (pickup.inventory is Weapon w && !_invBuffer.Contains(w.resourcePath))
				{
					float distance = Vector3.Distance(position, pickup.transform.position);
					float priority = (range - distance);


					if (priority > mentalTarget.priority)
					{
						mentalTarget.component = pickup;
						mentalTarget.priority = priority;
						mentalTarget.mentalPosition = pickup.transform.position;
					}
				}

			if (mentalTarget.priority > 0)
			{
				if (mentalTarget.component != _ai.mentalTarget.component)
					mentalTarget.firstSeen = Time.time;
				else
					visualTarget.firstSeen = _ai.mentalTarget.firstSeen;

				mentalTarget.lastSeen = Time.time;
				//mentalTarget.gameObject = mentalTarget.component.gameObject;

				_ai.mentalTarget = mentalTarget;
			}

			// Move Target
			if (mentalTarget.priority > 0 
				&& mentalTarget.priority > visualTarget.priority
				&& mentalTarget.priority > _ai.moveTarget.priority)
				moveTarget = mentalTarget;
			else if(visualTarget.priority > 0
				&& visualTarget.priority > _ai.moveTarget.priority)
				moveTarget = visualTarget;

			//if (moveTarget.component && moveTarget.component == ai.moveTarget.component)
			if(moveTarget.priority > 0)
			{
				if (moveTarget.component == _ai.moveTarget.component)
				{
					//moveTarget.mentalEstimatedPosition = ai.moveTarget.mentalEstimatedPosition;
					moveTarget.mentalPosition = _ai.moveTarget.mentalPosition;
					moveTarget.firstSeen = _ai.moveTarget.firstSeen;
					_ai.moveTarget = moveTarget;
				}
				else
				{
					_ai.moveTarget = moveTarget;
					_ai.path.Clear();
				} 
			}
		}

		bool IsTargetVisible(Component target, float viewAngle, out RaycastHit hit)
		{
			CharacterBody body = _ai.GetComponent<CharacterBody>();
			hit = default;

			return Vector3.Angle(target.transform.position - body.cameraSlot.position, body.cameraSlot.forward) <= viewAngle
				&& Physics.Raycast(body.cameraSlot.position,
					target.transform.position - body.cameraSlot.position,
					out hit,
					Vector3.Distance(body.cameraSlot.position, target.transform.position),
					_ai.layerMask,
					QueryTriggerInteraction.Ignore)
				&& hit.collider.gameObject.Equals(target.gameObject);
		}
	}
}
