using UnityEngine;
using MPCore;
using TMPro;

namespace MPGUI
{
    public class KeyMapSettingsGUI : MonoBehaviour
    {
        [SerializeField] private RectTransform parent;
        [SerializeField] private FloatField floatField;
        [SerializeField] private KeyBindField keyField;
        [SerializeField] private BoolField boolField;
        [SerializeField] private GameObject divider;
        public KeyBindList kbl;
        public ScriptFloat[] values;
        public KeyBindLayer[] keyOrder;

        private void Awake()
        {
            LoadBindButtons();
        }

        private void LoadBindButtons()
        {
            GameObject go;

            // Bools
            go = Instantiate(boolField.gameObject, parent);

            if (go.TryGetComponent(out BoolField bf))
                bf.SetReference(kbl, "alwaysRun", "Always Run");

            // Floats
            foreach(ScriptFloat val in values)
            {
                go = Instantiate(floatField.gameObject, parent);

                if (go.TryGetComponent(out FloatField ff))
                {
                    ff.SetReference(val, "value", val.name, "F2");
                    ff.SetRange(0f, 20f);
                }
            }

            // Keys
            foreach(KeyBindLayer l in keyOrder)
            {
                go = Instantiate(divider.gameObject, parent);

                if (go.TryGetComponentInChildren(out TextMeshProUGUI div))
                    div.SetText(l.name);

                foreach (KeyBind k in l.binds)
                {
                    go = Instantiate(keyField.gameObject, parent);

                    if (go.TryGetComponent(out KeyBindField kbf))
                        kbf.SetKey(k);
                }
            }
        }
    }
}
