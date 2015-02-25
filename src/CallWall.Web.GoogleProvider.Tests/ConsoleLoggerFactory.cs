using System;

namespace CallWall.Web.GoogleProvider.Tests
{
    public sealed class ConsoleLoggerFactory : ILoggerFactory
    {
        public ILogger CreateLogger(Type loggedType)
        {
            return new ConsoleLogger();
        }
    }
}