using UnityEngine.Events;

namespace MPGUI
{
    public class BoolButton : ValueButton
    {
        public bool value;
        public UnityEvent<bool> OnValueChange;

        private void OnValidate()
        {
            SetValueText(value.ToString());
        }

        public void SetValue(bool value)
        {
            SetValueText(value.ToString());

            if (this.value != value)
                OnValueChange?.Invoke(value);

            this.value = value;
        }

        public void ToggleValue()
        {
            SetValue(!value);
        }
    }
}
