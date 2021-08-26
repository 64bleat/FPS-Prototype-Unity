using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;

namespace MPCore
{
    public class ScoreboardViewModel : MonoBehaviour
    {
        [SerializeField] private GameObject _scoreboardPanel;
        [SerializeField] private TextMeshProUGUI _rowPrefab;

        private Scoreboard _scoreboard;
        private InputManager _input;

        private static readonly SortedList<float, DataRow> _sortedRows = new SortedList<float, DataRow>();

        private void Awake()
        {
            _scoreboard = Models.GetModel<Scoreboard>();
            _input = GetComponentInParent<InputManager>();
            _input.Bind("Scoreboard", Enable, this, KeyPressType.Down);
            _input.Bind("Scoreboard", Disable, this, KeyPressType.Up);
        }

        private void OnEnable()
        {
            Refresh();

            _scoreboard.OnTableChanged.AddListener(Refresh);
        }

        private void OnDisable()
        {
            _scoreboard.OnTableChanged.RemoveListener(Refresh);
        }

        private void OnDestroy()
        {
            _input.Unbind(this);
        }

        private void Enable()
        {
            _scoreboardPanel.SetActive(true);
        }

        private void Disable()
        {
            _scoreboardPanel.SetActive(false);
        }

        private void Refresh()
        {
            // Destroy Old
            int childCount = _scoreboardPanel.transform.childCount;

            _sortedRows.Clear();

            for (int i = 0; i < childCount; i++)
            {
                GameObject child = _scoreboardPanel.transform.GetChild(i).gameObject;

                if(child != _rowPrefab.gameObject)
                    Destroy(child);
            }

            // Get Sorted
            //foreach (DataRow row in _scoreboard.scoreTable.Rows)
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
            foreach(DataRow row in _sortedRows.Values)
            {
                GameObject entry = Instantiate(_rowPrefab.gameObject, _scoreboardPanel.transform);
                string name = _scoreboard.GetValue<string>(row, Scoreboard.Columns.Name);
                int kills = _scoreboard.GetValue<int>(row, Scoreboard.Columns.KillCount);
                int deaths = _scoreboard.GetValue<int>(row, Scoreboard.Columns.DeathCount);

                if (entry.TryGetComponent(out TextMeshProUGUI text))
                    text.SetText($"{name, 20} :{kills, 4}K :{deaths, 4}D");

                entry.SetActive(true);
            }
        }
    }
}
