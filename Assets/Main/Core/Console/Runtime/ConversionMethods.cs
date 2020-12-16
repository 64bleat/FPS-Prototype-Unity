﻿using UnityEngine;

namespace MPConsole
{

    public static class ConversionMethods
    {
        [Conversion]
        public static bool ParseBool(string s)
        {
            return bool.Parse(s);
        }

        [Conversion]
        public static float ParseFloat(string s)
        {
            return float.Parse(s);
        }

        [Conversion]
        public static int ParseInt(string s)
        {
            return int.Parse(s);
        }

        [Conversion]
        public static Vector3 MakeVector3(float a, float b, float c)
        {
            return new Vector3(a, b, c);
        }
    }
}
