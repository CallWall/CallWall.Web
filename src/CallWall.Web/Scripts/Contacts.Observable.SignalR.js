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
        self.totalResults = ko.observable(0);
        self.receivedResults = ko.observable(0);
        self.progress = ko.computed(function () {
            console.log('returning computed value..');
            return 100 * self.receivedResults() / self.totalResults();
        });

        contactsHub.client.ReceivedExpectedCount = function(count) {
            console.log('count = ' + count);
            self.totalResults(count);
        };
        
        contactsHub.client.ReceiveContactSummary = function (contact) {
            console.log('OnNext');
            self.contacts.push(contact);
            var i = self.receivedResults();
            console.log(i);
            i += 1;
            self.receivedResults(i);
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