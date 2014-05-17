using System;
using System.Net;
using CallWall.Web.EventStore.Configuration;
using EventStore.ClientAPI;

namespace CallWall.Web.EventStore
{
    public interface IEventStoreConnectionFactory
    {
        IEventStoreConnection CreateConnection();
    }

    public sealed class EventStoreConnectionFactory : IEventStoreConnectionFactory
    {
        private static readonly Lazy<CallWallEventStoreSection> Config = new Lazy<CallWallEventStoreSection>(CallWallEventStoreSection.GetConfig);
        private static readonly Lazy<IPAddress> ConfiguredIpAddress = new Lazy<IPAddress>(LoadIpAddressFromConfig);
        private static readonly Lazy<int> ConfiguredPort = new Lazy<int>(LoadPortFromConfig); 

        public IEventStoreConnection CreateConnection()
        {
            var connectionSettings = ConnectionSettings.Create();
            var endPoint = new IPEndPoint(ConfiguredIpAddress.Value, ConfiguredPort.Value);
            return EventStoreConnection.Create(connectionSettings, endPoint);
        }

        private static IPAddress LoadIpAddressFromConfig()
        {
            return Config.Value.Endpoint;

            //return new IPAddress(new byte[] {127, 0, 0, 1});
        }

        private static int LoadPortFromConfig()
        {
            return Config.Value.Port;
            
            //return 2113;
        }
    }
}