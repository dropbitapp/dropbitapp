<template>
  <div class="container">
    <!-- STEP 1 -->
    <div class="box" id="step1">
      <!-- STEPS -->
      <ul class="steps my-step-style has-content-centered has-gaps">
        <li class="steps-segment is-active">
          <a v-scroll-to="'#step1'" class="steps-marker">1</a>
          <div class="steps-content">
            <p class="is-size-3 is-italic">Material Type</p>
          </div>
        </li>
        <li class="steps-segment">
          <a v-scroll-to="'#step2'" class="steps-marker">2</a>
          <div class="steps-content">
            <p class="is-size-5">Vendor</p>
          </div>
        </li>
        <li class="steps-segment">
          <a v-scroll-to="'#step3'" class="steps-marker">3</a>
          <div class="steps-content">
            <p class="is-size-5">Storage</p>
          </div>
        </li>
        <li class="steps-segment">
          <a v-scroll-to="'#step4'" class="steps-marker">4</a>
          <div class="steps-content">
            <p class="is-size-5">Detail</p>
          </div>
        </li>
      </ul>
      <div class="help is-danger" v-show="validationInitialized && batchMaterialTypeMessage">
        <!-- eslint-disable-next-line vue/no-parsing-error -->
        {{ batchMaterialTypeMessage }}
        <br>
        <br>
      </div>
      <!-- MATERIAL TYPE TABLE -->
      <dbit-table
        ref="materialTypeTable"
        :data="rawMaterials"
        rowId="RawMaterialId"
        selectable
        v-on:updated:selectedRow="selectedMaterialType = $event"
      >
        <!-- TODO: Add empty table slot -->
        <template slot="columns" slot-scope="props">
          <!-- ID -->
          <b-table-column
            field="RawMaterialId"
            label="ID"
            width="50"
            sortable
          >{{ props.row.RawMaterialId }}</b-table-column>
          <!-- NAME -->
          <b-table-column field="RawMaterialName" label="Name" sortable>
            <span class="row-content">{{ props.row.RawMaterialName }}</span>
          </b-table-column>
          <!-- UNIT TYPE -->
          <b-table-column field="UnitType" label="Unit" sortable>
            <span class="row-content">{{ props.row.UnitType }}</span>
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
    <!-- STEP 2 -->
    <div class="box" id="step2">
      <!-- STEPS -->
      <ul class="steps my-step-style has-content-centered has-gaps">
        <li class="steps-segment">
          <a v-scroll-to="'#step1'" class="steps-marker">1</a>
          <div class="steps-content">
            <p class="is-size-5">Material Type</p>
          </div>
        </li>
        <li class="steps-segment is-active">
          <a v-scroll-to="'#step2'" class="steps-marker">2</a>
          <div class="steps-content">
            <p class="is-size-3 is-italic">Vendor</p>
          </div>
        </li>
        <li class="steps-segment">
          <a v-scroll-to="'#step3'" class="steps-marker">3</a>
          <div class="steps-content">
            <p class="is-size-5">Storage</p>
          </div>
        </li>
        <li class="steps-segment">
          <a v-scroll-to="'#step4'" class="steps-marker">4</a>
          <div class="steps-content">
            <p class="is-size-5">Detail</p>
          </div>
        </li>
      </ul>
      <div class="help is-danger" v-show="validationInitialized && batchVendorMessage">
        <!-- eslint-disable-next-line vue/no-parsing-error -->
        {{ batchVendorMessage }}
        <br>
        <br>
      </div>
      <!-- VENDOR TABLE -->
      <dbit-table
        ref="vendorTable"
        :data="vendors"
        rowId="VendorId"
        selectable
        v-on:updated:selectedRow="selectedVendor = $event"
      >
        <!-- TODO: Add empty table slot -->
        <template slot="columns" slot-scope="props">
          <!-- ID -->
          <!-- eslint-disable-next-line max-len -->
          <b-table-column field="VendorId" label="ID" width="50" sortable>{{ props.row.VendorId }}</b-table-column>
          <!-- NAME -->
          <b-table-column field="VendorName" label="Name" sortable>
            <span class="row-content">{{ props.row.VendorName }}</span>
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
      <!-- STEPS -->
      <ul class="steps my-step-style has-content-centered has-gaps">
        <li class="steps-segment">
          <a v-scroll-to="'#step1'" class="steps-marker">1</a>
          <div class="steps-content">
            <p class="is-size-5">Material Type</p>
          </div>
        </li>
        <li class="steps-segment">
          <a v-scroll-to="'#step2'" class="steps-marker">2</a>
          <div class="steps-content">
            <p class="is-size-5 is-italic">Vendor</p>
          </div>
        </li>
        <li class="steps-segment is-active">
          <a v-scroll-to="'#step3'" class="steps-marker">3</a>
          <div class="steps-content">
            <p class="is-size-3 is-italic">Storage</p>
          </div>
        </li>
        <li class="steps-segment">
          <a v-scroll-to="'#step4'" class="steps-marker">4</a>
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
    <!-- STEP 4 -->
    <div class="box" id="step4">
      <!-- TODO: Move steps to the bottom of the box? -->
      <!-- TODO: Add links to labels -->
      <!-- STEPS -->
      <ul class="steps my-step-style has-content-centered has-gaps">
        <li class="steps-segment">
          <a v-scroll-to="'#step1'" class="steps-marker">1</a>
          <div class="steps-content">
            <p class="is-size-5">Material Type</p>
          </div>
        </li>
        <li class="steps-segment">
          <a v-scroll-to="'#step2'" class="steps-marker">2</a>
          <div class="steps-content">
            <p class="is-size-5">Vendor</p>
          </div>
        </li>
        <li class="steps-segment">
          <a v-scroll-to="'#step3'" class="steps-marker">3</a>
          <div class="steps-content">
            <p class="is-size-5">Storage</p>
          </div>
        </li>
        <li class="steps-segment is-active">
          <a v-scroll-to="'#step4'" class="steps-marker">4</a>
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
        <!-- PURCHASE DATE -->
        <b-field
          horizontal
          label="Select a date"
          :message="errors.first('Batch Date')"
          :type="errors.first('Batch Date') ? 'is-danger' : ''"
        >
          <b-datepicker
            v-model="batchDate"
            v-validate="'required'"
            name="Batch Date"
            placeholder="Click to select..."
            icon="calendar-today"
          ></b-datepicker>
        </b-field>
        <!-- VOLUME -->
        <b-field
          horizontal
          label="Volume (gal)"
          :message="batchVolumeWeightMessage && validationInitialized ?
            batchVolumeWeightMessage : ''"
          :type="batchVolumeWeightMessage && validationInitialized ?
            'is-danger' : ''"
        >
          <!-- eslint-disable-next-line max-len -->
          <b-input v-model="batchVolume" autocomplete="off"></b-input>
        </b-field>
        <!-- WEIGHT -->
        <b-field
          horizontal
          label="Weight (lb)"
          :message="batchVolumeWeightMessage && validationInitialized ?
            batchVolumeWeightMessage : ''"
          :type="batchVolumeWeightMessage && validationInitialized ?
            'is-danger' : ''"
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
        <!-- PRICE -->
        <b-field
          horizontal
          label="Price"
          :message="errors.first('Price')"
          :type="errors.first('Price') ? 'is-danger' : ''"
        >
          <!-- eslint-disable-next-line max-len -->
          <b-input v-model="batchPrice" v-validate="'required'" name="Price" autocomplete="off"></b-input>
        </b-field>
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
            <option v-for="type in reportingSpiritTypes" :value="type.SpiritTypeReportingID" :key="type.SpiritTypeReportingID">{{ type.ProductTypeName }}</option>
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
  name: 'AddFermentedView',
  components: {
    DbitTable,
  },
  created() {
    this.$store.dispatch('purchase/getRawMaterialsForPurchase', 'Fermented');
    this.$store.dispatch('purchase/getPurchaseVendors');
    this.$store.dispatch('purchase/getPurchaseStorages');
    this.$store.dispatch('purchase/getReportingSpiritTypes');
  },
  computed: {
    rawMaterials() {
      return this.$store.state.purchase.rawMaterials || [{}];
    },
    vendors() {
      return this.$store.state.purchase.purchaseVendors || [{}];
    },
    storages() {
      return this.$store.state.purchase.purchaseStorages || [{}];
    },
    reportingSpiritTypes() {
      return this.$store.state.purchase.reportingSpiritTypes ? this.$store.state.purchase.reportingSpiritTypes.filter(type => type.ProductTypeName === 'Other' || type.ProductTypeName === 'Wine') : [{}];
    },
    batchMaterialTypeMessage() {
      return this.selectedMaterialType &&
        this.selectedMaterialType.RawMaterialId
        ? ''
        : 'The Material Type selection is required.';
    },
    batchVendorMessage() {
      return this.selectedVendor && this.selectedVendor.VendorId
        ? ''
        : 'The Vendor selection is required.';
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
      selectedMaterialType: {},
      selectedVendor: {},
      selectedStorages: [],
      batchName: '',
      batchDate: null,
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
          !this.batchMaterialTypeMessage &&
          !this.batchVendorMessage &&
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
      const purchase = {
        PurchaseType: 'Fermented',
        RecordName: this.selectedMaterialType.RawMaterialName,
        RecordId: this.selectedMaterialType.RawMaterialId,
        VendorName: this.selectedVendor.VendorName,
        VendorId: this.selectedVendor.VendorId,
        Storage: this.selectedStorages,
        PurBatchName: `PUR-${this.batchName}`,
        PurchaseDate: dateHelper.convertFromUTC(this.batchDate, true),
        Quantity: this.batchVolume,
        VolumeByWeight: this.batchWeight,
        AlcoholContent: this.batchAlcohol,
        ProofGallon: this.batchProof,
        SpiritTypeReportingID: this.batchReportingSpiritTypeId,
        Price: this.batchPrice,
        Note: this.batchNote,
      };
      this.$store
        .dispatch('purchase/createPurchase', purchase)
        .then(() => {
          this.clear();
          this.$toast.open({
            duration: 5000,
            message: `Successfully created fermented: ${purchase.PurBatchName}`,
            type: 'is-success',
          });
        })
        .catch(() => {
          this.$toast.open({
            duration: 5000,
            message: `Unable to create fermented: ${purchase.PurBatchName}`,
            type: 'is-danger',
          });
        });
    },
    clear() {
      // TODO: Set values to null rather than undefined
      // in all dictionary workflows, test out validation
      this.$refs.materialTypeTable.clearSelected();
      this.$refs.vendorTable.clearSelected();
      this.$refs.storageTable.clearSelected();
      this.batchName = '';
      this.batchDate = null;
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
  },
  watch: {
    // eslint-disable-next-line no-unused-vars
    batchAlcohol(newVal, oldVal) {
      this.batchProof = this.batchVolume ? ((parseInt(this.batchVolume, 10) * parseInt(newVal, 10) * 2) / 100) : '';
    },
  },
};
</script>
