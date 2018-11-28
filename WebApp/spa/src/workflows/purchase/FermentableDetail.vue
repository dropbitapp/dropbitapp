<template>
  <div>
    <h1>Fermentable {{id}}</h1>
    <button @click="edit()">Edit</button>
    <p>Detail for purchase fermentable batch</p>
    <router-link to="/purchase" exact>Back to Purchase Batch List</router-link>
  </div>
</template>

<script>
export default {
  name: 'FermentableDetail',
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
      this.$router.push(`/purchase/fermentable/edit/${this.id}`);
    },
  },
};
</script>
