﻿//TODO: provide internal group sorting
//TODO: provide search/filter while contacts still being loaded

(function (ko, callWall) {
    var ContactSummaryViewModel = function (contact) {
        var self = this;
        self.title = contact.Title;
        self.titleUpperCase = self.title.toUpperCase();
        self.primaryAvatar = contact.PrimaryAvatar || '/Content/images/AnonContact.svg';
        self.tags = contact.Tags;
        self.isVisible = ko.observable(true);
        self.filter = function(prefixTest) {
            var isVisible = (self.titleUpperCase.lastIndexOf(prefixTest, 0) === 0);
            self.isVisible(isVisible);
        };
    };
    
    var ContactSummaryGroup = function () {
        var self = this,
            filterText = '';
        self.contacts = ko.observableArray();
        self.visibleContacts = ko.computed(function() {
            return ko.utils.arrayFilter(self.contacts(), function(contactVm) {
                return contactVm.isVisible();
            });
        }, this);
        self.isVisible = ko.computed(function() {
            return self.visibleContacts().length > 0;
        });
        self.isValid = function() {
            throw 'This is intended to be an abstract class please do not use';
        };
        self.addContact = function(contact) {
            var vm = new ContactSummaryViewModel(contact);
            vm.filter(filterText);
            self.contacts.push(vm);
            self.contacts.sort(function (left, right) { return left.title.toUpperCase() == right.title.toUpperCase() ? 0 : (left.title.toUpperCase() < right.title.toUpperCase() ? -1 : 1); });
        };
        self.filter = function(filter) {
            filterText = filter.toUpperCase();
            var contacts = self.contacts();
            for (var i = 0; i < contacts.length; i++) {
                var contactVm = contacts[i];
                contactVm.filter(filterText);
            }
        };
    };
    var AnyContactSummaryGroup = function () {
        var self = this;
        ContactSummaryGroup.call(self);
        self.header = '';
        self.isValid = function() { return true; };
    };
    var AlphaContactSummaryGroup = function(startsWith) {
        var self = this;
        ContactSummaryGroup.call(self);
        self.header = startsWith;
        self.isValid = function(contact) {
            //TODO - there is duplication here and in the nested view model - see if we can extract this or rethink how this should work
            return contact.Title.toUpperCase().lastIndexOf(self.header, 0) === 0;
        };
    };

    var ContactSummariesViewModel = function () {
        var self = this;
        self.filterText = ko.observable('');
        self.contactGroups = ko.observableArray();
        self.totalResults = ko.observable(0);
        self.receivedResults = ko.observable(0);
        self.progress = ko.computed(function() {
            return 100 * self.receivedResults() / self.totalResults();
        });
        self.currentState = ko.observable('Initialising');
        self.isProcessing = ko.observable(true);

        var incrementProgress = function () {
            var i = self.receivedResults();
            i += 1;
            self.receivedResults(i);
            if (i == self.totalResults()) {
                self.isProcessing(false);
            }
        };

        var filterTextChangeSubscription = self.filterText.subscribe(function(newFilterText) {
            var cgs = self.contactGroups();
            for (var i = 0; i < cgs.length; i++) {
                cgs[i].filter(newFilterText);
            }
        });

        self.LoadContactGroups = function() {
            var charList = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            console.log(charList);
            for (var i = 0; i < charList.length; i++) {
                var h = charList[i];
                console.log('loading ' + h);
                self.contactGroups.push(new AlphaContactSummaryGroup(charList[i]));
            }
            self.contactGroups.push(new AnyContactSummaryGroup('123'));
        };

        self.addContact = function(contact) {
            var cgsLength = self.contactGroups().length;
            for (var j = 0; j < cgsLength; j++) {
                //TODO - switch this to just a look up? ie use the first char as the key look up? 
                var cg = self.contactGroups()[j];
                if (cg.isValid(contact)) {
                    cg.addContact(contact);

                    break;
                }
            }
            incrementProgress();
        };

        self.IncrementCount = function(addition) {
            console.log('append to count = ' + addition);
            var aggregateCount = self.totalResults() + addition;
            console.log('new count = ' + aggregateCount);
            self.totalResults(aggregateCount);
        };

        
    };
    //Publicly exposed object are attached to the callWall namespace
    callWall.ContactSummariesViewModel = ContactSummariesViewModel;
    //Exposed for testing, but not necessary to be hidden either
    callWall.ContactSummaryViewModel = ContactSummaryViewModel;
    callWall.AnyContactSummaryGroup = AnyContactSummaryGroup;
    callWall.AlphaContactSummaryGroup = AlphaContactSummaryGroup;

    
// ReSharper disable ThisInGlobalContext
}(ko, this.callWall = this.callWall || {}));
// ReSharper restore ThisInGlobalContext
