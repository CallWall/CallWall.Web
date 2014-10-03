(function (callWall) {
    callWall.Db = {};
    var contactDb = new PouchDB('callwall.contacts');    
    var persistContactUpdate = function (contactUpdate) {
        var record = translate(contactUpdate);
        contactDb.post(record, function (err, result) {
            if (err) {
                console.error('Could not save contact update:');
                console.error(contactUpdate);
                console.error(record);
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

    var observeChanges = function () {
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
                        model.processUpdate(contactUpdate);
                    });

            //TODO: Should this not be 'contactsHub.start().done...' instead of reaching out to $.connection.hub? -LC
            //check for updates
            $.connection.hub.start().done(function () {
                console.log('Getting server head version');
                contactsHub.server.getHeadVersion().done(function (serverHeadVersion) {
                    console.log("server headVersion : " + serverHeadVersion);
                    model.startingServerVersion(serverHeadVersion);
                });


                console.log('Subscribe');
                try {
                    callWall.Db.getContactsHeadVersion(function (clientHeadVersion) {
                        console.log("client headVersion : " + clientHeadVersion);
                        self.startingClientVersion(clientHeadVersion);
                        contactsHub.server.requestContactSummaryStream(clientHeadVersion);
                    });
                } catch (ex) {
                    console.log("failed on start-up of ContactSummaries Hub");
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

        contactsHub.client.ReceiveContactSummaryUpdate = function (contactUpdate) {
            console.log(contactUpdate);
            callWall.Db.persistContactUpdate(contactUpdate);
        };

        contactsHub.client.ReceiveError = function (error) {
            console.error(error);
            //TODO: Need a better resilience strategy -LC
            model.errorMessage('Sorry we are having connectivity problems');
        };
    };
    // ReSharper disable ThisInGlobalContext
}(this.callWall = this.callWall || {}));
// ReSharper restore ThisInGlobalContext