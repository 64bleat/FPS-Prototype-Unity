using MPCore;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace MPConsole
{
    [ContainsConsoleCommands]
    public class ConsoleInput : MonoBehaviour
    {
        [TextArea(2,8)]
        public string entryMessage;
        public string[] clickLayermask;
        public TextMeshProUGUI logText;
        public TextMeshProUGUI inputText;
        public Transform cursorPointer;

        private string command = "";
        private string commandHeader;
        private int cursorPosition = 0;
        private int commandCursor = 0;
        private int blankCount;
        private float cursorTime;
        private readonly float cursorShiftInterval = 0.125f;
        private int viewCursor = 0;
        private readonly int viewLines = 27;
        private string fullLogText = "";
        private InputManager input;

        private readonly List<string> previousCommands = new List<string>() { "" };
        private static readonly char[] cursorAnimation = new char[] { '/', '|', '\\', '|' };
        private readonly HashSet<char> blankChars = new HashSet<char>() { ' ' };

        [ConsoleCommand("clear", "Clears the console log.")]
        public void ClearConsole()
        {
            fullLogText = "";
            logText.SetText(fullLogText);
            Console.target = null;
        }

        [ConsoleCommand("log", "Log a message to the console.")]
        public string ConsoleLog(string message)
        {
            return message;
        }

        void Awake()
        {
            Console.AddInstance(this);

            input = GetComponentInParent<InputManager>();
            input.Bind("ConTarget", GetTargetAtMouse, this);
            input.Bind("ConUntarget", RemoveTarget, this);
            input.Bind("ConUp", GetPreviousCommand, this);
            input.Bind("ConDown", GetNextCommand, this);
            input.Bind("ConLeft", CursorLeft, this);
            input.Bind("ConRight", CursorRight, this);
        }

        private void Start()
        {
            fullLogText = entryMessage;
            logText.SetText(GetViewableLog(fullLogText));
        }

        private void OnDestroy()
        {
            Console.RemoveInstance(this);
        }

        private void CursorLeft()
        {
            cursorPosition = Mathf.Clamp(--cursorPosition, 0, command.Length);
        }

        private void CursorRight()
        {
            cursorPosition = Mathf.Clamp(++cursorPosition, 0, command.Length);
        }

        void GetTargetAtMouse()
        {
            Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance: 1000f, layerMask: ~LayerMask.GetMask(clickLayermask)))
                Console.target = hit.collider.gameObject;
        }

        void RemoveTarget()
        {
            Console.target = null;
        }

        void GetPreviousCommand()
        {
            if (previousCommands.Count != 0)
            {
                command = previousCommands[commandCursor = (++commandCursor) % previousCommands.Count];
                cursorPosition = command.Length;
            }
        }

        void GetNextCommand()
        {
            if (previousCommands.Count != 0)
            {
                if (--commandCursor < 0)
                    commandCursor = previousCommands.Count - 1;

                command = previousCommands[commandCursor %= previousCommands.Count];
                cursorPosition = command.Length;
            }
        }

        void Update()
        {
            //move cursor
            if (UnityEngine.Input.GetKey(KeyCode.LeftArrow))
            {
                if ((cursorTime += Time.deltaTime) > cursorShiftInterval)
                {
                    cursorPosition = Mathf.Max(0, cursorPosition - 1);
                    cursorTime = 0;
                }
            }
            else if (UnityEngine.Input.GetKey(KeyCode.RightArrow))
            {
                if ((cursorTime += Time.deltaTime) > cursorShiftInterval)
                {
                    cursorPosition = Mathf.Min(command.Length, cursorPosition + 1);
                    cursorTime = 0;
                }
            }
            else
                cursorTime = cursorShiftInterval;

            //scroll
            if(Input.GetKeyDown(KeyCode.PageUp))
            {
                viewCursor++;
                logText.SetText(GetViewableLog(fullLogText));
            }
            else if(Input.GetKeyDown(KeyCode.PageDown))
            {
                viewCursor = Mathf.Max(0, viewCursor - 1);
                logText.SetText(GetViewableLog(fullLogText));
            }

            //do typing
            foreach (char input in Input.inputString)
                switch(input)
                {
                    case '\n':
                    case '\r':
                        if (command.Length != 0)
                        {
                            string output = Console.Command(command)?.Trim();

                            if (output?.Length > 0)
                                logText.SetText(GetViewableLog(fullLogText += output + "\n")); // Command executed here!

                            previousCommands.Insert(1, command);
                            command = previousCommands[commandCursor = 0];
                            cursorPosition = command.Length;
                        }
                        break;
                    case '\b':
                        if(command.Length > 0)
                            command = command.Remove(--cursorPosition, 1);
                        break;
                    default:
                        command = command.Insert(cursorPosition++, input.ToString());
                        break;
                }

            //display entry text
            {
                string cur = cursorAnimation[(int)(Time.unscaledTime * cursorAnimation.Length) % cursorAnimation.Length].ToString();
                string text = command;
                commandHeader = (Console.target?.ToString() ?? "Console") + ": ";
                text = text.Insert(cursorPosition, cur);
                text = text.Insert(0, commandHeader);

                blankCount = 0;
                foreach (char c in text)
                    if (blankChars.Contains(c))
                        blankCount++;

                Cursorposition();

                inputText.SetText(text);
            }
        }

        private void Cursorposition()
        {
            Vector3[] verts = inputText.mesh.vertices;
            int index = Mathf.Min(verts.Length - 1, (commandHeader.Length + cursorPosition - blankCount - 1) * 4 + 2);

            if (index >= 0)
            {
                Vector3 lpos = verts[index];

                lpos.y = inputText.rectTransform.rect.height;
                cursorPointer.position = inputText.transform.TransformPoint(lpos);
            }
        }

        private string GetViewableLog(string text)
        {
            string log = "";
            string[] lines = text.Trim('\n').Split('\n');

            viewCursor = Mathf.Min(Mathf.Max(0, viewCursor), lines.Length);

            int end = lines.Length - viewCursor;

            for (int i = Mathf.Max(0, end - viewLines); i < end; i++)
                log += lines[i] + "\n";

            return log;
        }
    }
}
