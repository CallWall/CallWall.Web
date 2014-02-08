/// <reference path="jquery.signalR.version.js" />
(function ($, Rx, SignalRx) {
   
    var availableHubNames = function () {
        var result = '';
        for (var key in $.connection.hub.proxies) {
            if (result != '') result = result + ', ';
            result = result + key;
        }
        return '[' + result + ']';
    };

    SignalRx.ObserveHub = function (hub, subscriptionPayload) {
        if (hub == undefined)
            throw 'No hub provided. Available hubs are ' + availableHubNames();

        return Rx.Observable.create(function (observer) {
            console.log('Creating ObserveHub');
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
                console.log('[' + hub.hubName + '].OnNext');
                console.log(data);
                observer.onNext(data);
            };
            hub.client.OnError = function (error) {
                console.log('[' + hub.hubName + '].OnError');
                console.log(error);
                observer.onError(error);
                $.connection.hub.stop();
            };
            hub.client.OnCompleted = function () {
                console.log('[' + hub.hubName + '].OnCompleted');
                observer.onCompleted();
                //$.connection.hub.stop();
            };

            subscribe(subscriptionPayload);
            console.log('END Creating ObserveHub');
            return function () {
                console.log('[' + hub.hubName + '] subscription being disposed');
                //$.connection.hub.stop();
                console.log('Unsubscribed from hub [' + hub.hubName + '].');
            };
        });
    };
// ReSharper disable ThisInGlobalContext
}(jQuery, Rx, this.SignalRx = this.SignalRx || {}));
// ReSharper restore ThisInGlobalContext