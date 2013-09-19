/// <reference path="../scripts/jquery-1.9.1.js" />
/// <reference path="../scripts/jquery.signalR-1.1.2.js" />
/// <reference path="../scripts/knockout-2.2.0.debug.js" />
function createCustomBindings() {
    //Custom binding to allow ko to update JqueryUI progressbar
    //http://www.piotrwalat.net/using-jquery-ui-progress-bar-with-mvvm-knockout-and-web-workers/
    //http://knockoutjs.com/documentation/custom-bindings.html
    ko.bindingHandlers.progress = {
        init: function (element, valueAccessor) {
            var progressValue = ko.unwrap(valueAccessor());
            $(element).progressbar({
                value: progressValue
            });
        },
        update: function (element, valueAccessor) {
            var progressValue = ko.unwrap(valueAccessor());
            $(element).progressbar("value", progressValue);
        }
    };

}

$(function () {
    var contactDefViewModel = function () {
        var self = this, contactsHub = $.connection.contacts; // the generated client-side hub proxy
        self.contacts = ko.observableArray();
        self.progress = ko.observable(0);
        contactsHub.client.ReceiveContactSummary = function (contact) {
            console.log('OnNext');
            self.contacts.push(contact);
            var i = self.progress();
            i += 10;
            self.progress(i);
        };
        contactsHub.client.ReceiveError = function (error) {
            console.log(error);
        };
        contactsHub.client.ReceiveComplete = function () {
            console.log('OnComplete');
            $.connection.hub.stop();
        };
        self.StartHub = function () {
            $.connection.hub.start().done(function () {
                console.log('Subscribe');
                contactsHub.server.requestContactSummaryStream();
            });
        };
    };
    

    model = new contactDefViewModel();
    createCustomBindings();
    ko.applyBindings(model);
    // Start the connection
    model.StartHub();
});