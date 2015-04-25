using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace CallWall.Web.EventStore.Tests.Doubles
{
    [Obsolete("Use the EmbeddedEventStoreConnectionFactory instead.")]
    public sealed class InMemoryEventStoreConnectionFactory : IEventStoreConnectionFactory, IDisposable
    {
        private readonly IPEndPoint _ipEndPoint;
        private readonly Process _eventStoreProcess;

        public InMemoryEventStoreConnectionFactory()
            : this(@"..\..\..\..\Server\EventStore\EventStore.ClusterNode.exe", "127.0.0.1", 1113)
        { }
        public InMemoryEventStoreConnectionFactory(string eventStorePath, string ipAddress, int port)
        {
            _eventStoreProcess = Process.Start(eventStorePath, "--mem-db --skip-db-verify --run-projections=ALL");
            var ip = IPAddress.Parse(ipAddress);
            _ipEndPoint = new IPEndPoint(ip, port);

            Trace.Write("Waiting for process to warm up..");
            for (int i = 0; i < 3; i++)
            {
                Thread.Sleep(1000);
                Trace.Write(".");
            }
            Trace.WriteLine("Done (hopefully).");
        }

        public async Task<IEventStoreConnection> Connect()
        {
            var conn = EventStoreConnection.Create(_ipEndPoint);
            await conn.ConnectAsync();
            return conn;
        }

        public void Dispose()
        {
            _eventStoreProcess.Kill();
            _eventStoreProcess.Dispose();
            Trace.WriteLine("ConnectionFactory Disposed");
        }
    }
}