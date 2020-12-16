using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MPConsole
{
    [ContainsConsoleCommands]
    public static class Console
    {
        #region TODO: MOVE TIME REGION TO NEW CLASS
        public const int MAX_GAMETIME = 50;
        private static float gameTime = 1;
        private static bool paused = false;

        public static float GameTime
        {
            get => gameTime;
            set
            {
                gameTime = Mathf.Clamp(value, 0, MAX_GAMETIME);
                UpdateTimeScale();
            }
        }

        public static bool Paused
        {
            get => paused;
            set
            {
                paused = value;
                UpdateTimeScale();
            }
        }

        public static void UpdateTimeScale()
        {
            Time.timeScale = Paused ? 0 : gameTime;
            Time.fixedDeltaTime = 0.01f * gameTime;
        }
        #endregion

        public static object target;
        private static readonly Dictionary<string, IList<CommandInfo>> commands = new Dictionary<string, IList<CommandInfo>>();
        private static readonly Dictionary<char, char> parseCloser = new Dictionary<char, char>() { { '"', '"' }, { '(', ')' } };
        private static readonly Stack<char> grouping = new Stack<char>();
        private static readonly Queue<string> argBuffer = new Queue<string>();
        private static readonly Dictionary<Type, HashSet<object>> instanceRegistry = new Dictionary<Type, HashSet<object>>();
        private static readonly Dictionary<Type, MethodInfo> conversions = new Dictionary<Type, MethodInfo>();
        private static readonly Queue<string> rawArgs = new Queue<string>();

        private struct CommandInfo
        {
            public ConsoleCommandAttribute attribute;
            public Type type;
            public MethodInfo method;
        }

        [ConsoleCommand("help", "Please hire me uwu")]
        public static string Help(string command)
        {
            if (command == null)
                return "Help command received a null string.";
            if (commands.TryGetValue(command, out IList<CommandInfo> infos))
                return infos[0].attribute.info;
            else
                return $"{command} is not a registered command.";
        }

        [ConsoleCommand("find", "Finds commands containing a given part")]
        public static string Find(string part)
        {
            string ret = "------------\n";
            part = part ?? "";

            if (part.Length > 0)
            {
                SortedList<float, string> matches = new SortedList<float, string>();

                foreach (string key in commands.Keys)
                    if (key.Contains(part))
                    {
                        float position = key.Length / part.Length;

                        while (matches.ContainsKey(position))
                            position += 0.00001f;

                        matches.Add(position, key);
                    }

                foreach (string match in matches.Values)
                    ret += match + '\n';
            }
            else
            {
                SortedSet<string> matches = new SortedSet<string>();

                foreach (string key in commands.Keys)
                    matches.Add(key);

                foreach (string match in matches)
                    ret += match + '\n';
            }    

            return ret;
        }

        /// <summary> Send an argument to the console </summary>
        /// <remarks> Group spaced characters together withh quotation marks or parentheses </remarks>
        /// <returns> the concatenated string output of the performed executions </returns>
        public static string Command(string arg)
        {
            ParseCommand(arg);
            string output = "";
            string firstArg = argBuffer.Count > 0 ? argBuffer.Dequeue() : "";

            if (commands.TryGetValue(firstArg, out IList<CommandInfo> commandList))
                foreach (CommandInfo cmd in commandList)
                    output += Execute(cmd, argBuffer.ToArray()) + '\n';

            return output.TrimEnd('\n');
        }

        /// <summary> Register an instance of a Type to be affected by non-static commands </summary>
        public static void RegisterInstance(object instance)
        {
            Type type = instance.GetType();

            if (instanceRegistry.TryGetValue(type, out HashSet<object> list))
                list.Add(instance);
            else
                instanceRegistry.Add(type, new HashSet<object>() { instance });
        }

        /// <summary> Remove an instance of a Type so it is no longer affected by non-static commands </summary>
        public static void RemoveInstance(object instance)
        {
            Type type = instance.GetType();

            if (instanceRegistry.TryGetValue(type, out HashSet<object> list))
                list.Remove(instance);
        }

        private static string Execute(CommandInfo cmd, string[] args)
        {
            try
            {

                rawArgs.Clear();

                foreach (string arg in args)
                    rawArgs.Enqueue(arg);

                ParameterInfo[] parameters = cmd.method.GetParameters();
                object[] sendParams = new object[parameters.Length];

                for (int i = 0; i < sendParams.Length && rawArgs.Count > 0; i++)
                    sendParams[i] = Convert(parameters[i].ParameterType, rawArgs);

                if (!cmd.type.IsAbstract && !cmd.type.IsSealed)
                {
                    if (instanceRegistry.TryGetValue(cmd.type, out HashSet<object> instances) && instances.Count > 0)
                    {
                        string message = "";

                        foreach (object instance in instances)
                            if (cmd.method.Invoke(instance, sendParams) is string s)
                                message += s + '\n';

                        return message.TrimEnd('\n');
                    }
                    else
                        return "No available instances of " + cmd.type.Name;
                }
                else
                    return cmd.method.Invoke(null, sendParams) as string;
            }
            catch
            {
                return "";
            }
        }

        private static object Convert(Type type, Queue<string> args)
        {
            if (args.Count > 0)
            {
                if (type.Equals(typeof(string)))
                    return args.Dequeue();
                else if(type.Equals(typeof(string[])))
                {
                    string[] sendParams = args.ToArray();
                    args.Clear();
                    return sendParams;
                }
                else if (conversions.TryGetValue(type, out MethodInfo method))
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    object[] sendParams = new object[parameters.Length];

                    for (int i = 0; i < parameters.Length; i++)
                        sendParams[i] = Convert(parameters[i].ParameterType, args);

                    return method.Invoke(null, sendParams);
                }
            }

            return null;
        }

        private static void ParseCommand(string arg)
        {
            int last = 0;

            grouping.Clear();
            argBuffer.Clear();

            for (int i = 0; i < arg.Length; i++)
            {
                if ((i == arg.Length - 1)
                    || (grouping.Count == 0 && arg[i] == ' ')
                    || (grouping.Count != 0 && parseCloser.TryGetValue(grouping.Peek(), out char close) && arg[i] == close))
                {
                    if (grouping.Count != 0)
                        grouping.Pop();

                    string add = arg.Substring(last, i - last + 1).Trim(' ', '"');

                    if (add.Length > 0)
                        argBuffer.Enqueue(add);

                    last = i + 1;
                }
                else if (parseCloser.ContainsKey(arg[i]))
                {
                    grouping.Push(arg[i]);
                }
            }
        }

        public static void Reset()
        {
            Type containerAttribute = typeof(ContainsConsoleCommandsAttribute);
            Type commandAttributeType = typeof(ConsoleCommandAttribute);
            Type conversionAttribute = typeof(ConversionAttribute);

            conversions.Clear();
            commands.Clear();
            target = null;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                foreach (Type type in assembly.GetTypes())
                    if (type.GetCustomAttribute(containerAttribute, false) is ContainsConsoleCommandsAttribute)
                        foreach(MethodInfo method in type.GetMethods())
                            if(method.GetCustomAttribute(commandAttributeType, false) is ConsoleCommandAttribute cmdAttribute)
                            {
                                CommandInfo command;

                                command.attribute = cmdAttribute;
                                command.type = type;
                                command.method = method;

                                if (commands.TryGetValue(cmdAttribute.callname, out IList<CommandInfo> list))
                                    list.Add(command);
                                else
                                    commands.Add(cmdAttribute.callname, new List<CommandInfo>() { command });
                            }
                            else if(method.GetCustomAttribute(conversionAttribute, false) is ConversionAttribute)
                            {
                                conversions.Add(method.ReturnType, method);
                            }
        }
    }
}
