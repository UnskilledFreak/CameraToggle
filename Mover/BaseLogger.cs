using System;

namespace Mover
{
    public abstract class BaseLogger : ILogger
    {
        public abstract void Log(string message);
        
        public void LogException(Exception e)
        {
            if (e.InnerException != null)
            {
                LogException(e.InnerException);
                Log("---inner exception---");
            }
            
            Log(e.StackTrace);
            Log(e.Message);
            Log(e.GetType().ToString());
        }
    }
}