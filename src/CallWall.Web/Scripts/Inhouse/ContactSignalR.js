(function (callWall) {
    callWall.SignalR = callWall.SignalR || {};

    callWall.SignalR.ContactAdapter = function (contactsHub, model) {
        var self = this;
        self.StartHub = function () {
            $.connection.hub.start().done(function () {
                console.log('Subscribe');
                contactsHub.server.requestContactSummaryStream();
            });
        };

        contactsHub.client.ReceivedExpectedCount = function (count) {
            console.log('append to count = ' + count);
            var aggregateCount = model.totalResults() + count;
            console.log('new count = ' + aggregateCount);
            model.totalResults(aggregateCount);
        };

        contactsHub.client.ReceiveContactSummary = function (contact) {
            model.addContact(contact);
            model.IncrementProgress();
        };

        contactsHub.client.ReceiveError = function (error) {
            console.log(error);
            model.isProcessing(false);
        };

        contactsHub.client.ReceiveComplete = function () {
            console.log('OnComplete');
            var i = model.receivedResults();
            console.log(i);
            model.isProcessing(false);
            $.connection.hub.stop();
        };
    };
// ReSharper disable ThisInGlobalContext
}(this.callWall = this.callWall || {}));
// ReSharper restore ThisInGlobalContext