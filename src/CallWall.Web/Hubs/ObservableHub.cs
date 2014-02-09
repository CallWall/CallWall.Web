using System;
using System.Reactive.Disposables;
using Microsoft.AspNet.SignalR;

namespace CallWall.Web.Hubs
{
    public abstract class ObservableHub<T> : Hub
    {
        private readonly IObservableHubDataProvider<T> _provider;
        private readonly ILogger _logger;
        private readonly SerialDisposable _contactComunicationSubscription = new SerialDisposable();

        protected ObservableHub(ILoggerFactory loggerFactory, IObservableHubDataProvider<T> provider)
        {
            _provider = provider;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public void Subscribe(string[] contactKeys)
        {
            //TODO: Replace with Rx Logger --https://gist.github.com/LeeCampbell/3817281
            try
            {
                _logger.Debug("{0}.Subscribe({1})", GetType(), string.Join(",", contactKeys));
                var subscription = _provider.GetObservable()
                    .Subscribe(
                        message => Clients.Caller.OnNext(message),
                        ex =>
                        {
                            _logger.Error(ex, "Error in getting data with {0}", GetType());
                            Clients.Caller.OnError("Error in data");
                        },
                        () => Clients.Caller.OnCompleted());
                _contactComunicationSubscription.Disposable = subscription;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "{0} failure", GetType());
                Clients.Caller.OnError("Error in getting data");
            }
        }
    }
}