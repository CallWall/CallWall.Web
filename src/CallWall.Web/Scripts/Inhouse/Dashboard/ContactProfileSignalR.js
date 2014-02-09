/// <reference path="~/Scripts/Inhouse/SignalRx.js" />
(function (callWall) {
    callWall.SignalR = callWall.SignalR || {};

    callWall.SignalR.ContactProfileAdapter = function (contactProfileHub, model) {
        var self = this;
        self.contactProfileHub = contactProfileHub;
        self.subscription = null;
        self.StartHub = function (contactKeys) {
            self.subscription = SignalRx
                .ObserveHub(self.contactProfileHub, contactKeys)
                //.log('contactProfileHub', function (data) { return data.title; })
                .subscribe(
                    function(profile) { model.aggregate(profile); },
                    function (error) {
                         console.log(error);
                         model.isProcessing(false);
                    },
                    function() { model.isProcessing(false); });
        };
        self.CloseHub = function() {
            if(self.subscription)
                self.subscription.dispose();
        };
    };
    callWall.SignalR.ContactCommunicationAdapter = function (contactCommunicationHub, model) {
        var self = this;
        self.contactCommunicationHub = contactCommunicationHub;
        self.subscription = null;
        self.StartHub = function (contactKeys) {
            self.subscription = SignalRx
                .ObserveHub(self.contactCommunicationHub, contactKeys)
                .log('contactCommunicationHub', function (data) { return data.Subject; })
                .subscribe(
                    function (message) { model.add(message); },
                    function (error) {
                         console.log(error);
                         model.isProcessing(false);
                    },
                    function() { model.isProcessing(false); });
        };
        self.CloseHub = function() {
            if(self.subscription)
                self.subscription.dispose();
        };
    };
    callWall.SignalR.ContactCalendarAdapter = function (contactCalendarEventsHub, model) {
        var self = this;
        self.contactCommunicationHub = contactCalendarEventsHub;
        self.subscription = null;
        self.StartHub = function (contactKeys) {
            self.subscription = SignalRx
                .ObserveHub(self.contactCommunicationHub, contactKeys)
                .log('contactCalendarEventsHub', function (data) { return data.Subject; })
                .subscribe(
                    function (message) { model.add(message); },
                    function (error) {
                         console.log(error);
                         model.isProcessing(false);
                    },
                    function() { model.isProcessing(false); });
        };
        self.CloseHub = function() {
            if(self.subscription)
                self.subscription.dispose();
        };
    };
    callWall.SignalR.ContactGalleryAlbumAdapter = function (contactGalleryAlbumHub, model) {
        var self = this;
        self.contactGalleryAlbumHub = contactGalleryAlbumHub;
        self.subscription = null;
        self.StartHub = function (contactKeys) {
            self.subscription = SignalRx
                .ObserveHub(self.contactGalleryAlbumHub, contactKeys)
                .log('contactGalleryAlbumHub', function (data) { return data.Subject; })
                .subscribe(
                    function (album) { model.add(album); },
                    function (error) {
                        console.log(error);
                        model.isProcessing(false);
                    },
                    function () { model.isProcessing(false); });
        };
        self.CloseHub = function () {
            if (self.subscription)
                self.subscription.dispose();
        };
    };
    callWall.SignalR.ContactCollaborationAdapter = function (contactCollaborationHub, model) {
        var self = this;
        self.contactCollaborationHub = contactCollaborationHub;
        self.subscription = null;
        self.StartHub = function (contactKeys) {
            self.subscription = SignalRx
                .ObserveHub(self.contactCollaborationHub, contactKeys)
                .log('contactCollaborationHub', function (data) { return data.Subject; })
                .subscribe(
                    function (collaboration) { model.add(collaboration); },
                    function (error) {
                        console.log(error);
                        model.isProcessing(false);
                    },
                    function () { model.isProcessing(false); });
        };
        self.CloseHub = function () {
            if (self.subscription)
                self.subscription.dispose();
        };
    };
// ReSharper disable ThisInGlobalContext
}(this.callWall = this.callWall || {}));
// ReSharper restore ThisInGlobalContext