using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace MPCore
{
    public class GUIViewModel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _shortMessageArea;
        [SerializeField] private TextMeshProUGUI _bigMessageArea;

        private GUIModel _guiModel;

        private void Awake()
        {
            _guiModel = Models.GetModel<GUIModel>();
            _guiModel.ShortMessage.AddListener(SetShortMessage);
        }

        private void Start()
        {
            
        }

        private void SetShortMessage(string message)
        {
            _shortMessageArea.gameObject.SetActive(false);
            _shortMessageArea.SetText(message);
            _shortMessageArea.gameObject.SetActive(true);
        }
    }
}
