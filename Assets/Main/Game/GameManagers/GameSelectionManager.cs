using MPCore;
using System.Collections.Generic;
using UnityEngine;

namespace MPGame
{
	/// <summary>
	/// Loads the correct game mode from the selection model
	/// </summary>
	public class GameSelectionManager : MonoBehaviour
	{
		PlaySettingsModel _caseSelectionModel;

		readonly List<Mutator> _mutators = new List<Mutator>();

		void Awake()
		{
			_caseSelectionModel = Models.GetModel<PlaySettingsModel>();
			_mutators.AddRange(_caseSelectionModel.mutators);

			foreach (Mutator mutator in _mutators)
				mutator.Activate();

			GameManager game = Instantiate(_caseSelectionModel.game, transform);
		}

		void OnDestroy()
		{
			foreach (Mutator mutator in _mutators)
				mutator.Deactivate();
		}
	}
}
