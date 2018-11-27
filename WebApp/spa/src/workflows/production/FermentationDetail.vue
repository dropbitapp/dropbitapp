<template>
  <div>
    <h1>Fermentation {{id}}</h1>
    <button @click="edit()">Edit</button>
    <p>Detail for production fermentation batch</p>
    <router-link to="/production" exact>Back to Production Batch List</router-link>
  </div>
</template>

<script>
export default {
  name: 'FermentationDetail',
  props: {
    id: {
      type: [Number, String],
      validator(value) {
        return Number.isInteger(Number(value));
      },
    },
  },
  methods: {
    edit() {
      this.$router.push(`/production/fermentation/edit/${this.id}`);
    },
  },
};
</script>
