(function (ko, callWall) {
    //Provider
    var ProviderDescription = function(name, imageUrl) {
        var self = this;
        self.name = name;
        self.imageUrl = imageUrl;
    };
    var googleProvider = new ProviderDescription("Google", "/Content/Google/images/GoogleIcon.svg");
    var gmailProvider = new ProviderDescription("Gmail", "/Content/Google/images/Email_48x48.png");
    var hangoutsProvider = new ProviderDescription("Gmail", "/Content/Google/images/Hangouts_42x42.png");
    var linkedinProvider = new ProviderDescription("LinkedIn", "/Content/LinkedIn/images/LinkedIn_64x64.png");
    var twitterProvider = new ProviderDescription("Twitter", "/Content/Twitter/images/Twitter_64x64.png");

    //Contact Profile
    var ContactAssociation = function(name, association) {
        var self = this;
        self.name = name;
        self.association = association;
    };
    var ContactProfileViewModel = function() {
        var self = this;
        self.title = 'Lee Campbell';
        self.fullName = '';
        self.dateOfBirth = new Date(1979, 11, 27);
        self.tags = Array('Family', 'Dolphins', 'London');
        self.organizations = [new ContactAssociation('Consultant', 'Adaptive'), new ContactAssociation('Triathlon', 'Serpentine')];
        self.relationships = [new ContactAssociation('Wife', 'Erynne'), new ContactAssociation('Brother', 'Rhys')];
        self.phoneNumbers = [new ContactAssociation('Mobile - UK', '07827743025'), new ContactAssociation('Mobile - NZ', '021 254 3824')];
        self.emailAddresses = [new ContactAssociation('Home', 'lee.ryan.campbell@gmail.com'), new ContactAssociation('Work', 'lee.campbell@callwall.com')];
    };

    //Communication
    var Message = function (timestamp, isOutbound, subject, content, provider) {
        var self = this;
        
        self.timestamp = timestamp;
        self.isOutbound = isOutbound;
        self.subject = subject;
        self.content = content;
        self.provider = provider;
    };
    var ContactCommunicationViewModel = function () {
        var self = this;
        var n = now();
        self.messages = [
            new Message(n.addMinutes(-10), false, "On my way", null, hangoutsProvider),
            new Message(n.addMinutes(-13), true, "Dude, where are you?", null, hangoutsProvider),
            new Message(n.addDays(-2), false, "Pricing a cross example", "Here is the sample we were talking about the other day. It should cover the basic case, the complex multi-leg option case and all the variations in-between. If you have any questions, then just email me back on my home account.", linkedinProvider),
            new Message(n.addDays(-4), false, "I will bring the food for the Rugby", "From: James Alex To: You, Lee FAKE Camplell, Simon Real, Brian Baxter, Josh Taylor and Sally Hubbard", gmailProvider),
            new Message(n.addDays(-4), false, "#CallWall are recruiting engineers now!", "Retweets : 7", twitterProvider),
            new Message(n.addDays(-5), true, "Rugby at my place on Saturday morning", "To: James Alex, Simon Real + 3 others", gmailProvider)
        ];
    };


    //Calendar
    var CalendarEntry = function (date, title) {
        var self = this;
        self.date = date;
        self.title = title;
    };
    var ContactCalendarViewModel = function() {
        var self = this;
        var t = today();
        self.entries = [
            new CalendarEntry(t.addDays(2), 'Lunch KO with Lee'),
            new CalendarEntry(t.addDays(1), 'Training'),
            new CalendarEntry(t.addDays(0), 'Document Review'),
            new CalendarEntry(t.addDays(-2), 'Document design session'),
            new CalendarEntry(t.addDays(-3), 'Lunch with Lee')];
    };

    //Gallery
    var ContactGalleryViewModel = function () {
    };

    //Collaboration
    var ContactCollaborationViewModel = function () {
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

        self.LoadContactProfile = function() {};
    };


    //Publicly exposed object are attached to the callWall namespace
    callWall.DashboardViewModel = DashboardViewModel;
    
    // ReSharper disable ThisInGlobalContext
}(ko, this.callWall = this.callWall || {}));
// ReSharper restore ThisInGlobalContext