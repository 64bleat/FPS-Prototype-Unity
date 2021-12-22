namespace MPGUI
{
	public interface IClickable
	{
		/// <summary> Called when cursor is over the object. </summary>
		void OnMouseHover(MouseInfo mouse);

		/// <summary> Called when curor presses down over the object </summary>
		void OnMousePress(MouseInfo mouse);

		/// <summary> Called in the original object when a press is held. 
		/// It is called on the original object, but passes the current object as part of MouseEvent.</summary>
		void OnMouseHold(MouseInfo mouse);

		/// <summary> Called when a press is released on the original object </summary>
		void OnMouseClick(MouseInfo mouse);

		/// <summary> Called when a press is released while over a different object.
		/// It is called on the original object, but passes the current object as part of MouseEvent</summary>
		void OnMouseRelease(MouseInfo mouse);
	}
}
