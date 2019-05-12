<template>
  <div class="container">
    <div class="box">
      <dbit-table :data="storages" rowId="StorageId">
        <template slot="columns" slot-scope="props">
          <b-table-column
            field="StorageId"
            label="ID"
            width="50"
            sortable
          >{{ props.row.StorageId }}</b-table-column>
          <b-table-column
            field="StorageName"
            label="Name"
            sortable
          ><span class="row-content">{{ props.row.StorageName }}</span></b-table-column>
          <b-table-column
            field="SerialNumber"
            label="Serial"
            sortable
          ><span class="row-content">{{ props.row.SerialNumber }}</span></b-table-column>
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
  name: 'StorageView',
  components: {
    DbitTable,
  },
  created() {
    this.$store.dispatch('dictionary/getStorages');
  },
  computed: {
    storages() {
      if (this.$store.state.dictionary.storages) {
        return this.$store.state.dictionary.storages;
      }
      return [{}];
    },
  },
  methods: {
    deleteItem(storage) {
      this.$store
        .dispatch('dictionary/deleteStorage', storage.StorageId)
        .then(() => {
          this.$toast.open({
            duration: 5000,
            message: `Successfully deleted storage: ${storage.StorageName}`,
            type: 'is-success',
          });
        })
        .catch(() => {
          this.$toast.open({
            duration: 5000,
            message: `Unable to delete storage: ${storage.StorageName}`,
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
