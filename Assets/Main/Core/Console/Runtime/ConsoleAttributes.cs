 using System;

namespace MPConsole
{
    /// <summary> Flags a class as contianing more console related attributes</summary>
    /// <remarks> non-static classes require instances to be registered to the console. 
    /// Commands will run on every registered instance. </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public class ContainsConsoleCommandsAttribute : Attribute
    {

    }

    /// <summary> Flags a method to be available as a console command. </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ConsoleCommandAttribute : Attribute 
    {
        public string callname;
        public string info;

        public ConsoleCommandAttribute(string callname = null, string info = null)
        {
            this.callname = callname;
            this.info = info;
        }
    }

    /// <summary> Flags a method as one that converts args to a specific return type. </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ConversionAttribute : Attribute
    {

    }
}
