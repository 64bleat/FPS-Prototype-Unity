using UnityEngine;
using MPCore;

namespace MPGUI
{
    public class KeyMapSettingsGUI : MonoBehaviour
    {
        public GameObject entryTemplate;
        public GameObject floatButtonTemplate;
        public GameObject keyButtonTemplate;
        public GUIBoolButton boolButton;
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
                GUIBoolButton bb = button.GetComponent<GUIBoolButton>();
                bb.description.text = "Always Sprint";
                bb.SetValue(kbl.alwaysRun);
                bb.OnValueChange += b => kbl.alwaysRun = b;
            }



            buttonSet.AddTitle("Values");

            foreach(ScriptFloat val in values)
            {
                GameObject button = buttonSet.AddGameObject(floatButtonTemplate);
                GUIFloatButton b = button.GetComponentInChildren<GUIFloatButton>();
                b.description.text = val.name;
                b.SetValue(val.value);
                b.OnValueChange += v => val.value = v;
            }

            // Key Input Buttons
            foreach(KeyBindLayer l in keyOrder)
            {
                buttonSet.AddTitle(l.name);

                foreach(KeyBind k in l.binds)
                    buttonSet.AddGameObject(keyButtonTemplate)
                        .GetComponentInChildren<GUIKeyBindButton>()
                        .SetValue(k);
            }
        }
    }
}
