﻿/// <reference path="../scripts/jquery-1.9.1.js" />
/// <reference path="../scripts/jquery.signalR-1.1.2.js" />
/// <reference path="../scripts/knockout-2.2.0.debug.js" />

//TODO: provide internal group sorting
//TODO: provide search/filter while contacts still being loaded

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

var contactViewModel = function (contact) {
    var self = this;
    self.title = contact.Title;
    self.primaryAvatar = contact.PrimaryAvatar;
    self.tags = contact.Tags;
    self.isVisible = ko.observable(true);
};
var anyContactGroup = function (header) {
    var self = this;
    self.header = header;
    self.contacts = ko.observableArray();
    self.isVisible = ko.observable(true);
    self.isValid = function (contact) { return true; };
    self.addContact = function (contact) {
        var vm = new contactViewModel(contact);
        self.contacts.push(vm);
    };
    self.filter = function (filterText) {
        var prefixTest = filterText.toUpperCase();
        var contacts = self.contacts();
        for (var i = 0; i < contacts.length; i++) {
            var contact = contacts[i];
            var isVisible = (contact.title.toUpperCase().lastIndexOf(prefixTest, 0) === 0);
            contact.isVisible(isVisible);
        }
    };
};
var alphaContactGroup = function (startsWith) {
    var self = this;
    self.header = startsWith;
    self.contacts = ko.observableArray();
    self.isVisible = ko.observable(true);
    self.isValid = function (contact) {
        return contact.Title.toUpperCase().lastIndexOf(self.header, 0) === 0;
    };
    self.addContact = function (contact) {
        var vm = new contactViewModel(contact);
        self.contacts.push(vm);
    };
    self.filter = function(filterText) {
        var prefixTest = filterText.toUpperCase();
        var contacts = self.contacts();
        for (var i = 0; i < contacts.length; i++) {
            var contact = contacts[i];
            var isVisible = (contact.title.toUpperCase().lastIndexOf(prefixTest, 0) === 0);
            contact.isVisible(isVisible);
        }
    };
};


var contactDefViewModel = function (contactsHub) {
    var self = this;
    self.filterText = ko.observable('');
    self.contactGroups = ko.observableArray();
    self.totalResults = ko.observable(0);
    self.receivedResults = ko.observable(0);
    self.progress = ko.computed(function () {
        return 100 * self.receivedResults() / self.totalResults();
    });

    var filterTextChangeSubscription = self.filterText.subscribe(function (newFilterText) {
        var cgs = self.contactGroups();
        for (var i = 0; i < cgs.length; i++) {
            cgs[i].filter(newFilterText);
        }
    });

    self.LoadContactGroups = function () {
        var charList = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        console.log(charList);
        for (var i = 0; i < charList.length; i++) {
            var h = charList[i];
            console.log('loading ' + h);
            self.contactGroups.push(new alphaContactGroup(charList[i]));
        }

        self.contactGroups.push(new anyContactGroup('123'));
    };

    self.addContact = function (contact) {
        var cgsLength = self.contactGroups().length;
        for (var j = 0; j < cgsLength; j++) {
            var cg = self.contactGroups()[j];
            if (cg.isValid(contact)) {
                cg.addContact(contact);
                break;
            }
        }
    };
    
    self.StartHub = function () {
        $.connection.hub.start().done(function () {
            console.log('Subscribe');
            contactsHub.server.requestContactSummaryStream();
        });
    };

    self.IncrementProgress = function () {
        var i = self.receivedResults();
        console.log(i);
        i += 1;
        self.receivedResults(i);
    };

    contactsHub.client.ReceivedExpectedCount = function (count) {
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
};

$(function () {
    // $.connection.contacts =  the generated client-side hub proxy
    model = new contactDefViewModel($.connection.contacts);
    model.LoadContactGroups();
    createCustomBindings();
    ko.applyBindings(model);
    // Start the connection
    model.StartHub();
});