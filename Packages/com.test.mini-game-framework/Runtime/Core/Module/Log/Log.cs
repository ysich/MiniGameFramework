using System;
using System.Diagnostics;

namespace Core
{
    public static class Log
    {
        [Conditional("DEBUG")]
        public static void Trace(string msg)
        {
            Logger.Instance.Trace(msg);
        }

        [Conditional("DEBUG")]
        public static void Debug(string msg)
        {
            Logger.Instance.Debug(msg);
        }

        public static void Info(string msg)
        {
            Logger.Instance.Info(msg);
        }

        [Conditional("DEBUG")]
        public static void Warning(string msg)
        {
            Logger.Instance.Warning(msg);
        }

        public static void Error(string msg)
        {
            Logger.Instance.Error(msg);
        }

        public static void Error(Exception msg)
        {
            Logger.Instance.Error(msg);
        }

        [Conditional("DEBUG")]
        public static void Console(string msg)
        {
            Logger.Instance.Console(msg);
        }
    }
}
