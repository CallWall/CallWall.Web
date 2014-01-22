﻿(function (ko, callWall) {
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

    var ContactCommunicationViewModel = function () {
    };

    var CalendarEntry = function (date, title) {
        var self = this;
        self.date = date;
        self.title = title;
    };
    var ContactCalendarViewModel = function() {
        var self = this;

        self.entries = [
            new CalendarEntry(today.addDays(2), 'Lunch KO with Lee'),
            new CalendarEntry(today.addDays(1), 'Training'),
            new CalendarEntry(today.addDays(0), 'Document Review'),
            new CalendarEntry(today.addDays(-2), 'Document design session'),
            new CalendarEntry(today.addDays(-3), 'Lunch with Lee')];
    };

    var ContactGalleryViewModel = function () {
    };

    var ContactCollaborationViewModel = function () {
    };

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