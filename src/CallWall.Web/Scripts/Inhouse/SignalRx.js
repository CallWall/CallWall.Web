/// <reference path="jquery.signalR.version.js" />
(function (SignalRx) {
    SignalRx = SignalRx || {};

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

            var subscribe = function (payload) {
                console.log('Creating SignalR connection');

                $.connection.hub.start()
                    .done(function () {
                        console.log('Subscribing to hub [' + hub.hubName + ']...');
                        try {
                            hub.server.subscribe(payload)
                                .done(function () { console.log('Subscribed to hub [' + hub.hubName + '].'); })
                                .fail(function (error) {
                                    observer.onError('Failed to subscribe to hub [' + hub.hubName + '] - ' + error);
                                });
                        } catch (ex) {
                            observer.onError('Failed to subscribe to hub [' + hub.hubName + '] - ' + ex);
                        }
                    })
                    .fail(function (error) {
                        observer.onError('Failed to connect client to server - ' + error);
                    });
            };

            hub.client.OnNext = function (data) {

                observer.onNext(data);
            };
            hub.client.OnError = function (error) {
                observer.onError(error);
                $.connection.hub.stop();
            };
            hub.client.OnCompleted = function () {
                observer.onCompleted();
                $.connection.hub.stop();
            };

            subscribe(subscriptionPayload);

            return function () {
                $.connection.hub.stop();
                console.log('Unsubscribed from hub [' + hub.hubName + '].');
            };
        });
    };
}(this.SignalRx = this.SignalRx || {}));