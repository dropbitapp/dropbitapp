<template>
  <div>
    <h1>Spirit {{id}}</h1>
    <p>Edit dictionary spirit item</p>
    <router-link :to="{ path: `/dictionary/spirit/detail/${id}` }" exact>
      Back to Dictionary Item
    </router-link>
  </div>
</template>

<script>
export default {
  name: 'SpiritEdit',
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
