using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MPConsole
{
    [ContainsConsoleCommands]
    public static class Console
    {
        public static object target;

        private static readonly Dictionary<char, char> groupClosers = new Dictionary<char, char>() { { '"', '"' }, { '(', ')' } };

        private static readonly Dictionary<string, IList<CommandInfo>> commands = new Dictionary<string, IList<CommandInfo>>();
        private static readonly Dictionary<Type, HashSet<object>> cmdInstances = new Dictionary<Type, HashSet<object>>();
        private static readonly Dictionary<Type, MethodInfo> cmdConversions = new Dictionary<Type, MethodInfo>();

        private struct CommandInfo
        {
            public ConsoleCommandAttribute attribute;
            public MethodInfo method;
        }

        /// <summary> Send an argument to the console </summary>
        /// <remarks> Group spaced characters together withh quotation marks or parentheses </remarks>
        /// <returns> the concatenated string output of the performed executions </returns>
        public static string Command(string arg)
        {
            IList<string> argList = ParseCommandArgs(arg, out string cmdName);

            if (commands.TryGetValue(cmdName, out IList<CommandInfo> commandList))
                return commandList
                    .Select(cmd => Invoke(cmd, argList))
                    .Where(readback => readback?.Length > 0)
                    .Aggregate(string.Empty, (concat, readback) => $"{concat}\n{readback}");
            else
                return string.Empty;
        }



        /// <summary> Register an instance of a Type to be affected by non-static commands </summary>
        public static void AddInstance<T>(T instance)
        {
            Type type = typeof(T);

            if (cmdInstances.TryGetValue(type, out HashSet<object> list))
                list.Add(instance);
            else
                cmdInstances.Add(type, new HashSet<object>() { instance });
        }

        /// <summary> Remove an instance of a Type so it is no longer affected by non-static commands </summary>
        public static void RemoveInstance<T>(T instance)
        {
            Type type = typeof(T);

            if (cmdInstances.TryGetValue(type, out HashSet<object> list))
                list.Remove(instance);
        }

        private static string Invoke(CommandInfo cmd, IList<string> args)
        {
            try
            {
                Queue<string> paramConsumeBuffer = new Queue<string>(args);
                object[] invokeParams = FulfillParameters(cmd.method, paramConsumeBuffer);
                Type declaringType = cmd.method.DeclaringType;
                bool parameterIsStatic = declaringType.IsAbstract && declaringType.IsSealed;

                // Invoke Static Method
                if (parameterIsStatic)
                    return cmd.method.Invoke(null, invokeParams) as string;

                // Invoke Instanced Methods
                if (cmdInstances.TryGetValue(declaringType, out HashSet<object> instances) && instances.Count > 0)
                    return instances
                        .Select(instance => cmd.method.Invoke(instance, invokeParams) as string)
                        .Where(readout => readout?.Length > 0)
                        .Aggregate(string.Empty, (s, a) => $"{s}\n{a}");
                
                // Invoke Failed
                return $"No available instances of {declaringType.Name}";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        private static object[] FulfillParameters(MethodInfo method, Queue<string> paramConsumeBuffer)
        {
            ParameterInfo[] parameters = method.GetParameters();
            object[] invokeParams = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
                invokeParams[i] = FulfillParameter(parameters[i], paramConsumeBuffer);

            return invokeParams;
        }

        private static object FulfillParameter(ParameterInfo parameter, Queue<string> paramConsumeBuffer)
        {
            Type parameterType = parameter.ParameterType;

            // Buffer is Empty and Parameter has Default
            if (paramConsumeBuffer.Count == 0)
                if (parameter.HasDefaultValue)
                    return parameter.DefaultValue;
                else
                    return default;

            // Parameter is String
            if (parameterType.Equals(typeof(string)))
                return paramConsumeBuffer.Count > 0 ? paramConsumeBuffer.Dequeue() : default;

            // Parameter is String[]
            if (parameterType.Equals(typeof(string[])))
            {
                string[] invokeParams = paramConsumeBuffer.ToArray();
                paramConsumeBuffer.Clear();
                return invokeParams;
            }

            // Parameter is String-Convertable
            if (cmdConversions.TryGetValue(parameterType, out MethodInfo conversionMethod))
            {
                object[] invokeParams = FulfillParameters(conversionMethod, paramConsumeBuffer);

                return conversionMethod.Invoke(null, invokeParams);
            }

            // Parameter is a Value-Type
            if (parameterType.IsValueType)
                return Activator.CreateInstance(parameterType);

            // Parameter is a Reference-Type
            return null;
        }

        private static List<string> ParseCommandArgs(string arg, out string cmdName)
        {
            int last = 0;

            Stack<char> grouping = new Stack<char>();
            List<string> argList = new List<string>();

            for (int i = 0; i < arg.Length; i++)
            {
                if ((i == arg.Length - 1)
                    || (grouping.Count == 0 && arg[i] == ' ')
                    || (grouping.Count != 0 && groupClosers.TryGetValue(grouping.Peek(), out char close) && arg[i] == close))
                {
                    if (grouping.Count != 0)
                        grouping.Pop();

                    string sParam = arg.Substring(last, i - last + 1).Trim(' ', '"');

                    if (sParam.Length > 0)
                        argList.Add(sParam);

                    last = i + 1;
                }
                else if (groupClosers.ContainsKey(arg[i]))
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

            cmdConversions.Clear();
            commands.Clear();
            target = null;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                foreach (Type type in assembly.GetTypes())
                    if (type.GetCustomAttribute(containerAttribute, false) is ContainsConsoleCommandsAttribute)
                        foreach (MethodInfo method in type.GetMethods())
                            if (method.GetCustomAttribute(commandAttributeType, false) is ConsoleCommandAttribute cmdAttribute)
                            {
                                CommandInfo command = new(){
                                    attribute = cmdAttribute,
                                    method = method};

                                if (commands.TryGetValue(cmdAttribute.callname, out IList<CommandInfo> list))
                                    list.Add(command);
                                else
                                    commands.Add(cmdAttribute.callname, new List<CommandInfo>() { command });
                            }
                            else if(method.GetCustomAttribute(conversionAttribute, false) is ConversionAttribute)
                            {
                                cmdConversions.Add(method.ReturnType, method);
                            }
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
    }
}
