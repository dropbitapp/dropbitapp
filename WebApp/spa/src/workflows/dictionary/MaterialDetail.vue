<template>
  <div>
    <h1>Material {{id}}</h1>
    <button @click="edit()">Edit</button>
    <p>Detail for dictionary material item</p>
    <router-link to="/dictionary" exact>Back to Dictionary Item</router-link>
  </div>
</template>

<script>
export default {
  name: 'MaterialDetail',
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
      this.$router.push(`/dictionary/material/edit/${this.id}`);
    },
  },
};
</script>
