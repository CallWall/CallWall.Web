using System;
using System.Net;
using System.Threading.Tasks;
using CallWall.Web.EventStore.Configuration;
using EventStore.ClientAPI;
using IESLogger = EventStore.ClientAPI.ILogger;

namespace CallWall.Web.EventStore
{
    public interface IEventStoreConnectionFactory
    {
        Task<IEventStoreConnection> Connect();
    }

    public sealed class EventStoreConnectionFactory : IEventStoreConnectionFactory
    {
        private static readonly Lazy<CallWallEventStoreSection> Config = new Lazy<CallWallEventStoreSection>(CallWallEventStoreSection.GetConfig);
        private static readonly Lazy<IPAddress> ConfiguredIpAddress = new Lazy<IPAddress>(LoadIpAddressFromConfig);
        private static readonly Lazy<int> ConfiguredPort = new Lazy<int>(LoadPortFromConfig);
        private readonly CallWall.Web.ILogger _logger;
        private readonly EventStoreLoggerBridge _esLogger;

        public EventStoreConnectionFactory(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _esLogger = new EventStoreLoggerBridge(_logger);
        }

        //TODO: Opportunity to share a single connection. Check for best practices. -LC
        public async Task<IEventStoreConnection> Connect()
        {
            var connectionSettings = ConnectionSettings.Create()
                .KeepReconnecting()
                .UseCustomLogger(_esLogger);
            var endPoint = new IPEndPoint(ConfiguredIpAddress.Value, ConfiguredPort.Value);
            var conn =  EventStoreConnection.Create(connectionSettings, endPoint);
            await conn.ConnectAsync();
            
            return conn;
        }

        private static IPAddress LoadIpAddressFromConfig()
        {
            return Config.Value.Connection.Endpoint;

            //return new IPAddress(new byte[] {127, 0, 0, 1});
        }

        private static int LoadPortFromConfig()
        {
            return Config.Value.Connection.Port;
            
            //return 2113;
        }
    }

    class EventStoreLoggerBridge : IESLogger
    {
        private readonly ILogger _logger;

        public EventStoreLoggerBridge(CallWall.Web.ILogger logger)
        {
            _logger = logger;
        }

        public void Error(string format, params object[] args)
        {
            _logger.Error(format, args);
        }

        public void Error(Exception ex, string format, params object[] args)
        {
            _logger.Error(ex, format, args);
        }

        public void Info(string format, params object[] args)
        {
            _logger.Info(format, args);
        }

        public void Info(Exception ex, string format, params object[] args)
        {
            _logger.Info(ex, format, args);
        }

        public void Debug(string format, params object[] args)
        {
            _logger.Debug(format, args);
        }

        public void Debug(Exception ex, string format, params object[] args)
        {
            _logger.Debug(ex, format, args);
        }
    }
}