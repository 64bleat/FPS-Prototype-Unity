using MPCore;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace MPConsole
{
    [ContainsConsoleCommands]
    public class ConsoleManager : MonoBehaviour
    {
        const int VIEW_LINES = 27;
        static readonly char[] _cursorAnim = new char[] { '/', '|', '\\', '|' };
        static readonly HashSet<char> _whitespace = new HashSet<char>() { ' ' };

        [TextArea(2,8)]
        [FormerlySerializedAs("entryMessage")]
        [SerializeField]string _startText;
        [SerializeField] TMP_Text _txtLog;
        [SerializeField] TMP_Text _txtInput;

        private string command = "";
        private string _commandHeader;
        private int cursorPosition = 0;
        private int commandCursor = 0;
        private float cursorTime;
        private readonly float cursorShiftInterval = 0.125f;
        private int viewCursor = 0;
        private string _logText = "";
        InputManager _input;

        static readonly string[] _layerMaskNames = new string[]
            {"Player", "UI", "IgnoreRaycast", "FirstPerson"};
        static int _layerMask;

        readonly List<string> _history = new List<string>() { "" };


        void Awake()
        {
            _layerMask = ~LayerMask.GetMask(_layerMaskNames);
            Console.AddInstance(this);

            _input = GetComponentInParent<InputManager>();
            _input.Bind("ConTarget", GetTargetAtMouse, this);
            _input.Bind("ConUntarget", RemoveTarget, this);
            _input.Bind("ConUp", GetPreviousCommand, this);
            _input.Bind("ConDown", GetNextCommand, this);
            _input.Bind("ConLeft", CursorLeft, this);
            _input.Bind("ConRight", CursorRight, this);
        }

        void Start()
        {
            _logText = _startText;
            _txtLog.SetText(_logText);
        }

        void OnDestroy()
        {
            Console.RemoveInstance(this);
        }

        void CursorLeft()
        {
            cursorPosition = Mathf.Clamp(--cursorPosition, 0, command.Length);
        }

        void CursorRight()
        {
            cursorPosition = Mathf.Clamp(++cursorPosition, 0, command.Length);
        }

        void GetTargetAtMouse()
        {
            Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, _layerMask))
                Console.target = hit.collider.gameObject;
        }

        void RemoveTarget()
        {
            Console.target = null;
        }

        void GetPreviousCommand()
        {
            if (_history.Count != 0)
            {
                command = _history[commandCursor = (++commandCursor) % _history.Count];
                cursorPosition = command.Length;
            }
        }

        void GetNextCommand()
        {
            if (_history.Count != 0)
            {
                if (--commandCursor < 0)
                    commandCursor = _history.Count - 1;

                command = _history[commandCursor %= _history.Count];
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
                _txtLog.SetText(_logText);
            }
            else if(Input.GetKeyDown(KeyCode.PageDown))
            {
                viewCursor = Mathf.Max(0, viewCursor - 1);
                _txtLog.SetText(_logText);
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
                                _txtLog.SetText(_logText += output + "\n"); // Command executed here!

                            _history.Insert(1, command);
                            command = _history[commandCursor = 0];
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
                string cursor = _cursorAnim[(int)(Time.unscaledTime * _cursorAnim.Length) % _cursorAnim.Length].ToString();
                string targetName = Console.target is Object to ? $"{to.name} " : string.Empty;
                string targetType = Console.target != null ? Console.target.GetType().Name : "Console";
                string cmdText = command.Insert(cursorPosition, cursor);
                string input = $"{targetName}{targetType}: {cmdText}";

                _txtInput.SetText(input);
            }
        }

        [ConsoleCommand("clear", "Clears the console log.")]
        public void ClearConsole()
        {
            _logText = string.Empty;
            _txtLog.SetText(_logText);
            Console.target = null;
        }

        [ConsoleCommand("log", "Log a message to the console.")]
        public string ConsoleLog(string message)
        {
            return message;
        }
    }
}
