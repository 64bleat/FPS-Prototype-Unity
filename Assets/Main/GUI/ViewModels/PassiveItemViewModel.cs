using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MPCore
{
    public class PassiveItemViewModel : MonoBehaviour
    {
        [SerializeField] Image _background;
        [SerializeField] Image _selector;
        [SerializeField] TMP_Text _text;
        [SerializeField] Color _bgActive;
        [SerializeField] Color _bgInactive;
        [SerializeField] Color _txtActive;
        [SerializeField] Color _txtInactive;

        public void SetItem(Inventory item)
        {
            _text.SetText(item.displayName);
            Deselect();

            if (item.active)
                SetActive();
            else
                SetInactive();
        }

        public void SetActive()
        {
            _text.color = _txtActive;
            _background.color = _bgActive;
        }

        public void SetInactive()
        {
            _text.color = _txtInactive;
            _background.color = _bgInactive;
        }

        public void Select()
        {
            _selector.gameObject.SetActive(true);
        }

        public void Deselect()
        {
            _selector.gameObject.SetActive(false);
        }
    }
}
