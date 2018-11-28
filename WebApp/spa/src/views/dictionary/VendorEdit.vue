<template>
  <div>
    <h1>Vendor {{id}}</h1>
    <p>Edit dictionary vendor item</p>
    <router-link :to="{ path: `/dictionary/vendor/detail/${id}` }" exact>
      Back to Dictionary Item
    </router-link>
  </div>
</template>

<script>
export default {
  name: 'VendorEdit',
  props: {
    id: {
      type: [Number, String],
      validator(value) {
        return Number.isInteger(Number(value));
      },
    },
  },
};
</script>
