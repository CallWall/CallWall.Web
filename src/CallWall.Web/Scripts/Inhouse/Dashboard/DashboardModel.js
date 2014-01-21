(function (ko, callWall) {
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
        self.organizations = Array(new ContactAssociation('Consultant', 'Adaptive'), new ContactAssociation('Triathlon', 'Serpentine'));
        self.relationships = Array(new ContactAssociation('Wife', 'Erynne'), new ContactAssociation('Brother', 'Rhys'));
        self.phoneNumbers = Array(new ContactAssociation('Mobile - UK', '07827743025'), new ContactAssociation('Mobile - NZ', '021 254 3824'));
        self.emailAddresses = Array(new ContactAssociation('Home', 'lee.ryan.campbell@gmail.com'), new ContactAssociation('Work', 'lee.campbell@callwall.com'));
    };

    var DashboardViewModel = function () {
        var self = this;
        
        self.contactProfile = new ContactProfileViewModel();
        self.communications = null;
        self.calendar = null;
        self.gallery = null;
        self.collaboration = null;
        self.location = null;

        self.LoadContactProfile = function() {};
    };


    //Publicly exposed object are attached to the callWall namespace
    callWall.DashboardViewModel = DashboardViewModel;
    
    // ReSharper disable ThisInGlobalContext
}(ko, this.callWall = this.callWall || {}));
// ReSharper restore ThisInGlobalContext