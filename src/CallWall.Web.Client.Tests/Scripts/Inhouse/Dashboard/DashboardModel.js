﻿/// <reference path="../../knockout-3.0.0.debug.js" />

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

    var getProvider = function (provider) {
        //Provider is IProviderDescription
        if (provider.Name) {
            return new ProviderDescription(provider.Name, provider.Image);
        }
        //provider is a string
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
        self.name = data.Name;
        self.association = data.Association;
    };
    var ContactProfileViewModel = function () {
        var self = this;
        //TODO: Add this to the ko ObservableArray prototype.
        var concat = function (target, source) {
            concatMap(target, source, function (item) { return item; });
        };
        var concatMap = function (target, source, selector) {
            if (target == undefined || source == undefined) return;
            ko.utils.arrayForEach(source, function (item) { target.push(selector(item)); });
        };

        self.title = ko.observable('');
        self.fullName = ko.observable('');
        self.dateOfBirth = ko.observable();
        self.tags = ko.observableArray();
        self.organizations = ko.observableArray();
        self.relationships = ko.observableArray();
        self.phoneNumbers = ko.observableArray();
        self.emailAddresses = ko.observableArray();
        self.isProcessing = ko.observable(true);

        self.aggregate = function (data) {
            if (data.Title) self.title(data.Title);
            if (data.FullName) self.fullName(data.FullName);
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
    };

    var Message = function (data) {
        var self = this;
        //TODO - correct casing
        self.timestamp = new Date(data.Timestamp);
        self.isOutbound = data.IsOutbound;
        self.subject = data.Subject;
        self.content = data.Content;

        self.provider = getProvider(data.Provider);
    };
    
    var CalendarEntry = function (data) {
        var self = this;
        self.date = new Date(data.Date);
        self.title = data.Title;
    };
    
    var GalleryAlbum = function (data) {
        var self = this;
        console.log(data);
        self.createdDate = new Date(data.CreatedDate);
        self.lastModifiedDate = new Date(data.LastModifiedDate);
        self.title = data.Title;
        self.provider = data.Provider;
        self.imageUrls = data.ImageUrls;
    };
    
    var CollaborationAction = function (data) {
        var self = this;
        //self.project = project;   //Maybe use project/projectName instead of name.
        self.title = data.Title;
        self.actionDate = new Date(data.ActionDate);
        self.actionPerformed = data.ActionPerformed;
        self.isCompleted = data.IsCompleted;
        self.provider = getProvider(data.Provider);
    };
   
    var ListViewModel = function (map) {
        var self = this;
        self.entries = ko.observableArray();
        self.add = function (data) {
            self.entries.push(map(data));
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
        self.communications = new ListViewModel(Message);//todo entries vs messages
        self.calendar = new ListViewModel(CalendarEntry);
        self.gallery = new ListViewModel(GalleryAlbum);//todo entities vs albums
        self.collaboration = new ListViewModel(CollaborationAction);
        self.location = new ContactLocationViewModel();

        self.LoadContactProfile = function () { };
    };

    //Publicly exposed object are attached to the callWall namespace
    callWall.DashboardViewModel = DashboardViewModel;

    // ReSharper disable ThisInGlobalContext
}(ko, this.callWall = this.callWall || {}));
// ReSharper restore ThisInGlobalContext