<template>
  <div class="container">
    <div class="box">
      <dbit-table :data="fermented" rowId="PurchaseId">
        <!-- TODO: Add empty table slot -->
        <template slot="columns" slot-scope="props">
          <!-- ID -->
          <b-table-column field="PurchaseId" label="ID" width="50" sortable>
            {{ props.row.PurchaseId }}
          </b-table-column>
          <!-- NAME -->
          <b-table-column field="PurBatchName" label="Name" sortable>
            <span class="row-content">{{ props.row.PurBatchName }}</span>
          </b-table-column>
          <!-- DATE -->
          <b-table-column field="PurchaseDate" label="Date" sortable centered>
            <span class="row-content">
              <span class="tag is-success">
                {{ new Date(props.row.PurchaseDate).toLocaleDateString() }}
              </span>
            </span>
          </b-table-column>
          <!-- VENDOR -->
          <b-table-column field="VendorName" label="Vendor" sortable>
            <span class="row-content">{{ props.row.VendorName }}</span>
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
          <!-- PRICE -->
          <!-- TODO: Format currency -->
          <b-table-column field="Price" label="Price" sortable>
            <span class="row-content">{{ props.row.Price }}</span>
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
          <!-- TODO: Display Storage -->
          <div class="has-text-weight-bold">Material Type</div>
          <div v-if="props.row.RecordName">
            <p>{{ props.row.RecordName }}</p>
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
  name: 'FermentedView',
  components: {
    DbitTable,
  },
  created() {
    this.$store.dispatch('purchase/getPurchases', 'Fermented');
  },
  computed: {
    fermented() {
      // TODO: Refactor || empty object array
      if (this.$store.state.purchase.fermented) {
        return this.$store.state.purchase.fermented;
      }
      return [{}];
    },
  },
  methods: {
    deleteItem(purchase) {
      this.$store
        .dispatch('purchase/deletePurchase', {
          workflow: 'Fermented',
          id: purchase.PurchaseId,
        })
        .then(() => {
          this.$toast.open({
            duration: 5000,
            message: `Successfully deleted fermented purchase: ${
              purchase.PurBatchName
            }`,
            type: 'is-success',
          });
        })
        .catch(() => {
          this.$toast.open({
            duration: 5000,
            message: `Unable to delete fermented purchase: ${
              purchase.PurBatchName
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
