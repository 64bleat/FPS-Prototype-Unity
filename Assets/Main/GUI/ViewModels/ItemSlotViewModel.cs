using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MPCore
{
    public class ItemSlotViewModel : MonoBehaviour
    {
        [SerializeField] Image _background;
        [SerializeField] TMP_Text _text;
        [SerializeField] Color _bgActive;
        [SerializeField] Color _bgInactive;
        [SerializeField] Color _bgEmpty;
        [SerializeField] Color _txtActive;
        [SerializeField] Color _txtInactive;
        [SerializeField] Color _txtEmpty;

        private void Awake()
        {
            SetWeapon(null);
        }

        public void SetWeapon(Weapon weapon)
        {
            if(weapon)
            {
                _text.SetText($"{weapon.weaponSlot}: {weapon.shortName}");
                SetInactive();
            }
            else
            {
                _text.SetText(string.Empty);
                SetEmpty();
            }
        }

        public void SetItem(Inventory item)
        {
            _text.SetText(item.displayName);
            SetInactive();
        }

        public void SetInactive()
        {
            _text.color = _txtInactive;
            _background.color = _bgInactive;
        }

        public void SetActive()
        {
            _text.color = _txtActive;
            _background.color = _bgActive;
        }

        public void SetEmpty()
        {
            _text.color = _txtEmpty;
            _background.color = _bgEmpty;
        }
    }
}
