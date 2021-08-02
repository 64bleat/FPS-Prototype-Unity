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
        public readonly DataTable table = new DataTable("Scoreboard");
        public UnityEvent OnTableChanged;

        private Dictionary<int, int> rowId;
        private DataSet dataSet;
        private DataColumn id;
        public DataColumn kills;
        public DataColumn deaths;
        public DataColumn displayName;
        public DataColumn lastKill;

        public void Clear()
        {
            table.Clear();
            table.Reset();

            table.Columns.Add(id = new DataColumn("id", typeof(int))
            {
                Unique = true,
                AutoIncrement = false,
                ReadOnly = true
            });

            table.Columns.Add(kills = new DataColumn("kills", typeof(int)));
            table.Columns.Add(lastKill = new DataColumn("killTime", typeof(float)));
            table.Columns.Add(deaths = new DataColumn("deaths", typeof(int)));
            table.Columns.Add(displayName = new DataColumn("name", typeof(string)));
            table.PrimaryKey = new DataColumn[] { id };
            table.AcceptChanges();

            dataSet = new DataSet("ScoreData");
            rowId = new Dictionary<int, int>();
        }

        public void AddCharacter(CharacterInfo character)
        {
            DataRow row = table.NewRow();
            row[id] = character.id;
            row[displayName] = character.displayName;
            row[kills] = 0;
            row[deaths] = 0;
            row[lastKill] = 1f;
            rowId.Add(character.id, table.Rows.Count);
            table.Rows.Add(row);
            table.AcceptChanges();
            OnTableChanged?.Invoke();
        }

        public void AddKill(CharacterInfo killer, CharacterInfo victim)
        {
            if (killer && killer != victim && rowId.TryGetValue(killer.id, out int i))
            {
                table.Rows[i][kills] = (int)table.Rows[i][kills] + 1;
                table.Rows[i][lastKill] = Mathf.Log10(Time.time + 1f);
            }

            if (victim && rowId.TryGetValue(victim.id, out i))
                table.Rows[i][deaths] = (int)table.Rows[i][deaths] + 1;

            table.AcceptChanges();
            OnTableChanged?.Invoke();
        }
    }
}
