using System.Collections.Generic;

namespace MPCore
{
	public class KeyModel : Models
	{
		public List<KeyBind> keys;
		public List<KeyBindLayer> keyOrder;
		public DataValue<float> mouseSensitivity = new DataValue<float>();
		public DataValue<float> sprintToggleTime = new DataValue<float>();
		public DataValue<float> walkToggleTime = new DataValue<float>();
		public DataValue<float> crouchToggleTime = new DataValue<float>();
		public DataValue<bool> alwaysRun = new DataValue<bool>();
	}
}
