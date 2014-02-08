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
    var ContactAssociation = function (name, association) {
        var self = this;
        self.name = name;
        self.association = association;
    };
    var ContactProfileViewModel = function () {
        var self = this;
        //TODO: Add this to the ko ObservableArray prototype.
        var concat = function(target, source) {
            if (target == undefined || source == undefined) return;
            for (var i = 0; i < source.length; i++) {
                target.push(source[i]);
            }
        };
        var Aggregate = function (data) {
            if(data.title) self.title(data.title);
            if (data.fullName) self.fullName(data.fullName);
            if (data.dateOfBirth) {
                var dob = new Date(data.dateOfBirth);
                self.dateOfBirth(dob);
            }
            concat(self.tags, data.tags);
            
            concat(self.organizations, data.organizations);
            concat(self.relationships, data.relationships);
            concat(self.phoneNumbers, data.phoneNumbers);
            concat(self.emailAddresses, data.emailAddresses);
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
    var CalendarEntry = function (date, title) {
        var self = this;
        self.date = date;
        self.title = title;
    };
    var ContactCalendarViewModel = function () {
        var self = this;
        var t = today();
        self.isProcessing = ko.observable(true);
        self.entries = ko.observableArray();
        setTimeout(function() {self.entries.push(new CalendarEntry(t.addDays(2), 'Lunch KO with Lee'));}, 200);
        setTimeout(function() { self.entries.push(new CalendarEntry(t.addDays(1), 'Training'));}, 500);
        setTimeout(function() { self.entries.push(new CalendarEntry(t.addDays(0), 'Document Review'));}, 600);
        setTimeout(function() { self.entries.push(new CalendarEntry(t.addDays(-2), 'Document design session'));}, 1200);
        setTimeout(function () { self.entries.push(new CalendarEntry(t.addDays(-3), 'Lunch with Lee')); }, 1300);
        setTimeout(function () { self.isProcessing(false); }, 1400);
    };

    //Gallery
    var GalleryAlbum = function (createdDate, lastModifiedDate, title, provider, imageUrls) {
        var self = this;
        self.createdDate = createdDate;
        self.lastModifiedDate = lastModifiedDate;
        self.title = title;
        self.provider = provider;
        self.imageUrls = imageUrls;
    };
    var ContactGalleryViewModel = function () {
        var self = this;
        var t = today();
        self.albums = [
            new GalleryAlbum(t.addDays(-1), t.addDays(-1), 'Interlaken Cycle', facebookProvider,
                [
                    '/Content/images/pictures/Interlaken1.jpg',
                    '/Content/images/pictures/Interlaken2.jpg',
                    '/Content/images/pictures/Interlaken3.jpg',
                    '/Content/images/pictures/Interlaken4.jpg',
                    '/Content/images/pictures/Interlaken5.jpg'

                ]),
            new GalleryAlbum(t.addDays(-2), t.addDays(-2), 'Landscape shots', microsoftProvider,
                [
                    '/Content/images/pictures/Landscape1.jpg',
                    '/Content/images/pictures/Landscape2.jpg',
                    '/Content/images/pictures/Landscape3.jpg',
                    '/Content/images/pictures/Landscape4.jpg',
                    '/Content/images/pictures/Landscape5.jpg'
                ])
        ];
        self.isProcessing = ko.observable(true);
        setTimeout(function () { self.isProcessing(false); }, 1100);
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