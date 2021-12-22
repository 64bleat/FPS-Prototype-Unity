using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace MPCore
{
	/// <summary>
	/// Keeps a target close to the scene origin to prevent visual jittering
	/// </summary>
	public class FloatingOriginManager : MonoBehaviour
	{
		const float BOUND = 1024;
		static readonly string[] IGNORE_LAYERS = new string[] { "UI" };

		Vector3d _doublePosition;
		int _layermask;
		GameModel _gameModel;
		Transform _target;

		void Awake()
		{
			_layermask = ~LayerMask.GetMask(IGNORE_LAYERS);
			_gameModel = Models.GetModel<GameModel>();
			_gameModel.currentView.Subscribe(SetTarget);
			_doublePosition = new Vector3d(0d, 0d, 0d);
		}

		void Update()
		{
			if (_target)
				Rebase(_target.position);
			else
				enabled = false;
		}

		void SetTarget(DeltaValue<Transform> target)
		{
			_target = target.newValue;
			enabled = target.newValue;
		}

		void Rebase(Vector3 offset)
		{
			offset.x -= offset.x % BOUND;
			offset.y -= offset.y % BOUND;
			offset.z -= offset.z % BOUND;

			if (offset.magnitude >= BOUND)
			{
				GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();

				foreach (GameObject go in roots)
					if ((1 << go.layer & _layermask) != 0)
						go.transform.position -= offset;

				_doublePosition += new Vector3d(offset);
			}
		}
	}
}
