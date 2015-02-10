/// <reference path="../../knockout-3.1.0.debug.js" />

(function (ko, callWall) {
    var addRange = function (targetKoArrar, newItems, selector) {
        if (newItems == null) return;
        if (!selector) {
            selector = function (x) { return x; };
        }
        for (var i = 0; i < newItems.length; i++) {
            var item = selector(newItems[i]);
            targetKoArrar.push(selector(item));
        }
    }
    
    //Provider
    var ProviderDescription = function (name, imageUrl) {
        var self = this;
        self.name = name;
        self.imageUrl = imageUrl;
    };

    //TODO: Why not just change the data sent down to be 'imageUrl' instead of 'image'?
    var getProvider = function (provider) {
        return new ProviderDescription(provider.name, provider.image);
    };

    //Contact Profile
    var ContactAssociation = function (data) {
        var self = this;
        self.name = data.name;
        self.association = data.association;
    };
    var ContactProfileViewModel = function () {
        var self = this;
        
        self.title = ko.observable('');
        self.fullName = ko.observable('');
        self.dateOfBirth = ko.observable();
        self.tags = ko.observableArray();
        self.organizations = ko.observableArray();
        self.relationships = ko.observableArray();
        self.handles = ko.observableArray();
        self.isProcessing = ko.observable(true);

        //TODO: Make some sort of carousel that is bound to an observable array.
        //self.avatars = ko.observableArray();
        //self.avatars.push('/Content/images/AnonContact.svg');
        self.avatar = ko.observable('/Content/images/AnonContact.svg');
        //self.avatar = ko.observable('/Content/images/pictures/Interlaken1.jpg');

        self.aggregate = function (data) {
            if (data.title) self.title(data.title);
            if (data.fullName) self.fullName(data.fullName);
            if (data.dateOfBirth) {
                var dob = new Date(data.dateOfBirth);
                self.dateOfBirth(dob);
            }
            if (data.avatarUris && data.avatarUris.length > 0) {
                self.avatar(data.avatarUris[0]);
            }
            addRange(self.tags, data.tags);
            addRange(self.organizations, data.organizations, function(d) { return new ContactAssociation(d); });
            addRange(self.relationships, data.relationships, function (d) { return new ContactAssociation(d); });
            addRange(self.handles, data.handles);
        };
    };

    var Message = function (data) {
        var self = this;
        self.timestamp = new Date(data.timestamp);
        self.isOutbound = data.isOutbound;
        self.subject = data.subject;
        self.content = data.content;

        self.provider = getProvider(data.provider);
    };

    var CalendarEntry = function (data) {
        var self = this;
        self.date = new Date(data.date);
        self.title = data.title;
    };

    var GalleryAlbum = function (data) {
        var self = this;
        self.createdDate = new Date(data.createdDate);
        self.lastModifiedDate = new Date(data.lastModifiedDate);
        self.title = data.title;
        self.provider = getProvider(data.provider);
        self.imageUrls = data.imageUrls;
    };

    var CollaborationAction = function (data) {
        var self = this;
        self.title = data.title;
        self.actionDate = new Date(data.actionDate);
        self.actionPerformed = data.actionPerformed;
        self.isCompleted = data.isCompleted;
        self.provider = getProvider(data.provider);
    };

    var ListViewModel = function (ctor) {
        var self = this;
        self.entries = ko.observableArray();
        self.add = function (data) {
            self.entries.push(new ctor(data));
        };
        self.isProcessing = ko.observable(true);
    };

    //Location
    var ContactLocationViewModel = function () {
        //TODO - What do i do?
    };

    var DashboardViewModel = function () {
        var self = this;

        self.contactProfile = new ContactProfileViewModel();
        self.communications = new ListViewModel(Message);
        self.calendar = new ListViewModel(CalendarEntry);
        self.gallery = new ListViewModel(GalleryAlbum);
        self.collaboration = new ListViewModel(CollaborationAction);
        self.location = new ContactLocationViewModel();

        self.LoadContactProfile = function () { };
    };

    //Publicly exposed object are attached to the callWall namespace
    callWall.DashboardViewModel = DashboardViewModel;

    // ReSharper disable ThisInGlobalContext
}(ko, this.callWall = this.callWall || {}));
// ReSharper restore ThisInGlobalContext