﻿/// <reference path="../scripts/jquery-1.9.1.js" />
/// <reference path="../scripts/jquery.signalR-1.1.2.js" />
/// <reference path="../scripts/knockout-2.3.0.debug.js" />

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

var ContactViewModel = function (contact) {
    var self = this;
    self.title = contact.Title;
    self.titleUpperCase = self.title.toUpperCase();
    self.primaryAvatar = contact.PrimaryAvatar || '/Content/images/AnonContact.svg';
    self.tags = contact.Tags;
    self.isVisible = ko.observable(true);
    self.filter = function (prefixTest) {
        var isVisible = (self.titleUpperCase.lastIndexOf(prefixTest, 0) === 0);
        self.isVisible(isVisible);
    };
};
var ContactGroup = function (startsWith) {
    var self = this,
        filterText = '';
    self.header = startsWith;
    self.contacts = ko.observableArray();
    self.visibleContacts = ko.computed(function () {
        return ko.utils.arrayFilter(self.contacts(), function (contactVm) {
            return contactVm.isVisible();
        });
    }, this);
    self.isVisible = ko.computed(function () {
        return self.visibleContacts().length > 0;
    });
    self.isValid = function (contact) {
        throw 'This is intended to be an abstract class please do not use';
    };
    self.addContact = function (contact) {
        var vm = new ContactViewModel(contact);
        vm.filter(filterText);
        self.contacts.push(vm);
    };
    self.filter = function (filter) {
        filterText = filter.toUpperCase();
        var contacts = self.contacts();
        for (var i = 0; i < contacts.length; i++) {
            var contactVm = contacts[i];
            contactVm.filter(filterText);
        }
    };
};
var AnyContactGroup = function (header) {
    var self = this;
    ContactGroup.call(self, header);
    self.isValid = function () { return true; };
};
var AlphaContactGroup = function (startsWith) {
    var self = this;
    ContactGroup.call(self, startsWith);
    self.isValid = function (contact) {
        //TODO - there is duplication here and in the nested view model - see if we can extract this or rethink how this should work
        return contact.Title.toUpperCase().lastIndexOf(self.header, 0) === 0;
    };
};


var ContactDefViewModel = function (contactsHub) {
    var self = this;
    self.filterText = ko.observable('');
    self.contactGroups = ko.observableArray();
    self.totalResults = ko.observable(0);
    self.receivedResults = ko.observable(0);
    self.progress = ko.computed(function () {
        return 100 * self.receivedResults() / self.totalResults();
    });
    self.isProcessing = ko.observable(true);

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
            self.contactGroups.push(new AlphaContactGroup(charList[i]));
        }

        self.contactGroups.push(new AnyContactGroup('123'));
    };

    self.addContact = function (contact) {
        var cgsLength = self.contactGroups().length;
        for (var j = 0; j < cgsLength; j++) {
            //TODO - switch this to just a look up? ie use the first char as the key look up? 
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
        //console.log(i);
        i += 1;
        self.receivedResults(i);
    };

    contactsHub.client.ReceivedExpectedCount = function (count) {
        console.log('append to count = ' + count);
        var aggregateCount = self.totalResults() + count;
        console.log('new count = ' + aggregateCount);
        self.totalResults(aggregateCount);
    };
    contactsHub.client.ReceiveContactSummary = function (contact) {
        //console.log('OnNext...');
        self.addContact(contact);
        self.IncrementProgress();
    };
    contactsHub.client.ReceiveError = function (error) {
        console.log(error);
        self.isProcessing(false);
    };
    contactsHub.client.ReceiveComplete = function () {
        console.log('OnComplete');
        var i = self.receivedResults();
        console.log(i);
        self.isProcessing(false);
        $.connection.hub.stop();
    };
};

$(function () {
    // $.connection.contacts =  the generated client-side hub proxy
    model = new ContactDefViewModel($.connection.contacts);
    model.LoadContactGroups();
    createCustomBindings();
    ko.applyBindings(model);
    // Start the connection
    model.StartHub();
});