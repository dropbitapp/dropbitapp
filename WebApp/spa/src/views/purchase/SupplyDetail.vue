<template>
  <div>
    <h1>Supply {{id}}</h1>
    <button @click="edit()">Edit</button>
    <p>Detail for purchase supply batch</p>
    <router-link to="/purchase" exact>Back to Purchase Batch</router-link>
  </div>
</template>

<script>
export default {
  name: 'SupplyDetail',
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
      this.$router.push(`/purchase/supply/edit/${this.id}`);
    },
  },
};
</script>
