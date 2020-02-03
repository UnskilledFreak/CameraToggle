using Mover;

namespace Console
{
    public class ConsoleLogger : BaseLogger
    {
        public override void Log(string message)
        {
            System.Console.WriteLine(message);
        }
    }
}