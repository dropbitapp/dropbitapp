<template>
  <div>
    <h1>Distilled {{id}}</h1>
    <button @click="edit()">Edit</button>
    <p>Detail for purchase distilled batch</p>
    <router-link to="/purchase" exact>Back to Purchase Batch List</router-link>
  </div>
</template>

<script>
export default {
  name: 'DistilledDetail',
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
      this.$router.push(`/purchase/distilled/edit/${this.id}`);
    },
  },
};
</script>
