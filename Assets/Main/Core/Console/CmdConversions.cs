using System.Text.RegularExpressions;
using UnityEngine;
using System.Reflection;
using System;

namespace MPConsole
{
    [ContainsConsoleCommands]
    public static class CmdConversions
    {
        [Conversion]
        public static bool ParseBool(string s)
        {
            return bool.Parse(s);
        }

        [Conversion]
        public static int ParseInt(string s)
        {
            return int.Parse(s);
        }

        [Conversion]
        public static long ParseLong(string s)
        {
            return long.Parse(s);
        }

        [Conversion]
        public static float ParseFloat(string s)
        {
            return float.Parse(s);
        }

        [Conversion]
        public static double ParseDouble(string s)
        {
            return double.Parse(s);
        }

        [Conversion]
        public static Vector3 ParseVector3(float a, float b, float c)
        {
            return new Vector3(a, b, c);
        }

        [Conversion]
        public static Vector3Int ParseVector3Int(int a, int b, int c)
        {
            return new Vector3Int(a, b, c);
        }

        [Conversion]
        public static DateTime ParseDateTime(string timeCode)
        {
            switch (timeCode)
            {
                case "now":
                    return DateTime.Now;
                case "utcnow":
                    return DateTime.UtcNow;
                default:
                    if (DateTime.TryParse(timeCode, out DateTime result))
                        return result;
                    else
                        return default;
            }
        }

        [Conversion]
        public static GameObject ParseGameObject(string name)
        {
            if (name == "target")
                return Console.target as GameObject;
            else
                return GameObject.Find(name);
        }

        [Conversion]
        public static Component ParseComponent(string name)
        {
            string[] path = name.Split('/');
            string gameObjectName = path[path.Length - 2];
            string componentTypeName = path[path.Length - 1];
            GameObject gameObject = ParseGameObject(gameObjectName);
            Type componentType = Type.GetType(componentTypeName);

            gameObject.TryGetComponent(componentType, out Component component);

            return component;
        }
    }
}
