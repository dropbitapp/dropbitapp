<template>
  <div>
    <h1>Additive {{id}}</h1>
    <button @click="edit()">Edit</button>
    <p>Detail for purchase additive batch</p>
    <router-link to="/purchase" exact>Back to Purchase Batch List</router-link>
  </div>
</template>

<script>
export default {
  name: 'AdditiveDetail',
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
      this.$router.push(`/purchase/additive/edit/${this.id}`);
    },
  },
};
</script>
