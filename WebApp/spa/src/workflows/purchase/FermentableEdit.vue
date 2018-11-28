<template>
  <div>
    <h1>Fermentable {{id}}</h1>
    <p>Edit purchase fermentable batch</p>
    <router-link :to="{ path: `/purchase/fermentable/detail/${id}` }" exact>
      Back to Purchase Batch
    </router-link>
  </div>
</template>

<script>
export default {
  name: 'FermentableEdit',
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
