using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;

namespace MPCore
{
	public class ScoreboardViewModel : MonoBehaviour
	{
		[SerializeField] GameObject _scoreboardPanel;
		[SerializeField] TextMeshProUGUI _rowPrefab;

		Scoreboard _scoreboard;
		InputManager _input;
		GameModel _gameModel;

		readonly SortedList<float, DataRow> _sortedRows = new SortedList<float, DataRow>();

		void Awake()
		{
			_gameModel = Models.GetModel<GameModel>();
			_scoreboard = Models.GetModel<Scoreboard>();

			_input = GetComponentInParent<InputManager>();
			_input.Bind("Scoreboard", Enable, this, KeyPressType.Down);
			_input.Bind("Scoreboard", Disable, this, KeyPressType.Up);

			MessageBus.Subscribe<GameModel.GameReset>(ResetScoreboard);
			MessageBus.Subscribe<GameModel.CharacterJoined>(AddCharacter);
			MessageBus.Subscribe<GameModel.CharacterDied>(ScoreKill);

			_scoreboard.Reset();
		}

		void OnEnable()
		{
			Refresh();
			_scoreboard.OnTableChanged.AddListener(Refresh);
		}

		void OnDisable()
		{ 
			_scoreboard.OnTableChanged.RemoveListener(Refresh);
		}

		void OnDestroy()
		{
			_input.Unbind(this);
			MessageBus.Unsubscribe<GameModel.CharacterDied>(ScoreKill);
			MessageBus.Unsubscribe<GameModel.CharacterJoined>(AddCharacter);
			MessageBus.Unsubscribe<GameModel.GameReset>(ResetScoreboard);
		}

		void ResetScoreboard(GameModel.GameReset _) => _scoreboard.Reset();
		void ScoreKill(GameModel.CharacterDied death) => _scoreboard.AddKill(death);
		void AddCharacter(GameModel.CharacterJoined join) => _scoreboard.AddCharacter(join);

		void Enable()
		{
			_scoreboardPanel.SetActive(true);
		}

		void Disable()
		{
			_scoreboardPanel.SetActive(false);
		}

		void Refresh()
		{
			// Destroy Old
			_sortedRows.Clear();

			foreach (Transform child in _scoreboardPanel.transform)
				if (child != _rowPrefab.transform)
					Destroy(child.gameObject);

			// Get Sorted
			foreach (DataRow row in _scoreboard.GetRows())
			{
				int killCount = _scoreboard.GetValue<int>(row, Scoreboard.Columns.KillCount);
				float timeVal = _scoreboard.GetValue<float>(row, Scoreboard.Columns.LastKillTime);
				float sortVal = -killCount - timeVal;

				while (_sortedRows.ContainsKey(sortVal))
					sortVal += Random.value * 0.0001f;

				_sortedRows.Add(sortVal, row);
			}

			// Instantiate new
			foreach (DataRow row in _sortedRows.Values)
			{
				TMP_Text entry = Instantiate(_rowPrefab, _scoreboardPanel.transform);
				string name = _scoreboard.GetValue<string>(row, Scoreboard.Columns.Name);
				int kills = _scoreboard.GetValue<int>(row, Scoreboard.Columns.KillCount);
				int deaths = _scoreboard.GetValue<int>(row, Scoreboard.Columns.DeathCount);

				entry.SetText($"{name,20} :{kills,4}K :{deaths,4}D");
				entry.gameObject.SetActive(true);
			}
		}
	}
}
