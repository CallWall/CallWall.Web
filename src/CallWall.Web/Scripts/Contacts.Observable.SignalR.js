/// <reference path="../scripts/jquery-1.9.1.js" />
/// <reference path="../scripts/jquery.signalR-1.1.2.js" />

/*!
    ASP.NET SignalR Stock Ticker Sample
*/

// Crockford's supplant method (poor man's templating)
if (!String.prototype.supplant) {
    String.prototype.supplant = function (o) {
        return this.replace(/{([^{}]*)}/g,
            function (a, b) {
                var r = o[b];
                return typeof r === 'string' || typeof r === 'number' ? r : a;
            }
        );
    };
}
$(function () {
    var contactsHub = $.connection.contacts, // the generated client-side hub proxy
          $contactList = $('#ContactDefList');
    
    contactsHub.client.ReceiveContactSummary = function (contact) {
        var template = '<div style="display: inline-block; margin:10px; width:300px; height:100px; overflow: hidden;">'
        + '<table style="display: inline-table; background:transparent;">'
            + '<tr>'
            + '<td rowspan="2" style="width: 100px; height: 100px;background: #808080"><img src="{PrimaryAvatar}" style="max-height: 100px; max-width: 100px;"/></td>'
            + '<td style="width: 200px; background: #808080"><h3 style="text-overflow:ellipsis; max-width:200px; overflow: hidden;">{Title}</h3></td>'
            + '</tr><tr><td style="background: #808080;">';
        var output = '';
        
        $.each(contact.Tags, function () {
            var tag = this;
            output += '<span class="badge">' + tag + '</span>\r\n';
        });

        template += output + '</td></tr></table></div>';

        $contactList.append(template.supplant(contact));
    };

    // Start the connection
    $.connection.hub.start()
        .done(function () {
            contactsHub.server.requestContactSummaryStream();
        });
});