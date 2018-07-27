var genWHelpers = function () {
    //genWHelpers - Generic Workflow Helpers is the class that will contain misc functions that we could not allocate anywhere else

    var dateModificationHelper = new dateHelper();
    // Function maps AdditiveName or AdditiveId to Units that should be used for this particular Additive
    /*
    params: AdditiveName - optional if AdditiveId is provided,
    *       dditiveId - optional if AdditiveName is provided
    */
    this.mapAdditivesToUnits = function (AdditiveName/*optional if AdditiveId is provided*/, AdditiveId/*optional if AdditiveName is provided*/) {
        // for now, I am hard coding it due to time constraints
        //todo: need to send ajax request here or use session object to retrieve values
        if (AdditiveName != "" || AdditiveName != undefined) {
            if (AdditiveName == "Sugar") {
                return "lbs";
            } else if (AdditiveName == "Water") {
                return "gal";
            }
        }
        if (AdditiveId != "" || AdditiveId != undefined) {
            //do majic for figuring out units based on additive ids
            return "Additive Id was used";
        }
        return "Parameters did'n't match any known records in DB when trying to get Additive Units";
    }

     /**
     * Rounding of a number with Number.EPSILON
     * Takes care of little quirks with wrong roundings 
     * in some cases when .round and .toFixed functions used
     * details are here - https://stackoverflow.com/questions/11832914/round-to-at-most-2-decimal-places-only-if-necessary
     * @param   {Number}     type   The type of adjustment.
     * @returns {Number}            The adjusted value.
     */
    this.roundWithEpsilon = function (valueToBeRounded){
        if (Number.EPSILON === undefined) {
            Number.Epsilon = Math.pow(2, -52);
        }
        return Math.round((valueToBeRounded + Number.EPSILON) * 100) / 100;
    }

    // this function empties the controls on request.
    /*
    params: fieldSet- is a artificial set of  subset of selectors in html. we make them up ourselves so make sure this name is unique.
    example function usage is in Blending workflow in Blending.cshtml 
    */
    
    this.emptyControls = function () {
        var date = new Date() // explicity specifying date value so that HH:mm is set to 00:00 
        var mm = date.getMonth();
        var dd = date.getDate();
        var yy = date.getFullYear();

        $('*').jqxInput('val', '');
        $('*').jqxCheckBox('uncheck');
        $('*').jqxDateTimeInput('setDate', new Date(yy, mm, dd));
        $('*').jqxDropDownList('clearSelection');
        $('*').jqxListBox('refresh');
        $('*').jqxListBox('clearSelection');
        $('*').jqxNumberInput('clear');
        $('*').jqxTextArea('val', '');
    }

    this.emptyFillTest = function () {
        var date = new Date(); // explicity specifying date value so that HH:mm is set to 00:00
        var mm = date.getMonth();
        var dd = date.getDate();
        var yy = date.getFullYear();

        $('#FillAlcoholContent').jqxNumberInput('val', '');
        $('#FillTestDate').jqxDateTimeInput('setDate', new Date(yy, mm, dd));
        $('#FillVariation').jqxNumberInput('val', '');
        $('#CorrectiveAction').jqxTextArea('val', '');


    }

    // Function that convert local time to UTC and vice versa
    /* 
        Params { 
            inputDate: date to be converted. It could be a string or a type date
            toUtc: boolean flag when if set to true, function spits out time in UTC if set to false, spits out local time. If this parameter is not passed at all, function does nothing
        }
    */
    this.convertUTC = function (inputDate, toUTC) {
        if (toUTC) {
            // convert to UTC.
            // Assumption here is that the string is of format mm/dd/yyyy
            var date = new Date(inputDate);
            var localOffset = date.getTimezoneOffset() * 60000;
            var date = date.getTime();
            date =  date + localOffset;
            date = new Date(date);
            return date;
        }
        else if (!toUTC) {
            // convert UTC to local
            var parsedDate;
            var regEx = /Date\([0-9]+\)/;

            if (typeof inputDate === "string") { // check whether the variable of type string because regex can only compare strings
                if (inputDate.match(regEx)) { // we are doing this here because we only want to parse out milisecond value from json date: /Date(1487750400000)/
                    parsedDate = new Date(parseInt(inputDate.substr(6)));
                }
            }
            else {
                parsedDate = new Date(inputDate);
            }

            var localOffset = parsedDate.getTimezoneOffset() * 60000;
            var date = parsedDate.getTime();
            date = date - localOffset;
            date = new Date(date);
            return date;
        }
    }

    //this function sets up the source to be used in jqxwidgets adapter
    /*
    *params: sourceName - this tells the function which source it should set up. Example: Blending records source or Blending Additive source
    */
    this.setUpSourceForAdapter = function (sourceName) {
        var source = "";
        if (sourceName == "BlendingAdditves") { 
            source =
            {
                dataType: "json",
                dataFields: [{
                    name: 'AdditiveName'
                    ,//name: 'UnitOfMeasurement'
                }],
                id: 'AdditiveId',
                url: '/Production/GetAdditiveData',
                async: false
            };
            return source;
        }
        else if (sourceName == "BlendingIds") {
            source =
            {
                dataType: "json",
                dataFields: [{ name: 'BlendingId' }],
                id: 'BlendingId',
                url: '/Production/GetBlendingIds',
                async: false
            };
            return source;
        }
        else if (sourceName == "FinalSpiritIds") {
            source =
            {
                dataType: "json",
                dataFields: [{ name: 'FinalSpiritId' }],
                id: 'FinalSpiritId',
                url: '/Production/GetFinalSpiritIds',
                async: false
            };
            return source;
        }
        else if (sourceName == "") {

        }
        else {
            return source;
        }
    }
    //this function gets selected row with all it's contents. I am assuming only one row has been selected
    /*
    *params: tableName - table selector
    */
    this.getSingleSelectedRowInJQTable = function (tableName/*allowed param is #dataTable*/) {
        if (tableName == "" || tableName != undefined) {
            return "tableName param is empty";
        }
        var selection = $(tableName).jqxDataTable('getSelection');
        //for (var i = 0; i < selection.length; i++) {
        //    // get a selected row.
        //    var rowData = selection[i];
        //}
    }

    // Calculates total used material proof
    // Parameter: usedMatsList array
    this.getUsedMaterialProofTotal = function (arr) {

        let proofTotal = 0;
        let dc = new distillCompute();

        if (arr.length > 0) {
            for (let i = 0; i < arr.length; i++) {
                let vol = arr[i].NewVal;
                let alc = arr[i].AlcoholContent;
                if (vol >= 0 &&
                    vol !== undefined &&
                    alc >= 0 &&
                    alc !== undefined) {
                    proofTotal += dc.calculateProof(vol, alc);
                }
            }
        }
        
        return proofTotal;
    }

    this.constrainProductionDate = function (prodStartDate, prodEndDate, listBox, workflow) {
        var minDate = new Date();
        var recordDate;
        // var listBoxName = '#' + listBox;
        // set prdouction min date based on purchase/prod object used.
        var checkedItems = $('#' + listBox).jqxListBox('getCheckedItems');
        $.each(checkedItems, function (index) {
            if (workflow == 'Distillation' || workflow == 'Blending') {
                if (this.originalItem.DistillableOrigin == 'pur') {
                    recordDate = dateModificationHelper.setMinDate(this.originalItem.PurchaseDate)
                }
                if (this.originalItem.DistillableOrigin == 'prod') {
                    recordDate = dateModificationHelper.setMinDate(this.originalItem.ProductionEndDate)
                }
            } else {
                if (this.label.includes('PUR')) {
                    recordDate = dateModificationHelper.setMinDate(this.originalItem.PurchaseDate)
                }
                if (this.label.includes('PROD')) {
                    recordDate = dateModificationHelper.setMinDate(this.originalItem.ProductionEndDate)
                }
            }
            
            minDate = recordDate < minDate ? recordDate : minDate;
        });
        $('#' + prodStartDate).jqxDateTimeInput({
            min: minDate
        })
        $('#' + prodEndDate).jqxDateTimeInput({
            min: minDate
        })
    }
};