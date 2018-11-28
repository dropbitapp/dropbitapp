<template>
  <div>
    <h1>Fermented {{id}}</h1>
    <button @click="edit()">Edit</button>
    <p>Detail for purchase fermented batch</p>
    <router-link to="/purchase" exact>Back to Purchase Batch List</router-link>
  </div>
</template>

<script>
export default {
  name: 'FermentedDetail',
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
      this.$router.push(`/purchase/fermented/edit/${this.id}`);
    },
  },
};
</script>
