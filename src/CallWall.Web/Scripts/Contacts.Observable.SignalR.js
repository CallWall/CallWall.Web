/// <reference path="../scripts/jquery-1.9.1.js" />
/// <reference path="../scripts/jquery.signalR-1.1.2.js" />
/// <reference path="../scripts/knockout-2.2.0.debug.js" />

$(function () {
    var contactDefViewModel = function () {
        var self = this, contactsHub = $.connection.contacts; // the generated client-side hub proxy
        self.contacts = ko.observableArray();
        contactsHub.client.ReceiveContactSummary = function (contact) {
            self.contacts.push(contact);
        };
        self.StartHub = function () {
            $.connection.hub.start().done(function () {
                console.log('Started');
                contactsHub.server.requestContactSummaryStream();
            });
        };
    };

    model = new contactDefViewModel();
    ko.applyBindings(model);
    // Start the connection
    model.StartHub();
});