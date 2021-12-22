using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MPGUI
{
	/// <summary>
	/// Assign events to a button click, or just a button animator for other IGUIClickables
	/// </summary>
	public class Button : MonoBehaviour, IClickable
	{
		[SerializeField] private Sprite pressImage = null;
		[SerializeField] private bool directClicksOnly = false;
		[SerializeField] private bool locked = false;
		public UnityEvent clickEvents = null;

		private Sprite upSprite;
		private bool pressed = false;

		private void Awake()
		{
			if(TryGetComponent(out Image image))
				upSprite = image.sprite;
		}

		public void OnMouseClick(MouseInfo mouse)
		{
			if (!locked && pressed)
			{
				if (upSprite && TryGetComponent(out Image image))
					image.sprite = upSprite;

				if (!directClicksOnly || gameObject == mouse.upInfo.gameObject)
					clickEvents?.Invoke();
			}
		}

		public void OnMouseHold(MouseInfo mouse)
		{
		}

		public void OnMouseHover(MouseInfo mouse)
		{
		}

		public void OnMousePress(MouseInfo mouse)
		{
			if (!locked)
			{
				pressed = true;

				if(TryGetComponent(out Image image))
					image.sprite = pressImage;
			}
		}

		public void OnMouseRelease(MouseInfo mouse)
		{
			if (!locked)
			{
				pressed = false; 

				if (TryGetComponent(out Image image))
					image.sprite = upSprite;
			}
		}

		public void LockButton(bool isLocked)
		{
			if (TryGetComponent(out Image image))
					image.sprite = isLocked ? pressImage : upSprite;

			locked = isLocked;
		}
	}
}
