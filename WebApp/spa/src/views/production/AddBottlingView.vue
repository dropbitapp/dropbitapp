<template>
  <div class="container">
    <!-- STEP 1 -->
    <div class="box" id="step1">
      <!-- STEPS -->
      <ul class="steps my-step-style has-content-centered has-gaps">
        <li class="steps-segment is-active">
          <a v-scroll-to="'#step1'" class="steps-marker">1</a>
          <div class="steps-content">
            <p class="is-size-3 is-italic">Available for Bottling</p>
          </div>
        </li>
        <li class="steps-segment">
          <a v-scroll-to="'#step2'" class="steps-marker">2</a>
          <div class="steps-content">
            <p class="is-size-3">Fill Test</p>
          </div>
        </li>
        <li class="steps-segment">
          <a v-scroll-to="'#step3'" class="steps-marker">3</a>
          <div class="steps-content">
            <p class="is-size-3">Details</p>
          </div>
        </li>
      </ul>
      <div class="help is-danger" v-show="validationInitialized && batchQuantityTypeMessage">
        <!-- eslint-disable-next-line vue/no-parsing-error -->
        {{ batchQuantityTypeMessage }}
        <br>
        <br>
      </div>
      <!-- Quantity TYPE TABLE -->
      <dbit-table
        ref="QuantityTypeTable"
        :data="blendingList"
        rowId="ProductionId"
        checkable
        v-on:updated:checkedRows="updateSelectedMaterials($event)"
      >
        <!-- TODO: Add empty table slot -->
        <template slot="columns" slot-scope="props">
          <!-- ID -->
          <b-table-column
            field="ProductionId"
            label="ID"
            width="50"
            sortable
          >{{ props.row.ProductionId }}</b-table-column>
          <!-- NAME -->
          <b-table-column field="BatchName" label="Blend Name" sortable>
            <span class="row-content">{{ props.row.BatchName }}</span>
          </b-table-column>
          <!-- UNIT TYPE -->
          <b-table-column field="ProofGallon" label="Proof Gallons" sortable>
            <span class="row-content">{{ props.row.ProofGallon }}</span>
          </b-table-column>
        </template>
        <template slot="detail" slot-scope="props">
          <div class="has-text-weight-bold">Notes</div>
          <div v-if="props.row.Note">
            <p>{{ props.row.Note }}</p>
          </div>
          <div v-else>
            <p>-</p>
          </div>
        </template>
      </dbit-table>
      <hr v-show="selectedMaterials.length > 0">
      <div class="columns is-multiline">
        <div
          class="column is-half"
          v-for="material in selectedMaterials"
          :key="material.ProductionId"
        >
          <div class="card">
            <header class="card-header">
              <p class="card-header-title">{{ material.BatchName }}</p>
            </header>
            <div class="card-content">
              <div class="content">
                <b-field label="Date">{{ material.ProductionEndDate }}</b-field>
                <b-field label="Method">
                  <div>
                    <b-radio
                      v-model="material.BurningDownMethod"
                      native-value="volume"
                      @input="() => material.BurningDownMethodInitialized = true"
                      :disabled="material.BurningDownMethod && !(material.Quantity && material.VolumeByWeight)"
                    >Volume</b-radio>
                    <b-radio
                      v-model="material.BurningDownMethod"
                      native-value="weight"
                      @input="() => material.BurningDownMethodInitialized = true"
                      :disabled="material.BurningDownMethod && !(material.Quantity && material.VolumeByWeight)"
                    >Weight</b-radio>
                  </div>
                </b-field>
                <!-- VOLUME BURNDOWN -->
                <b-field
                  v-if="material.BurningDownMethod === 'volume' || !material.BurningDownMethod"
                  label="Amount Used"
                  :message="errors.first(material.ProductionId.toString()) ? 'Volume used is required' : (material.UsedQty > material.Quantity ? 'Amount used exceeds amount available' : '')"
                  :type="errors.first(material.ProductionId.toString()) || material.UsedQty > material.Quantity ? 'is-danger' : ''"
                >
                  <b-input
                    v-model="material.UsedQty"
                    v-validate="'required'"
                    :name="material.ProductionId.toString()"
                    autocomplete="off"
                  ></b-input>
                </b-field>
                <b-field
                  v-if="material.BurningDownMethod === 'volume' || !material.BurningDownMethod"
                  label="Amount Available"
                >
                  <b-input v-model="material.Quantity" autocomplete="off" disabled></b-input>
                </b-field>
                <!-- WEIGHT BURNDOWN -->
                <b-field
                  v-if="material.BurningDownMethod === 'weight'"
                  label="Amount Used"
                  :message="errors.first(material.ProductionId.toString()) ? 'Weight used is required' : (material.UsedQty > material.VolumeByWeight ? 'Amount used exceeds amount available' : '')"
                  :type="errors.first(material.ProductionId.toString()) || material.UsedQty > material.VolumeByWeight ? 'is-danger' : ''"
                >
                  <b-input
                    v-model="material.UsedQty"
                    v-validate="'required'"
                    :name="material.ProductionId.toString()"
                    autocomplete="off"
                  ></b-input>
                </b-field>
                <b-field v-if="material.BurningDownMethod === 'weight'" label="Amount Available">
                  <b-input v-model="material.Quantity" autocomplete="off" disabled></b-input>
                </b-field>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    <!-- STEP 2 -->
    <div class="box" id="step2">
      <!-- STEPS -->
      <ul class="steps my-step-style has-content-centered has-gaps">
        <li class="steps-segment">
          <a v-scroll-to="'#step1'" class="steps-marker">1</a>
          <div class="steps-content">
            <p class="is-size-3">Available for Bottling</p>
          </div>
        </li>
        <li class="steps-segment">
          <a v-scroll-to="'#step2'" class="steps-marker">2</a>
          <div class="steps-content">
            <p class="is-size-3 is-italic">Fill Test</p>
          </div>
        </li>
        <li class="steps-segment">
          <a v-scroll-to="'#step3'" class="steps-marker">3</a>
          <div class="steps-content">
            <p class="is-size-3">Details</p>
          </div>
        </li>
      </ul>
      <form v-on:submit.prevent="addFillTest">
         <!-- FILL TEST ALCOHOL CONTENT -->
        <b-field
          horizontal
          label="Alcohol Content"
        >
          <!-- eslint-disable-next-line max-len -->
          <b-input
            v-model="batchFillTestAlcohol"
            name="Alcohol Content"
            autocomplete="off"
          ></b-input>
        </b-field>
        <!-- FILL TEST VARIATION PERCENT -->
        <b-field
          horizontal
          label="Fill Variation"
        >
          <!-- eslint-disable-next-line max-len -->
          <b-input
            v-model="batchFillTestVariation"
            name="Fill Variation"
            autocomplete="off"
          ></b-input>
        </b-field>
        <!-- FILL TEST DATE -->
        <b-field
          horizontal
          label="Fill Test Date"
        >
          <b-datepicker
            v-model="batchFillTestDate"
            name="Fill Test Date"
            placeholder="Click to select..."
            icon="calendar-today"
          ></b-datepicker>
          <b-timepicker
            v-model="batchFillTestTime"
            name="Batch Start Time"
            placeholder="Select Time..."
            icon="clock"
            hour-format="12"
          ></b-timepicker>
        </b-field>
        <!-- FILL TEST NOTES -->
        <b-field horizontal label="Corrective Action">
          <!-- eslint-disable-next-line max-len -->
          <textarea v-model="batchFillTestNote" class="textarea"></textarea>
        </b-field>
        <!-- FILL TEST ADD/REMOVE BUTTONS -->
        <b-field grouped position="is-centered">
            <p class="control">
              <button class="button level is-primary" type="reset">Remove Fill Test</button>
            </p>
            <p class="control">
              <button class="button level is-primary" type="submit">Add Fill Test</button>
            </p>
          </b-field>
        </form>
        <!-- FILL TEST LIST -->
      <dbit-table
        ref="fillTestTable"
        :data="fillTestList"
        rowId="FillTestId"
        >
        <!-- TODO: Add empty table slot -->
        <template slot="columns" slot-scope="props">
          <!-- ID -->
          <b-table-column
            field="Fill"
            label="ID"
            width="50"
            sortable
          >{{ props.row.FillTestId }}</b-table-column>
          <!-- FILL TEST ALCOHOL CONTENT -->
          <b-table-column field="FillTestAlcohol" label="Alcohol Content" sortable>
            <span class="row-content">{{ props.row.AlcoholContent }}</span>
          </b-table-column>
          <!-- FILL VARATION -->
          <b-table-column field="FillTestVariation" label="Fill Variation" sortable>
            <span class="row-content">{{ props.row.FillVariation}}</span>
          </b-table-column>
          <!-- FILL TEST DATE -->
          <b-table-column field="FillTestDate" label="Fill Test Date" sortable>
            <span class="row-content">{{ props.row.FillDate}}</span>
          </b-table-column>
        </template>
          <!-- FILL TEST CORRECTIVE ACTION -->
        <template slot="detail" slot-scope="props">
          <div class="has-text-weight-bold">Corrective Action</div>
          <div v-if="props.row.Note">
            <p>{{ props.row.Note }}</p>
          </div>
          <div v-else>
            <p>-</p>
          </div>
        </template>
      </dbit-table>
    </div>
    <!-- STEP 3 -->
    <div class="box" id="step3">
      <!-- STEPS -->
      <ul class="steps my-step-style has-content-centered has-gaps">
        <li class="steps-segment">
          <a v-scroll-to="'#step1'" class="steps-marker">1</a>
          <div class="steps-content">
            <p class="is-size-3">Available for Bottling</p>
          </div>
        </li>
        <li class="steps-segment">
          <a v-scroll-to="'#step2'" class="steps-marker">2</a>
          <div class="steps-content">
            <p class="is-size-3">Fill Test</p>
          </div>
        </li>
        <li class="steps-segment">
          <a v-scroll-to="'#step3'" class="steps-marker">3</a>
          <div class="steps-content">
            <p class="is-size-3 is-italic">Details</p>
          </div>
        </li>
      </ul>
       <form @submit.prevent="validateBeforeSubmit">
        <!-- BATCH NAME -->
        <b-field
          horizontal
          label="Batch Name"
          :message="errors.first('Batch Name')"
          :type="errors.first('Batch Name') ? 'is-danger' : ''"
        >
          <!-- eslint-disable-next-line max-len -->
          <b-input v-model="batchName" v-validate="'required'" name="Batch Name" autocomplete="off"></b-input>
        </b-field>
        <!-- BOTTLING START DATE -->
        <b-field
          horizontal
          label="Bottling Start Date"
          :message="errors.first('Bottling Start Date')"
          :type="errors.first('Bottling Start Date') ? 'is-danger' : ''"
        >
          <b-datepicker
            v-model="batchStartDate"
            v-validate="'required'"
            name="Bottling Start Date"
            placeholder="Click to select..."
            icon="calendar-today"
          ></b-datepicker>
          <b-timepicker
            v-model="batchStartTime"
            v-validate="'required'"
            name="Batch Start Time"
            placeholder="Select Time..."
            icon="clock"
            hour-format="12"
          ></b-timepicker>
        </b-field>
        <!-- BOTTLING END DATE -->
        <b-field
          horizontal
          label="Bottling End Date"
          :message="errors.first('Bottling End Date')"
          :type="errors.first('Bottling End Date') ? 'is-danger' : ''"
        >
          <b-datepicker
            v-model="batchEndDate"
            v-validate="'required'"
            name="Bottling End Date"
            placeholder="Click to select..."
            icon="calendar-today"
          ></b-datepicker>
          <b-timepicker
            v-model="batchEndTime"
            v-validate="'required'"
            name="Batch End Time"
            placeholder="Select Time..."
            icon="clock"
            hour-format="12"
          ></b-timepicker>
        </b-field>
        <!-- CASE CAPACITY (BOTTLES) -->
        <b-field
          horizontal
          label="# of Bottles in a Case"
          :message="errors.first('bottleCaseCapacity')"
          :type="errors.first('bottleCaseCapacity') ? 'is-danger' : ''"
        >
          <!-- eslint-disable-next-line max-len -->
          <b-input v-model="batchCaseCapacity" v-validate="'required'" name="Batch Case Capacity" autocomplete="off"></b-input>
        </b-field>
        <!-- CASE QUANTITY -->
        <b-field
        horizontal
        label="# of Cases"
        :message="errors.first('caseQuantity')"
        :type="errors.first('caseQuantity') ? 'is-danger' : ''"
      >
        <!-- eslint-disable-next-line max-len -->
        <b-input v-model="batchCaseQuantity" v-validate="'required'" name="Batch Case Quantity" autocomplete="off"></b-input>
      </b-field>
        <!-- BOTTLING QUANTITY -->
        <b-field
          horizontal
          label="# of Bottles"
          :message="errors.first('bottleQuantity')"
          :type="errors.first('bottleQuantity') ? 'is-danger' : ''"
        >
          <!-- eslint-disable-next-line max-len -->
          <b-input v-model="batchBottleQuantity" v-validate="'required'" name="Batch Bottle Quantity" autocomplete="off"></b-input>
        </b-field>
        <!-- BOTTLE CAPACITY (mL) -->
        <b-field
            horizontal
            label="Bottle Capacity (mL)"
            :message="errors.first('bottleCapacity')"
            :type="errors.first('bottleCapacity') ? 'is-danger' : ''"
          >
            <!-- eslint-disable-next-line max-len -->
            <b-input v-model="batchBottleCapacity" v-validate="'required'" name="Batch Bottle Capacity" autocomplete="off"></b-input>
         </b-field>
        <!-- REPORTING SPIRIT TYPE -->
        <b-field
            horizontal
            label="Spirit Type">
            <!-- eslint-disable-next-line max-len -->
            <b-input v-model="reportingSpiritType" name="Reporting Spirit Type" autocomplete="off" disabled></b-input>
        </b-field>
        <!-- VOLUME -->
        <b-field
          horizontal
          label ="Volume (gal)"
          :message ="volumeWeightMessage
          && validationInitialized ? batchVolumeWeightMessage : ''"
          :type ="batchVolumeWeightMessage
          && validationInitialized ? 'is-danger' : ''"
        >
          <!-- eslint-disable-next-line max-len -->
          <b-input v-model="batchVolume" autocomplete="off"></b-input>
        </b-field>
        <!-- LABELED ALCOHOL CONTENT -->
        <b-field
          horizontal
          label="Labeled Alcohol Content"
          :message="errors.first('Alcohol Content')"
          :type="errors.first('Alcohol Content') ? 'is-danger' : ''"
        >
          <!-- eslint-disable-next-line max-len -->
          <b-input
            v-model="batchAlcohol"
            v-validate="'required'"
            name="Alcohol Content"
            autocomplete="off"
          ></b-input>
        </b-field>
        <!-- TOTAL PROOF GALLONS -->
        <b-field
          horizontal
          label="Total (Proof Gallons)"
          :message="errors.first('Proof')"
          :type="errors.first('Proof') ? 'is-danger' : ''"
        >
          <!-- eslint-disable-next-line max-len -->
          <b-input v-model="batchProof" v-validate="'required'" name="Proof" autocomplete="off"></b-input>
        </b-field>
        <!-- LOSSES PROOF GALLONS -->
        <b-field
          horizontal
          label="Losses (Proof Gallons)"
          :message="errors.first('Proof Losses')"
          :type="errors.first('Proof Losses') ? 'is-danger' : ''"
        >
          <!-- eslint-disable-next-line max-len -->
          <b-input v-model="batchLossesProof" v-validate="'required'" name="Proof" autocomplete="off"></b-input>
        </b-field>
        <!-- NOTE -->
        <b-field horizontal label="Note">
          <!-- eslint-disable-next-line max-len -->
          <textarea v-model="batchNote" class="textarea"></textarea>
        </b-field>
        <!--BOTTLING SUBMIT/CLEAR BUTTONS -->
        <b-field grouped position="is-centered">
            <p class="control">
              <button class="button level is-primary" type="reset" @click="clear()">Clear</button>
            </p>
            <p class="control">
              <button class="button level is-primary" type="submit">Submit</button>
            </p>
          </b-field>
      </form>
    </div>
  </div>
</template>

<script>
import DbitTable from '../../components/DbitTable.vue';
import dateHelper from '../../helpers/date-helper';
import distillMathHelper from '../../helpers/math-helper';
let fillTestId = 0;

export default {
  name: 'AddBottlingView',
  props: {
  },
  components: {
    DbitTable,
  },
  created() {
    this.$store.dispatch('production/getRecordsAvaiableForBottling', 'Bottling');
    this.$store.dispatch('purchase/getReportingSpiritTypes');
  },
  computed: {
    blendingList() {
      return this.$store.state.production.blendsForBottling || [{}];
    },
    fillTestList() {
      return this.$store.state.production.fillList || [{}];
    },
    batchQuantityTypeMessage() {
      return this.selectedMaterials
        ? ''
        : 'Please select at least one record to be Bottled.';
    },
    batchVolumeWeightMessage() {
      return (this.batchVolume && !this.batchWeight) ||
        (this.batchWeight && !this.batchVolume)
        ? ''
        : 'Either Batch Volume or Batch Weight field is required.';
    },
  },
  data() {
    return {
      selectedMaterials: [],
      selectedMaterialProof: {
        totalProof: null,
        proofs:[{
        productionId: null,
        proof: null,
        checkBoxState: false,
        }]
      },
      batchName: null,
      batchStartDate: null,
      batchStartTime: null,
      batchEndDate: null,
      batchEndTime: null,
      batchVolume: null,
      batchAlcohol: null,
      batchProof: null,
      batchCaseCapacity: 12,
      batchCaseQuantity: null,
      batchBottleQuantity: null,
      batchBottleCapacity: null,
      reportingSpiritType: null,
      reportingSpiritTypeId: null,
      batchNote: null,
      validationInitialized: false,
    };
  },
  methods: {
    validateBeforeSubmit() {
      this.validationInitialized = true;
      this.$validator.validateAll().then((result) => {
        // eslint-disable-next-line max-len
        if (
          result &&
          !this.selectedMaterialsTypeMessage &&
          !this.batchVolumeWeightMessage
        ) {
          this.submit();
        } else {
          this.$toast.open({
            message: 'Form is not valid! Please check the fields.',
            type: 'is-danger',
            position: 'is-bottom',
          });
        }
      });
    },
    submit() { /* eslint-disable no-param-reassign */
      this.selectedMaterials.map((item) => {
        if (item.BurningDownMethod === 'volume') {
          item.OldVal = item.Quantity - item.UsedQty;
          item.NewVal = item.UsedQty;
        } else if (item.BurningDownMethod === 'weight') {
          item.OldVal = item.VolumeByWeight - item.UsedQty;
          item.NewVal = item.UsedQty;
        } else {
          throw new Error('Source materials contain item without selected burndown method!');
        }
        item.Proof = distillMathHelper.calculateProof(item.OldVal, item.AlcoholContent);
        return item;
      });
      const bottlingDetail = {
        CaseCapacity: this.batchCaseCapacity,
        CaseQuantity: this.batchCaseQuantity,
        BottleQuantity: this.batchBottleQuantity,
        BottleCapacity: this.batchBottleCapacity,
      };

      const start = dateHelper.formDateTimeObject(this.batchStartDate, this.batchStartTime);
      const end = dateHelper.formDateTimeObject(this.batchEndDate, this.batchEndTime);

      const production = {
        ProductionType: 'Bottling',
        Quantity: this.batchVolume,
        AlcoholContent: this.batchAlcohol,
        ProofGallon: this.batchProof,
        Note: this.batchNote,
        BatchName: `PROD-${this.batchName}`,
        ProductionStart: dateHelper.convertToUTC(start),
        ProductionEnd: dateHelper.convertToUTC(end),
        ProductionDate: dateHelper.convertToUTC(end),
        GainLoss: this.batchLossesProof,
        SpiritId: this.reportingSpiritTypeId,
        SpiritName: this.reportingSpiritType,
        BottlingInfo: bottlingDetail,
        FillTestList: this.fillTestList,
        UsedMats: this.selectedMaterials,
        Gauged: true,
      };
      this.$store
        .dispatch('production/createProduction', production)
        .then(() => {
          this.clear();
          this.$toast.open({
            duration: 5000,
            message: `Successfully created bottling: ${production.batchName}`,
            type: 'is-success',
          });
        })
        .catch(() => {
          this.$toast.open({
            duration: 5000,
            message: `Unable to create bottling: ${production.batchName}`,
            type: 'is-danger',
          });
        });
    },
    addFillTest() {
      const fillTestDateTime = dateHelper.formDateTimeObject(this.batchFillTestDate, this.batchFillTestTime);
      const fillTest = {
        FillTestId: ++fillTestId,
        FillAlcoholContent: this.batchFillTestAlcohol,
        FillVariation: this.batchFillTestVariation,
        FillDate: fillTestDateTime.toLocaleDateString(),
        CorrectiveAction: this.batchFillTestNote,
      };
      this.$store.dispatch('production/addFillTest', fillTest);
    },
    updateSelectedMaterials(items) { /* eslint-disable no-param-reassign */
      // Clear used source quantities
      items.map((item) => {
        if (item.UsedQty) {
          item.UsedQty = null;
        }
        if (
          (!item.BurningDownMethod && item.Quantity && item.VolumeByWeight) ||
          (!item.BurningDownMethod && item.Quantity && !item.VolumeByWeight)
        ) {
          item.BurningDownMethod = 'volume';
          item.NewVal = item.Quantity - item.UsedQty;
          item.OldVal = item.Quantity;
        } else if (!item.BurningDownMethod && !item.Quantity && item.VolumeByWeight) {
          item.BurningDownMethod = 'weight';
          item.NewVal = item.VolumeByWeight - item.UsedQty;
          item.OldVal = item.VolumeByWeight;
        }
        item.ID = item.ProductionId;
        // calculate maximum used proof for Losses
        const selectedProofs = this.selectedMaterialProof;
        selectedProofs.proofs.map(proofItem => {
        if (proofItem.productionId !== item.ProductionID) {
          selectedProofs.totalProof += item.ProofGallon;
          proofItem.proof = item.ProofGallon;
          proofItem.productionId = item.ProductionId;
          proofItem.checkBoxState = true;
          selectedProofs.proofs.push(proofItem);
        }
        else if (proofItem.productionId === item.ProductionID) {
          if (proofItem.checkBoxState == true) {
            selectedProofs.totalProof -= item.ProofGallon;
            proofItem.checkBoxState = false;
          }
          else if (proofItem.checkBoxState == true) {
            selectedProofs.totalProof += item.ProofGallon;
            proofItem.checkBoxState = false;
          }
        }
        return selectedProofs;
        });
        this.selectedMaterialProof = selectedProofs;
        this.batchLossesProof = distillMathHelper.roundToTwoDecimals((this.selectedMaterialProof.totalProof && this.batchProof) ?
        this.selectedMaterialProof.totalProof - this.batchProof : 0);

        // update spirit type input
        this.reportingSpiritType = item.SpiritName ? item.SpiritName : '';
        this.reportingSpiritTypeId =  item.SpiritId ? item.SpiritId : '';
        // update Alcohol field
        this.batchAlcohol = item.AlcoholContent ? item.AlcoholContent: '';
        return items;
      });
      this.selectedMaterials = items;
    },
    clear() {
      this.$refs.QuantityTypeTable.clearSelected();
      this.$refs.fillTestTable.clearSelected();
      this.batchName = null;
      this.batchStartDate = null;
      this.batchStartTime = null;
      this.batchEndDate = null;
      this.batchEndTime = null;
      this.batchVolume = null;
      this.batchAlcohol = null;
      this.batchProof = null;
      this.reportingSpiritType = null;
      this.batchNote = null;
      this.batchCaseQuantity = null;
      this.batchBottleQuantity = null;
      this.batchBottleCapacity = null;
      this.$validator.reset();
      this.validationInitialized = false;
    },
  },
  watch: {
    /**
     * calculate proof content based on alcohol and volume
     */
    batchAlcohol() {
      // this.batchProof = distillMathHelper.calculateProof(this.batchVolume ? this.batchVolume : 0, this.batchAlcohol ? this.batchAlcohol : 0) > 0 ? distillMathHelper.calculateProof(this.batchVolume, this.batchAlcohol) : '';
       this.batchProof = distillMathHelper.calculateProof(this.batchVolume, this.batchAlcohol);
    },
    /**
     * calculate number of bottles based on case capacity and case quantity
     */
    batchCaseCapacity() {
      this.batchBottleQuantity = (this.batchCaseCapacity * this.batchCaseQuantity) !== 0 ? (this.batchCaseCapacity * this.batchCaseQuantity) : '';
    },
    /**
     * calculate number of bottles based on case quantity and case capacity
     */
    batchCaseQuantity() {
      this.batchBottleQuantity = this.batchBottleQuantity = (this.batchCaseCapacity * this.batchCaseQuantity) !== 0 ? (this.batchCaseCapacity * this.batchCaseQuantity) : '';
    },
    /**
     * calculate volume based on bottle quantity and bottle capacity
     */
    batchBottleQuantity() {
      this.batchVolume = distillMathHelper.calculateGallonQuantity(this.batchBottleCapacity, this.batchBottleQuantity) > 0 ? distillMathHelper.calculateGallonQuantity(this.batchBottleCapacity, this.batchBottleQuantity) : '';
      this.batchProof = distillMathHelper.calculateProof(this.batchVolume, this.batchAlcohol);
    },
    /**
     * calculate volume based on bottle capacity and bottle quantity
     */
    batchBottleCapacity() {
      this.batchVolume = distillMathHelper.calculateGallonQuantity(this.batchBottleCapacity, this.batchBottleQuantity) > 0 ? distillMathHelper.calculateGallonQuantity(this.batchBottleCapacity, this.batchBottleQuantity) : '';
      this.batchProof = distillMathHelper.calculateProof(this.batchVolume, this.batchAlcohol);
    },
    /**
     * calculate Proof Losses based on original proof and proof used in bottling
     */
    batchProof() {
      this.batchLossesProof = distillMathHelper.roundToTwoDecimals((this.selectedMaterialProof.totalProof && this.batchProof) ? this.selectedMaterialProof.totalProof - this.batchProof : 0);
    },
    /**
     * todo: we need to figure out how to reactively update fields in the list-like
     * compnents like 'cards'
     **/
    // selectedMaterials(newVal, prevVal) {
    //   deep: true,
    //   // console.log('newVal', newVal);
    //   // console.log('oldVal', oldVal);
    //   // Clear used source quantities
    //   items.map((item) => {
    //     debugger;
    //     if (item.UsedQty)
    //       item.Quantity = item.Quantity - item.UsedQty;
    //     return items;
    //   });
    //   this.selectedMaterials = items;
    // },
  },
};
</script>
