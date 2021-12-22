using UnityEngine;
using UnityEngine.UI;

namespace MPCore
{
    /// <summary>
    /// Serializes/Deserializes a CanvasScaler
    /// </summary>
    [RequireComponent(typeof(CanvasScaler))]
    public class CanvasScalerSerializer : MonoBehaviour
    {
        const string KEY = "scaleFactor";

        SettingsModel _settingsModel;

        void Awake()
        {
            _settingsModel = Models.GetModel<SettingsModel>();
            _settingsModel.OnSerialize.AddListener(Serialize);
            _settingsModel.OnDeserialize.AddListener(Deserialize);
        }

        void Deserialize(SettingsData data)
        {
            CanvasScaler cs = GetComponent<CanvasScaler>();
            cs.scaleFactor = float.Parse(data.GetValue(KEY));
        }

        void Serialize(SettingsData data)
        {
            CanvasScaler cs = GetComponent<CanvasScaler>();
            data.SetValue(KEY, cs.scaleFactor.ToString());
        }
    }
}
