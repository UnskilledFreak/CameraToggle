using System.Threading;

namespace Console
{
    class Program
    {
        static void Main(string[] args)
        {
            new Mover.Factory(new ConsoleLogger());

            // set main thread to do nothing
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
