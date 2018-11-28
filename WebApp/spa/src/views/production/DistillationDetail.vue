<template>
  <div>
    <h1>Distillation {{id}}</h1>
    <button @click="edit()">Edit</button>
    <p>Detail for production distillation batch</p>
    <router-link to="/production" exact>Back to Production Batch</router-link>
  </div>
</template>

<script>
export default {
  name: 'DistillationDetail',
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
      this.$router.push(`/production/distillation/edit/${this.id}`);
    },
  },
};
</script>
