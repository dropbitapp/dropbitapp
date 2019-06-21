<template>
  <div class="container">
    <section class="hero is-primary">
      <div class="hero-body">
        <div class="container is-bold">
          <h1 class="title">Sales</h1>
          <h2 class="subtitle">
            Record your sales here
          </h2>
        </div>
      </div>
    </section>
    <div class="box">
      <div class="help is-danger"
        v-show="validationInitialized
        && batchQuantityTypeMessage">
        <!-- eslint-disable-next-line vue/no-parsing-error -->
        {{ batchQuantityTypeMessage }}
        <br>
        <br>
      </div>
      <!-- BOTTLINGS TABLE -->
      <dbit-table
        ref="QuantityTypeTable"
        :data="bottlingList"
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
        <b-table-column field="BatchName" label="Bottling Batch Name" sortable>
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
              <!-- BOTTLING SALE DATE -->
              <b-field
                label="Sale Date"
                :message="errors.first('Sale Date')"
                :type="errors.first('Sale Date') ? 'is-danger' : ''"
              >
                <b-datepicker
                  v-model="material.ProductionEndDate"
                  v-validate="'required'"
                  name="Sale Date"
                  placeholder="Click to select..."
                  icon="calendar-today"
                ></b-datepicker>
              </b-field>
              <!-- PFG BURNDOWN -->
              <b-field
                label="PFG Sold"
                :message="errors.first(material.ProductionId.toString())
                ? 'PFG Sold  is required'
                : (material.TaxedProof > material.ProofGallon
                ? 'Amount used exceeds amount available' : '')"
                :type="errors.first(material.ProductionId.toString())
                || material.TaxedProof > material.ProofGallon
                ? 'is-danger' : ''">
              <b-input
                v-model="material.TaxedProof"
                v-validate="'required'"
                :name="material.ProductionId.toString()"
                autocomplete="off"
              ></b-input>
              </b-field>
              <b-field
                label="Amount Available">
                <b-input v-model="material.ProofGallon"
                autocomplete="off" disabled>
                </b-input>
              </b-field>
            </div>
          </div>
        </div>
      </div>
    </div>
    </div>
       <form @submit.prevent="validateBeforeSubmit">
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
</template>

<script>
import DbitTable from '../../components/DbitTable.vue';
import dateHelper from '../../helpers/date-helper';
import distillMathHelper from '../../helpers/math-helper';

export default {
  name: 'SellView',
  props: {
  },
  components: {
    DbitTable,
  },
  created() {
    this.$store.dispatch('production/getProductions', 'Bottling');
  },
  computed: {
    bottlingList() {
      return this.$store.state.production.bottlings || [{}];
    },
  },
  data() {
    return {
      selectedMaterials: [],
      validationInitialized: false,
    };
  },
  methods: {
    validateBeforeSubmit() { /* eslint-disable max-len */
      this.validationInitialized = true;
      this.$validator.validateAll().then((result) => {
        if (result) {
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
    const taxedRecords = [];
      this.selectedMaterials.map((item) => {
        const TaxedRecord = {
          TaxedProof: null,
          WithdrawalDate: null,
          ProofGallon: null,
          ProductionId: null,
        };
        TaxedRecord.ProofGallon = item.ProofGallon - item.TaxedProof;
        TaxedRecord.WithdrawalDate = item.WithdrawalDate;
        TaxedRecord.TaxedProof = item.TaxedProof;
        TaxedRecord.ProductionId = item.ProductionId;
        taxedRecords.push(TaxedRecord);
      });
      const recordsList = {
        TaxedRecords: taxedRecords,
      }
      this.$store
        .dispatch('inventory/createTaxRecords', recordsList)
        .then(() => {
          this.$store.dispatch('production/getProductions', 'Bottling');
          this.clear();
          this.$toast.open({
            duration: 5000,
            message: `Successfully created tax record`,
            type: 'is-success',
          });
        })
        .catch((e) => {
          this.$toast.open({
            duration: 5000,
            message: `Unable to create tax records`,
            type: 'is-danger',
          });
        });
    },
    updateSelectedMaterials(items) {
      /* eslint-disable-next-line no-param-reassign */
      this.selectedMaterials = items;
    },
    clear() {
      this.selectedMaterials = [];
      this.$validator.reset();
      this.validationInitialized = false;
    },
  },
  watch: {
    /**
     * update withdrawal date in selectedMaterials object
     */
    selectedMaterials(newVal, prevVal) {
      deep: true,
      newVal.map((item) => {
        if (item.ProductionEnd)
          item.WithdrawalDate = item.ProductionEndDate;
        return newVal;
      });
      this.selectedMaterials = newVal;
    },
  },
};
</script>
