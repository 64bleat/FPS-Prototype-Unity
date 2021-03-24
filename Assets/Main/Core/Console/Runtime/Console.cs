using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MPConsole
{
    [ContainsConsoleCommands]
    public static class Console
    {
        public static object target;

        private static readonly Dictionary<char, char> parseCloser = new Dictionary<char, char>() { { '"', '"' }, { '(', ')' } };

        private static readonly Dictionary<string, IList<CommandInfo>> commands = new Dictionary<string, IList<CommandInfo>>();
        private static readonly Dictionary<Type, HashSet<object>> instanceRegistry = new Dictionary<Type, HashSet<object>>();
        private static readonly Dictionary<Type, MethodInfo> paramTypeConversions = new Dictionary<Type, MethodInfo>();

        private struct CommandInfo
        {
            public ConsoleCommandAttribute attribute;
            public Type baseType;
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
            string start;

            part ??= string.Empty;

            if (part.Length == 0)
                start = "\n--- Displaying all commands ---";
            else 
                start = $"\n--- Displaying commands containing '{part}' ---";

            if (part.Length > 0)
                return commands
                    .Where(kvp => kvp.Key.Contains(part))
                    .OrderBy(kvp => kvp.Key.Length / part.Length)
                    .Select(kvp => new { name = kvp.Value[0].attribute.callname, info = kvp.Value[0].attribute.info })
                    .Aggregate(start, (str, cmd) => $"{str}\n{cmd.name,-16}: {cmd.info}");
            else
                return commands
                    .OrderBy(kvp => kvp.Key)
                    .Select(kvp => new { name = kvp.Value[0].attribute.callname, info = kvp.Value[0].attribute.info })
                    .Aggregate(start, (str, cmd) => $"{str}\n{cmd.name,-16}: {cmd.info}");
        }

        /// <summary> Send an argument to the console </summary>
        /// <remarks> Group spaced characters together withh quotation marks or parentheses </remarks>
        /// <returns> the concatenated string output of the performed executions </returns>
        public static string Command(string arg)
        {
            IList<string> argList = ParseCommand(arg, out string cmdName);

            if (commands.TryGetValue(cmdName, out IList<CommandInfo> commandList))
                return commandList
                    .Select(cmd => Execute(cmd, argList))
                    .Where(s => s?.Length > 0)
                    .Aggregate(string.Empty, (str, s) => $"{str}\n{s}");
            else
                return string.Empty;
        }

        /// <summary> Register an instance of a Type to be affected by non-static commands </summary>
        public static void AddInstance(object instance)
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

        private static string Execute(CommandInfo cmd, IList<string> args)
        {
            try
            {
                Queue<string> paramBuffer = new Queue<string>(args);
                ParameterInfo[] parameters = cmd.method.GetParameters();
                object[] sendParams = new object[parameters.Length];
                bool isStatic = cmd.baseType.IsAbstract && cmd.baseType.IsSealed;

                for (int i = 0; i < sendParams.Length; i++)
                    sendParams[i] = PullInvokeParameters(parameters[i].ParameterType, paramBuffer);

                if (isStatic)
                    return cmd.method.Invoke(null, sendParams) as string;
                else if (instanceRegistry.TryGetValue(cmd.baseType, out HashSet<object> instances) && instances.Count > 0)
                    return instances
                        .Select(i => cmd.method.Invoke(i, sendParams) as string)
                        .Where(s => s?.Length > 0)
                        .Aggregate(string.Empty, (s, a) => $"{s}\n{a}");
                else
                    return $"No available instances of {cmd.baseType.Name}";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        private static object PullInvokeParameters(Type convertType, Queue<string> sBuffer)
        {
            if (convertType.Equals(typeof(string)))
                return sBuffer.Count > 0 ? sBuffer.Dequeue() : default;
            else if (convertType.Equals(typeof(string[])))
            {
                string[] invokeParams = sBuffer.ToArray();
                sBuffer.Clear();
                return invokeParams;
            }
            else if (paramTypeConversions.TryGetValue(convertType, out MethodInfo method))
            {
                ParameterInfo[] parameters = method.GetParameters();
                object[] invokeParams = new object[parameters.Length];

                for (int i = 0; i < parameters.Length; i++)
                    if (sBuffer.Count > 0)
                        invokeParams[i] = PullInvokeParameters(parameters[i].ParameterType, sBuffer);
                    else
                        invokeParams[i] = default;

                return method.Invoke(null, invokeParams);
            }
            else
                return default;
        }

        private static List<string> ParseCommand(string arg, out string cmdName)
        {
            int last = 0;

            Stack<char> grouping = new Stack<char>();
            List<string> argList = new List<string>();

            for (int i = 0; i < arg.Length; i++)
            {
                if ((i == arg.Length - 1)
                    || (grouping.Count == 0 && arg[i] == ' ')
                    || (grouping.Count != 0 && parseCloser.TryGetValue(grouping.Peek(), out char close) && arg[i] == close))
                {
                    if (grouping.Count != 0)
                        grouping.Pop();

                    string sParam = arg.Substring(last, i - last + 1).Trim(' ', '"');

                    if (sParam.Length > 0)
                        argList.Add(sParam);

                    last = i + 1;
                }
                else if (parseCloser.ContainsKey(arg[i]))
                {
                    grouping.Push(arg[i]);
                }
            }

            if (argList.Count > 0)
            {
                cmdName = argList[0];
                argList.RemoveAt(0);
            }
            else
                cmdName = "";

            return argList;
        }

        public static void Reset()
        {
            Type containerAttribute = typeof(ContainsConsoleCommandsAttribute);
            Type commandAttributeType = typeof(ConsoleCommandAttribute);
            Type conversionAttribute = typeof(ConversionAttribute);

            paramTypeConversions.Clear();
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
                                command.baseType = type;
                                command.method = method;

                                if (commands.TryGetValue(cmdAttribute.callname, out IList<CommandInfo> list))
                                    list.Add(command);
                                else
                                    commands.Add(cmdAttribute.callname, new List<CommandInfo>() { command });
                            }
                            else if(method.GetCustomAttribute(conversionAttribute, false) is ConversionAttribute)
                            {
                                paramTypeConversions.Add(method.ReturnType, method);
                            }
        }
    }
}
