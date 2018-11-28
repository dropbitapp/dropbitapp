<template>
  <div>
    <h1>Blending {{id}}</h1>
    <button @click="edit()">Edit</button>
    <p>Detail for production blending batch</p>
    <router-link to="/production" exact>Back to Production Batch</router-link>
  </div>
</template>

<script>
export default {
  name: 'BlendingDetail',
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
      this.$router.push(`/production/blending/edit/${this.id}`);
    },
  },
};
</script>
