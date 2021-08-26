using UnityEngine;
using MPCore;
using TMPro;

namespace MPGUI
{
    /// <summary>
    /// Populates the KeyModel settings panel.
    /// </summary>
    public class KeyModelSettingsManager : MonoBehaviour
    {
        const string VALUE = "Value";
        const string F2 = "F2";

        [SerializeField] RectTransform _panel;
        [SerializeField] FloatField _floatField;
        [SerializeField] KeyBindField _keyField;
        [SerializeField] BoolField _boolField;
        [SerializeField] GameObject _divider;

        void Awake()
        { 
            KeyModel _keyModel = Models.GetModel<KeyModel>();

            // Bools
            BoolField bf = Instantiate(_boolField, _panel);
            bf.SetReference(_keyModel.alwaysRun, VALUE, "Always Run");

            // Floats
            FloatField ff = Instantiate(_floatField, _panel);
            ff.SetRange(0, 10);
            ff.SetFormat(F2);
            ff.SetReference(_keyModel.mouseSensitivity, VALUE);
            ff = Instantiate(ff, _panel);
            ff.SetReference(_keyModel.sprintToggleTime, VALUE);
            ff = Instantiate(ff, _panel);
            ff.SetReference(_keyModel.walkToggleTime, VALUE);
            ff = Instantiate(ff, _panel);
            ff.SetReference(_keyModel.crouchToggleTime, VALUE);

            // Keys
            KeyBindField kbf;
            foreach(KeyBindLayer l in _keyModel.keyOrder)
            {
                GameObject d = Instantiate(_divider, _panel);

                if (d.TryGetComponentInChildren(out TextMeshProUGUI div))
                    div.SetText(l.name);

                foreach (KeyBind k in l.binds)
                {
                    kbf = Instantiate(_keyField, _panel);
                    kbf.SetKey(k);
                }
            }
        }
    }
}
