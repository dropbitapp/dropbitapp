<template>
  <div class="container">
    <div class="box">
      <dbit-table :data="vendors" rowId="VendorId">
        <template slot="columns" slot-scope="props">
          <b-table-column
            field="VendorId"
            label="ID"
            width="50"
            sortable
          >{{ props.row.VendorId }}</b-table-column>
          <b-table-column
            field="VendorName"
            label="Name"
            sortable
          ><span class="row-content">{{ props.row.VendorName }}</span></b-table-column>
          <b-table-column>
            <button class="button is-small is-primary is-pulled-right" @click="deleteItem(props.row)">
              <b-icon icon="delete-outline"></b-icon>
            </button>
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
  </div>
</template>

<script>
import DbitTable from '../../components/DbitTable.vue';

export default {
  name: 'VendorView',
  components: {
    DbitTable,
  },
  created() {
    this.$store.dispatch('dictionary/getVendors');
  },
  computed: {
    vendors() {
      if (this.$store.state.dictionary.vendors) {
        return this.$store.state.dictionary.vendors;
      }
      return [{}];
    },
  },
  methods: {
    deleteItem(vendor) {
      this.$store
        .dispatch('dictionary/deleteVendor', vendor.VendorId)
        .then(() => {
          this.$toast.open({
            duration: 5000,
            message: `Successfully deleted vendor: ${vendor.VendorName}`,
            type: 'is-success',
          });
        })
        .catch(() => {
          this.$toast.open({
            duration: 5000,
            message: `Unable to delete vendor: ${vendor.VendorName}`,
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
