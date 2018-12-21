<template>
  <div>
    <h1>Add Dictionary Item</h1>
    <p>View for adding new dictionary items</p>
    <div>
      <button @click="addSpirit()">Add Spirit</button>
      <button @click="addVendor()">Add Vendor</button>
    </div>
    <router-link to="/dictionary" exact>Back to Dictionary Item List</router-link>
  </div>
</template>

<script>
export default {
  name: 'AddDictionary',
  methods: {
    addSpirit() {
      const spirit = {
        ProcessingReportTypeID: 1, SpiritName: 'TEST', Note: 'TEST',
      };
      this.$store.dispatch('dictionary/addSpirit', spirit);
    },
    addVendor() {
      const vendor = {
        id: 1,
        name: 'Supply Store',
        type: 'vendor',
      };
      this.$store.commit('addVendor', vendor);
    },
  },
};
</script>

<style scoped>
div button {
  margin: 0 10px 10px;
}
</style>
