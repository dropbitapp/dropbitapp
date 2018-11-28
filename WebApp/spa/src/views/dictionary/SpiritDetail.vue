<template>
  <div>
    <h1>Spirit {{id}}</h1>
    <button @click="edit()">Edit</button>
    <p>Detail for dictionary spirit item</p>
    <router-link to="/dictionary" exact>Back to Dictionary Item</router-link>
  </div>
</template>

<script>
export default {
  name: 'SpiritDetail',
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
      this.$router.push(`/dictionary/spirit/edit/${this.id}`);
    },
  },
};
</script>
