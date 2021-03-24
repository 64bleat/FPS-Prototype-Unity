using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MPGUI
{
    /// <summary>
    /// ROOT COMPONENT:
    ///     This controls all input related to GUI.
    /// </summary>
    public class GUIInputManager : MonoBehaviour
    {
        private static readonly KeyCode[] MOUSE_KEYS = {
            KeyCode.Mouse0, KeyCode.Mouse1, KeyCode.Mouse2,
            KeyCode.Mouse3, KeyCode.Mouse4, KeyCode.Mouse5};
        private static readonly KeyCode[] MODIFIERS = { 
            KeyCode.LeftShift, KeyCode.RightShift,
            KeyCode.LeftAlt, KeyCode.RightAlt,
            KeyCode.LeftControl, KeyCode.RightControl };
        internal static readonly Dictionary<KeyCode, uint> bitmask = new Dictionary<KeyCode, uint>() {
            {KeyCode.Mouse0, 1 << 0 },
            {KeyCode.Mouse1, 1 << 1 },
            {KeyCode.Mouse2, 1 << 2 },
            {KeyCode.Mouse3, 1 << 3 },
            {KeyCode.Mouse4, 1 << 4 },
            {KeyCode.Mouse5, 1 << 5 },
            {KeyCode.LeftShift, 1 << 6 },
            {KeyCode.RightShift, 1 << 6},
            {KeyCode.LeftAlt, 1 << 7 },
            {KeyCode.RightAlt, 1 << 7 },
            {KeyCode.LeftControl, 1 << 8 },
            {KeyCode.RightControl, 1 << 8 }};

        private static PointerEventData ped;
        private static readonly List<RaycastResult> mouseCastList = new List<RaycastResult>();
        private static readonly HashSet<IGUISelectable> selectionAddHash = new HashSet<IGUISelectable>();
        private static readonly List<IGUISelectable> selectionAddList = new List<IGUISelectable>();

        private readonly List<IGUISelectable> selectionCurrentList = new List<IGUISelectable>();
        private readonly HashSet<IGUISelectable> selectionCurrentHash = new HashSet<IGUISelectable>();
        private readonly MouseInfo mDown = new MouseInfo();
        private static readonly MouseInfo mRaw = new MouseInfo();

        private void OnEnable()
        {
            ped = new PointerEventData(EventSystem.current);
        }

        private void LateUpdate()
        {
            GetMouseInfo(mRaw);

            uint downKeyMask = 0;
            uint holdKeyMask = 0;

            // Get mouse buttons down
            for (int i = 0; i < MOUSE_KEYS.Length; i++)
                if(bitmask.TryGetValue(MOUSE_KEYS[i], out uint bit)
                    && Input.GetKeyDown(MOUSE_KEYS[i]))
                    downKeyMask |= bit;

            // Get mouse buttons held
            for (int i = 0; i < MOUSE_KEYS.Length; i++)
                if (bitmask.TryGetValue(MOUSE_KEYS[i], out uint bit)
                    && Input.GetKey(MOUSE_KEYS[i]))
                    holdKeyMask |= bit;

            if (!mDown.downInfo.isValid && downKeyMask != 0) // MOUSE DOWN
            {
                GetSelectables(mRaw.downInfo.gameObject, selectionAddList);
                Select(selectionAddList);

                // Get modifiers held
                for (int i = 0; i < MODIFIERS.Length; i++)
                    if (bitmask.TryGetValue(MODIFIERS[i], out uint bit)
                        && Input.GetKey(MODIFIERS[i]))
                        downKeyMask |= bit;

                //mDown = current;
                mDown.keyMask = downKeyMask;
                mDown.downTime = Time.time;
                mDown.downInfo = mRaw.downInfo;
                mDown.holdInfo = default;
                mDown.upInfo = default;
                mDown.Invoke(PressType.Down);
            }
            else if (mDown.downInfo.isValid && holdKeyMask != 0) // Mouse HOLD
            {
                mDown.holdInfo = mRaw.downInfo;
                mDown.Invoke(PressType.Hold);
            }
            else if (mDown.downInfo.isValid && holdKeyMask == 0) // Mouse Up
            {
                mDown.upInfo = mRaw.downInfo;

                if (mDown.downInfo.gameObject && mDown.upInfo.gameObject && mDown.downInfo.gameObject.Equals(mDown.upInfo.gameObject))
                    mDown?.Invoke(PressType.Click);
                else
                    mDown?.Invoke(PressType.Up);

                mDown.keyMask = 0u;
                mDown.downTime = 0;
                mDown.downInfo = default;
                mDown.holdInfo = default;
                mDown.upInfo = default;

            }
            else if (mRaw.downInfo.isValid) // Mouse Hover
            {
                mRaw.Invoke(PressType.Hover);
            }
        }

        private static void GetSelectables(GameObject g, List<IGUISelectable> list)
        {
            Transform t = g ? g.transform : null;

            list.Clear();

            while (t)
            {
                if (t.TryGetComponent(typeof(IGUISelectable), out Component c))
                    list.Add(c as IGUISelectable);

                t = t.parent;
            }

            list.Reverse();
        }

        public void Select(List<IGUISelectable> addSelection)
        {
            selectionAddHash.Clear();
            foreach (IGUISelectable s in addSelection)
                selectionAddHash.Add(s);

            selectionCurrentList.Reverse();

            foreach (IGUISelectable s in selectionCurrentList)
                if (!selectionAddHash.Contains(s))
                    s.OnDeselect();

            foreach (IGUISelectable s in addSelection)
                if (!selectionCurrentHash.Contains(s))
                    s.OnSelect();

            selectionCurrentList.Clear();
            selectionCurrentHash.Clear();

            foreach (IGUISelectable s in addSelection)
            {
                selectionCurrentList.Add(s);
                selectionCurrentHash.Add(s);
            }
        }

        public void Deselect(IGUISelectable select)
        {
            if (selectionCurrentHash.Contains(select))
            {
                select.OnDeselect();
                selectionCurrentHash.Remove(select);
                selectionCurrentList.Remove(select);
            }
        }

        private static void GetMouseInfo(MouseInfo info)
        {
            int topDepth = int.MinValue;
            RaycastResult topObject = default;

            ped.position = Input.mousePosition;

            EventSystem.current.RaycastAll(ped, mouseCastList);

            for(int i = 0, ie = mouseCastList.Count; i < ie; i++)
                if(mouseCastList[i].depth > topDepth)
                {
                    topDepth = mouseCastList[i].depth;
                    topObject = mouseCastList[i];
                }

            mouseCastList.Clear();

            info.keyMask = 0;
            info.downInfo = topObject;
            info.holdInfo = default;
            info.upInfo = default;
            info.downTime = Time.time;
        }
    }

    public enum PressType { Hover, Down, Hold, Up, Click }
    public class MouseInfo
    {
        public uint keyMask;
        public RaycastResult downInfo;
        public RaycastResult holdInfo;
        public RaycastResult upInfo;
        public float downTime;

        public bool Contains(KeyCode press)
        {
            if (GUIInputManager.bitmask.TryGetValue(press, out uint bit))
                return (keyMask & bit) != 0;
            else 
                return false;
        }

        public void Invoke(PressType type)
        {
            // WHY DOES THIS NOT WORK I DON'T WANNA ALLOCATE MEMORY
            //if (downInfo.gameObject)
            //{
            //    Transform t = downInfo.gameObject.transform;

            //    while (t)
            //    {
            //        if (t.TryGetComponent(typeof(IGUIClickable), out Component c))
            //        {
            //            IGUIClickable click = c as IGUIClickable;

            //            switch (type)
            //            {
            //                case PressType.Hover: click.OnMouseHover(this); break;
            //                case PressType.Down: click.OnMousePress(this); break;
            //                case PressType.Hold: click.OnMouseHold(this); break;
            //                case PressType.Up: click.OnMouseRelease(this); break;
            //                case PressType.Click: click.OnMouseClick(this); break;
            //            }
            //        }

            //        t = t.parent;
            //    }
            //}

            if (downInfo.gameObject)
                foreach (IClickable click in downInfo.gameObject.GetComponentsInParent<IClickable>())
                    switch (type)
                    {
                        case PressType.Hover: click.OnMouseHover(this); break;
                        case PressType.Down: click.OnMousePress(this); break;
                        case PressType.Hold: click.OnMouseHold(this); break;
                        case PressType.Up: click.OnMouseRelease(this); break;
                        case PressType.Click: click.OnMouseClick(this); break;
                    }
        }
    }
}