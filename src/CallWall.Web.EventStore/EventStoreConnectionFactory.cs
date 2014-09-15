using System;
using System.Net;
using CallWall.Web.EventStore.Configuration;
using EventStore.ClientAPI;

namespace CallWall.Web.EventStore
{
    public interface IEventStoreConnectionFactory
    {
        IEventStoreConnection Connect();
    }

    public sealed class EventStoreConnectionFactory : IEventStoreConnectionFactory
    {
        private static readonly Lazy<CallWallEventStoreSection> Config = new Lazy<CallWallEventStoreSection>(CallWallEventStoreSection.GetConfig);
        private static readonly Lazy<IPAddress> ConfiguredIpAddress = new Lazy<IPAddress>(LoadIpAddressFromConfig);
        private static readonly Lazy<int> ConfiguredPort = new Lazy<int>(LoadPortFromConfig); 

        //TODO: Opportunity to share a single connection. Check for best practices. -LC
        public IEventStoreConnection Connect()
        {
            var connectionSettings = ConnectionSettings.Create();
            var endPoint = new IPEndPoint(ConfiguredIpAddress.Value, ConfiguredPort.Value);
            var conn =  EventStoreConnection.Create(connectionSettings, endPoint);
            conn.Connect();
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
}