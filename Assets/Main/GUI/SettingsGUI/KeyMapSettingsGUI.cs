using UnityEngine;
using MPCore;
using TMPro;

namespace MPGUI
{
    public class KeyMapSettingsGUI : MonoBehaviour
    {
        public GameObject entryTemplate;
        public GameObject floatButtonTemplate;
        public GameObject keyButtonTemplate;
        public BoolButton boolButton;
        public KeyBindList kbl;
        public ScriptFloat[] values;
        public KeyBindLayer[] keyOrder;

        private GUIButtonSet buttonSet;

        private void Awake()
        {
            buttonSet = GetComponentInChildren<GUIButtonSet>();
        }

        private void OnEnable()
        {
            LoadBindButtons();
        }

        private void OnDisable()
        {
            buttonSet.Clear();
        }

        private void LoadBindButtons()
        {
            buttonSet.AddTitle("Settings");

            {
                GameObject button = buttonSet.AddGameObject(boolButton.gameObject);
                BoolButton bb = button.GetComponent<BoolButton>();

                bb.SetLabel("Always Run");
                bb.SetValue(kbl.alwaysRun);
                bb.OnValueChange.AddListener( b => kbl.alwaysRun = b);
            }

            buttonSet.AddTitle("Values");

            foreach(ScriptFloat val in values)
            {
                GameObject button = buttonSet.AddGameObject(floatButtonTemplate);
                FloatButton b = button.GetComponentInChildren<FloatButton>();
                //b.description.text = val.name;
                b.SetValue(val.value);
                b.OnValueChange.AddListener(v => val.value = v);
            }

            // Key Input Buttons
            foreach(KeyBindLayer l in keyOrder)
            {
                buttonSet.AddTitle(l.name);

                foreach(KeyBind k in l.binds)
                    buttonSet.AddGameObject(keyButtonTemplate)
                        .GetComponentInChildren<KeyBindButton>()
                        .SetValue(k);
            }
        }
    }
}
