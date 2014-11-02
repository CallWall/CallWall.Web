/// <reference path="jquery.signalR.version.js" />
/// <reference path="rx.js" />

Rx.Observable.prototype.log = function (sourceName, valueSelector) {
    var source = this;

    return Rx.Observable.create(function(observer) {
        console.log(sourceName + '.Subscribe()');

        var disposal = Rx.Disposable.create(function() {
            console.log(sourceName + '.Dispose()');
        });

        var subscription = source.do(
                //function(x) { console.log(sourceName + '.onNext(' + valueSelector(x) + ')'); },   //Turn on if required, but this can hammer the logs. Maybe check if the value selector is provided? -LC
                function(x) { console.log('%s.OnNext(%O)', sourceName, x); },   //Turn on if required, but this can hammer the logs. Maybe check if the value selector is provided? -LC
                function(err) { console.error(sourceName + '.onError(' + err + ')'); },
                function() { console.log(sourceName + '.onCompleted()'); }
            )
            .subscribe(observer);

        return new Rx.CompositeDisposable(disposal, subscription);
    });
};
//TODO: Enable intellisense for VS IDE. http://msdn.microsoft.com/en-us/library/bb385682.aspx
(function ($, Rx, SignalRx) {
   
    var availableHubNames = function () {
        var result = '';
        for (var key in $.connection.hub.proxies) {
            if (result != '') result = result + ', ';
            result = result + key;
        }
        return '[' + result + ']';
    };

    ///
    SignalRx.ObserveHub = function (hub, subscriptionPayload) {
        /// <summary>Subscribes to a SignalRx compliant Hub. Creates the SignalR connection if required.</summary>
        /// <param name="hub" mayBeNull="false">The SignalR hub to subscribe to.</param>
        /// <param name="subscriptionPayload" mayBeNull="false">The payload to send to the SignalR hub's subscribe method.</param>

        if (hub == undefined)
            throw 'No hub provided. Available hubs are ' + availableHubNames();


        //TODO: Investigate if I can convert these promises (hub.start(), server.subscribe()) to Observable with Rx.Observable.fromPromise and then just selectMany them? -LC
        return Rx.Observable.create(function (observer) {
            var subscribe = function (payload) {
                console.log('Creating SignalR connection');

                $.connection.hub.start()
                    .done(function () {
                        console.log('Subscribing to hub [' + hub.hubName + ']...');
                        try {
                            hub.server.subscribe(payload)
                                .done(function() {
                                     console.log('Subscribed to hub [' + hub.hubName + '].');
                                })
                                .fail(function (error) {
                                    console.log('FAIL - Failed to subscribe to hub [' + hub.hubName + '] - ' + error);
                                    observer.onError('Failed to subscribe to hub [' + hub.hubName + '] - ' + error);
                                });
                        } catch (ex) {
                            console.log('CATCH - Failed to subscribe to hub [' + hub.hubName + '] - ' + ex);
                            observer.onError('Failed to subscribe to hub [' + hub.hubName + '] - ' + ex);
                        }
                    })
                    .fail(function (error) {
                        console.log('Failed to connect client to server - ' + error);
                        observer.onError('Failed to connect client to server - ' + error);
                    });
                console.log('END Creating SignalR connection');
            };

            hub.client.OnNext = function (data) {
                observer.onNext(data);
            };
            hub.client.OnError = function (error) {
                observer.onError(error);
            };
            hub.client.OnCompleted = function () {
                observer.onCompleted();
            };

            subscribe(subscriptionPayload);
            return function () {
                console.log('Unsubscribed from hub [' + hub.hubName + '].');
            };
        });
    };
// ReSharper disable ThisInGlobalContext
}(jQuery, Rx, this.SignalRx = this.SignalRx || {}));
// ReSharper restore ThisInGlobalContext