<template>
  <div>
    <h1>Fermentation {{id}}</h1>
    <p>Edit production fermentation batch</p>
    <router-link :to="{ path: `/production/fermentation/detail/${id}` }" exact>
      Back to Production Batch
    </router-link>
  </div>
</template>

<script>
export default {
  name: 'FermentationEdit',
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
