//TODO: provide internal group sorting
//TODO: provide search/filter while contacts still being loaded

(function (ko, callWall) {
    
    var ContactViewModel = function (contact) {
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
    
    var ContactGroup = function () {
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
            var vm = new ContactViewModel(contact);
            vm.filter(filterText);
            self.contacts.push(vm);
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
    var AnyContactGroup = function () {
        var self = this;
        ContactGroup.call(self);
        self.header = '';
        self.isValid = function() { return true; };
    };
    var AlphaContactGroup = function(startsWith) {
        var self = this;
        ContactGroup.call(self);
        self.header = startsWith;
        self.isValid = function(contact) {
            //TODO - there is duplication here and in the nested view model - see if we can extract this or rethink how this should work
            return contact.Title.toUpperCase().lastIndexOf(self.header, 0) === 0;
        };
    };

    var ContactDefViewModel = function () {
        var self = this;
        self.filterText = ko.observable('');
        self.contactGroups = ko.observableArray();
        self.totalResults = ko.observable(0);
        self.receivedResults = ko.observable(0);
        self.progress = ko.computed(function() {
            return 100 * self.receivedResults() / self.totalResults();
        });
        self.isProcessing = ko.observable(true);

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
                self.contactGroups.push(new AlphaContactGroup(charList[i]));
            }
            self.contactGroups.push(new AnyContactGroup('123'));
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
        };

        self.IncrementProgress = function() {
            var i = self.receivedResults();
            i += 1;
            self.receivedResults(i);
        };
    };
    //Publicly exposed object are attached to the callWall namespace
    callWall.ContactDefViewModel = ContactDefViewModel;
    //Exposed for testing, but not nessecary to be hidden either
    callWall.ContactViewModel = ContactViewModel;
    callWall.AnyContactGroup = AnyContactGroup;
    callWall.AlphaContactGroup = AlphaContactGroup;

    
// ReSharper disable ThisInGlobalContext
}(ko, this.callWall = this.callWall || {}));
// ReSharper restore ThisInGlobalContext