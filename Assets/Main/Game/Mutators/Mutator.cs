using UnityEngine;

namespace MPGame
{
	/// <summary>
	/// Mutators change game behaviour and can be selected from the Play window
	/// </summary>
	public abstract class Mutator : ScriptableObject
	{
		public string displayName;
		public string description;

		public virtual void Activate() { }
		public virtual void Deactivate() { }
	}
}
