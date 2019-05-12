<template>
  <div class="container">
    <div class="box">
      <dbit-table :data="distillation" rowId="ProductionId">
        <!-- TODO: Add empty table slot -->
        <template slot="columns" slot-scope="props">
          <!-- ID -->
          <b-table-column field="ProductionId" label="ID" width="50" sortable>
            {{ props.row.ProductionId }}
          </b-table-column>
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
          <!-- SPIRIT CUT -->
          <b-table-column field="SpiritCutName" label="Cut" sortable>
            <span class="row-content">
              <span class="row-content">{{ props.row.SpiritCutName }}</span>
            </span>
          </b-table-column>
          <!-- VOLUME(GAL) -->
          <b-table-column field="Quantity" label="Gal" sortable>
            <template v-if="props.row.Quantity">
              <span class="row-content">{{ props.row.Quantity }}</span>
            </template>
            <template v-else>-</template>
          </b-table-column>
          <!-- WEIGHT(LB) -->
          <b-table-column field="VolumeByWeight" label="Lb" sortable>
            <!-- TODO: Add for all empty optional cell values -->
            <template v-if="props.row.VolumeByWeight">
              <span class="row-content">{{ props.row.VolumeByWeight }}</span>
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
  name: 'DistillationView',
  components: {
    DbitTable,
  },
  created() {
    this.$store.dispatch('production/getProductions', 'Distillation');
  },
  computed: {
    distillation() {
      // TODO: Refactor || empty object array
      if (this.$store.state.production.distillations) {
        return this.$store.state.production.distillations;
      }
      return [{}];
    },
  },
  methods: {
    deleteItem(production) {
      this.$store
        .dispatch('production/deleteProduction', {
          workflow: 'Distillation',
          id: production.ProductionId,
        })
        .then(() => {
          this.$toast.open({
            duration: 5000,
            message: `Successfully deleted distillation: ${
              production.BatchName
            }`,
            type: 'is-success',
          });
        })
        .catch(() => {
          this.$toast.open({
            duration: 5000,
            message: `Unable to delete distillation: ${
              production.BatchName
            }`,
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
