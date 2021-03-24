using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AI Interest Points to be evaulated in CharacterAI components
/// </summary>
public static class AIBlackboard
{
    public static readonly List<Component> visualTargets = new List<Component>();
    public static readonly List<Component> mentalTargets = new List<Component>();
}
