using System;
using System.Diagnostics;

namespace Hspi
{
    internal interface IDebugLogger
    {
        void LogDebug(string message);
    }

    internal class HSTraceListener : TraceListener
    {
        public HSTraceListener(IDebugLogger logger)
        {
            loggerWeakReference = new WeakReference<IDebugLogger>(logger);
        }

        public override void Write(string message)
        {
            Log(message);
        }

        public override void WriteLine(string message)
        {
            Log(message);
        }

        private void Log(string message)
        {
            if (loggerWeakReference.TryGetTarget(out var logger))
            {
                logger.LogDebug(message);
            }
        }

        private readonly WeakReference<IDebugLogger> loggerWeakReference;
    }
}