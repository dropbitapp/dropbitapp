<template>
  <div>
    <h1>Blending {{id}}</h1>
    <p>Edit production blending batch</p>
    <router-link :to="{ path: `/production/blending/detail/${id}`}" exact>
      Back to Production Batch List
    </router-link>
  </div>
</template>

<script>
export default {
  name: 'BlendingEdit',
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
