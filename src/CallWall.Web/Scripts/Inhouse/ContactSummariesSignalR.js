(function (callWall) {
    callWall.SignalR = callWall.SignalR || {};

    var observeOnScheduler = Rx.Scheduler.timeout;
    callWall.SignalR.ContactSummariesAdapter = function (contactsHub, model) {
        var self = this;
        self.StartHub = function () {
            //Load existing contacts
            callWall.Db.getContactCount
                .observeOn(observeOnScheduler)
                .subscribe(model.IncrementCount);

            callWall.Db.allContacts
                .log("PouchDB Contacts", function(x){return x.Title;})
                .observeOn(observeOnScheduler)
                .subscribe(function (contact) {
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

        contactsHub.client.ReceivedExpectedCount = model.IncrementCount;

        contactsHub.client.ReceiveContactSummary = function (contact) {
            observeOnScheduler.scheduleWithState(contact, function (c) { callWall.Db.persistContact(c); });
        };

        contactsHub.client.ReceiveError = function (error) {
            console.error(error);
            //TODO: Some sort of visual indicator should be shown to the user to indicate an error -LC
            //TODO: Some sort of retry or resilience should be put in place here -LC
            //TODO: Some how we need know now how to hide the progress bar at some point -LC
            //model.isProcessing(false);
            $.connection.hub.stop();
        };

        contactsHub.client.ReceiveComplete = function (completionData) {
            console.log('contactsHub.client.OnComplete()');
            $.connection.hub.stop();
            callWall.Db.setProvidersLastUpdateTimestamps(completionData);
        };
    };
    // ReSharper disable ThisInGlobalContext
}(this.callWall = this.callWall || {}));
// ReSharper restore ThisInGlobalContext