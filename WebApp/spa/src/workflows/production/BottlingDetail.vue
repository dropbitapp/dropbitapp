<template>
  <div>
    <h1>Bottling {{id}}</h1>
    <button @click="edit()">Edit</button>
    <p>Detail for production bottling batch</p>
    <router-link to="/production" exact>Back to Production Batch List</router-link>
  </div>
</template>

<script>
export default {
  name: 'BottlingDetail',
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
      this.$router.push(`/production/bottling/edit/${this.id}`);
    },
  },
};
</script>
