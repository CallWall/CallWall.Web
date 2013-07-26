/// <reference path="../scripts/jquery-1.9.1.js" />
/// <reference path="../scripts/jquery.signalR-1.1.2.js" />

/*!
    ASP.NET SignalR Stock Ticker Sample
*/
$(function () {
    var contactsHub = $.connection.contacts, // the generated client-side hub proxy
          $contactList = $('#ContactDefList');

    contactsHub.client.ReceiveContactSummary = function (contact) {
        $contactList.append('<dt>' + contact.Title + '</dt>');
        var tags = '<dd>';
        
        $.each(contact.Tags, function () {
            var tag = this;
            tags += '<span class="badge">' + tag + '</span>\r\n';
        });
        tags += '</dd>';
        $contactList.append(tags);
    };

    // Start the connection
    $.connection.hub.start()
        .done(function () {
            contactsHub.server.requestContactSummaryStream();
        });
});