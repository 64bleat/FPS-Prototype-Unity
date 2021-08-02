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
    public ValueEvent<float> elapsedTime = new ValueEvent<float>();
    public ValueEvent<TimeRecord> bestTime = new ValueEvent<TimeRecord>();
    public ValueEvent<Character> currentPlayer = new ValueEvent<Character>();
    public ValueEvent<bool> isReset = new ValueEvent<bool>();
    public ValueEvent<State> gameState = new ValueEvent<State>();
    
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


