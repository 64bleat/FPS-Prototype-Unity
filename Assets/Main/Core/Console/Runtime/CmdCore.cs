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

        //[ConsoleCommand("slomo", "Change the speed of the game. Default is 1.")]
        //public static string Slomo(float timeScale)
        //{
        //    Console.GameTimeProp = timeScale;

        //    return "Time scale set to " + Console.GameTimeProp;
        //}
    }
}
