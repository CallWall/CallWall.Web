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
    var anyContactGroup = function(header) {
        var self = this;
        self.header = header;
        self.isValid = function (contact) { return true; };
        self.contacts = ko.observableArray();
    };
    var alphaContactGroup = function (startsWith) {
        var self = this;
        self.header = startsWith;
        self.isValid = function (contact) {
            return contact.Title.toUpperCase().lastIndexOf(self.header, 0) === 0;
        };
        self.contacts = ko.observableArray();
    };
    var contactDefViewModel = function () {
        var self = this;
        var contactsHub = $.connection.contacts; // the generated client-side hub proxy
        self.contactGroups = ko.observableArray();
        self.totalResults = ko.observable(0);
        self.receivedResults = ko.observable(0);
        self.progress = ko.computed(function () {
            return 100 * self.receivedResults() / self.totalResults();
        });

        self.LoadContactGroups = function() {
            var charList = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            console.log(charList);
            for (var i = 0; i < charList.length; i++) {
                var h = charList[i];
                console.log('loading ' + h);
                self.contactGroups.push(new alphaContactGroup(charList[i]));
            }
            
            self.contactGroups.push(new anyContactGroup('123'));
        };

        //TODO: provide internal group sorting

        self.addContact = function (contact) {
            var cgsLength = self.contactGroups().length;
            for (var j = 0; j < cgsLength; j++) {
                var cg = self.contactGroups()[j];
                if (cg.isValid(contact)) {
                    cg.contacts.push(contact);
                    break;
                }
            }
        };

        contactsHub.client.ReceivedExpectedCount = function(count) {
            console.log('count = ' + count);
            self.totalResults(count);
        };
        
        contactsHub.client.ReceiveContactSummary = function (contact) {
            console.log('OnNext...');
            self.addContact(contact);
            self.IncrementProgress();
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

        self.IncrementProgress = function() {
            var i = self.receivedResults();
            console.log(i);
            i += 1;
            self.receivedResults(i);
        };
    };
    

    model = new contactDefViewModel();
    model.LoadContactGroups();
    createCustomBindings();
    ko.applyBindings(model);
    // Start the connection
    model.StartHub();
});