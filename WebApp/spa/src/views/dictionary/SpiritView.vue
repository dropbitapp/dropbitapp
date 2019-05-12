<template>
  <div class="container">
    <div class="box">
      <dbit-table :data="spirits" rowId="SpiritId">
        <template slot="columns" slot-scope="props">
          <b-table-column
            field="SpiritId"
            label="ID"
            width="50"
            sortable
          >{{ props.row.SpiritId }}</b-table-column>
          <b-table-column
            field="SpiritName"
            label="Name"
            sortable
          ><span class="row-content">{{ props.row.SpiritName }}</span></b-table-column>
          <b-table-column
            field="ProcessingReportTypeName"
            label="Processing Report Type"
            sortable
          ><span class="row-content">{{ props.row.ProcessingReportTypeName }}</span></b-table-column>
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
  name: 'SpiritView',
  components: {
    DbitTable,
  },
  created() {
    this.$store.dispatch('dictionary/getSpirits');
  },
  computed: {
    spirits() {
      if (this.$store.state.dictionary.spirits) {
        return this.$store.state.dictionary.spirits;
      }
      return [{}];
    },
  },
  methods: {
    deleteItem(spirit) {
      this.$store
        .dispatch('dictionary/deleteSpirit', spirit.SpiritId)
        .then(() => {
          this.$toast.open({
            duration: 5000,
            message: `Successfully deleted spirit: ${spirit.SpiritName}`,
            type: 'is-success',
          });
        })
        .catch(() => {
          this.$toast.open({
            duration: 5000,
            message: `Unable to delete spirit: ${spirit.SpiritName}`,
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
