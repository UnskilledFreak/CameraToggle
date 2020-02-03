using System;
using Mover;

namespace CameraPlusExternalMover
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}