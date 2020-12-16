/******************************************************************************      
 * author: David Martinez Copyright 2020 all rights reserved
 * description: A simpler alternative to Input with some new features. 
 *      Able to bind listeners to keys to reduce input update polling. 
 *      Inputs can be ignored through masking.
 * procedure: 
 *  1.  Things that require input will grab the closest parent InputManager.
 *  2.  Bind Actions to keys so they are called when they are pressed.
 *      Actions can be bound to key press events Down, Up, and Hold.
 *      set owner to a GameObject that will be null when the action
 *      is no longer valid. Events will not call when GameObject owner is inactive.
 *  notes:
 *  -   Listeners with a null owner are automatically removed when
 *      found in LateUpdate.
 *  -   Listeners are only invoked when their listeners are active.
 *  -   If an lambda expression is used for an Action, the only way
 *      to unbind it is to destroy the owner.
 *****************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MPCore
{
    public enum KeyPressType { Down, Up, Held }

    public class InputManager : MonoBehaviour
    {
        public bool isPlayer = false;
        public KeyBindList loadKeyBindList = null;
        public string excludeLayer = "";

        private readonly Dictionary<string, KeyBind> keyBinds = new Dictionary<string, KeyBind>();
        private readonly Dictionary<string, List<KeyListener>> keyListeners = new Dictionary<string, List<KeyListener>>();
        private readonly List<string> listenerKeys = new List<string>();
        private readonly Dictionary<string, BotPress> botPresses = new Dictionary<string, BotPress>();
        private readonly HashSet<BotMouse> botMouse = new HashSet<BotMouse>();
        private static readonly HashSet<string> keyMasks = new HashSet<string>();
        private Vector2 botMouseDelta;
        private Vector2 mousePositionDelta;
        private Vector3 lastMousePosition;
        private CanvasScaler canvasScale;
        // LateUpdate no allocation
        private static readonly List<Action> actionDownSet = new List<Action>();
        private static readonly List<Action> actionHoldSet = new List<Action>();
        private static readonly List<Action> actionUpSet = new List<Action>();
        private static readonly List<BotPress> botPressEnd = new List<BotPress>();
        private static readonly List<BotPress> botPressStart = new List<BotPress>();
        private static readonly List<KeyListener> removeKL = new List<KeyListener>();
        private static readonly List<BotMouse> removeBP = new List<BotMouse>();

        private class KeyListener
        {
            internal Action action;
            internal Component owner;
            internal KeyPressType pressType;
        }

        private class BotPress
        {
            /// <summary> What key press is being simulated </summary>
            internal string key;
            /// <summary> When the press is lifted </summary>
            internal float unpressTime;
            /// <summary> Prevents bouncing on <c>GetKeyDown</c></summary>
            internal bool started = false;
        }

        private class BotMouse
        {
            internal Func<float, Vector2> path;
            internal float unpressTime;
        }

        // UNITY
        private void Awake()
        {
            canvasScale = GetComponentInChildren<CanvasScaler>();
            lastMousePosition = Input.mousePosition;
            LoadKeyBindList(loadKeyBindList);
        }

        private void OnDestroy()
        {
            keyMasks.Clear();
        }

        private void LateUpdate()
        {
            foreach (BotPress bp in botPresses.Values)
            {
                if (!bp.started)
                    botPressStart.Add(bp);

                if (bp.unpressTime < Time.time)
                    botPressEnd.Add(bp);
            }

            //foreach (KeyValuePair<string, HashSet<KeyListener>> kvp in keyListeners)
            foreach(string key in listenerKeys)
                if(keyListeners.TryGetValue(key, out List<KeyListener> value))
            {
                bool isDown = GetKeyDown(key);
                bool isUp = GetKeyUp(key);
                bool isHeld = GetKey(key);

                if (isDown || isUp || isHeld)
                { 
                    foreach (KeyListener listener in value)
                        if (!listener.owner)
                            removeKL.Add(listener);
                        else if (listener.owner.gameObject.activeInHierarchy)
                            if (listener.pressType == KeyPressType.Down && isDown)
                                actionDownSet.Add(listener.action);
                            else if (listener.pressType == KeyPressType.Held && isHeld)
                                actionHoldSet.Add(listener.action);
                            else if (listener.pressType == KeyPressType.Up && isUp)
                                actionUpSet.Add(listener.action);

                    foreach (KeyListener listener in removeKL)
                        value.Remove(listener);

                    removeKL.Clear();
                }
            }

            if (isPlayer)
            {
                mousePositionDelta = Input.mousePosition - lastMousePosition;

                if (canvasScale)
                    mousePositionDelta /= canvasScale.scaleFactor;

                lastMousePosition = Input.mousePosition;
            }

            foreach (Action action in actionDownSet)
                action?.Invoke();
            foreach (Action action in actionHoldSet)
                action?.Invoke();
            foreach (Action action in actionUpSet)
                action?.Invoke();
            foreach (BotPress press in botPressStart)
                press.started = true;
            foreach (BotPress press in botPressEnd)
                botPresses.Remove(press.key);

            // Mouse movement
            botMouseDelta.x = botMouseDelta.y = 0f;

            foreach (BotMouse bot in botMouse)
            {
                botMouseDelta += bot?.path?.Invoke(bot.unpressTime - Time.time) ?? Vector3.zero;

                if (Time.time > bot.unpressTime)
                    removeBP.Add(bot);
            }

            foreach (BotMouse bot in removeBP)
                botMouse.Remove(bot);

            removeBP.Clear();

            actionDownSet.Clear();
            actionHoldSet.Clear();
            actionUpSet.Clear();
            botPressEnd.Clear();
            botPressStart.Clear();
        }

        // Load Keys
        private void LoadKeyBindList(KeyBindList kbl)
        {
            keyBinds.Clear();

            foreach (KeyBind kb in kbl.keyBinds)
                if (excludeLayer.Length == 0 || !kb.layer.name.Contains(excludeLayer))
                    keyBinds.Add(kb.name, kb);

            loadKeyBindList = kbl;
        }

        // ROBOT
        public void Press(string key, float seconds = 0f)
        {
            if (botPresses.TryGetValue(key, out BotPress press))
            {
                press.unpressTime = Time.time + seconds;
                press.started = false;
            }
            else
                botPresses.Add(key, new BotPress
                {
                    key = key,
                    unpressTime = Time.time + seconds
                });
        }

        public void Release(string key)
        {
            if (botPresses.TryGetValue(key, out BotPress press))
                press.unpressTime = -1f;
        }

        public void Move(Func<float, Vector2> mouseDelta, float seconds = 0f)
        {
            botMouse.Add(new BotMouse(){
                path = mouseDelta,
                unpressTime = Time.time + seconds});
        }

        public void Stop()
        {
            botMouse.Clear();
        }

        // GETTING
        public bool GetKey(string keyName)
        {
            if (keyMasks.Contains(keyName))
                return false;

            if (keyBinds.TryGetValue(keyName, out KeyBind bn))
            {
                if (botPresses.TryGetValue(keyName, out BotPress press))
                        return true;

                foreach (KeyCode kc in bn.keyCombo)
                    if (isPlayer && Input.GetKey(kc))
                        return true;
            }

            return false;
        }

        public bool GetKeyDown(string keyName)
        {
            if (keyMasks.Contains(keyName))
                return false;

            if (keyBinds.TryGetValue(keyName, out KeyBind bn))
            {
                if (botPresses.TryGetValue(keyName, out BotPress press))
                    if (!press.started)
                        return true;

                foreach (KeyCode k in bn.keyCombo)
                    if (isPlayer && Input.GetKeyDown(k))
                        return true;
            }

            return false;
        }

        public bool GetKeyUp(string keyName)
        {
            if (keyMasks.Contains(keyName))
                return false;

            if (keyBinds.TryGetValue(keyName, out KeyBind bn))
            {
                if (botPresses.TryGetValue(keyName, out BotPress press))
                    if (Time.time > press.unpressTime)
                        return true;

                foreach (KeyCode k in bn.keyCombo)
                    if (isPlayer && Input.GetKeyUp(k))
                        return true;
            }

            return false;
        }

        // MASKING
        public void Mask(string layer = "Ingame")
        {
            keyMasks.UnionWith(new HashSet<string>(
                from kvp in keyBinds
                where kvp.Value.layer.name.Contains(layer)
                select kvp.Key)
                {
                    layer
                });
        }

        public void Unmask(string layer = "Ingame")
        {
            keyMasks.ExceptWith(new HashSet<string>(
                from kvp in keyBinds
                where kvp.Value.layer.name.Contains(layer)
                select kvp.Key)
                {
                    layer
                });
        }

        // BINDING
        public void Bind(string key, Action action, Component owner, KeyPressType pressType = KeyPressType.Down)
        {
            KeyListener newKeyListener = new KeyListener
            {
                action = action,
                owner = owner,
                pressType = pressType
            };

            if (!keyListeners.TryGetValue(key, out List<KeyListener> actionList))
            {
                keyListeners.Add(key, new List<KeyListener> { newKeyListener });
                listenerKeys.Add(key);
            }
            else
                actionList.Add(newKeyListener);
        }

        public void Unbind(string key, Action action)
        {
            if (keyListeners.TryGetValue(key, out List<KeyListener> listeners))
                foreach (KeyListener listener in listeners.ToArray())
                    if (listener.action.Equals(action))
                        listeners.Remove(listener);
        }

        public void Unbind(Component owner)
        {
            foreach (List<KeyListener> listeners in keyListeners.Values.ToArray())
                foreach (KeyListener listener in listeners.ToArray())
                    if (listener.owner.Equals(owner))
                        listeners.Remove(listener);
        }

        // MOUSE
        public float MouseX => GetMouseAxis("Mouse X");
        public float MouseY => GetMouseAxis("Mouse Y");
        public Vector2 MousePositionDelta => mousePositionDelta;

        private float GetMouseAxis(string axis)
        {
            float robotAxis = axis.Equals("Mouse X") ? botMouseDelta.x : botMouseDelta.y;

            return robotAxis + (!keyMasks.Contains("Ingame") ? (isPlayer ? Input.GetAxis(axis) : 0f) * loadKeyBindList.sensitivity.value : 0);
        }
    }
}
