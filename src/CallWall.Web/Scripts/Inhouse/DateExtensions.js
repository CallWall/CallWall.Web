var now = function () {
    return new Date();
};
var today = function () {
    var now = new Date();
    return new Date(now.getFullYear(), now.getMonth(), now.getDate());
};
var monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
var minutes = 1000 * 60;
var hours = minutes * 60;
var days = hours * 24;
var years = days * 365;
Date.prototype.isToday = function () {
    var t = today();
    return (this.getFullYear() == t.getFullYear()
        && this.getMonth() == t.getMonth()
        && this.getDate() == t.getDate());
};
Date.prototype.isPast = function () {
    var t = today();
    return (this.getFullYear() < t.getFullYear()
        || (this.getFullYear() == t.getFullYear() && this.getMonth() < t.getMonth())
        || (this.getFullYear() == t.getFullYear() && this.getMonth() == t.getMonth() && this.getDate() < t.getDate())
        );
};
Date.prototype.addDays = function (days) {
    var dat = new Date(this.valueOf());
    dat.setDate(dat.getDate() + days);
    return dat;
};
Date.prototype.addSeconds = function (s) {
    var dat = new Date(this.valueOf());
    dat.setTime(this.getTime() + (s * 1000));
    return dat;
};
Date.prototype.addMinutes = function (m) {
    return this.addSeconds(m * 60);
};
Date.prototype.addHours = function (h) {
    return this.addMinutes(h * 60);
};
Date.prototype.getMonthName = function () {
    return monthNames[this.getMonth()];
};
Date.prototype.format = function () {
    return '' + this.getDate()
    + '-' + this.getMonthName()
    + '-' + this.getFullYear();
};
Date.prototype.untilToday = function () {
    var msDelta = Math.abs(now().getTime() - this.getTime());
    if (msDelta > years) return '' + Math.round(msDelta / years) + 'y';
    if (msDelta > days) return '' + Math.round(msDelta / days) + 'd';
    if (msDelta > hours) return '' + Math.round(msDelta / hours) + 'h';
    return Math.round(msDelta / minutes) + 'm';
};
Date.prototype.todayDeltaFormat = function () {
    var msDelta = now().getTime() - this.getTime();
    var absMsDelta = Math.abs(msDelta);

    var count = Math.round(absMsDelta / days);
    if (count > 14) return this.format();

    var suffix = (msDelta > 0) ? 'ago' : 'to go';
    if (absMsDelta > 0) {
        return count == 1 ? '1 day ' + suffix : '' + count + ' days ' + suffix;
    }
    if (absMsDelta > hours) {
        count = Math.round(absMsDelta / hours);
        return count == 1 ? '1 hour ' + suffix : '' + count + ' hours ' + suffix;
    }
    count = Math.round(absMsDelta / minutes);
    return count == 1 ? '1 minute ' + suffix : '' + count + ' minute ' + suffix;
};
function formatDate(source) {
    alert(source);
    if (source == undefined) return null;
    //alert('formatDate(source) source != undefined');
    alert(Object.prototype.toString.call(source));
    if (Object.prototype.toString.call(source) === '[object Date]') {
        return source.format();
    }
    alert('returning source');
    return source;
}

ko.bindingHandlers.date = {
    update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var value = valueAccessor();
        var date = value();
        if (date != null) {
            $(element).text(date.format());
        }
     }
};