<template>
  <div>
    <h1>Fermented {{id}}</h1>
    <p>Edit purchase fermented batch</p>
    <router-link :to="{ path: `/purchase/fermented/detail/${id}`}" exact>
      Back to Purchase Batch List
    </router-link>
  </div>
</template>

<script>
export default {
  name: 'FermentedEdit',
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
