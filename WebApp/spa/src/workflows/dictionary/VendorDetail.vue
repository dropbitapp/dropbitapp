<template>
  <div>
    <h1>Vendor {{id}}</h1>
    <button @click="edit()">Edit</button>
    <p>Detail for dictionary vendor item</p>
    <router-link to="/dictionary" exact>Back to Dictionary Item</router-link>
  </div>
</template>

<script>
export default {
  name: 'VendorDetail',
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
      this.$router.push(`/dictionary/vendor/edit/${this.id}`);
    },
  },
};
</script>
