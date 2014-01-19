(function (callWall) {
    callWall.Db = {};
    var contactDb = new PouchDB('callwall.contacts');
    var providerContactDb = new PouchDB('callwall.providerContacts');
    var persistContact = function (contact) {
        contact._id = contact.Provider + '-' + contact.ProviderId;
        contactDb.put(contact, function (err, result) {
            if (err) {
                console.log('Could not save contact');
                console.log(contact);
                console.log(err);
            }
        });
    };
    var getAllContacts = function (callback) {
        contactDb.allDocs({ include_docs: true }, function (err, response) {
            if (err) {
                console.error('Could not retrieve contacts');
                console.error(err);
            }
            console.log('Persisted Contacts summary :');
            console.log('Contacts count :' + response.total_rows);
            callback(response.rows);
        });
    };
    var getProvidersLastUpdateTimestamps = function (callback) {
        providerContactDb.allDocs({ include_docs: true }, function (err, response) {
            if (err) {
                console.error('Could not retrieve contacts');
                console.error(err);
            }
            console.log('Persisted Contacts summary :');
            console.log('Contacts count :' + response.total_rows);
            var timestamps = $.map(response.rows, function (val) {
                return val.doc;
            });
            callback(timestamps);
        });
    };
    var setProvidersLastUpdateTimestamps = function (timestamps) {
        console.log(timestamps);
        timestamps.forEach(function (timestamp) {
            timestamp._id = timestamp.Provider;
            timestamp._rev = timestamp.Revision;
            console.log(timestamp);
            providerContactDb.put(timestamp, function (err) {
                if (err) {
                    console.error('Could not save last updated timestamp');
                    console.error(timestamp);
                    console.error(err);
                }
            });
        });
    };
    callWall.Db.providerDatabase = providerContactDb;
    callWall.Db.contactsDatabase = contactDb;
    callWall.Db.persistContact = persistContact;
    callWall.Db.getAllContacts = getAllContacts;
    callWall.Db.getProvidersLastUpdateTimestamps = getProvidersLastUpdateTimestamps;
    callWall.Db.setProvidersLastUpdateTimestamps = setProvidersLastUpdateTimestamps;
    callWall.Db.NukeDbs = function () {
        PouchDB.destroy('callwall.contacts');
        PouchDB.destroy('callwall.providerContacts');
    };
    // ReSharper disable ThisInGlobalContext
}(this.callWall = this.callWall || {}));
// ReSharper restore ThisInGlobalContext

(function (callWall) {
    callWall.SignalR = callWall.SignalR || {};

    callWall.SignalR.ContactAdapter = function (contactsHub, model) {
        var self = this;
        self.StartHub = function () {
            //Load existing contacts
            callWall.Db.getAllContacts(function (contactRecords) {
                contactRecords.forEach(function (contactRecord) {
                    model.addContact(contactRecord.doc);
                });
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
                    console.log(ex);
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
            model.addContact(contact);
            model.IncrementProgress();
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