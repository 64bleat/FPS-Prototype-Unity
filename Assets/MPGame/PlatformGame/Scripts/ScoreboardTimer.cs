using MPCore;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ScoreboardTimer : MonoBehaviour
{
    private PlatformGameModel _gameModel;
    private TextMeshProUGUI _text;

    private void OnEnable()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _gameModel = Models.GetModel<PlatformGameModel>();

        _gameModel.elapsedTime.OnSet.AddListener(SetText);
    }

    private void OnDisable()
    {
        _gameModel.elapsedTime.OnSet.RemoveListener(SetText);
    }

    private void SetText(DeltaValue<float> seconds)
    {
        int sec = (int)seconds.newValue;
        float fSec = seconds.newValue % 1;
        string timeText = $"{sec / 60:D2}:{sec % 60:D2}:{(int)(fSec * 60):D2}";

        _text.SetText(timeText);
    }
}
