<template>
  <div>
    <h1>Purchase</h1>
    <p>View for displaying a list of existing purchase batches</p>
    <router-link to="/purchase/add" exact>
      <button>Add Purchase Batch</button>
    </router-link>
    <div class="table-wrapper">
      <table>
        <tr>
          <th>id</th>
          <th>name</th>
          <th>type</th>
          <th>actions</th>
        </tr>
        <template v-for="batch in purchaseBatches">
          <tr :key="batch.id">
            <td @click="showDetail(batch)">{{batch.id}}</td>
            <td @click="showDetail(batch)">{{batch.name}}</td>
            <td @click="showDetail(batch)">{{batch.type}}</td>
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
  name: 'PurchaseView',
  data() {
    return {
      purchaseBatches: [
        { id: 1, type: 'fermentable', name: 'Grapes' },
        { id: 2, type: 'fermented', name: 'Pomace' },
        { id: 3, type: 'distilled', name: 'Vodka' },
        { id: 4, type: 'supply', name: 'Bottles' },
        { id: 5, type: 'additive', name: 'Special Ingrients' },
      ],
    };
  },
  methods: {
    showDetail(batch) {
      this.$router.push(`/purchase/${batch.type}/detail/${batch.id}`);
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
