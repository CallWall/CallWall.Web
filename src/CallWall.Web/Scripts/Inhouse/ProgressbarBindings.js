﻿(function ($, ko, callWall) {
    callWall.createCustomProgressbarBindings = function () {
        //Custom binding to allow ko to update JqueryUI progressbar
        //http://www.piotrwalat.net/using-jquery-ui-progress-bar-with-mvvm-knockout-and-web-workers/
        //http://knockoutjs.com/documentation/custom-bindings.html
        ko.bindingHandlers.progress = {
            init: function (element, valueAccessor) {
                var progressValue = ko.unwrap(valueAccessor());
                $(element).progressbar({
                    value: progressValue
                });
            },
            update: function (element, valueAccessor) {
                var progressValue = ko.unwrap(valueAccessor());
                $(element).progressbar("value", progressValue);
            }
        };
    };
// ReSharper disable ThisInGlobalContext
}(jQuery, ko, this.callWall = this.callWall || {}));
// ReSharper restore ThisInGlobalContext