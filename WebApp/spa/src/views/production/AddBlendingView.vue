<template>
  <div class="container">
    <!-- STEP 1 -->
    <div class="box" id="step1">
      <!-- STEPS -->
      <ul class="steps my-step-style has-content-centered has-gaps">
        <li class="steps-segment is-active">
          <a v-scroll-to="'#step1'" class="steps-marker">1</a>
          <div class="steps-content">
            <p class="is-size-3 is-italic">Additives</p>
          </div>
        </li>
        <li class="steps-segment">
          <a v-scroll-to="'#step2'" class="steps-marker">2</a>
          <div class="steps-content">
            <p class="is-size-5">Source</p>
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
      <div class="help is-danger" v-show="validationInitialized && batchSourceAdditivesMessage">
        <!-- eslint-disable-next-line vue/no-parsing-error -->
        {{ batchSourceAdditivesMessage }}
        <br>
        <br>
      </div>
      <!-- MATERIAL TABLE -->
      <dbit-table
        ref="sourceAdditiveTable"
        :data="sourceAdditives"
        rowId="RawMaterialId"
        checkable
        v-on:updated:checkedRows="updateSelectedAdditives($event)"
      >
        <!-- TODO: Add empty table slot -->
        <template slot="columns" slot-scope="props">
          <!-- ID -->
          <!-- eslint-disable-next-line max-len -->
          <b-table-column field="RawMaterialId" label="ID" width="50" sortable>{{ props.row.RawMaterialId }}</b-table-column>
          <!-- NAME -->
          <b-table-column field="RawMaterialName" label="Name" sortable>
            <span class="row-content">{{ props.row.RawMaterialName }}</span>
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
      <hr v-show="selectedSourceAdditives.length > 0">
      <div class="columns is-multiline">
        <div
          class="column is-half"
          v-for="additive in selectedSourceAdditives"
          :key="additive.RawMaterialId"
        >
          <div class="card">
            <header class="card-header">
              <p class="card-header-title">{{additive.RawMaterialName}}</p>
            </header>
            <div class="card-content">
              <div class="content">
                <!-- ADDITIVE QUANTITY -->
                <b-field
                  label="Quantity"
                  :message="errors.first(additive.RawMaterialId.toString()) ? 'Quantity is required' : ''"
                  :type="errors.first(additive.RawMaterialId.toString()) ? 'is-danger' : ''"
                >
                  <b-input
                    v-model="additive.UsedQty"
                    v-validate="'required'"
                    :name="additive.RawMaterialId.toString()"
                    autocomplete="off"
                  ></b-input>
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
            <p class="is-size-5">Additives</p>
          </div>
        </li>
        <li class="steps-segment is-active">
          <a v-scroll-to="'#step2'" class="steps-marker">2</a>
          <div class="steps-content">
            <p class="is-size-3 is-italic">Source</p>
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
        rowId="RecordId"
        checkable
        v-on:updated:checkedRows="updateSelectedSourceMaterials($event)"
      >
        <!-- TODO: Add empty table slot -->
        <template slot="columns" slot-scope="props">
          <!-- ID -->
          <!-- eslint-disable-next-line max-len -->
          <b-table-column field="RecordId" label="ID" width="50" sortable>{{ props.row.RecordId }}</b-table-column>
          <!-- NAME -->
          <b-table-column field="BatchName" label="Name" sortable>
            <span class="row-content">{{ props.row.BatchName }}</span>
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
          :key="material.RecordId"
        >
          <div class="card">
            <header class="card-header">
              <p class="card-header-title">{{material.BatchName}}</p>
            </header>
            <div class="card-content">
              <div class="content">
                <!-- TODO: Need to dateHelper.convertFromUTC in getMaterialsForProduction date for corresponding material type, also handle if unavailable -->
                <b-field label="Date">{{material.PurchaseDate ? material.PurchaseDate.toLocaleDateString() : ''}}</b-field>
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
                  :message="errors.first(material.RecordId.toString()) ? 'Volume used is required' : (material.UsedQty > material.Quantity ? 'Amount used exceeds amount available' : '')"
                  :type="errors.first(material.RecordId.toString()) || material.UsedQty > material.Quantity ? 'is-danger' : ''"
                >
                  <b-input
                    v-model="material.UsedQty"
                    v-validate="'required'"
                    :name="material.RecordId.toString()"
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
                  :message="errors.first(material.RecordId.toString()) ? 'Weight used is required' : (material.UsedQty > material.VolumeByWeight ? 'Amount used exceeds amount available' : '')"
                  :type="errors.first(material.RecordId.toString()) || material.UsedQty > material.VolumeByWeight ? 'is-danger' : ''"
                >
                  <b-input
                    v-model="material.UsedQty"
                    v-validate="'required'"
                    :name="material.RecordId.toString()"
                    autocomplete="off"
                  ></b-input>
                </b-field>
                <b-field v-if="material.BurningDownMethod === 'weight'" label="Amount Available">
                  <b-input v-model="material.VolumeByWeight" autocomplete="off" disabled></b-input>
                </b-field>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    <!-- STEP 3 -->
    <div class="box" id="step3">
      <!-- STEPS -->
      <ul class="steps my-step-style has-content-centered has-gaps">
        <li class="steps-segment">
          <a v-scroll-to="'#step1'" class="steps-marker">1</a>
          <div class="steps-content">
            <p class="is-size-5">Additives</p>
          </div>
        </li>
        <li class="steps-segment">
          <a v-scroll-to="'#step2'" class="steps-marker">2</a>
          <div class="steps-content">
            <p class="is-size-5">Source</p>
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
            <p class="is-size-5">Additives</p>
          </div>
        </li>
        <li class="steps-segment">
          <a v-scroll-to="'#step2'" class="steps-marker">2</a>
          <div class="steps-content">
            <p class="is-size-5">Source</p>
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
          label="Spirit Type"
          :message="errors.first('Spirit Type')"
          :type="errors.first('Spirit Type') ? 'is-danger' : ''"
        >
          <!-- eslint-disable-next-line max-len -->
          <b-select
            v-model="batchSpiritType"
            placeholder="Select a Spirit Type"
            v-validate="'required'"
            name="Spirit Type"
          >
            <!-- eslint-disable-next-line max-len -->
            <option
              v-for="type in spiritTypes"
              :value="type"
              :key="type.SpiritId"
            >{{ type.SpiritName }}</option>
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
  name: 'AddBlendingView',
  components: {
    DbitTable,
  },
  created() {
    this.$store.dispatch('production/getAdditives', 'Additive');
    this.$store.dispatch('production/getMaterialsForProduction', 'blending');
    this.$store.dispatch('production/getStorages');
    this.$store.dispatch('production/getSpiritTypes');
  },
  computed: {
    sourceAdditives() {
      return this.$store.state.production.additives || [{}];
    },
    sourceMaterials() {
      return this.$store.state.production.materialsForProduction || [{}];
    },
    storages() {
      return this.$store.state.production.storages || [{}];
    },
    spiritTypes() {
      return this.$store.state.production.spiritTypes || [{}];
    },
    batchSourceAdditivesMessage() {
      return this.selectedSourceMaterials &&
        this.selectedSourceMaterials.length > 0
        ? ''
        : 'The Additive selection is required.';
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
      selectedSourceAdditives: [],
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
      batchSpiritType: null,
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
          !this.batchSourceAdditivesMessage &&
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
      this.selectedSourceAdditives.map((item) => {
        // eslint-disable-next-line no-param-reassign
        item.RawMaterialQuantity = parseFloat(item.UsedQty);
        // eslint-disable-next-line no-param-reassign
        item.UnitOfMeasurement = item.UnitType;
        return item;
      });
      this.selectedSourceMaterials.map((item) => {
        // eslint-disable-next-line no-param-reassign
        item.UsedQty = parseFloat(item.UsedQty);
        if (item.BurningDownMethod === 'volume') {
          // eslint-disable-next-line no-param-reassign
          item.OldVal = item.Quantity - item.UsedQty;
          // eslint-disable-next-line no-param-reassign
          item.NewVal = item.Quantity;
        } else if (item.BurningDownMethod === 'weight') {
          // eslint-disable-next-line no-param-reassign
          item.OldVal = item.VolumeByWeight - item.UsedQty;
          // eslint-disable-next-line no-param-reassign
          item.NewVal = item.VolumeByWeight;
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
        ProductionType: 'Blending',
        Storage: this.selectedStorages,
        BatchName: `PROD-${this.batchName}`,
        ProductionDate: dateHelper.convertFromUTC(start, true),
        ProdcutionEnd: dateHelper.convertFromUTC(end, true),
        ProductionStart: dateHelper.convertFromUTC(start, true),
        Quantity: parseFloat(this.batchVolume) || 0,
        VolumeByWeight: parseFloat(this.batchWeight) || 0,
        AlcoholContent: parseFloat(this.batchAlcohol),
        ProofGallon: parseFloat(this.batchProof),
        Gauged: true,
        BlendingAdditives: this.selectedSourceAdditives,
        UsedMats: this.selectedSourceMaterials,
        SpiritId: this.batchSpiritType.SpiritId,
        SpiritName: this.batchSpiritType.SpiritName,
        Note: this.batchNote,
      };
      this.$store
        .dispatch('production/createProduction', production)
        .then(() => {
          this.clear();
          this.$toast.open({
            duration: 5000,
            message: `Successfully created blending: ${
              production.BatchName
            }`,
            type: 'is-success',
          });
          this.$store.dispatch('production/getAdditives', 'Additive');
          this.$store.dispatch('production/getMaterialsForProduction', 'blending');
          this.$store.dispatch('production/getStorages');
        })
        .catch(() => {
          this.$toast.open({
            duration: 5000,
            message: `Unable to create blending: ${production.BatchName}`,
            type: 'is-danger',
          });
        });
    },
    clear() {
      // TODO: Set values to null rather than undefined
      // in all dictionary workflows, test out validation
      this.$refs.sourceAdditiveTable.clearSelected();
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
      this.batchSpiritType = null;
      this.batchNote = '';
      this.$validator.reset();
      this.validationInitialized = false;
    },
    updateSelectedAdditives(items) {
      items.map((item) => {
        // eslint-disable-next-line no-param-reassign
        if (item.UsedQty) item.UsedQty = null;
        return item;
      });
      this.selectedSourceAdditives = items;
    },
    updateSelectedSourceMaterials(items) {
      // Clear used source quantities
      items.map((item) => {
        // eslint-disable-next-line no-param-reassign
        if (item.UsedQty) item.UsedQty = null;
        if (item.DistillableOrigin === 'prod') {
          // eslint-disable-next-line no-param-reassign
          item.ID = item.ProductionId;
        } else if (item.DistillableOrigin === 'pur') {
          // eslint-disable-next-line no-param-reassign
          item.ID = item.PurchaseId;
        }
        if (
          (!item.BurningDownMethod && item.Quantity && item.VolumeByWeight) ||
          (!item.BurningDownMethod && item.Quantity && !item.VolumeByWeight)
        ) {
          // eslint-disable-next-line no-param-reassign
          item.BurningDownMethod = 'volume';
        } else if (!item.BurningDownMethod && !item.Quantity && item.VolumeByWeight) {
          // eslint-disable-next-line no-param-reassign
          item.BurningDownMethod = 'weight';
        }
        return item;
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
