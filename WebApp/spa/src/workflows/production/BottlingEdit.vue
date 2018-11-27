<template>
  <div>
    <h1>Bottling {{id}}</h1>
    <p>Edit production bottling batch</p>
    <router-link :to="{ path: `/production/bottling/detail/${id}`}" exact>
      Back to Production Batch List
    </router-link>
  </div>
</template>

<script>
export default {
  name: 'BottlingEdit',
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
