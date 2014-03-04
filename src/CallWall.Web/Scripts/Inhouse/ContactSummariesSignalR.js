(function (callWall) {
    callWall.SignalR = callWall.SignalR || {};

    callWall.SignalR.ContactSummariesAdapter = function (contactsHub, model) {
        var self = this;
        self.StartHub = function () {
            //Load existing contacts
            callWall.Db.allContacts.subscribe(function(contact) {
                model.addContact(contact);
            });
            
            //check for updates
            $.connection.hub.start().done(function () {
                console.log('Subscribe');
                try {
                    callWall.Db.getProvidersLastUpdateTimestamps(function (timestamps) {
                        console.log("timestamps");
                        console.log(timestamps);
                        var formattedTimestamps = $.map(timestamps, function (dbObject) {
                            return {
                                LastUpdated: dbObject.LastUpdated,
                                Provider: dbObject.Provider,
                                Revision: dbObject._rev
                            };
                        }); 
                        contactsHub.server.requestContactSummaryStream(formattedTimestamps);
                    });
                } catch (ex) {
                    console.log("failed on startup of ContactSummaries Hub");
                    console.log(ex);
                    console.log("Attempting to stop ContactSummaries Hub");
                    try {
                        $.connection.hub.stop();    
                    } catch (ex){
                        console.log("failed to stop ContactSummaries Hub");
                        console.log(ex);
                    }
                }
            });
        };

        contactsHub.client.ReceivedExpectedCount = function (count) {
            console.log('append to count = ' + count);
            var aggregateCount = model.totalResults() + count;
            console.log('new count = ' + aggregateCount);
            model.totalResults(aggregateCount);
        };

        contactsHub.client.ReceiveContactSummary = function (contact) {
            callWall.Db.persistContact(contact);
            model.IncrementProgress();//TODO: Refactor to be part of the db write through -LC
        };

        contactsHub.client.ReceiveError = function (error) {
            console.error(error);
            model.isProcessing(false);
        };

        contactsHub.client.ReceiveComplete = function (completionData) {
            console.log('OnComplete');
            var i = model.receivedResults();
            console.log(i);
            model.isProcessing(false);
            $.connection.hub.stop();
            callWall.Db.setProvidersLastUpdateTimestamps(completionData);
        };
    };
    // ReSharper disable ThisInGlobalContext
}(this.callWall = this.callWall || {}));
// ReSharper restore ThisInGlobalContext