var today = new Date();
var monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
Date.prototype.isToday = function () {
    return (this.getFullYear() == today.getFullYear()
        && this.getMonth() == today.getMonth()
        && this.getDate() == today.getDate());
};
Date.prototype.isPast = function () {
    return (this.getFullYear() < today.getFullYear()
        || (this.getFullYear() == today.getFullYear() && this.getMonth() < today.getMonth())
        || (this.getFullYear() == today.getFullYear() && this.getMonth() == today.getMonth() && this.getDate() < today.getDate())
        );
};
Date.prototype.addDays = function (days) {
    var dat = new Date(this.valueOf());
    dat.setDate(dat.getDate() + days);
    return dat;
};
Date.prototype.getMonthName = function () {
    return monthNames[this.getMonth()];
};
Date.prototype.format = function () {
    return '' + this.getDate()
    + '-' + this.getMonthName()
    + '-' + this.getFullYear();
};