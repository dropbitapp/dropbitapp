<template>
  <div>
    <h1>Distilled {{id}}</h1>
    <p>Edit purchase distilled batch</p>
    <router-link :to="{ path: `/purchase/distilled/detail/${id}`}" exact>
      Back to Purchase Batch List
    </router-link>
  </div>
</template>

<script>
export default {
  name: 'DistilledEdit',
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
