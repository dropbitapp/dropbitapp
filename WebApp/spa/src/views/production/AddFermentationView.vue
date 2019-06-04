<template>
  <div class="container">
    <!-- STEP 1 -->
    <div class="box" id="step1">
      <!-- STEPS -->
      <ul class="steps my-step-style has-content-centered has-gaps">
        <li class="steps-segment is-active">
          <a v-scroll-to="'#step1'" class="steps-marker">1</a>
          <div class="steps-content">
            <p class="is-size-3 is-italic">Source</p>
          </div>
        </li>
        <li class="steps-segment">
          <a v-scroll-to="'#step2'" class="steps-marker">2</a>
          <div class="steps-content">
            <p class="is-size-5">Storage</p>
          </div>
        </li>
        <li class="steps-segment">
          <a v-scroll-to="'#step3'" class="steps-marker">3</a>
          <div class="steps-content">
            <p class="is-size-5">Detail</p>
          </div>
        </li>
      </ul>
      <div class="help is-danger" v-show="validationInitialized && batchSourceMaterialsMessage">
        <!-- eslint-disable-next-line vue/no-parsing-error -->
        {{ batchSourceMaterialsMessage }}
        <br>
        <br>
      </div>
      <!-- MATERIAL TABLE -->
      <dbit-table
        ref="sourceMaterialTable"
        :data="sourceMaterials"
        rowId="PurchaseId"
        checkable
        v-on:updated:checkedRows="updateSelectedSourceMaterials($event)"
      >
        <!-- TODO: Add empty table slot -->
        <template slot="columns" slot-scope="props">
          <!-- ID -->
          <!-- eslint-disable-next-line max-len -->
          <b-table-column
            field="PurchaseId"
            label="ID"
            width="50"
            sortable
          >{{ props.row.PurchaseId }}</b-table-column>
          <!-- NAME -->
          <b-table-column field="PurchaseBatchName" label="Name" sortable>
            <span class="row-content">{{ props.row.PurchaseBatchName }}</span>
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
      <hr v-show="selectedSourceMaterials.length > 0">
      <div class="columns is-multiline">
        <div
          class="column is-half"
          v-for="material in selectedSourceMaterials"
          :key="material.PurchaseId"
        >
          <div class="card">
            <header class="card-header">
              <p class="card-header-title">{{material.PurchaseBatchName}}</p>
            </header>
            <div class="card-content">
              <div class="content">
                <b-field label="Date">{{material.PurchaseDate.toLocaleDateString()}}</b-field>
                <b-field label="Method">
                  <div>
                    <b-radio
                      v-model="material.BurningDownMethod"
                      native-value="volume"
                      @input="() => material.BurningDownMethodInitialized = true"
                      :disabled="material.BurningDownMethod && !(material.QtyGal && material.VBW)"
                    >Volume</b-radio>
                    <b-radio
                      v-model="material.BurningDownMethod"
                      native-value="weight"
                      @input="() => material.BurningDownMethodInitialized = true"
                      :disabled="material.BurningDownMethod && !(material.QtyGal && material.VBW)"
                    >Weight</b-radio>
                  </div>
                </b-field>
                <!-- VOLUME BURNDOWN -->
                <b-field
                  v-if="material.BurningDownMethod === 'volume' || !material.BurningDownMethod"
                  label="Amount Used"
                  :message="errors.first(material.PurchaseId.toString()) ? 'Volume used is required' : (material.UsedQty > material.QtyGal ? 'Amount used exceeds amount available' : '')"
                  :type="errors.first(material.PurchaseId.toString()) || material.UsedQty > material.QtyGal ? 'is-danger' : ''"
                >
                  <b-input
                    v-model="material.UsedQty"
                    v-validate="'required'"
                    :name="material.PurchaseId.toString()"
                    autocomplete="off"
                  ></b-input>
                </b-field>
                <b-field
                  v-if="material.BurningDownMethod === 'volume' || !material.BurningDownMethod"
                  label="Amount Available"
                >
                  <b-input v-model="material.QtyGal" autocomplete="off" disabled></b-input>
                </b-field>
                <!-- WEIGHT BURNDOWN -->
                <b-field
                  v-if="material.BurningDownMethod === 'weight'"
                  label="Amount Used"
                  :message="errors.first(material.PurchaseId.toString()) ? 'Weight used is required' : (material.UsedQty > material.VBW ? 'Amount used exceeds amount available' : '')"
                  :type="errors.first(material.PurchaseId.toString()) || material.UsedQty > material.VBW ? 'is-danger' : ''"
                >
                  <b-input
                    v-model="material.UsedQty"
                    v-validate="'required'"
                    :name="material.PurchaseId.toString()"
                    autocomplete="off"
                  ></b-input>
                </b-field>
                <b-field v-if="material.BurningDownMethod === 'weight'" label="Amount Available">
                  <b-input v-model="material.VBW" autocomplete="off" disabled></b-input>
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
            <p class="is-size-5">Source</p>
          </div>
        </li>
        <li class="steps-segment is-active">
          <a v-scroll-to="'#step2'" class="steps-marker">2</a>
          <div class="steps-content">
            <p class="is-size-3 is-italic">Storage</p>
          </div>
        </li>
        <li class="steps-segment">
          <a v-scroll-to="'#step3'" class="steps-marker">3</a>
          <div class="steps-content">
            <p class="is-size-5">Detail</p>
          </div>
        </li>
      </ul>
      <div class="help is-danger" v-show="validationInitialized && batchStorageMessage">
        <!-- eslint-disable-next-line vue/no-parsing-error -->
        {{ batchStorageMessage }}
        <br>
        <br>
      </div>
      <!-- STORAGE TABLE -->
      <dbit-table
        ref="storageTable"
        :data="storages"
        rowId="StorageId"
        checkable
        v-on:updated:checkedRows="selectedStorages = $event"
      >
        <!-- TODO: Add empty table slot -->
        <template slot="columns" slot-scope="props">
          <!-- ID -->
          <!-- eslint-disable-next-line max-len -->
          <b-table-column field="StorageId" label="ID" width="50" sortable>{{ props.row.StorageId }}</b-table-column>
          <!-- NAME -->
          <b-table-column field="StorageName" label="Name" sortable>
            <span class="row-content">{{ props.row.StorageName }}</span>
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
    </div>
    <!-- STEP 3 -->
    <div class="box" id="step3">
      <!-- TODO: Move steps to the bottom of the box? -->
      <!-- TODO: Add links to labels -->
      <!-- STEPS -->
      <ul class="steps my-step-style has-content-centered has-gaps">
        <li class="steps-segment">
          <a v-scroll-to="'#step1'" class="steps-marker">1</a>
          <div class="steps-content">
            <p class="is-size-5">Source</p>
          </div>
        </li>
        <li class="steps-segment">
          <a v-scroll-to="'#step2'" class="steps-marker">2</a>
          <div class="steps-content">
            <p class="is-size-5">Storage</p>
          </div>
        </li>
        <li class="steps-segment is-active">
          <a v-scroll-to="'#step3'" class="steps-marker">3</a>
          <div class="steps-content">
            <p class="is-size-3 is-italic">Detail</p>
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
        <!-- PRODUCTION START DATE -->
        <b-field
          horizontal
          label="Select Start date"
          :message="errors.first('Batch Start Date') || errors.first('Batch Start Time')"
          :type="errors.first('Batch Start Date') || errors.first('Batch Start Time')? 'is-danger' : ''"
        >
          <b-datepicker
            v-model="batchStartDate"
            v-validate="'required'"
            name="Batch Start Date"
            placeholder="Select Date..."
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
        <!-- PRODUCTION END DATE -->
        <b-field
          horizontal
          label="Select End date"
          :message="errors.first('Batch End Date') || errors.first('Batch End Time')"
          :type="errors.first('Batch End Date') || errors.first('Batch End Time') ? 'is-danger' : ''"
        >
          <b-datepicker
            v-model="batchEndDate"
            v-validate="'required'"
            name="Batch End Date"
            placeholder="Select Date..."
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
        <!-- VOLUME -->
        <b-field
          horizontal
          label="Volume (gal)"
          :message="batchVolumeWeightMessage && validationInitialized ? batchVolumeWeightMessage : ''"
          :type="batchVolumeWeightMessage && validationInitialized ? 'is-danger' : ''"
        >
          <!-- eslint-disable-next-line max-len -->
          <b-input v-model="batchVolume" autocomplete="off"></b-input>
        </b-field>
        <!-- WEIGHT -->
        <b-field
          horizontal
          label="Weight (lb)"
          :message="batchVolumeWeightMessage && validationInitialized ? batchVolumeWeightMessage : ''"
          :type="batchVolumeWeightMessage && validationInitialized ? 'is-danger' : ''"
        >
          <!-- eslint-disable-next-line max-len -->
          <b-input v-model="batchWeight" autocomplete="off"></b-input>
        </b-field>
        <!-- ALCOHOL CONTENT -->
        <b-field
          horizontal
          label="Alcohol Content"
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
        <!-- PROOF GALLONS -->
        <b-field
          horizontal
          label="Proof"
          :message="errors.first('Proof')"
          :type="errors.first('Proof') ? 'is-danger' : ''"
        >
          <!-- eslint-disable-next-line max-len -->
          <b-input v-model="batchProof" v-validate="'required'" name="Proof" autocomplete="off"></b-input>
        </b-field>
        <!-- SPIRIT TYPE -->
        <b-field
          horizontal
          label="Reporting Spirit Type"
          :message="errors.first('Reporting Spirit Type')"
          :type="errors.first('Reporting Spirit Type') ? 'is-danger' : ''"
        >
          <!-- eslint-disable-next-line max-len -->
          <b-select
            v-model="batchReportingSpiritTypeId"
            placeholder="Select a Reporting Spirit Type"
            v-validate="'required'"
            name="Reporting Spirit Type"
          >
            <!-- eslint-disable-next-line max-len -->
            <option
              v-for="type in reportingSpiritTypes"
              :value="type.SpiritTypeReportingID"
              :key="type.SpiritTypeReportingID"
            >{{ type.ProductTypeName }}</option>
          </b-select>
        </b-field>
        <!-- NOTE -->
        <b-field horizontal label="Note">
          <!-- eslint-disable-next-line max-len -->
          <textarea v-model="batchNote" class="textarea"></textarea>
        </b-field>
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

export default {
  name: 'AddFermntationView',
  components: {
    DbitTable,
  },
  created() {
    this.$store.dispatch('production/getRawMaterialsForFermentation');
    this.$store.dispatch('production/getStorages');
    this.$store.dispatch('purchase/getReportingSpiritTypes');
  },
  computed: {
    sourceMaterials() {
      return this.$store.state.production.rawMaterialsForFermentation || [{}];
    },
    storages() {
      return this.$store.state.production.storages || [{}];
    },
    reportingSpiritTypes() {
      return this.$store.state.purchase.reportingSpiritTypes
        ? this.$store.state.purchase.reportingSpiritTypes.filter(item =>
          item.ProductTypeName === 'Other' ||
              item.ProductTypeName === 'Wine')
        : [{}];
    },
    batchSourceMaterialsMessage() {
      return this.selectedSourceMaterials &&
        this.selectedSourceMaterials.length > 0
        ? ''
        : 'The Material Type selection is required.';
    },
    batchStorageMessage() {
      return this.selectedStorages.length > 0
        ? ''
        : 'The Storage selection is required.';
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
      selectedSourceMaterials: [],
      selectedStorages: [],
      batchName: '',
      batchStartDate: null,
      batchStartTime: null,
      batchEndDate: null,
      batchEndTime: null,
      batchVolume: '',
      batchWeight: '',
      batchAlcohol: '',
      batchProof: '',
      batchPrice: '',
      batchReportingSpiritTypeId: null,
      batchNote: '',
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
          !this.batchSourceMaterialsMessage &&
          !this.batchStorageMessage &&
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
    submit() {
      this.selectedSourceMaterials.map((item) => {
        if (item.BurningDownMethod === 'volume') {
          // eslint-disable-next-line no-param-reassign
          item.OldVal = item.QtyGal - item.UsedQty;
          // eslint-disable-next-line no-param-reassign
          item.NewVal = item.QtyGal;
        } else if (item.BurningDownMethod === 'weight') {
          // eslint-disable-next-line no-param-reassign
          item.OldVal = item.VBW - item.UsedQty;
          // eslint-disable-next-line no-param-reassign
          item.NewVal = item.VBW;
        } else {
          throw new Error('Source materials contain item without selected burndown method!');
        }
        return item;
      });
      const start = new Date(
        this.batchStartDate.getFullYear(),
        this.batchStartDate.getMonth(),
        this.batchStartDate.getDate(),
        this.batchStartTime.getHours(),
        this.batchStartTime.getMinutes(),
        this.batchStartTime.getSeconds(),
        0,
      );
      const end = new Date(
        this.batchEndDate.getFullYear(),
        this.batchEndDate.getMonth(),
        this.batchEndDate.getDate(),
        this.batchEndTime.getHours(),
        this.batchEndTime.getMinutes(),
        this.batchEndTime.getSeconds(),
        0,
      );
      const production = {
        ProductionType: 'Fermentation',
        Storage: this.selectedStorages,
        BatchName: `PROD-${this.batchName}`,
        ProductionDate: dateHelper.convertFromUTC(start, true),
        ProdcutionEnd: dateHelper.convertFromUTC(end, true),
        ProductionStart: dateHelper.convertFromUTC(start, true),
        Quantity: parseFloat(this.batchVolume),
        VolumeByWeight: parseFloat(this.batchWeight),
        AlcoholContent: parseFloat(this.batchAlcohol),
        ProofGallon: parseFloat(this.batchProof),
        SpiritTypeReportingID: this.batchReportingSpiritTypeId,
        Gauged: true,
        UsedMats: this.selectedSourceMaterials,
        Note: this.batchNote,
      };
      this.$store
        .dispatch('production/createProduction', production)
        .then(() => {
          this.clear();
          this.$toast.open({
            duration: 5000,
            message: `Successfully created fermentation: ${
              production.BatchName
            }`,
            type: 'is-success',
          });
          this.$store.dispatch('production/getRawMaterialsForFermentation');
          this.$store.dispatch('production/getStorages');
        })
        .catch(() => {
          this.$toast.open({
            duration: 5000,
            message: `Unable to create fermentation: ${production.BatchName}`,
            type: 'is-danger',
          });
        });
    },
    clear() {
      // TODO: Set values to null rather than undefined
      // in all dictionary workflows, test out validation
      this.$refs.sourceMaterialTable.clearSelected();
      this.$refs.storageTable.clearSelected();
      this.batchName = '';
      this.batchStartDate = null;
      this.batchStartTime = null;
      this.batchEndDate = null;
      this.batchEndTime = null;
      this.batchVolume = '';
      this.batchWeight = '';
      this.batchAlcohol = '';
      this.batchProof = '';
      this.batchPrice = '';
      this.batchReportingSpiritTypeId = null;
      this.batchNote = '';
      this.$validator.reset();
      this.validationInitialized = false;
    },
    updateSelectedSourceMaterials(items) {
      // Clear used source quantities
      items.map((item) => {
        // eslint-disable-next-line no-param-reassign
        if (item.UsedQty) item.UsedQty = null;
        // eslint-disable-next-line no-param-reassign
        item.ID = item.PurchaseId;
        if (
          (!item.BurningDownMethod && item.QtyGal && item.VBW) ||
          (!item.BurningDownMethod && item.QtyGal && !item.VBW)
        ) {
          // eslint-disable-next-line no-param-reassign
          item.BurningDownMethod = 'volume';
        } else if (!item.BurningDownMethod && !item.QtyGal && item.VBW) {
          // eslint-disable-next-line no-param-reassign
          item.BurningDownMethod = 'weight';
        }
        return items;
      });
      this.selectedSourceMaterials = items;
    },
  },
  watch: {
    // eslint-disable-next-line no-unused-vars
    batchAlcohol(newVal, oldVal) {
      this.batchProof = this.batchVolume
        ? (parseInt(this.batchVolume, 10) * parseInt(newVal, 10) * 2) / 100
        : '';
    },
  },
};
</script>
