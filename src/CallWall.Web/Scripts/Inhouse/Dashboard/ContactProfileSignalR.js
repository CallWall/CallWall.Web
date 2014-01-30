(function (callWall) {
    callWall.SignalR = callWall.SignalR || {};

    callWall.SignalR.ContactProfileAdapter = function (contactProfileHub, model) {
        var self = this;
        self.StartHub = function (contactKeys) {
            $.connection.hub.start().done(function () {
                console.log('Subscribe');
                try {
                    contactProfileHub.server.requestContactProfile(contactKeys);
                } catch (ex) {
                    console.log(ex);
                }
            });
        };

        contactProfileHub.client.ReceivedContactProfileDelta = function (profile) {
            console.log('ReceivedContactProfileDelta(..)');
            model.aggregate(profile);
        };

        contactProfileHub.client.ReceiveError = function (error) {
            console.error(error);
            model.isProcessing(false);
        };

        contactProfileHub.client.ReceiveComplete = function () {
            console.log('OnComplete');
            model.isProcessing(false);
            contactProfileHub.stop();
            //SHould that be $.connection.hub.stop()?
        };
    };
    // ReSharper disable ThisInGlobalContext
}(this.callWall = this.callWall || {}));
// ReSharper restore ThisInGlobalContext