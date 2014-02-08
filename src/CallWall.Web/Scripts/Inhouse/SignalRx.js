/// <reference path="jquery.signalR.version.js" />
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