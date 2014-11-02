/// <reference path="../../knockout-3.0.0.debug.js" />

(function (ko, callWall) {
    var observableArrayExtensions = {
        'concat': function (source, selector) {
            var target = this();
            if (source == undefined) return;
            if (!selector) {
                selector = function (i) { return i; };
            }
            ko.utils.arrayForEach(source, function (item) { target.push(selector(item)); });
        }
    };
    var customObservableArray = function () {
        var obsArray = ko.observableArray();
        ko.utils.extend(obsArray, observableArrayExtensions);
        return obsArray;
    };
    
    //Provider
    var ProviderDescription = function (name, imageUrl) {
        var self = this;
        self.name = name;
        self.imageUrl = imageUrl;
    };
    var googleProvider = new ProviderDescription('Google', '/Content/Google/images/GoogleIcon.svg');
    var gmailProvider = new ProviderDescription('Gmail', '/Content/Google/images/Email_48x48.png');
    var hangoutsProvider = new ProviderDescription('Hangouts', '/Content/Google/images/Hangouts_42x42.png');
    var googleDriveProvider = new ProviderDescription('Google Drive', '/Content/Google/images/Drive_128x128.png');
    var linkedinProvider = new ProviderDescription('LinkedIn', '/Content/LinkedIn/images/LinkedIn_64x64.png');
    var twitterProvider = new ProviderDescription('Twitter', '/Content/Twitter/images/Twitter_64x64.png');
    var facebookProvider = new ProviderDescription('Facebook', '/Content/Facebook/images/Facebook_64x64.png');
    var microsoftProvider = new ProviderDescription('Microsoft', '/Content/Microsoft/images/Microsoft_64x64.png');
    var githubProvider = new ProviderDescription('GitHub', '/Content/Github/images/Github_64x64.png');

    var providers = [
       googleProvider,
       gmailProvider,
       hangoutsProvider,
       googleDriveProvider,
       linkedinProvider,
       twitterProvider,
       facebookProvider,
       microsoftProvider,
       githubProvider
    ];

    var getProvider = function (provider) {
        //Provider is IProviderDescription
        if (provider.name) {
            return new ProviderDescription(provider.name, provider.image);
        }
        //provider is a string
        console.error('We should not require this provider mapping functionality. It means this client js file is tightly coupled to providers. We should be supplying all we need via the providers (including fakes)');
        var whitespaceGlobalRegex = / /g;
        for (var i = 0; i < providers.length; i++) {
            if (provider.toLowerCase() === providers[i].name.toLowerCase().replace(whitespaceGlobalRegex, '')) {
                return providers[i];
            }
        }
        throw new Error("No providers found with name " + name);
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
        self.tags = customObservableArray();
        self.organizations = customObservableArray();
        self.relationships = customObservableArray();
        self.phoneNumbers = customObservableArray();
        self.emailAddresses = customObservableArray();
        self.isProcessing = ko.observable(true);

        self.aggregate = function (data) {
            if (data.Title) self.title(data.title);
            if (data.FullName) self.fullName(data.fullName);
            if (data.dateOfBirth) {
                var dob = new Date(data.dateOfBirth);
                self.dateOfBirth(dob);
            }
            self.tags.concat(data.tags);

            self.organizations.concat(data.organizations, function (d) { return new ContactAssociation(d); });
            self.relationships.concat(data.relationships, function (d) { return new ContactAssociation(d); });
            self.phoneNumbers.concat(data.phoneNumbers, function (d) { return new ContactAssociation(d); });
            self.emailAddresses.concat(data.emailAddresses, function (d) { return new ContactAssociation(d); });
        };
    };

    var Message = function (data) {
        var self = this;
        //TODO - correct casing
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
        self.provider = data.provider;
        self.imageUrls = data.imageUrls;
    };

    var CollaborationAction = function (data) {
        var self = this;
        //self.project = project;   //Maybe use project/projectName instead of name.
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