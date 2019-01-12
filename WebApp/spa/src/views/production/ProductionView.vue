<template>
  <div>
    <h1>Production</h1>
    <p>View for displaying a list of existing production batches</p>
    <router-link to="/production/add" exact>
      <button>Add Production Batch</button>
    </router-link>
    <div class="table-wrapper">
      <table>
        <tr>
          <th>id</th>
          <th>name</th>
          <th>type</th>
          <th>actions</th>
        </tr>
        <template v-for="batch in productions">
          <tr :key="batch.ProductionId">
            <td @click="showDetail(batch)">{{batch.ProductionId}}</td>
            <td @click="showDetail(batch)">{{batch.BatchName}}</td>
            <td @click="showDetail(batch)">{{batch.ProductionType}}</td>
            <td>
              <button @click="deleteBatch(batch)">delete</button>
            </td>
          </tr>
        </template>
      </table>
    </div>
  </div>
</template>

<script>
export default {
  name: 'ProductionView',
  created() {
    this.$store.dispatch('production/getDistilled');
  },
  computed: {
    productions() {
      return this.$store.state.production.distilled;
    },
  },
  methods: {
    showDetail(batch) {
      this.$router.push(`/production/${batch.type}/detail/${batch.id}`);
    },
    deleteBatch(batch) {
      // eslint-disable-next-line no-alert
      // eslint-disable-next-line no-restricted-globals
      confirm(`Delete ${batch.name}?`);
    },
  },
};
</script>

<style scoped>
table {
  margin-top: 15px;
}

table, th, td {
    border: 1px solid black;
    border-collapse: collapse;
}

th, td {
    padding: 15px;
}

.table-wrapper {
  display: flex;
  justify-content: center;
}
</style>
