using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;

namespace MPCore
{
    public class ScoreboardViewer : MonoBehaviour
    {
        public Scoreboard scoreboard;
        public TextMeshProUGUI entryTemplate;

        private static readonly SortedList<float, DataRow> sort = new SortedList<float, DataRow>();
        private InputManager input;

        private void Awake()
        {
            input = GetComponentInParent<InputManager>();
            input.Bind("Scoreboard", Disable, this, KeyPressType.Up);
            input.Bind("Scoreboard", Enable, this, KeyPressType.Down);
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            Refresh();

            scoreboard.OnTableChanged.AddListener(Refresh);
        }

        private void OnDisable()
        {
            scoreboard.OnTableChanged.RemoveListener(Refresh);
        }

        private void OnDestroy()
        {
            input.Unbind(this);
        }

        private void Enable()
        {
            gameObject.SetActive(true);
        }

        private void Disable()
        {
            gameObject.SetActive(false);
        }

        public void Refresh()
        {
            // Destroy Old
            int childCount = transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;

                if (child.activeSelf)
                    Destroy(child);
            }

            // Create New

            foreach (DataRow row in scoreboard.table.Rows)
            {
                float priority = -(int)row[scoreboard.kills] - 1f / (float)row[scoreboard.lastKill];

                while (sort.ContainsKey(priority))
                    priority += 0.0001f;

                sort.Add(priority, row);
            }

            foreach(DataRow row in sort.Values)
            {
                GameObject entry = Instantiate(entryTemplate.gameObject, transform);
                string name = (string)row[scoreboard.displayName];
                int kills = (int)row[scoreboard.kills];
                int deaths = (int)row[scoreboard.deaths];

                if (entry.TryGetComponent(out TextMeshProUGUI text))
                    text.SetText($"{name, 20} :{kills, 4}K :{deaths, 4}D");

                entry.SetActive(true);
            }

            sort.Clear();
        }
    }
}
