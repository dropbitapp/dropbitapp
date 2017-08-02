// Ajax library

// This Ajax library provides a source for ajax calls using jQuery.
var Ajax = new function () {

    //this.WebServiceBaseURL = "";

    //function getURL(urlSuffix) {
    //    if (urlSuffix) {
    //        return Ajax.WebServiceBaseURL + "/" + urlSuffix;
    //    }
    //    else {
    //        return Ajax.WebServiceBaseURL;
    //    }
    //}

    this.sendAjaxRequest = function(httpMethod, url, reqData, callback, errCallback) {
        $.ajax({
            url: url,
            type: httpMethod,
            success: callback,
            data: reqData,
            error: errCallback
        });
    }
    // end of Ajax
}