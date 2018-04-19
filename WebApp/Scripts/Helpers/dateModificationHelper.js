var dateHelper = function () {
    //dateHelper - Helpers for various data conversion/modification use cases. 
    this.getReportingMonthAndYear = function (reportingYear, reportingMonth) {
        var monthList = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
        var year = reportingYear.value;
        var month = monthList.indexOf(reportingMonth.value);
        var firstDay = new Date( year, month, 1 );
        var lastDay = new Date( year, month + 1, 0 );
        // set time to the earliest possible time in the day
        firstDay.setHours( 00, 00, 00 );
        // set the time to the latest possible time in the day
        lastDay.setHours( 23, 59, 59, 999 );
        return [firstDay, lastDay];
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

    this.setMinTimeOfDay = function(dateObj) {
        var date = new Date();
        return date;
    }
};