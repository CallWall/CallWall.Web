(function (callWall) {
    callWall.Db = {};
    var db = new PouchDB('contacts');
    var persistContact = function (contact) {
        db.put(contact, function (err, result) {
            console.log('result');
            console.log(result);
            if (err) {
                console.log('Could not save contact');
                console.log(contact);
                console.log(err);
            }
        });
    };
    var getAllContacts = function (callback) {
        db.allDocs({ include_docs: true }, function (err, response) {
            if (err) {
                console.log('Could not retrieve contacts');
                console.log(err);
            }
            console.log('Persisted Contacts summary :');
            console.log('Contacts count :' + response.total_rows);
            callback(response.rows);
        });
    };

    callWall.Db.contactsDatabase = db;
    callWall.Db.persistContact = persistContact;
    callWall.Db.getAllContacts = getAllContacts;
// ReSharper disable ThisInGlobalContext
}(this.callWall = this.callWall || {}));
// ReSharper restore ThisInGlobalContext

(function (callWall) {
    callWall.SignalR = callWall.SignalR || {};

    //want to get contacts from the persistent store (1st 'observable')
    //want to only get (via signalR) deltas of the contacts (2nd observable)
    //want to persist contacts as they come in if they are new or update if they are changes (will need an appropriate Id to do so)


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
            callWall.Db.persistContact(contact);
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