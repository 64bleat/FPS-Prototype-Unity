using System;

namespace MPCore
{
	public class TimeModel : Models
	{
		public DataValue<DateTime> currentTime = new();
		public DataValue<DateTime> currentUtcTime = new();
		public DataValue<float> timeScale = new();
		public DataValue<float> dayScale = new();
	}
}
