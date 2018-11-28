<template>
  <div>
    <h1>Storage {{id}}</h1>
    <button @click="edit()">Edit</button>
    <p>Detail for dictionary storage item</p>
    <router-link to="/dictionary" exact>Back to Dictionary Item</router-link>
  </div>
</template>

<script>
export default {
  name: 'StorageDetail',
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
      this.$router.push(`/dictionary/storage/edit/${this.id}`);
    },
  },
};
</script>
