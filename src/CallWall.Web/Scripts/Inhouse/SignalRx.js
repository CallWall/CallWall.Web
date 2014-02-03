//https://github.com/Reactive-Extensions/RxJS/blob/master/doc/howdoi/wrap.md
//Create a SignalRx bridge

/// <reference path="jquery.signalR.version.js" />
(function (callWall) {
    callWall.SignalRx = callWall.SignalRx || {};
    callWall.SignalRx.ObserveHub = function (hub, subscriptionPayload) {
        if (hub == undefined)
            throw 'No hub provided';
        return Rx.Observable.create(function(observer) {

            var subscribe = function(payload) {
                $.connection.hub.start()
                    .done(function() {
                        console.log('Subscribing...');
                        try {
                            hub.server.Subscribe(payload)
                                .done(function() { console.log('Subscribed.'); })
                                .fail(function(error) { observer.onError('Failed to subscribe to hub - ' + error); });
                        } catch (ex) {
                            observer.onError('Failed to subscribe to hub - ' + ex);
                        }
                    })
                    .fail(function(error) {
                        observer.onError('Failed to connect client to server - ' + error);
                    });
            };

            hub.client.ReceivedData = function(data) {
                observer.onNext(data);
            };
            hub.client.ReceiveError = function(error) {
                observer.onError(error);
            };
            hub.client.ReceiveComplete = function() {
                observer.onComplete(error);
            };

            subscribe(subscriptionPayload);
            return function() {
                //Should that be $.connection.hub.stop()?
                hub.stop();
                console.log('Unsubscribed.');
            };
        });
    };

// ReSharper disable ThisInGlobalContext
}(this.callWall = this.callWall || {}));
// ReSharper restore ThisInGlobalContext
