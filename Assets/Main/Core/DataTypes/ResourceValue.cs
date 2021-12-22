using System;
using UnityEngine;

namespace MPCore
{
	/// <summary> a resource type and value of that type </summary>
	[Serializable]
	public class ResourceValue : DataValue<int>
	{
		[SerializeField] int _maxValue = 100;

		public int MaxValue
		{
			get => _maxValue;
			set
			{
				_maxValue = value;

				if (value > _maxValue)
					Value = MaxValue;
			}
		}

		public override int Value 
		{ 
			get => base.Value;
			set
			{
				value = Mathf.Min(value, _maxValue);
				base.Value = value;
			}
		}

		public ResourceValue()
		{

		}

		public ResourceValue(int value = default)
		{
			MaxValue = value;
			Value = value;
		}
	}
}
