using System;
using System.Diagnostics;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Embedded;
using EventStore.Core;
using EventStore.Core.Data;

namespace CallWall.Web.EventStore.Tests.Doubles
{
    public sealed class EmbeddedEventStoreConnectionFactory : IEventStoreConnectionFactory, IDisposable
    {
        private readonly SingleAssignmentDisposable _statusSubscription = new SingleAssignmentDisposable();
        private readonly IConnectableObservable<VNodeState> _status;
        private readonly ClusterVNode _vnode;
        private readonly IEventStoreConnection _conn;
        private int _hasConnectionBeenRequested = 0;

        public EmbeddedEventStoreConnectionFactory()
        {
            var noIp = new IPEndPoint(IPAddress.None, 0);

            _vnode = EmbeddedVNodeBuilder.AsSingleNode()
                .RunInMemory()
                .WithExternalTcpOn(noIp)
                .WithInternalTcpOn(noIp)
                .WithExternalHttpOn(noIp)
                .WithInternalHttpOn(noIp)
                .Build();

            _status = Observable.FromEventPattern<VNodeStatusChangeArgs>(
                h => _vnode.NodeStatusChanged += h,
                h => _vnode.NodeStatusChanged -= h)
                .Select(e => e.EventArgs.NewVNodeState)
                .Log(new ConsoleLogger(), "VNode.State")
                .Publish(VNodeState.Unknown);

            _statusSubscription.Disposable = _status.Connect();

            _vnode.Start();
            _conn = EmbeddedEventStoreConnection.Create(_vnode);
        }

        public async Task<IEventStoreConnection> Connect()
        {
            await _status.Where(IsRunning)
                .Take(1)
                .ToTask();

            //If this is the first connection request, then make the connection.
            if (Interlocked.CompareExchange(ref _hasConnectionBeenRequested, 1, 0) == 0)
            {
                Trace.WriteLine("Connecting...");
                await _conn.ConnectAsync();
                Trace.WriteLine("Connected.");
            }
            return _conn;
        }

        private static bool IsRunning(VNodeState vNodeState)
        {
            switch (vNodeState)
            {
                case VNodeState.PreReplica:
                case VNodeState.CatchingUp:
                case VNodeState.Clone:
                case VNodeState.Slave:
                case VNodeState.PreMaster:
                case VNodeState.Master:
                case VNodeState.Manager:
                    return true;
                case VNodeState.Initializing:
                case VNodeState.Unknown:
                case VNodeState.ShuttingDown:
                case VNodeState.Shutdown:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException("vNodeState");
            }
        }

        public void Dispose()
        {
            _statusSubscription.Dispose();
            _vnode.Stop();
            Trace.WriteLine("EmbeddedEventStoreConnectionFactory Disposed");
        }
    }
}