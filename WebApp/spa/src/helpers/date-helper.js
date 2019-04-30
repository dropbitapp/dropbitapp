/**
 * Helper class that handles date modification and conversion
 */

export default class DateHelper {
    /**
     * returns reporting month and year 
     * @param {reportingYear} string
     * @param {reportingMonth} string
     * return {[firstDay, lastDay]} an array with first and last javascript object date
     */
    static getReportingMonthAndYear = function(reportingYear, reportingMonth) {
        const monthList = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
        // todo: either uncomment it or remove it once we know in what format the date will be sent
        // let year = reportingYear.value;
        // let month = monthList.indexOf(reportingMonth.value);
        // let firstDay = new Date( year, month, 1 );
        // let lastDay = new Date( year, month + 1, 0 );

        let month = monthList.indexOf(reportingMonth);
        let firstDay = new Date(reportingYear, month, 1);
        let lastDay = new Date(reportingYear, month + 1, 0);
        // set time to the earliest possible time of the first day
        firstDay.setHours(0o00, 0o00, 0o00);
        // set the time to the latest possible time of the last day
        lastDay.setHours(23, 59, 59, 999);
        return [firstDay, lastDay];
    }

    /**
     * convert JSON date to javasript date
     * @param {jsonDate} date in JSON format
     * @return {date} Javascript date object
     */
    static convertJSONdatetoJS = function(jsonDate) {
        let date = new Date(parseInt(jsonDate.substr(6)));
        return this.date;
    }

    /**
     * configures minimum date for production workflow give date in JSON format
     * @param {jsonDate} JSON string 
     * @return {minDate} returns new min date object
     */
    static setMinDate = function(jsonDate) {
        let minDate = this.convertJSONdatetoJS(jsonDate);
        minDate.setDate(minDate.getDate() - 1);
        return this.minDate;
    }

    /**
     * Function that convert local time to UTC
     * @param {inputDate} assumping that the string is of format mm/dd/yyyy
     * @return {dateUTC} date in UTC format
     */
    static convertToUTC(inputDate) {
        const date = new Date(inputDate);
        const localOffset = date.getTimezoneOffset() * 60000;
        const dateGetTime = date.getTime();
        const dateWOffset = dateGetTime + localOffset;
        const utcDate = new Date(dateWOffset);
        return utcDate;
    }

    /**
     * Function that convert UTC to local time
     * @param {inputDate} date in UTC format
     * @return {localDate} local time date object
     */
    static convertFromUTC(inputDate) {
        let parsedDate;
        const regEx = /Date\([0-9]+\)/;

        if (typeof inputDate === "string") { // check whether the variable of type string because regex can only compare strings
            if (inputDate.match(regEx)) { // we are doing this here because we only want to parse out milisecond value from json date: /Date(1487750400000)/
                parsedDate = new Date(parseInt(inputDate.substr(6)));
            }
        } else {
            parsedDate = new Date(inputDate);
        }

        let localOffset = parsedDate.getTimezoneOffset() * 60000;
        let localDate = parsedDate.getTime();
        localDate = localDate - localOffset;
        localDate = new Date(localDate);

        return localDate;
    }
};