using MPCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MPGUI
{
	public class BoolField : MonoBehaviour, IClickable
	{
		public DataValue<bool> value = new(); 
		[SerializeField] Image check;
		[SerializeField] TextMeshProUGUI description;

		void Awake()
		{
			value.Subscribe(b => check.enabled = b.newValue);
		}

		public void Initialize(bool value, string description)
		{
			this.description.SetText(description);
			this.value.Value = value;
		}

		public void OnMouseClick(MouseInfo mouse) 
		{
			value.Value = !value.Value;
		}

		public void OnMouseHold(MouseInfo mouse) { }

		public void OnMouseHover(MouseInfo mouse) { }

		public void OnMousePress(MouseInfo mouse) { }

		public void OnMouseRelease(MouseInfo mouse) { }
	}
}
