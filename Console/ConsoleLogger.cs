using Mover;

namespace Console
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            System.Console.WriteLine(message);
        }
    }
}