using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace MPCore.AI
{
	public class FollowTarget : StateMachineBehaviour
	{
		private Animator animator;
		private CharacterAIAnimator ai;
		private Character character;
		private CharacterBody body;
		private InputManager input;
		private KeyModel _keyModel;

		// Pathfinding
		private JobHandle pathJob;

		// Debug
		GameModel _gameModel;
		public LineRenderer debugLine;
		private LineRenderer pathLine;

		private static readonly Dictionary<Type, float> satisfactionDistances = new Dictionary<Type, float>()
		{
			{ typeof(InventoryPickup), 0.25f },
			{ typeof(Character), 1 }
		};


		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			_gameModel = Models.GetModel<GameModel>();
			_keyModel = Models.GetModel<KeyModel>();
			this.animator = animator;
			animator.TryGetComponent(out ai);
			animator.TryGetComponent(out body);
			animator.TryGetComponent(out input);
			animator.TryGetComponent(out character);

			if(ai.path.Count == 0)
				RequestNewPath();
		}

		public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			MoveAlongPath();

			if (_gameModel.debug.Value)
			{
				if (!pathLine && debugLine)
					pathLine = Instantiate(debugLine).GetComponent<LineRenderer>();

				if (pathLine)
				{
					pathLine.positionCount = ai.path.Count;

					for (int i = 0; i < ai.path.Count; i++)
						pathLine.SetPosition(i, ai.path[i]);
				}
			}
			else if (pathLine)
				Destroy(pathLine);
		}

		private void RequestNewPath()
		{
			// Determin Destination
			float lastSeen = Time.time - ai.moveTarget.lastSeen;
			Vector3 estimate;

			if (lastSeen > 5f)
			{
				estimate = Navigator.RandomPoint(body.HitBox.height / 2);
			}
			else if (ai.moveTarget.component && ai.moveTarget.component is Character)
			{
				Vector3 rand = UnityEngine.Random.insideUnitSphere;
				Vector3 offset = Vector3.zero;

				// Positioning for selected weapon
				if (ai.moveTarget.component.TryGetComponent(out WeaponSwitcher weapons) && weapons.currentWeapon)
					offset += rand * weapons.currentWeapon.preferredCombatDistance;

				// Positioning for hidden extrapolation
				offset += rand * lastSeen * 5f;

				// Positioning to avoid clipping
				if (Physics.Raycast(ai.moveTarget.mentalPosition, offset, out RaycastHit hit, offset.magnitude, ai.layerMask, QueryTriggerInteraction.Ignore))
					offset = hit.point;

				// Logging mental position
				estimate = offset;
			}
			else
				estimate = ai.moveTarget.mentalPosition;

			// Request Path
			Vector3 localPathOffset = new Vector3(0, -body.HitBox.height * 0.5f, 0);
			Vector3 position = ai.transform.TransformPoint(localPathOffset);

			ai.path.Clear();
			pathJob = Navigator.RequestPath(position, estimate, ai.path);
		}

		private void MoveAlongPath()
		{
			if (ai.path.Count > 0)
			{
				Vector3 localPathOffset = new Vector3(0, -body.HitBox.height * 0.5f, 0);
				Vector3 position = ai.transform.TransformPoint(localPathOffset);
				Vector3 pathEndPosition = ai.path[ai.path.Count - 1];
				float flatEndOffset = Vector3.ProjectOnPlane(pathEndPosition - position, ai.transform.up).magnitude;
				Vector3 pClamp = Navigator.ClampToPath(ai.path, position, out float pIndex);
				float groundDistance = Vector3.ProjectOnPlane(pClamp - position, ai.transform.up).magnitude;
				float foretravel = Mathf.Max(1f, 2f - groundDistance);
				Vector3 moveDestination = Navigator.PathLerp(ai.path, pIndex, foretravel);
				Type targetType = ai.moveTarget.component ? ai.moveTarget.component.GetType() : null;

				if (targetType == null || !satisfactionDistances.TryGetValue(targetType, out float completionDistance))
					completionDistance = 0.5f;

				{   // DEBUG
					Color color;
					if (groundDistance > 2f) color = Color.blue;
					else if (flatEndOffset > completionDistance) color = Color.green;
					else color = Color.red;

					Debug.DrawLine(position, moveDestination, color, 1f, true);
				}

				if (flatEndOffset > completionDistance)
				{
					Vector3 direction = Vector3.ProjectOnPlane(moveDestination - position, ai.transform.up);
					float fAngle = Vector3.Angle(ai.transform.forward, direction);
					float rAngle = Vector3.Angle(ai.transform.right, direction);

					if (fAngle < 67.5f)
						input.BotKeyDown("Forward");
					else if (fAngle > 112.5)
						input.BotKeyDown("Reverse");

					if (rAngle < 67.5f)
						input.BotKeyDown("Right");
					else if (rAngle > 112.5)
						input.BotKeyDown("Left");

					if (!character.IsPlayer || !_keyModel.alwaysRun)
						input.BotKeyDown("Sprint");

					if (body.currentState == CharacterBody.MoveState.Grounded)
					{
						float upAngle = Vector3.Angle(ai.transform.up, moveDestination - position);

						if (upAngle < 40
							|| Physics.SphereCast(ai.transform.position, body.HitBox.radius * 0.5f, ai.transform.forward, out _, body.HitBox.radius * 2))
						{
							input.BotKeyDown("Jump", 0.125f);
							input.BotKeyDown("Crouch", 0.25f);
							input.BotKeyDown("Forward", 0.25f);
						}
					}

					if (groundDistance > 2f)
						RequestNewPath();
					else if(ai.moveTarget.component == ai.visualTarget.component
						&& Time.time - ai.moveTarget.lastSeen < 0.5f)
					{
						float tDistance = Vector3.ProjectOnPlane(pathEndPosition - ai.moveTarget.mentalPosition, ai.transform.up).magnitude;
						if (tDistance > 5f)
							RequestNewPath();
					}
				}
				else
				{
					ai.path.Clear();
					ai.moveTarget = default;
					animator.SetTrigger("GetNewTarget");
				}
			}
		}
	}
}
