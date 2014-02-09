(function (ko, callWall) {
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

    var getProvider = function (name) {
        for (var i = 0; i < providers.length; i++) {
            if (name.toLowerCase() === providers[i].name.toLowerCase()) {
                return providers[i];
            }
        }
        throw new Error("No providers found with name " + name);
    };


    //Contact Profile
    var ContactAssociation = function (data) {
        var self = this;
        self.name = data.Name;
        self.association = data.Association;
    };
    var ContactProfileViewModel = function () {
        var self = this;
        //TODO: Add this to the ko ObservableArray prototype.
        var concat = function (target, source) {
            if (target == undefined || source == undefined) return;
            for (var i = 0; i < source.length; i++) {
                target.push(source[i]);
            }
        };
        var concatMap = function (target, source, selector) {
            if (target == undefined || source == undefined) return;
            for (var i = 0; i < source.length; i++) {
                target.push(selector(source[i]));
            }
        };
        
        var Aggregate = function (data) {
            if (data.title) self.title(data.Title);
            if (data.fullName) self.fullName(data.FullName);
            if (data.DateOfBirth) {
                var dob = new Date(data.DateOfBirth);
                self.dateOfBirth(dob);
            }
            concat(self.tags, data.Tags);

            concatMap(self.organizations, data.Organizations, function (d) { return new ContactAssociation(d); });
            concatMap(self.relationships, data.Relationships, function (d) { return new ContactAssociation(d); });
            concatMap(self.phoneNumbers, data.PhoneNumbers, function (d) { return new ContactAssociation(d); });
            concatMap(self.emailAddresses, data.EmailAddresses, function (d) { return new ContactAssociation(d); });
        };

        self.aggregate = Aggregate;
        self.title = ko.observable('');
        self.fullName = ko.observable('');
        self.dateOfBirth = ko.observable();
        self.tags = ko.observableArray();
        self.organizations = ko.observableArray();
        self.relationships = ko.observableArray();
        self.phoneNumbers = ko.observableArray();
        self.emailAddresses = ko.observableArray();
        self.isProcessing = ko.observable(true);
    };

    //Communication
    var Message = function (data) {
        var self = this;
        //TODO - correct casing
        self.timestamp = new Date(data.Timestamp);
        self.isOutbound = data.IsOutbound;
        self.subject = data.Subject;
        self.content = data.Content;

        self.provider = getProvider(data.Provider);
    };
    var ContactCommunicationViewModel = function () {
        var self = this;
        self.isProcessing = ko.observable(true);
        self.messages = ko.observableArray();
        self.add = function (message) {
            self.messages.push(new Message(message));
        };
    };

    //Calendar
    var CalendarEntry = function (data) {
        var self = this;
        self.date = new Date(data.Date);
        self.title = data.Title;
    };
    var ContactCalendarViewModel = function () {
        var self = this;
        self.isProcessing = ko.observable(true);
        self.entries = ko.observableArray();
        self.add = function (message) {
            self.entries.push(new CalendarEntry(message));
        };
    };

    //Gallery
    var GalleryAlbum = function (data) {
        var self = this;
        console.log(data);
        self.createdDate = new Date(data.CreatedDate);
        self.lastModifiedDate = new Date(data.LastModifiedDate);
        self.title = data.Title;
        self.provider = data.Provider;
        self.imageUrls = data.ImageUrls;
    };
    var ContactGalleryViewModel = function () {
        var self = this;
        self.albums =  ko.observableArray();
        self.isProcessing = ko.observable(true);
        self.add = function(galleryAlbum) {
            self.albums.push(new GalleryAlbum(galleryAlbum));
        };
    };

    //Collaboration
    var CollaborationAction = function (title, actionDate, actionPerformed, isCompleted, provider) {
        var self = this;
        //self.project = project;   //Maybe use project/projectName instead of name.
        self.title = title;
        self.actionDate = actionDate;
        self.actionPerformed = actionPerformed;
        self.isCompleted = isCompleted;
        self.provider = provider;
    };
    var ContactCollaborationViewModel = function () {
        var self = this;
        var t = today();
        self.entries = [
                new CollaborationAction('Design KO Standards', t.addMinutes(-35), 'Created Document', false, googleDriveProvider),
                new CollaborationAction('EOY 2013 Reports', t.addDays(-8), 'Modified Document', false, googleDriveProvider),
                new CollaborationAction('Pricing a cross example', t.addDays(-37), 'Modified Document', false, googleDriveProvider),
                new CollaborationAction('CallWall #122 - install Https', t.addDays(-40), 'Closed issue', true, githubProvider),
                new CollaborationAction('Pricing a cross example', t.addDays(-45), 'Created document', false, googleDriveProvider)
        ];
        self.isProcessing = ko.observable(true);
        setTimeout(function () { self.isProcessing(false); }, 1800);
    };

    //Location
    var ContactLocationViewModel = function () {
    };

    var DashboardViewModel = function () {
        var self = this;

        self.contactProfile = new ContactProfileViewModel();
        self.communications = new ContactCommunicationViewModel();
        self.calendar = new ContactCalendarViewModel();
        self.gallery = new ContactGalleryViewModel();
        self.collaboration = new ContactCollaborationViewModel();
        self.location = new ContactLocationViewModel();

        self.LoadContactProfile = function () { };
    };

    //Publicly exposed object are attached to the callWall namespace
    callWall.DashboardViewModel = DashboardViewModel;

    // ReSharper disable ThisInGlobalContext
}(ko, this.callWall = this.callWall || {}));
// ReSharper restore ThisInGlobalContext