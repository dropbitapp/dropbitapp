// Function that convert local time to UTC and vice versa
//     Params {
//         inputDate: date to be converted. It could be a string or a type date
//         toUtc: boolean flag when if set to true, function spits out time in UTC
//         if set to false, spits out local time. If this parameter is not passed at all,
//         function does nothing
//     }
const convertUTC = function convertUTC(inputDate, toUTC) {
  let newDate = {};
  if (toUTC) {
    // convert to UTC.
    // Assumption here is that the string is of format mm/dd/yyyy
    const date = new Date(inputDate);
    let dateMsec = date.getTime();
    dateMsec += date.getTimezoneOffset() * 60000;
    newDate = new Date(dateMsec);
  } else if (!toUTC) {
    // convert UTC to local
    let parsedDate = {};
    const regEx = /Date\([0-9]+\)/;

    if (typeof inputDate === 'string') {
      // check whether the variable of type string because regex can only compare strings
      if (inputDate.match(regEx)) {
        // we are doing this here because we only want to parse out milisecond value
        // from json date: /Date(1487750400000)/
        parsedDate = new Date(parseInt(inputDate.substr(6), 10));
      }
    } else {
      parsedDate = new Date(inputDate);
    }
    newDate = new Date(parsedDate.getTime() - (parsedDate.getTimezoneOffset() * 60000));
  }
  return newDate;
};

export { convertUTC as default };
