//TODO: provide internal group sorting
//TODO: provide search/filter while contacts still being loaded

(function (ko, callWall) {
    var contactSummaryViewModel = function (contact) {
        var self = this;
        self.id = contact._id;
        self.title = ko.observable(contact.newTitle);
        self.titleUpperCase = contact.newTitle.toUpperCase();
        self.primaryAvatar = '/Content/images/AnonContact.svg';//contact.PrimaryAvatar || '/Content/images/AnonContact.svg';
        self.tags = [];//contact.Tags;
        self.isVisible = ko.observable(true);
        self.filter = function(prefixTest) {
            var isVisible = (self.titleUpperCase.lastIndexOf(prefixTest, 0) === 0);
            self.isVisible(isVisible);
        };
        self.update = function(newTitle) {
            self.title(newTitle);
            self.titleUpperCase = newTitle.toUpperCase();
        };
    };
    
    var contactSummaryGroup = function () {
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
        self.containsId = function(id) {
            var contactCount = self.contacts().length;
            for (var i = 0; i < contactCount; i++) {
                var contact = self.contacts()[i];
                if (contact.id == id) {
                    return true;
                }
            }
            return false;
        };
        self.addContact = function (contact) {
            var vm = new contactSummaryViewModel(contact);
            vm.filter(filterText);
            self.contacts.push(vm);
            self.contacts.sort(function (left, right) { return left.titleUpperCase == right.titleUpperCase ? 0 : (left.titleUpperCase < right.titleUpperCase ? -1 : 1); });
        };
        self.tryRemoveById = function(id) {
            var removedItems = self.contacts.remove(function(item) { return item.id == id; });
            if (removedItems == null || removedItems.length == 0)
                return false;
            return true;
        };
        self.tryUpdateContact = function (contact) {
            //TODO: An update to title, could mean the contact needs to be moved -LC
            var contactCount = self.contacts().length;
            for (var i = 0; i < contactCount; i++) {
                var item = self.contacts()[i];
                if (item.id == contact._id) {
                    if (contact.newTitle != null) {
                        item.update(contact.newTitle);
                    }

                    return true;
                }
            }
            return false;
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
    var anyContactSummaryGroup = function () {
        var self = this;
        contactSummaryGroup.call(self);
        self.header = '';
        self.isValid = function() { return true; };
    };
    var alphaContactSummaryGroup = function(startsWith) {
        var self = this;
        contactSummaryGroup.call(self);
        self.header = startsWith;
        self.isValid = function(contact) {
            //TODO - there is duplication here and in the nested view model - see if we can extract this or rethink how this should work
            return contact.newTitle.toUpperCase().lastIndexOf(self.header, 0) === 0;
        };
    };

    var contactSummariesViewModel = function () {
        var self = this;
        self.filterText = ko.observable('');
        self.contactGroups = ko.observableArray();
        self.startingServerVersion = ko.observable(0);
        self.startingClientVersion = ko.observable(0);
        self.currentClientVersion = ko.observable(0);
        self.progress = ko.computed(function() {
            if (self.currentClientVersion() >= self.startingServerVersion())
                return 100;
            
            var batchSize = self.startingServerVersion() - self.startingClientVersion();
            var progress = self.currentClientVersion() - self.startingClientVersion();
            if (batchSize <= 0)
                return 100;

            return 100 * progress / batchSize;
        });
        self.currentState = ko.observable('Initializing');
        self.isProcessing = ko.computed(function () {
            return self.progress() < 100;
        });
        self.errorMessage = ko.observable('');

        self.filterText.subscribe(function(newFilterText) {
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
                self.contactGroups.push(new alphaContactSummaryGroup(charList[i]));
            }
            self.contactGroups.push(new anyContactSummaryGroup('123'));
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
        self.updateContact = function (contact) {
            var cgsLength = self.contactGroups().length;
            for (var i = 0; i < cgsLength; i++) {
                var cg = self.contactGroups()[i];
                if (cg.tryUpdateContact(contact)) {
                    return;
                }
            }
            self.addContact(contact);
        };
        self.removeContact = function (id) {
            var cgsLength = self.contactGroups().length;
            for (var i = 0; i < cgsLength; i++) {
                var cg = self.contactGroups()[i];
                if (cg.tryRemoveById(id)) {
                    break;
                }
            }            
            console.error("Failed to delete contact - id: %i", id);
        };
        

        self.processUpdate = function (contactUpdate) {
            console.log("Processing contactUpdate %O:", contactUpdate);
            self.incrementProgress();
            if (contactUpdate.isDeleted) {
                self.removeContact(contactUpdate._id);
            } else if (parseInt(contactUpdate.version) == 1) {
                self.addContact(contactUpdate);
            } else {
                self.updateContact(contactUpdate);
            }
        };

        self.incrementProgress = function() {
            var i = self.currentClientVersion();
            i += 1;
            self.currentClientVersion(i);
        };
    };
    //Publicly exposed object are attached to the callWall namespace
    callWall.ContactSummariesViewModel = contactSummariesViewModel;
    //Exposed for testing, but not necessary to be hidden either
    callWall.ContactSummaryViewModel = contactSummaryViewModel;
    callWall.AnyContactSummaryGroup = anyContactSummaryGroup;
    callWall.AlphaContactSummaryGroup = alphaContactSummaryGroup;

    
// ReSharper disable ThisInGlobalContext
}(ko, this.callWall = this.callWall || {}));
// ReSharper restore ThisInGlobalContext
