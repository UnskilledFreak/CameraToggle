using System;

namespace Mover
{
    public abstract class BaseLogger : ILogger
    {
        public abstract void Log(string message);
        
        public void LogException(Exception e)
        {
            Log(e.GetType().ToString());
            Log(e.Message);
            Log(e.StackTrace);

            if (e.InnerException != null)
            {
                Log("---inner exception---");
                LogException(e.InnerException);
            }
        }
    }
}