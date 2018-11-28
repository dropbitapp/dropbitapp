<template>
  <div>
    <h1>Storage {{id}}</h1>
    <p>Edit dictionary storage item</p>
    <router-link :to="{ path: `/dictionary/storage/detail/${id}` }" exact>
      Back to Dictionary Item
    </router-link>
  </div>
</template>

<script>
export default {
  name: 'StorageEdit',
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
