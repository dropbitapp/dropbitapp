var distillCompute = function () {

    /*
    What is proof?
    
    Proof is a method of measuring the alcohol content of spirits. You calculate the proof of a spirits product by multiplying the percent of alcohol by volume by two (2). 
    For example, a spirits product that has a 40% alcohol content by volume is 80 proof [40 multiplied by 2 = 80].
    
    Converting U.S. gallons into proof gallons for tax purposes:
        1. Multiply U.S. gallons by the percent of alcohol by volume
        2. Multiply by 2
        3. Divide by 100
    
    Sample calculation:
        1. 100 U.S. gallons x 40% alcohol by volume = 4000
        2. 4000 x 2 = 8000
        3. 8000/100 = 80 proof gallons
    */

    // Calculate proof gallons, accepts volume in gallons and alcohol content in percent as parameters
    this.calculateProof = function (Gallons, AlcoholContent) {
        return ((Gallons * AlcoholContent * 2) / 100);
    }

    // Recalculate Bottle Quantity upon change to Case Quantity for Blending Workflow 
    this.caseQuantityUpdateEvent = function (BottleQuantitySelector, CaseCapacitySelector, CaseQuantitySelector) {
        if (!($(CaseCapacitySelector).val() == undefined || $(CaseCapacitySelector).val() == "")) {
            var bottleQuantity = $(CaseQuantitySelector).val() * $(CaseCapacitySelector).val();
            $(BottleQuantitySelector).val(bottleQuantity);
        }
    }

    // Recalculate Case Quantity upon change to Bottle Quantity for Blending Workflow 
    this.bottleQuantityUpdateEvent = function (CaseCapacitySelector, CaseQuantitySelector, BottleQuantitySelector) {
        if (!($(CaseCapacitySelector).val() == undefined || $(CaseCapacitySelector).val() == "") && ($(CaseCapacitySelector).val() > 0)) {
            var caseQuantity = $(BottleQuantitySelector).val() / $(CaseCapacitySelector).val();
            $(CaseQuantitySelector).val(caseQuantity);
        }
    }

    /* this funcation calculates total quantity of liquid, given the number of bottles
       in question and a capcity of a bottle.
       Assumption here is that all bottles are of the same capacity
    */
     this.calculateTotalQuant = function (botCap /*bottle capacity in mL*/, botQnty/*bottle quantity*/) {
        if (botCap == 0 || botCap == undefined || botCap == "" || botQnty == 0 || botQnty == undefined || botQnty == "") {
            return 0;
        }
        else {
            // get total capacity in litters, first
            var litCap = (botCap * botQnty) / 1000;
            // convert litters into gallons
            var gals = litCap / 3.785411784;
            return gals;
        }
    }
};