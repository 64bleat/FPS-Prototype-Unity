using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MPConsole
{
    [ContainsConsoleCommands]
    public static class CmdCore
    {
        [ConsoleCommand("exit", "Exits the game immediately.")]
        public static string Exit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif

            return "Exiting game";
        }

        [ConsoleCommand("destroy", "Destroys targeted gameobjects.")]
        public static string DestroyTarget()
        {
            GameObject go = Console.target as GameObject;

            if (!go)
                return "No target to destroy";

            GameObject.Destroy(go);
            Console.target = null;

            return null;
        }

        [ConsoleCommand("open", "Opens a scene by name.")]
        public static string OpenScene(string name)
        {
            try
            {
                if (int.TryParse(name, out int i) && i < SceneManager.sceneCount)
                    SceneManager.LoadScene(i);
                else
                    SceneManager.LoadScene(name);

                return "Opening: " + name;
            }
            catch (Exception)
            {
                return "Map not found.";
            }
        }

        [ConsoleCommand("slomo", "Change the speed of the game. Default is 1.")]
        public static string Slomo(float timeScale)
        {
            Console.GameTime = timeScale;

            return "Time scale set to " + Console.GameTime;
        }
    }
}


    //[ConsoleCommand("set", "set [componentName] [fieldName] [value]")]
    //public class CmdSet : ConsoleCommand
    //{
    //    public CmdSet()
    //    {
    //        callName = "set";
    //        helpMessage = "set [componentName] [fieldName] [value]";
    //    }

    //    public override string Execute(string[] args)
    //    {
    //        try
    //        {
    //            object target = Console.target;

    //            if (target != null)// && args.Length >= 4)
    //            {
    //                if (target is GameObject)
    //                {

    //                    if (args[1].Equals("?"))
    //                    {
    //                        string message = "\nComponents\n";
    //                        Component[] comps = ((GameObject)target).GetComponents<Component>();

    //                        foreach (Component c in comps)
    //                            message += c.GetType().Name + "\n";

    //                        return message;
    //                    }
    //                    object comp = ((GameObject)target).GetComponent(args[1]);

    //                    if (comp != null)
    //                    {
    //                        FieldInfo fi = comp.GetType().GetField(args[2]);
    //                        PropertyInfo pi = comp.GetType().GetProperty(args[2]);
    //                        MethodInfo mi = comp.GetType().GetMethod(args[2]);

    //                        // LIST VALUES ==============================
    //                        if (args[2].Equals("?"))
    //                        {
    //                            FieldInfo[] fields = comp.GetType().GetFields();
    //                            PropertyInfo[] properties = comp.GetType().GetProperties();
    //                            MethodInfo[] methods = comp.GetType().GetMethods();

    //                            string message = "";

    //                            args[2] = args[2].Trim('?');
    //                            message += "\n--- " + comp.GetType().ToString() + " ---\nFIELDS:\n";

    //                            for (int i = 0; i < fields.Length; i++)
    //                            {
    //                                object value = fields[i].GetValue(comp);

    //                                message += (fields[i].IsPublic ? "public " : "private ") + fields[i].FieldType.Name + " " + fields[i].Name + " = " + (value != null ? value.ToString() : "null") + ";\n";
    //                            }

    //                            message += "\nPROPERTIES:\n";

    //                            for (int i = 0; i < properties.Length; i++)
    //                            {

    //                                object value = null;

    //                                try { value = properties[i].GetValue(comp); }
    //                                catch (Exception) { }

    //                                if (properties[i].CanWrite)
    //                                    message += properties[i].PropertyType.Name + " " + properties[i].Name + " = " + (value != null ? value.ToString() : "null") + ";\n";
    //                            }

    //                            //message += "\nMETHODS\n";

    //                            //for(int i = 0; i < methods.Length; i++)
    //                            //{
    //                            //    string mstring = methods[i].Name + "(";

    //                            //    ParameterInfo[] parameters = methods[i].GetParameters();

    //                            //    for (int j = 0; j < parameters.Length; j++)
    //                            //        mstring += parameters[j].ParameterType.Name + " " + parameters[j].Name + (j < parameters.Length - 1 ? ", " : "");

    //                            //    message += mstring + ")\n";
    //                            //}

    //                            return message;
    //                        }
    //                        // SET FIELD VALUES ===============================
    //                        else if (fi != null)
    //                        {
    //                            // check field type ...........................
    //                            if (args[3].Equals("?"))
    //                            {
    //                                return "Enter a " + fi.FieldType.Name + ".";
    //                            }
    //                            // set string field ...........................
    //                            else if (fi.FieldType == typeof(string))
    //                            {
    //                                fi.SetValue(comp, args[3]);
    //                            }
    //                            // set float field ............................
    //                            else if (fi.FieldType == typeof(float))
    //                            {
    //                                if (float.TryParse(args[3], out float value))
    //                                    fi.SetValue(comp, value);
    //                                else
    //                                    return "Field requires a valid float.";
    //                            }
    //                            // set int field ..............................
    //                            else if (fi.FieldType == typeof(int))
    //                            {
    //                                if (int.TryParse(args[3], out int value))
    //                                    fi.SetValue(comp, value);
    //                                else
    //                                    return "Field requires a valid int.";
    //                            }
    //                            // set bool field .............................
    //                            else if (fi.FieldType == typeof(bool))
    //                            {
    //                                if (bool.TryParse(args[3], out bool value))
    //                                    fi.SetValue(comp, value);
    //                                else
    //                                    return "Field requires a valid bool.";
    //                            }
    //                            // set failed .................................
    //                            else
    //                                return "Field Type " + fi.FieldType.ToString() + " is not supported.";
    //                        }
    //                        // SET PROPERTY VALUES ============================
    //                        else if(pi != null)
    //                        {
    //                            // check property type ........................
    //                            if (args[3].Equals("?"))
    //                                return "Enter a " + pi.PropertyType.Name + ".";
    //                            // set string property
    //                            else if(pi.PropertyType == typeof(string))
    //                                pi.SetValue(comp, args[3]);
    //                            // set float property .........................
    //                            else if (pi.PropertyType == typeof(float))
    //                            {
    //                                if (float.TryParse(args[3], out float value))
    //                                    pi.SetValue(comp, value);
    //                                else
    //                                    return "Property requires a valid float.";
    //                            }
    //                            // set int property ...........................
    //                            else if (pi.PropertyType == typeof(int))
    //                            {
    //                                if (int.TryParse(args[3], out int value))
    //                                    pi.SetValue(comp, value);
    //                                else
    //                                    return "Property requires a valid int.";
    //                            }
    //                            // set bool property ..........................
    //                            else if (pi.PropertyType == typeof(bool))
    //                            {
    //                                if (bool.TryParse(args[3], out bool value))
    //                                    pi.SetValue(comp, value);
    //                                else
    //                                    return "Property requires a valid bool.";
    //                            }
    //                            // set failed .................................
    //                            else
    //                                return "Property type " + pi.PropertyType.Name + " is not supported.";
    //                        }
    //                        else
    //                            return "Field " + args[2] + " not found.";
    //                    }
    //                    else
    //                        return "Component " + args[1] + " not found.";
    //                }
    //                else
    //                    return "Target Type " + target.GetType() + " is not supported";
    //            }
    //            else
    //                return "Target is null.";

    //            return "";
    //        }
    //        catch (Exception) { return "Cound not set a value."; }
    //    }
    //}

    //[ConsoleCommand("target", "Sets the target of your commands. Special targets: player, camera")]
    //public class CmdTarget : ConsoleCommand
    //{
    //    public CmdTarget()
    //    {
    //        callName = "target";
    //        helpMessage = "Sets the target of your commands. Special targets: player, camera";
    //    }

    //    public override string Execute(string[] args)
    //    {
    //        if (args.Length > 1)
    //            switch (args[1])
    //            {
    //                default:
    //                    Console.target = GameObject.Find(args[1]); break;
    //            }

    //        if (Console.target == null)
    //            return "No Target";
    //        else
    //            return "Target set to " + Console.target.GetType();
    //    }
    //}

    //[ConsoleCommand("find", "Lists commands. can be filtered.")]
    //public class CmdFind : ConsoleCommand
    //{
    //    public CmdFind()
    //    {
    //        callName = "find";
    //        helpMessage = "Lists commands. can be filtered.";
    //    }

    //    public override string Execute(string[] args)
    //    {
    //        string output = "---COMMAND LIST---\n";

    //        IEnumerable<string> list = from string s in Console.commands.Keys
    //                                   where args.Length <= 1 || args[1].Length == 0 || s.Contains(args[1])
    //                                   orderby s 
    //                                   select s + " ";

    //        foreach (string s in list)
    //            output += s;

    //        return output;
    //    }
    //}

    //[ConsoleCommand("help", "looks like you already got it down.")]
    //public class CmdHelp : ConsoleCommand
    //{
    //    public CmdHelp()
    //    {
    //        callName = "help";
    //        helpMessage = "Oh no, you really do need help.";
    //    }

    //    public override string Execute(string[] args)
    //    {
    //        if (args.Length <= 1)
    //            return "type \"help [command name]\" for command help!";
    //        else if (Console.commands.TryGetValue(args[1], out HashSet<ConsoleCommand> set))
    //        {
    //            string help = "";

    //            foreach (ConsoleCommand cmd in set)
    //                help += cmd.helpMessage + '\n';

    //            return help.TrimEnd('\n');
    //        }
    //        else
    //            return "No such command exists. FIND some commands...";
    //    }
    //}
