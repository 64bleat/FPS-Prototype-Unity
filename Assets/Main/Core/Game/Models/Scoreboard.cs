using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Events;

namespace MPCore
{
    /// <summary>
    /// Scoring data for deathmatch games
    /// </summary>
    public class Scoreboard : Models
    {
        public enum Columns { KillCount, DeathCount, Name, LastKillTime}

        public UnityEvent OnTableChanged = new UnityEvent();

        private Dictionary<int, int> _characterMap;
        private DataSet _dataSet;
        private DataTable _scoreTable = new DataTable("Scoreboard");
        private DataColumn _characterId;
        private DataColumn _killCount;
        private DataColumn _deathCount;
        private DataColumn _displayName;
        private DataColumn _lastKillTime;

        private void Awake()
        {
            Reset();
        }

        /// <summary>
        /// Re-initializes the table
        /// </summary>
        public void Reset()
        {
            _scoreTable = new DataTable("Scoreboard");
            _scoreTable.Clear();
            _scoreTable.Reset();

            _characterId = new DataColumn("id", typeof(int))
            {
                Unique = true,
                AutoIncrement = false,
                ReadOnly = true
            };

            _scoreTable.Columns.Add(_characterId);
            _scoreTable.Columns.Add(_killCount = new DataColumn("kills", typeof(int)));
            _scoreTable.Columns.Add(_lastKillTime = new DataColumn("killTime", typeof(float)));
            _scoreTable.Columns.Add(_deathCount = new DataColumn("deaths", typeof(int)));
            _scoreTable.Columns.Add(_displayName = new DataColumn("name", typeof(string)));
            _scoreTable.PrimaryKey = new DataColumn[] { _characterId };
            _scoreTable.AcceptChanges();

            _dataSet = new DataSet("ScoreData");
            _characterMap = new Dictionary<int, int>();
        }

        /// <summary>
        /// Adds a new row to the table representing character.
        /// </summary>
        public void AddCharacter(CharacterInfo character, bool isPlayer)
        {
            DataRow row = _scoreTable.NewRow();
            row[_characterId] = character.id;
            row[_displayName] = character.displayName;
            row[_killCount] = 0;
            row[_deathCount] = 0;
            row[_lastKillTime] = 1f;
            _characterMap.Add(character.id, _scoreTable.Rows.Count);
            _scoreTable.Rows.Add(row);
            _scoreTable.AcceptChanges();
            OnTableChanged?.Invoke();
        }

        /// <summary>
        /// Record a kill to the table
        /// </summary>
        public void AddKill(DeathInfo death)
        {
            CharacterInfo killer = death.instigator;
            CharacterInfo victim = death.victim;

            if (victim && killer 
                && killer != victim 
                && _characterMap.TryGetValue(killer.id, out int i))
            {
                _scoreTable.Rows[i][_killCount] = (int)_scoreTable.Rows[i][_killCount] + 1;
                _scoreTable.Rows[i][_lastKillTime] = Mathf.Log10(Time.time + 1f);
            }

            if (victim && _characterMap.TryGetValue(victim.id, out i))
                _scoreTable.Rows[i][_deathCount] = (int)_scoreTable.Rows[i][_deathCount] + 1;

            _scoreTable.AcceptChanges();
            OnTableChanged?.Invoke();
        }

        /// <summary>
        /// Iterates over the table rows
        /// </summary>
        public IEnumerable<DataRow> GetRows()
        {
            foreach (DataRow row in _scoreTable.Rows)
                yield return row;
        }

        /// <summary>
        /// Returns the value contained in a given cell
        /// </summary>
        public T GetValue<T>(DataRow row, Columns value)
        {
            switch (value)
            {
                case Columns.KillCount:
                    return (T)row[_killCount];
                case Columns.DeathCount:
                    return (T)row[_deathCount];
                case Columns.Name:
                    return (T)row[_displayName];
                case Columns.LastKillTime:
                    return (T)row[_lastKillTime];
            }

            return default;
        }
    }
}