using System;

namespace CallWall.Web.EventStore.Tests.Doubles
{
    public sealed class ConsoleLoggerFactory : ILoggerFactory
    {
        public ILogger CreateLogger(Type loggedType)
        {
            return new ConsoleLogger();
        }
    }
}