<template>
  <div>
    <h1>Distillation {{id}}</h1>
    <p>Edit production distillation batch</p>
    <router-link :to="{ path: `/production/distillation/detail/${id}` }" exact>
      Back to Production Batch
    </router-link>
  </div>
</template>

<script>
export default {
  name: 'DistillationEdit',
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
