<template>
  <div>
    <h1>Purchase</h1>
    <p>View for displaying a list of existing purchase batches</p>
    <router-link to="/purchase/add" exact>
      <button>Add Purchase Batch</button>
    </router-link>
    <div class="table-wrapper">
      Raw Materials List
      <table>
        <tr>
          <th>Material Id</th>
          <th>Material Name</th>
          <th>Unit of Measurement</th>
        </tr>
        <template v-for="batch in rawMaterials">
          <tr :key="batch.RawMaterialId">
            <td @click="showDetail(batch)">{{batch.RawMaterialId}}</td>
            <td @click="showDetail(batch)">{{batch.RawMaterialName}}</td>
            <td @click="showDetail(batch)">{{batch.UnitType}}</td>
          </tr>
        </template>
      </table>
    </div>
    <div class="table-wrapper">
      Purchases
      <table>
        <tr>
          <th>id</th>
          <th>name</th>
          <th>type</th>
          <th>actions</th>
        </tr>
        <template v-for="batch in purchases">
          <tr :key="batch.purchaseId">
            <td @click="showDetail(batch)">{{batch.purchaseId}}</td>
            <td @click="showDetail(batch)">{{batch.PurBatchName}}</td>
            <td @click="showDetail(batch)">{{batch.PurchaseType}}</td>
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
  created() {
    this.$store.dispatch('purchase/getFermentable');
    this.$store.dispatch('dictionary/getRawMaterials');
  },
  computed: {
    purchases() {
      return this.$store.state.purchase.fermentable;
    },
    rawMaterials(){
      return this.$store.state.dictionary.rawMaterials;
    },
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
