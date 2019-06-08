/**
 * helper class that fills gaps in
 * 1.JS Math library
 * 2.Provides conversion formulas used in
 * distillation process
*/
export default class Formulas {
  // Class constants
  /**
   * baseForEpsilon supplied to Math.pow to calculate Epsilon
   */
  static get baseForEpsilon() {
    return 2;
  }
  
  /**
   * powerForEpsilon supplied to Math.pow to calculate Epsilon
   */
  static get powerForEpsilon() {
    return -52;
  }

  /**
   * liter constant used in liter to gallon calculations
   */
  static get literToGallonConstant() {
    return 3.785411784;
  }

  /**
   * Calculate proof gallons
   * @param {gallons} Number
   * @param {alcoholContent} Number
   * @return {proof} Number or undefined
   */
  // eslint-disable-next-line func-names
  static calculateProof = function(gallons, alcoholContent) {
    if (gallons === null || isNaN(gallons)
      || gallons === undefined
      || gallons === 0
      || alcoholContent === null
      || isNaN(alcoholContent)
      || alcoholContent === undefined
      || alcoholContent === 0) {
      return 0;
    }
    else {
      const proof = ((gallons * alcoholContent * 2) / 100);
      return Formulas.roundToTwoDecimals(proof);
    }
  }

   /**
    * calculates total quantity of liquid in gallons, given
    * the number of bottles in question and a capcity of a bottle.
    * @param {bottleCapacity} Number bottle capacity in mL
    * @param {bottleQnty} Number bottle quantity
    * @returns {gallons} returns gallon quantity || undefined
    */
   // eslint-disable-next-line func-names
  static calculateGallonQuantity = function (bottleCapacity, bottleQnty) {
    if (bottleCapacity === null || isNaN(bottleCapacity)
      || bottleCapacity === undefined
      || bottleCapacity === 0
      || bottleQnty === null
      || isNaN(bottleQnty)
      || bottleQnty === undefined
      || bottleQnty === 0) {
      return;
    }
    else {
      // get total capacity in litters, first
      const litCap = (bottleCapacity * bottleQnty) / 1000;
      // convert litters into gallons
      const gallons = litCap / Formulas.literToGallonConstant;
      return Formulas.roundToTwoDecimals(gallons);
    }
  }

  /**
   * Rounding of a number with Number.EPSILON
   * Takes care of little quirks with wrong roundings 
   * in some cases when .round and .toFixed functions used
   * details are here -
   * https://stackoverflow.com/questions/11832914/round-to-at-most-2-decimal-places-only-if-necessary
   * @param {toBeRounded} Number to be rounded
   * @param {numbersAfterDecimal} Number how many digits after decimal
   * @returns {rounded} rounded value.
   */
  static roundToTwoDecimals = function (toBeRounded) {
    if (toBeRounded === null || isNaN(toBeRounded)
      || toBeRounded === undefined) {
      return;
    }
    else {
      if (Number.EPSILON === undefined) {
        Number.EPSILON = Math.pow(Formulas.baseForEpsilon, Formulas.powerForEpsilon);
      }
      const rounded = Math.round((toBeRounded + Number.EPSILON) * 100) / 100;
      return rounded;
    }
  }
}
