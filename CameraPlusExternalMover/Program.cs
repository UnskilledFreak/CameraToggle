using System.Threading;

namespace CameraPlusExternalMover
{
    class Program
    {
        static void Main(string[] args)
        {
            new Mover(new ConsoleLogger());

            // set main thread to do nothing
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
