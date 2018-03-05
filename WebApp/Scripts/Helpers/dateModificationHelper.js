var dateHelper = function () {
    //dateHelper - Helpers for various data conversion/modification use cases. 
    this.getReportingMonthAndYear = function (reportingYear, reportingMonth) {
        var monthList = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
        var year = reportingYear.value;
        var month = monthList.indexOf(reportingMonth.value);
        var firstDay = this.normalizeDateToDashFormat(new Date(year, month, 1));
        var lastDay = this.normalizeDateToDashFormat(new Date(year, month + 1, 0));
        return [firstDay, lastDay];
    }

    // normalizeDateToDashFormat function converts JS date object into yyyy-mm-dd format
    this.normalizeDateToDashFormat = function (dateObj) {
        var yearObj = dateObj.getFullYear();
        var monthObj = dateObj.getMonth() + 1;
        if (monthObj < 10) {
            monthObj = 0 + '' + monthObj;
        }
        var dayObj = dateObj.getDate();
        if (dayObj < 10) {
            dayObj = 0 + '' + dayObj;
        }
        var normalizedDate = yearObj + '-' + monthObj + '-' + dayObj;
        return normalizedDate;
    }

    // convert JSON date to javasript date
    this.convertJSONdatetoJS = function (jsonDate) {
        var date = new Date(parseInt(jsonDate.substr(6)));
        return date;
    }

    // configures minimum date for production workflow 
    this.setMinDate = function (jsonDate) {
        var minDate = this.convertJSONdatetoJS(jsonDate);
        minDate.setDate(minDate.getDate() - 1);
        return minDate;
    }
};