using System;

namespace Mover
{
    public interface ILogger
    {
        void Log(string message);
        void LogException(Exception e);
    }
}