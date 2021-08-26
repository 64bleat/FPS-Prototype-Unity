using MPCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class PlatformGameModel : Models
{
    public enum State { Playing, Stopped, Reset }
    public DataValue<float> elapsedTime = new DataValue<float>();
    public DataValue<TimeRecord> bestTime = new DataValue<TimeRecord>();
    public DataValue<Character> currentPlayer = new DataValue<Character>();
    public DataValue<bool> isReset = new DataValue<bool>();
    public DataValue<State> gameState = new DataValue<State>();
    
    public UnityEvent<Character> OnStart = new UnityEvent<Character>();
    public UnityEvent OnEnd = new UnityEvent();
    public UnityEvent OnReset = new UnityEvent();
    public UnityEvent<Platform, Character> OnPlatformTouch = new UnityEvent<Platform, Character>();

    [Serializable]
    public struct TimeRecord
    {
        public float time;
        public CharacterInfo holder;
    }
}


