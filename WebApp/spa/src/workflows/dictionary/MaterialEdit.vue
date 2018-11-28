<template>
  <div>
    <h1>Material {{id}}</h1>
    <p>Edit dictionary material item</p>
    <router-link :to="{ path: `/dictionary/material/detail/${id}` }" exact>
      Back to Dictionary Item
    </router-link>
  </div>
</template>

<script>
export default {
  name: 'MaterialEdit',
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
