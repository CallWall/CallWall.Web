(function (callWall) {
    callWall.SignalR = callWall.SignalR || {};

    callWall.SignalR.ContactProfileAdapter = function (contactProfileHub, model) {
        var self = this;

        //TODO: Implement RxJs here.
        //https://github.com/Reactive-Extensions/RxJS/blob/master/doc/howdoi/wrap.md
        //Create a SignalRx bridge
        /*
function SignalRx.ObserveHub(hub, subscriptionPayload) {
    if(hub==undefined || hub==null)
        throw 'No hub provided';
    return Rx.Observable.create(function (observer) {
        
        var subscribe = function (payload) {
            $.connection.hub.start()
                .done(function () {
                    console.log('Subscribing...');
                    try {
                        hub.server.Subscribe(payload)
                                  .done(function () { console.log('Subscribed.');})
                                  .fail(function(error){ observer.onError('Failed to subscribe to hub - ' + error);});
                    } catch (ex) {
                        observer.onError('Failed to subscribe to hub - ' + ex);
                    }
                })
                .fail(function(error){ 
                    observer.onError('Failed to connect client to server - ' + error);
                });
        };

        hub.client.ReceivedData = function (data) {
            observer.onNext(data);
        };
        hub.client.ReceiveError = function (error) {
            observer.onError(error);
        };
        hub.client.ReceiveComplete = function () {
            observer.onComplete(error);
        };

        subscribe(subscriptionPayload);
        return function () {
            //Should that be $.connection.hub.stop()?
            hub.stop();
            console.log('Unsubscribed.');
        };
    });
 }*/



        self.StartHub = function (contactKeys) {
            $.connection.hub.start().done(function () {
                console.log('Subscribe');
                try {
                    contactProfileHub.server.subscribe(contactKeys);
                } catch (ex) {
                    console.log(ex);
                }
            });
        };

        contactProfileHub.client.OnNext = function (profile) {
            console.log('contactProfileHub.client.OnNext(..)');
            model.aggregate(profile);
        };

        contactProfileHub.client.OnError = function (error) {
            console.error(error);
            model.isProcessing(false);
        };

        contactProfileHub.client.OnCompleted = function () {
            console.log('OnComplete');
            model.isProcessing(false);
            contactProfileHub.stop();
            //SHould that be $.connection.hub.stop()?
        };
    };
    // ReSharper disable ThisInGlobalContext
}(this.callWall = this.callWall || {}));
// ReSharper restore ThisInGlobalContext