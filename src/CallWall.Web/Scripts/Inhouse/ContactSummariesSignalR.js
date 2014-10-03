(function (callWall) {
    callWall.Db = {};
    var contactDb = new PouchDB('callwall.contacts');    
    var persistContactUpdate = function (contactUpdate) {
        var record = translate(contactUpdate);
        //console.log("persisting update:");
        //console.log(record);
        contactDb.post(record, function (err, result) {
            if (err) {
                console.log('Could not save contact update');
                console.log(contactUpdate);
                console.log(record);
                console.error(err);
            }
        });
    };

    var translate = function(dto) {
        if (dto.isDeleted) {
            return {
                //_id: dto.Id.toString(),
                //_rev: dto.Version.toString(),
                isDeleted: true
            };
        } else {
            return {
                //_id: dto.Id.toString(),
                //_rev: dto.Version.toString(),
                newTitle: dto.newTitle
                /*addedTags: dto.AddedTags,
                removedTags: dto.RemovedTags,
                addedAvatars: dto.AddedAvatars,
                removedAvatars : dto.RemovedAvatars,
                addedProviders : dto.AddedProviders,
                removedProviders: dto.RemovedProviders*/
            };
            }
    };

    //var getAllContacts = function (callback) {
    //    contactDb.allDocs({ include_docs: true }, function (err, response) {
    //        if (err) {
    //            console.error('Could not retrieve contacts');
    //            console.error(err);
    //        }
    //        console.log('Persisted Contacts summary :');
    //        console.log('Contacts count :' + response.total_rows);
    //        callback(response.rows);
    //    });
    //};
    var observeChanges = function () {
        console.log(contactDb);
        //console.log(contactDb.observeChanges());
        //return contactDb.observeChanges();

        return Rx.Observable.createWithDisposable(function (o) {

            var query = contactDb.info(function (infoError, info) {
                if (infoError != null) {
                    o.onError(infoError);
                } else {
                    contactDb.changes({
                        since: 0,
                        live: true,
                        include_docs: true,
                    }).on('change', function (change) {
                        o.onNext(change.doc);
                    }).on('error', function (err) {
                        console.error('Error listening to contactsDb changes');
                console.error(err);
                        o.onError(err);
                    });
                }
            });

            return function () { query.cancel(); };
        });
    };
    var getHeadVersion = function (callback) {
        contactDb.info(function (err, response) {
            if (err) {
                console.error('Could not retrieve head version');
                console.error(err);
            }
            console.log('contactDb info :');
            console.log(response);
            var headVersion = response.update_seq;
            callback(headVersion);
        });
    };

    //TODO: remove this from the API surface area?
    callWall.Db.contactsDatabase = contactDb;
    callWall.Db.persistContactUpdate = persistContactUpdate;
    callWall.Db.observeChanges = observeChanges;
    callWall.Db.getContactsHeadVersion = getHeadVersion;    
    callWall.Db.NukeDbs = function () {
        PouchDB.destroy('callwall.contacts');
        PouchDB.destroy('callwall.providerContacts');
    };
    // ReSharper disable ThisInGlobalContext
}(this.callWall = this.callWall || {}));
// ReSharper restore ThisInGlobalContext

(function (callWall) {
    callWall.SignalR = callWall.SignalR || {};

    //TODO: Rename to ContactSummariesAdapter -LC
    callWall.SignalR.ContactAdapter = function (contactsHub, model) {
        var self = this;
        self.StartHub = function () {
            callWall.Db.observeChanges()
                .subscribe(
                    function (contactUpdate) {
                        console.log('Got contactUpdate from db');
                        model.processUpdate(contactUpdate);
                });

            //TODO: Should this not be 'contactsHub.start().done...' instead of reaching out to $.connection.hub? -LC
            //check for updates
            $.connection.hub.start().done(function () {
                console.log('Subscribe');
                try {
                    callWall.Db.getContactsHeadVersion(function (headVersion) {
                        console.log("headVersion : " + headVersion);
                        contactsHub.server.requestContactSummaryStream(headVersion);
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

        //contactsHub.client.ReceivedExpectedCount = function (count) {
        //    console.log('append to count = ' + count);
        //    var aggregateCount = model.totalResults() + count;
        //    console.log('new count = ' + aggregateCount);
        //    model.totalResults(aggregateCount);
        //};

        contactsHub.client.ReceiveContactSummaryUpdate = function (contactUpdate) {
            console.log(contactUpdate);
            callWall.Db.persistContactUpdate(contactUpdate);
            //model.addContact(contact);
            //model.IncrementProgress();
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