using System;
using System.Threading;

namespace CallWall.Web.EventStore.Tests.Doubles
{
    public sealed class ConsoleLogger : ILogger
    {
        /// <summary>
        /// Writes the message and optional exception to the log for the given level.
        /// </summary>
        /// <param name="level">The Logging level to apply. Useful for filtering messages</param>
        /// <param name="message">The message to be logged</param>
        /// <param name="exception">An optional exception to be logged with the message</param>
        /// <remarks>
        /// It is preferable to use the <see cref="ILogger"/> extension methods found in the <see cref="Web.LoggerExtensions"/> static type.
        /// </remarks>
        public void Write(LogLevel level, string message, Exception exception)
        {
            var threadName = ThreadName();
            Console.WriteLine("[{2}] {0} - {1}", level, message, threadName);
            if (exception != null)
            {
                Console.WriteLine(exception);
            }

        }

        private static string ThreadName()
        {
            var name = Thread.CurrentThread.Name;
            var id = Thread.CurrentThread.ManagedThreadId;
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (name.Length > 15)
                {
                    name = name.Substring(0, 15);
                }
            }
            else
            {
                name = string.Empty;
            }
            name = string.Format("{0}-{1:0000}", name.PadRight(15), id);
            return name;
        }
    }
}