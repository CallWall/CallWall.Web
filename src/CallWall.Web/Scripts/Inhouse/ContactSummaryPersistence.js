/// <reference path="rx.js" />
(function (callWall) {
    callWall.Db = {};
    var contactDb = new PouchDB('callwall.contacts');
    var providerContactDb = new PouchDB('callwall.providerContacts');

    var allContacts = Rx.Observable.create(function (observer) {
        var changes = contactDb.changes({
            since: 0,
            continuous: true,
            include_docs: true,
            onChange: function (change) {
                observer.onNext(change.doc);
            }
        });

        return Rx.Disposable.create(function () { changes.cancel(); });
    });

    var persistContact = function (contact) {
        contact._id = contact.Title + '-' + contact.Provider + '-' + contact.ProviderId;
        contactDb.put(contact, function (err, result) {
            if (err) {
                console.log('Could not save contact');
                //console.log(contact);
                //console.error(err);
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
    callWall.Db.allContacts = allContacts;
    callWall.Db.getProvidersLastUpdateTimestamps = getProvidersLastUpdateTimestamps;
    callWall.Db.setProvidersLastUpdateTimestamps = setProvidersLastUpdateTimestamps;
    callWall.Db.NukeDbs = function () {
        PouchDB.destroy('callwall.contacts');
        PouchDB.destroy('callwall.providerContacts');
    };
    // ReSharper disable ThisInGlobalContext
}(this.callWall = this.callWall || {}));
// ReSharper restore ThisInGlobalContext