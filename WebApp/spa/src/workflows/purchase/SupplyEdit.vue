<template>
  <div>
    <h1>Supply {{id}}</h1>
    <p>Edit purchase supply batch</p>
    <router-link :to="{ path: `/purchase/supply/detail/${id}` }" exact>
      Back to Purchase Batch
    </router-link>
  </div>
</template>

<script>
export default {
  name: 'SupplyEdit',
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
