<template>
  <div class="container">
    <div class="box">
      <dbit-table :data="bottling" rowId="ProductionId">
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
          <b-table-column field="BatchName" label="Name" sortable>
            <span class="row-content">{{ props.row.BatchName }}</span>
          </b-table-column>
          <!-- DATE -->
          <b-table-column field="ProductionDate" label="Date" sortable centered>
            <span class="row-content">
              <span class="tag is-success">
                {{ new Date(props.row.ProductionDate).toLocaleDateString() }}
              </span>
            </span>
          </b-table-column>
          <!-- SPIRIT NAME -->
          <b-table-column field="SpiritName" label="Spirit" sortable>
            <span class="row-content">
              <span class="row-content">{{ props.row.SpiritName }}</span>
            </span>
          </b-table-column>
          <!-- VOLUME(GAL) -->
          <b-table-column field="Quantity" label="Gal" sortable>
            <template v-if="props.row.Quantity">
              <span class="row-content">{{ props.row.Quantity }}</span>
            </template>
            <template v-else>-</template>
          </b-table-column>
          <!-- PROOF -->
          <b-table-column field="ProofGallon" label="Proof" sortable>
            <template v-if="props.row.ProofGallon">
              <span class="row-content">{{ props.row.ProofGallon }}</span>
            </template>
            <template v-else>-</template>
          </b-table-column>
          <!-- ALCOHOL CONTENT -->
          <b-table-column field="AlcoholContent" label="Alc%" sortable>
            <template v-if="props.row.AlcoholContent">
              <span class="row-content">{{ props.row.AlcoholContent }}</span>
            </template>
            <template v-else>-</template>
          </b-table-column>
          <!-- DELETE -->
          <b-table-column>
            <!-- TODO: Add confirm snackbar at the top -->
            <button class="button is-small is-primary is-pulled-right" @click="deleteItem(props.row)">
              <b-icon icon="delete-outline"></b-icon>
            </button>
          </b-table-column>
        </template>
        <template slot="detail" slot-scope="props">
          <!-- TODO: Add Spirit Type Reporting -->
          <!-- TODO: Display Storage -->
          <div class="has-text-weight-bold">Case Capacity</div>
          <div v-if="props.row.BottlingInfo.CaseCapacity">
            <p>{{ props.row.BottlingInfo.CaseCapacity }}</p>
          </div>
          <div v-else>
            <p>-</p>
          </div>
          <br>
          <div class="has-text-weight-bold">Case Quantity</div>
          <div v-if="props.row.BottlingInfo.CaseQuantity">
            <p>{{ props.row.BottlingInfo.CaseQuantity }}</p>
          </div>
          <div v-else>
            <p>-</p>
          </div>
          <br>
          <div class="has-text-weight-bold">Bottles</div>
          <div v-if="props.row.BottlingInfo.BottleQuantity">
            <p>{{ props.row.BottlingInfo.BottleQuantity }}</p>
          </div>
          <div v-else>
            <p>-</p>
          </div>
          <br>
          <div class="has-text-weight-bold">Bottle Capacity</div>
          <div v-if="props.row.BottlingInfo.BottleCapacity">
            <p>{{ props.row.BottlingInfo.BottleCapacity }}</p>
          </div>
          <div v-else>
            <p>-</p>
          </div>
          <br>
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
  </div>
</template>

<script>
import DbitTable from '../../components/DbitTable.vue';

export default {
  name: 'BottlingView',
  components: {
    DbitTable,
  },
  created() {
    this.$store.dispatch('production/getProductions', 'Bottling');
  },
  computed: {
    bottling() {
      // TODO: Refactor || empty object array
      if (this.$store.state.production.bottlings) {
        return this.$store.state.production.bottlings;
      }
      return [{}];
    },
  },
  methods: {
    deleteItem(production) {
      this.$store
        .dispatch('production/deleteProduction', {
          workflow: 'Bottling',
          id: production.ProductionId,
        })
        .then(() => {
          this.$toast.open({
            duration: 5000,
            message: `Successfully deleted bottling: ${production.BatchName}`,
            type: 'is-success',
          });
        })
        .catch(() => {
          this.$toast.open({
            duration: 5000,
            message: `Unable to delete bottling: ${production.BatchName}`,
            type: 'is-danger',
          });
        });
    },
  },
};
</script>

<style>
span.row-content {
  word-break: break-all;
}
</style>
