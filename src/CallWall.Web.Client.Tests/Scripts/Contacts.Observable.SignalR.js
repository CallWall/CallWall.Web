/// <reference path="../scripts/jquery-1.9.1.js" />
/// <reference path="../scripts/jquery.signalR-1.1.2.js" />
/// <reference path="../scripts/knockout-2.3.0.debug.js" />

//TODO: provide internal group sorting
//TODO: provide search/filter while contacts still being loaded







$(function () {
    // $.connection.contacts =  the generated client-side hub proxy
    model = new callWall.ContactDefViewModel();
    model.LoadContactGroups();
    callWall.createCustomContactBindings();
    ko.applyBindings(model);
    // Start the connection
    var adapter = new callWall.SignalR.ContactAdapter($.connection.contacts, model);
    adapter.StartHub();
});