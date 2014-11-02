//TODO: Support updates to title that should move it to another group e.g. Campbell, Lee -> Lee Campbell (from 'C' group to 'L' group)
(function (ko, callWall) {
    var contactSummaryViewModel = function (contact) {
        var self = this;
        self.id = contact._id;
        self.title = ko.observable(contact.newTitle);
        self.titleUpperCase = contact.newTitle.toUpperCase();        
        self.avatars = ko.observableArray();
        if (contact.addedAvatars) {
            for (var i = 0; i < contact.addedAvatars.length; i++) {
                self.avatars.push(contact.addedAvatars[i]);
            }
        }
        self.tags = [];//contact.Tags;
        self.isVisible = ko.observable(true);
        self.primaryAvatar = ko.computed(function () {
            if (self.avatars().length == 0) {
                return '/Content/images/AnonContact.svg';
            } else {
                return self.avatars()[0];
            }
        });
        self.filter = function(prefixTest) {
            var isVisible = (self.titleUpperCase.lastIndexOf(prefixTest, 0) === 0);
            self.isVisible(isVisible);
        };
        self.update = function (contactUpdate) {
            if (contactUpdate.newTitle != null) {
                self.title(newTitle);
                self.titleUpperCase = newTitle.toUpperCase();
            }
            if (contact.removedAvatars) {
                for (var i = 0; i < contact.removedAvatars.length; i++) {
                    self.avatars.remove(contact.removedAvatars[i]);
                }
            }
            if (contact.addedAvatars) {
                for (var i = 0; i < contact.addedAvatars.length; i++) {
                    self.avatars.push(contact.addedAvatars[i]);
                }
            }

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
        self.addContact = function (contactViewModel) {
            contactViewModel.filter(filterText);
            self.contacts.push(contactViewModel);
            //TODO: Consider more efficient alg than sorting after every add.
            self.contacts.sort(function (left, right) { return left.titleUpperCase == right.titleUpperCase ? 0 : (left.titleUpperCase < right.titleUpperCase ? -1 : 1); });
        };
        self.tryRemoveById = function(id) {
            var removedItems = self.contacts.remove(function(item) { return item.id == id; });
            if (removedItems == null || removedItems.length == 0)
                return false;
            return true;
        };
        self.tryUpdateContact = function (contactUpdate) {
            //TODO: An update to title, could mean the contact needs to be moved -LC
            var contactCount = self.contacts().length;
            for (var i = 0; i < contactCount; i++) {
                var item = self.contacts()[i];
                if (item.id == contactUpdate._id) {
                    item.update(contactUpdate);
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
            return contact.titleUpperCase.lastIndexOf(self.header, 0) === 0;
        };
    };

    var contactSummariesViewModel = function () {
        var self = this;
        self.filterText = ko.observable('');
        self.contactGroups = ko.observableArray();
        self.serverHead = ko.observable(0);
        self.initialClientHead = ko.observable(0);
        self.currentClientHead = ko.observable(0);
        self.progress = ko.computed(function () {
            if (self.currentClientHead() >= self.serverHead())
                return 100;
            
            var batchSize = self.serverHead() - self.initialClientHead();
            var progress = self.currentClientHead() - self.initialClientHead();
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

        self.addContact = function (contactUpdate) {
            var vm = new contactSummaryViewModel(contactUpdate);
            var cgsLength = self.contactGroups().length;
            for (var j = 0; j < cgsLength; j++) {
                //TODO - switch this to just a look up? ie use the first char as the key look up? 
                var cg = self.contactGroups()[j];
                if (cg.isValid(vm)) {
                    cg.addContact(vm);
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
            try {
                var eventId = parseInt(contactUpdate.eventId);
                self.currentClientHead(eventId);

                if (contactUpdate.isDeleted) {
                    self.removeContact(contactUpdate._id);
                } else if (contactUpdate.version == 1) {
                    self.addContact(contactUpdate);
                } else {
                    self.updateContact(contactUpdate);
                }
            } catch (e) {
                console.log("Processing contactUpdate %O:", contactUpdate);
                console.error("Failed - %O", e);
            } 
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
