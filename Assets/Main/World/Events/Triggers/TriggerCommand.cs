using MPConsole;
using UnityEngine;

namespace MPWorld
{
    public class TriggerCommand : MonoBehaviour
    {
        public void Command(string command)
        {
            Console.Command(command);
        }
    }
}
