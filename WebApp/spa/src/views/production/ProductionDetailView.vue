<template>
  <div>
    <h1>Production Detail</h1>
    <p>
      View for editing and displaying details of a specific production batch.
      Type of information may differ depending on production type
    </p>
    <button @click="edit()">Edit</button>
    <div>
      <router-link to="/production" exact>Back to Production Batch List</router-link>
    </div>
    <ul>
      <li>ID: {{batch.id}}</li>
      <li>Type: {{batch.type}}</li>
      <li>Name: {{batch.name}}</li>
    </ul>
  </div>
</template>

<script>
export default {
  name: 'ProductionDetail',
  props: {
    productionType: { type: String },
    id: {
      type: [Number, String],
      validator(value) {
        return Number.isInteger(Number(value));
      },
    },
    batch: { type: Object },
  },
  methods: {
    edit() {
      // TODO: Separate edit into separate view, then v-if depending on productionType
      // or separate into each individual view for each form type
      console.log(`edit production batch ${this.batch.id}`);
    },
  },
};
</script>

<style scoped>
</style>
